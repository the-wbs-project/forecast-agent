import type {
	PagedResult,
	PaginationQuery,
	WeatherForecastDto,
	WeatherRiskAnalysisDto,
} from "../../dto";
import type { WeatherGuardApiService } from "../api-services/external-api.service";
import type { KVService } from "./kv.service";

export interface WeatherDataService {
	getAnalysisById(analysisId: string): Promise<WeatherRiskAnalysisDto | null>;
	getAnalysesByProject(
		projectId: string,
		query?: PaginationQuery,
	): Promise<PagedResult<WeatherRiskAnalysisDto>>;
	createAnalysis(analysis: WeatherRiskAnalysisDto): Promise<void>;
	updateAnalysis(
		analysisId: string,
		updates: Partial<WeatherRiskAnalysisDto>,
	): Promise<void>;
	deleteAnalysis(analysisId: string): Promise<void>;
	getHighRiskAnalyses(
		organizationId?: string,
		query?: PaginationQuery,
	): Promise<PagedResult<WeatherRiskAnalysisDto>>;
	storeForecastData(
		key: string,
		forecast: WeatherForecastDto[],
		expirationSeconds?: number,
	): Promise<void>;
	getForecastData(key: string): Promise<WeatherForecastDto[] | null>;
}

export class KVWeatherDataService implements WeatherDataService {
	constructor(
		private kvService: KVService,
		private apiService?: WeatherGuardApiService,
	) {}

	private getAnalysisKey(analysisId: string): string {
		return `weather_analysis:${analysisId}`;
	}

	private getProjectAnalysisKey(projectId: string, analysisId: string): string {
		return `weather_analysis:project:${projectId}:${analysisId}`;
	}

	private getForecastKey(key: string): string {
		return `weather_forecast:${key}`;
	}

	private getRiskLevelKey(riskLevel: string, analysisId: string): string {
		return `weather_analysis:risk:${riskLevel}:${analysisId}`;
	}

	async getAnalysisById(
		analysisId: string,
	): Promise<WeatherRiskAnalysisDto | null> {
		try {
			// Try KV first
			const analysis = await this.kvService.get<WeatherRiskAnalysisDto>(
				this.getAnalysisKey(analysisId),
			);

			if (!analysis && this.apiService) {
				// Fallback to API - note: WeatherGuardApiService doesn't have getAnalysisById
				// This would need to be implemented in the API service if needed
				console.warn(
					`Weather analysis ${analysisId} not found in KV and API fallback not implemented`,
				);
			}

			return analysis;
		} catch (error) {
			// On any error, clear KV cache to ensure consistency
			await this.clearAnalysisCache(analysisId);
			throw error;
		}
	}

	async getAnalysesByProject(
		projectId: string,
		query?: PaginationQuery,
	): Promise<PagedResult<WeatherRiskAnalysisDto>> {
		try {
			const pageNumber = query?.pageNumber || 1;
			const pageSize = query?.pageSize || 10;

			// Try KV first
			const analysisListResult = await this.kvService.list(
				`weather_analysis:project:${projectId}:`,
			);
			let analysisIds = analysisListResult.keys
				.map((key) => key.name.split(":").pop())
				.filter((id): id is string => id !== undefined);

			// If no data in KV and API service available, try API fallback
			if (analysisIds.length === 0 && this.apiService) {
				try {
					const apiAnalyses = (await this.apiService.getWeatherAnalyses(
						projectId,
						query as Record<string, string>,
					)) as PagedResult<WeatherRiskAnalysisDto>;
					// Cache API results in KV
					if (apiAnalyses?.items && Array.isArray(apiAnalyses.items)) {
						for (const analysis of apiAnalyses.items as WeatherRiskAnalysisDto[]) {
							await this.createAnalysis(analysis);
						}
						analysisIds = (apiAnalyses.items as WeatherRiskAnalysisDto[]).map(
							(a: WeatherRiskAnalysisDto) => a.id,
						);
					}
				} catch (apiError) {
					console.warn(
						`API fallback failed for project ${projectId} weather analyses:`,
						apiError,
					);
				}
			}

			const totalCount = analysisIds.length;
			const totalPages = Math.ceil(totalCount / pageSize);
			const startIndex = (pageNumber - 1) * pageSize;
			const paginatedIds = analysisIds.slice(startIndex, startIndex + pageSize);

			const analyses = await Promise.all(
				paginatedIds.map((id) => this.getAnalysisById(id)),
			);

			const validAnalyses = analyses.filter(
				(a): a is WeatherRiskAnalysisDto => a !== null,
			);

			return {
				items: validAnalyses,
				totalCount,
				pageNumber,
				pageSize,
				totalPages,
			};
		} catch (error) {
			// Clear cache on errors
			await this.clearProjectAnalysesCache(projectId);
			throw error;
		}
	}

