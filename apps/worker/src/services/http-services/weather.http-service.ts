import type { Context } from "../../config";
import type {
	ApiResponse,
	CreateWeatherRiskAnalysisRequest,
	GenerateAnalysisRequest,
	PagedResult,
	PaginationQuery,
	TokenPayload,
	WeatherForecastDto,
	WeatherRiskAnalysisDto,
} from "../../dto";

export class WeatherHttpService {
	async getForecastAsync(ctx: Context): Promise<Response> {
		try {
			const query = ctx.req.query();
			const latitude = Number.parseFloat(query.latitude);
			const longitude = Number.parseFloat(query.longitude);
			const days = Number.parseInt(query.days || "7");

			if (Number.isNaN(latitude) || Number.isNaN(longitude)) {
				return ctx.json(
					{
						success: false,
						message: "Valid latitude and longitude are required",
					},
					400,
				);
			}

			if (latitude < -90 || latitude > 90) {
				return ctx.json(
					{
						success: false,
						message: "Latitude must be between -90 and 90",
					},
					400,
				);
			}

			if (longitude < -180 || longitude > 180) {
				return ctx.json(
					{
						success: false,
						message: "Longitude must be between -180 and 180",
					},
					400,
				);
			}

			if (days < 1 || days > 14) {
				return ctx.json(
					{
						success: false,
						message: "Days must be between 1 and 14",
					},
					400,
				);
			}

			// Check cache first
			const cacheKey = `${latitude}_${longitude}_${days}`;
			let forecast = await ctx.var.weatherService.getForecastData(cacheKey);

			if (!forecast) {
				forecast = await ctx.var.weatherApiService.getForecast(
					latitude,
					longitude,
					days,
				);
				// Cache for 1 hour
				await ctx.var.weatherService.storeForecastData(
					cacheKey,
					forecast,
					3600,
				);
			}

			return ctx.json<ApiResponse<WeatherForecastDto[]>>({
				success: true,
				data: forecast,
			});
		} catch (error) {
			ctx.var.logger.trackException(
				"Failed to get weather forecast",
				error as Error,
				{
					latitude: ctx.req.query().latitude,
					longitude: ctx.req.query().longitude,
					days: ctx.req.query().days,
				},
			);
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error
							? error.message
							: "Failed to get weather forecast",
				},
				500,
			);
		}
	}

	async getAnalysesByProjectAsync(ctx: Context): Promise<Response> {
		try {
			const projectId = ctx.req.param("projectId");
			const query = ctx.req.query();

			const pagination: PaginationQuery = {
				pageNumber: Number.parseInt(query.pageNumber || "1"),
				pageSize: Math.min(Number.parseInt(query.pageSize || "10"), 100),
			};

			const result = await ctx.var.weatherService.getAnalysesByProject(
				projectId,
				pagination,
			);

			return ctx.json<ApiResponse<PagedResult<WeatherRiskAnalysisDto>>>({
				success: true,
				data: result,
			});
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error
							? error.message
							: "Failed to get weather analyses",
				},
				500,
			);
		}
	}

	async getAnalysisByIdAsync(ctx: Context): Promise<Response> {
		try {
			const analysisId = ctx.req.param("id");
			const analysis = await ctx.var.weatherService.getAnalysisById(analysisId);

			if (!analysis) {
				return ctx.json(
					{
						success: false,
						message: "Weather analysis not found",
					},
					404,
				);
			}

			return ctx.json<ApiResponse<WeatherRiskAnalysisDto>>({
				success: true,
				data: analysis,
			});
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error
							? error.message
							: "Failed to get weather analysis",
				},
				500,
			);
		}
	}

	async getHighRiskAnalysesAsync(ctx: Context): Promise<Response> {
		try {
			const user = ctx.var.user as TokenPayload;
			const query = ctx.req.query();

			const pagination: PaginationQuery = {
				pageNumber: Number.parseInt(query.pageNumber || "1"),
				pageSize: Math.min(Number.parseInt(query.pageSize || "10"), 100),
			};

			const organizationId = query.organizationId || user.organizationId;
			const result = await ctx.var.weatherService.getHighRiskAnalyses(
				organizationId,
				pagination,
			);

			return ctx.json<ApiResponse<PagedResult<WeatherRiskAnalysisDto>>>({
				success: true,
				data: result,
			});
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error
							? error.message
							: "Failed to get high risk analyses",
				},
				500,
			);
		}
	}

	async createAnalysisAsync(ctx: Context): Promise<Response> {
		try {
			const user = ctx.var.user as TokenPayload;
			const request = await ctx.req.json<CreateWeatherRiskAnalysisRequest>();

			if (!request.projectId || !request.startDate || !request.endDate) {
				return ctx.json(
					{
						success: false,
						message: "Project ID, start date, and end date are required",
					},
					400,
				);
			}

			if (Number.isNaN(request.latitude) || Number.isNaN(request.longitude)) {
				return ctx.json(
					{
						success: false,
						message: "Valid latitude and longitude are required",
					},
					400,
				);
			}

			const analysisId = crypto.randomUUID();
			const now = new Date().toISOString();

			// Get weather forecast for the period
			const days = Math.ceil(
				(new Date(request.endDate).getTime() -
					new Date(request.startDate).getTime()) /
					(1000 * 60 * 60 * 24),
			);
			const forecast = await ctx.var.weatherApiService.getForecast(
				request.latitude,
				request.longitude,
				Math.min(days, 14),
			);

			// Simple risk calculation based on weather conditions
			let riskScore = 0;
			let riskLevel = "low";
			const riskFactors: string[] = [];

			for (const day of forecast) {
				if (day.precipitation.probability > 70) {
					riskScore += 30;
					riskFactors.push("High precipitation probability");
				}
				if (day.windSpeed > 15) {
					riskScore += 20;
					riskFactors.push("High wind speeds");
				}
				if (day.temperature.max > 35 || day.temperature.min < 0) {
					riskScore += 15;
					riskFactors.push("Extreme temperatures");
				}
			}

			if (riskScore > 60) riskLevel = "critical";
			else if (riskScore > 30) riskLevel = "high";
			else if (riskScore > 15) riskLevel = "medium";

			const analysis: WeatherRiskAnalysisDto = {
				id: analysisId,
				projectId: request.projectId,
				taskId: request.taskId,
				analysisDate: now,
				latitude: request.latitude,
				longitude: request.longitude,
				startDate: request.startDate,
				endDate: request.endDate,
				riskLevel,
				riskScore,
				weatherCondition:
					request.weatherCondition || forecast[0]?.condition || "Unknown",
				impactDescription: `Risk analysis based on ${days} days of weather forecast data`,
				recommendedActions:
					riskFactors.length > 0
						? [
								"Monitor weather conditions closely",
								"Consider adjusting schedule for high-risk days",
								"Ensure proper equipment and safety measures",
							]
						: ["Continue with planned schedule"],
				delayRisk: Math.min(riskScore / 10, 10),
				costImpact: riskScore > 30 ? riskScore * 100 : undefined,
				createdBy: user.userId,
				createdAt: now,
				updatedAt: now,
			};

			await ctx.var.weatherService.createAnalysis(analysis);

			ctx.var.logger.trackEvent("Weather analysis created", "Info", {
				analysisId: analysis.id,
				projectId: analysis.projectId,
				riskLevel: analysis.riskLevel,
				riskScore: analysis.riskScore,
			});

			return ctx.json<ApiResponse<WeatherRiskAnalysisDto>>(
				{
					success: true,
					data: analysis,
				},
				201,
			);
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error
							? error.message
							: "Failed to create weather analysis",
				},
				500,
			);
		}
	}

	async generateAnalysisAsync(ctx: Context): Promise<Response> {
		try {
			const user = ctx.var.user as TokenPayload;
			const projectId = ctx.req.param("projectId");
			const request = await ctx.req.json<GenerateAnalysisRequest>();

			if (!request.startDate || !request.endDate) {
				return ctx.json(
					{
						success: false,
						message: "Start date and end date are required",
					},
					400,
				);
			}

			// This would typically integrate with the project service to get project location
			// For now, using default coordinates
			const analysisRequest: CreateWeatherRiskAnalysisRequest = {
				projectId,
				latitude: 40.7128, // Default to NYC coordinates
				longitude: -74.006,
				startDate: request.startDate,
				endDate: request.endDate,
				weatherCondition: "Mixed conditions",
			};

			// Reuse the analysis creation logic
			const createRequest = new Request("http://localhost/analyses", {
				method: "POST",
				headers: { "Content-Type": "application/json" },
				body: JSON.stringify(analysisRequest),
			});

			// This is a simplified approach - in a real implementation,
			// you'd call the analysis creation logic directly
			const response = await ctx.var.weatherApiService.fetch(createRequest, {});
			const result = await response.json();

			return ctx.json(result, response.status);
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error
							? error.message
							: "Failed to generate weather analysis",
				},
				500,
			);
		}
	}

	async updateAnalysisAsync(ctx: Context): Promise<Response> {
		try {
			const analysisId = ctx.req.param("id");
			const updates = await ctx.req.json<Partial<WeatherRiskAnalysisDto>>();

			const existingAnalysis =
				await ctx.var.weatherService.getAnalysisById(analysisId);
			if (!existingAnalysis) {
				return ctx.json(
					{
						success: false,
						message: "Weather analysis not found",
					},
					404,
				);
			}

			await ctx.var.weatherService.updateAnalysis(analysisId, updates);

			const updatedAnalysis =
				await ctx.var.weatherService.getAnalysisById(analysisId);

			if (!updatedAnalysis) {
				return ctx.json(
					{
						success: false,
						message: "Failed to retrieve updated weather analysis",
					},
					500,
				);
			}

			return ctx.json<ApiResponse<WeatherRiskAnalysisDto>>({
				success: true,
				data: updatedAnalysis,
			});
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error
							? error.message
							: "Failed to update weather analysis",
				},
				500,
			);
		}
	}

	async deleteAnalysisAsync(ctx: Context): Promise<Response> {
		try {
			const analysisId = ctx.req.param("id");

			const existingAnalysis =
				await ctx.var.weatherService.getAnalysisById(analysisId);
			if (!existingAnalysis) {
				return ctx.json(
					{
						success: false,
						message: "Weather analysis not found",
					},
					404,
				);
			}

			await ctx.var.weatherService.deleteAnalysis(analysisId);

			return ctx.json({
				success: true,
				message: "Weather analysis deleted successfully",
			});
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error
							? error.message
							: "Failed to delete weather analysis",
				},
				500,
			);
		}
	}
}