	async createAnalysis(analysis: WeatherRiskAnalysisDto): Promise<void> {
		const analysisKey = this.getAnalysisKey(analysis.id);
		const projectAnalysisKey = this.getProjectAnalysisKey(
			analysis.projectId,
			analysis.id,
		);
		const riskLevelKey = this.getRiskLevelKey(analysis.riskLevel, analysis.id);

		const metadata = {
			id: analysis.id,
			createdAt: analysis.createdAt,
			updatedAt: analysis.updatedAt,
			createdBy: analysis.createdBy,
			tags: [
				"weather_analysis",
				analysis.riskLevel,
				analysis.weatherCondition,
				analysis.projectId,
				...(analysis.taskId ? [analysis.taskId] : []),
			],
		};

		await Promise.all([
			this.kvService.put(analysisKey, analysis, metadata),
			this.kvService.put(projectAnalysisKey, analysis.id, metadata),
			this.kvService.put(riskLevelKey, analysis.id, metadata),
		]);
	}

	async updateAnalysis(
		analysisId: string,
		updates: Partial<WeatherRiskAnalysisDto>,
	): Promise<void> {
		try {
			const existingAnalysis = await this.getAnalysisById(analysisId);
			if (!existingAnalysis) throw new Error("Weather analysis not found");

			const updatedAnalysis = {
				...existingAnalysis,
				...updates,
				updatedAt: new Date().toISOString(),
			};

			const metadata = {
				id: analysisId,
				createdAt: existingAnalysis.createdAt,
				updatedAt: updatedAnalysis.updatedAt,
				createdBy: existingAnalysis.createdBy,
				tags: [
					"weather_analysis",
					updatedAnalysis.riskLevel,
					updatedAnalysis.weatherCondition,
					updatedAnalysis.projectId,
					...(updatedAnalysis.taskId ? [updatedAnalysis.taskId] : []),
				],
			};

			await this.kvService.put(
				this.getAnalysisKey(analysisId),
				updatedAnalysis,
				metadata,
			);

			// Update risk level index if changed
			if (existingAnalysis.riskLevel !== updatedAnalysis.riskLevel) {
				await this.kvService.delete(
					this.getRiskLevelKey(existingAnalysis.riskLevel, analysisId),
				);
				await this.kvService.put(
					this.getRiskLevelKey(updatedAnalysis.riskLevel, analysisId),
					analysisId,
					metadata,
				);
			}
		} catch (error) {
			// Clear cache on update errors
			await this.clearAnalysisCache(analysisId);
			throw error;
		}
	}

	async deleteAnalysis(analysisId: string): Promise<void> {
		const analysis = await this.getAnalysisById(analysisId);
		if (!analysis) return;

		// Always clear from KV when deleting
		await this.clearAnalysisCache(analysisId, analysis);
	}

	async getHighRiskAnalyses(
		organizationId?: string,
		query?: PaginationQuery,
	): Promise<PagedResult<WeatherRiskAnalysisDto>> {
		const pageNumber = query?.pageNumber || 1;
		const pageSize = query?.pageSize || 10;

		// Get high risk analyses
		const highRiskResult = await this.kvService.list(
			"weather_analysis:risk:high:",
		);
		const criticalRiskResult = await this.kvService.list(
			"weather_analysis:risk:critical:",
		);

		const analysisIds = [
			...highRiskResult.keys
				.map((key) => key.name.split(":").pop())
				.filter((id): id is string => id !== undefined),
			...criticalRiskResult.keys
				.map((key) => key.name.split(":").pop())
				.filter((id): id is string => id !== undefined),
		];

		// Get analysis details
		const analyses = await Promise.all(
			analysisIds.map((id) => this.getAnalysisById(id)),
		);

		const validAnalyses = analyses.filter(
			(a: WeatherRiskAnalysisDto | null): a is WeatherRiskAnalysisDto =>
				a !== null,
		);

		// Filter by organization if provided (would need project lookup)
		// For now, assuming organizationId filtering is handled at a higher level

		// Sort by risk score descending
		validAnalyses.sort(
			(a: WeatherRiskAnalysisDto, b: WeatherRiskAnalysisDto) =>
				b.riskScore - a.riskScore,
		);

		const totalCount = validAnalyses.length;
		const totalPages = Math.ceil(totalCount / pageSize);
		const startIndex = (pageNumber - 1) * pageSize;
		const paginatedAnalyses = validAnalyses.slice(
			startIndex,
			startIndex + pageSize,
		);

		return {
			items: paginatedAnalyses,
			totalCount,
			pageNumber,
			pageSize,
			totalPages,
		};
	}

	async storeForecastData(
		key: string,
		forecast: WeatherForecastDto[],
		expirationSeconds = 3600,
	): Promise<void> {
		const forecastKey = this.getForecastKey(key);
		const metadata = {
			id: key,
			createdAt: new Date().toISOString(),
			updatedAt: new Date().toISOString(),
			tags: ["weather_forecast", "cached"],
		};

		// Note: KV expiration would be handled by Cloudflare KV TTL if supported
		await this.kvService.put(
			forecastKey,
			{
				data: forecast,
				expiresAt: new Date(
					Date.now() + expirationSeconds * 1000,
				).toISOString(),
			},
			metadata,
		);
	}

	async getForecastData(key: string): Promise<WeatherForecastDto[] | null> {
		const forecastKey = this.getForecastKey(key);
		const result = await this.kvService.get<{
			data: WeatherForecastDto[];
			expiresAt: string;
		}>(forecastKey);

		if (!result) return null;

		// Check if expired
		if (new Date(result.expiresAt) < new Date()) {
			await this.kvService.delete(forecastKey);
			return null;
		}

		return result.data;
	}

	private async clearAnalysisCache(
		analysisId: string,
		analysis?: WeatherRiskAnalysisDto,
	): Promise<void> {
		try {
			const promises = [this.kvService.delete(this.getAnalysisKey(analysisId))];

			if (analysis) {
				promises.push(
					this.kvService.delete(
						this.getProjectAnalysisKey(analysis.projectId, analysisId),
					),
					this.kvService.delete(
						this.getRiskLevelKey(analysis.riskLevel, analysisId),
					),
				);
			}

			await Promise.all(promises);
		} catch (error) {
			console.warn(
				`Failed to clear weather analysis cache for ${analysisId}:`,
				error,
			);
		}
	}

	private async clearProjectAnalysesCache(projectId: string): Promise<void> {
		try {
			const analysisListResult = await this.kvService.list(
				`weather_analysis:project:${projectId}:`,
			);
			const deletePromises = analysisListResult.keys.map((key) =>
				this.kvService.delete(key.name),
			);
			await Promise.all(deletePromises);
		} catch (error) {
			console.warn(
				`Failed to clear project analyses cache for ${projectId}:`,
				error,
			);
		}
	}
}
