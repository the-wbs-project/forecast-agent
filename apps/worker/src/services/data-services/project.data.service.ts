import type { PagedResult, PaginationQuery, ProjectDto } from "../../dto";
import type { WeatherGuardApiService } from "../api-services/external-api.service";
import type { KVService } from "./kv.service";

export interface ProjectDataService {
	getProjectById(projectId: string): Promise<ProjectDto | null>;
	getProjectsByOrganization(
		organizationId: string,
		query?: PaginationQuery,
	): Promise<PagedResult<ProjectDto>>;
	createProject(project: ProjectDto): Promise<void>;
	updateProject(projectId: string, updates: Partial<ProjectDto>): Promise<void>;
	deleteProject(projectId: string): Promise<void>;
	searchProjects(
		query: string,
		organizationId?: string,
		pagination?: PaginationQuery,
	): Promise<PagedResult<ProjectDto>>;
}

export class KVProjectDataService implements ProjectDataService {
	constructor(
		private kvService: KVService,
		private apiService: WeatherGuardApiService,
	) { }

	private getProjectKey(projectId: string): string {
		return `project:${projectId}`;
	}

	async getProjectById(projectId: string): Promise<ProjectDto | null> {
		try {
			// Try KV first
			let project = await this.kvService.get<ProjectDto>(
				this.getProjectKey(projectId),
			);

			if (!project) {
				// Fallback to API
				try {
					project = (await this.apiService.getProject(projectId)) as ProjectDto;
					if (project) {
						// Cache in KV for future requests
						await this.createProject(project);
					}
				} catch (apiError) {
					console.warn(
						`API fallback failed for project ${projectId}:`,
						apiError,
					);
				}
			}

			return project;
		} catch (error) {
			// On any error, clear KV cache to ensure consistency
			await this.clearProjectCache(projectId);
			throw error;
		}
	}

	async getProjectsByOrganization(
		organizationId: string,
		query?: PaginationQuery,
	): Promise<PagedResult<ProjectDto>> {
		const pageNumber = query?.pageNumber || 1;
		const pageSize = query?.pageSize || 10;

		// Get list of project IDs for this organization
		const projectListResult = await this.kvService.list(
			`project:org:${organizationId}:`,
		);
		let projectIds = projectListResult.keys
			.map((key) => key.name.split(":").pop())
			.filter((id): id is string => id !== undefined);

		// If no data in KV and API service available, try API fallback
		if (projectIds.length === 0) {
			try {
				const apiProjects = (await this.apiService.getProjects({
					organizationId,
					...query,
				})) as PagedResult<ProjectDto>;
				// Cache API results in KV
				if (apiProjects?.items && Array.isArray(apiProjects.items)) {
					for (const project of apiProjects.items as ProjectDto[]) {
						await this.createProject(project);
					}
					projectIds = (apiProjects.items as ProjectDto[]).map(
						(p: ProjectDto) => p.id,
					);
				}
			} catch (apiError) {
				console.warn(
					`API fallback failed for organization ${organizationId} projects:`,
					apiError,
				);
			}
		}

		// Calculate pagination
		const totalCount = projectIds.length;
		const totalPages = Math.ceil(totalCount / pageSize);
		const startIndex = (pageNumber - 1) * pageSize;
		const endIndex = startIndex + pageSize;
		const paginatedIds = projectIds.slice(startIndex, endIndex);

		// Fetch project details
		const projects = await Promise.all(
			paginatedIds.map((id) => this.getProjectById(id)),
		);

		const validProjects = projects.filter(
			(p: ProjectDto | null): p is ProjectDto => p !== null,
		);

		return {
			items: validProjects,
			totalCount,
			pageNumber,
			pageSize,
			totalPages,
		};
	}

	async createProject(project: ProjectDto): Promise<void> {
		try {
			await this.apiService.createProject(project);

			// Only cache in KV if API succeeded
			const projectKey = this.getProjectKey(project.id);
			const orgProjectKey = `project:org:${project.organizationId}:${project.id}`;

			const metadata = {
				id: project.id,
				createdAt: project.createdAt,
				updatedAt: project.updatedAt,
				createdBy: project.createdBy,
				tags: ["project", project.status, project.organizationId],
			};

			await Promise.all([
				this.kvService.put(projectKey, project, metadata),
				this.kvService.put(orgProjectKey, project.id, metadata), // Index for organization
			]);
		} catch (apiError) {
			console.warn(`API create failed for project ${project.id}:`, apiError);
			// Clear any existing KV data on API failure
			await this.clearProjectCache(project.id, project.organizationId);
			throw apiError;
		}
	}

	async updateProject(
		projectId: string,
		updates: Partial<ProjectDto>,
	): Promise<void> {
		const existingProject = await this.getProjectById(projectId);
		if (!existingProject) throw new Error("Project not found");

		const updatedProject = {
			...existingProject,
			...updates,
			updatedAt: new Date().toISOString(),
		};

		try {
			await this.apiService.updateProject(projectId, updates);

			// Only update KV cache if API succeeded
			const metadata = {
				id: projectId,
				createdAt: existingProject.createdAt,
				updatedAt: updatedProject.updatedAt,
				createdBy: existingProject.createdBy,
				tags: [
					"project",
					updatedProject.status,
					updatedProject.organizationId,
				],
			};

			await this.kvService.put(
				this.getProjectKey(projectId),
				updatedProject,
				metadata,
			);
		} catch (apiError) {
			console.warn(`API update failed for project ${projectId}:`, apiError);
			// Clear KV data on API failure to maintain consistency
			await this.clearProjectCache(projectId, existingProject.organizationId);
			throw apiError;
		}
	}

	async deleteProject(projectId: string): Promise<void> {
		const project = await this.getProjectById(projectId);
		if (!project) return;

		try {
			await this.apiService.deleteProject(projectId);

			// Only clear from KV if API deletion succeeded
			await this.clearProjectCache(projectId, project.organizationId);
		} catch (apiError) {
			console.warn(`API delete failed for project ${projectId}:`, apiError);
			// On API failure, don't clear KV data to maintain state
			throw apiError;
		}
	}

	async searchProjects(
		query: string,
		organizationId?: string,
		pagination?: PaginationQuery,
	): Promise<PagedResult<ProjectDto>> {
		const pageNumber = pagination?.pageNumber || 1;
		const pageSize = pagination?.pageSize || 10;

		// Get all projects for organization or all projects
		const prefix = organizationId
			? `project:org:${organizationId}:`
			: "project:";
		const projectListResult = await this.kvService.list(prefix);

		let allProjects: ProjectDto[] = [];

		if (projectListResult.keys.length === 0) {
			// If no data in KV and API service available, try API fallback
			try {
				const apiProjects = (await this.apiService.getProjects({
					search: query,
					organizationId,
					...pagination,
				})) as PagedResult<ProjectDto>;

				if (apiProjects?.items && Array.isArray(apiProjects.items)) {
					// Cache API results in KV
					for (const project of apiProjects.items as ProjectDto[]) {
						await this.createProject(project);
					}
					allProjects = apiProjects.items as ProjectDto[];
				}
			} catch (apiError) {
				console.warn(
					`API search fallback failed for query "${query}":`,
					apiError,
				);
			}
		} else {
			// Get project details from KV
			const projectPromises = projectListResult.keys.map(async (key) => {
				const projectId = organizationId
					? key.name.split(":").pop()
					: key.name.replace("project:", "");
				if (!projectId) return null;
				return await this.getProjectById(projectId);
			});

			allProjects = (await Promise.all(projectPromises)).filter(
				(p: ProjectDto | null): p is ProjectDto => p !== null,
			);
		}

		// Filter by search query
		const queryLower = query.toLowerCase();
		const filteredProjects = allProjects.filter(
			(project: ProjectDto) =>
				project.name.toLowerCase().includes(queryLower) ||
				project.description?.toLowerCase().includes(queryLower) ||
				project.location?.toLowerCase().includes(queryLower),
		);

		// Apply pagination
		const totalCount = filteredProjects.length;
		const totalPages = Math.ceil(totalCount / pageSize);
		const startIndex = (pageNumber - 1) * pageSize;
		const paginatedProjects = filteredProjects.slice(
			startIndex,
			startIndex + pageSize,
		);

		return {
			items: paginatedProjects,
			totalCount,
			pageNumber,
			pageSize,
			totalPages,
		};
	}

	private async clearProjectCache(
		projectId: string,
		organizationId?: string,
	): Promise<void> {
		try {
			const promises = [this.kvService.delete(this.getProjectKey(projectId))];

			if (organizationId) {
				const orgProjectKey = `project:org:${organizationId}:${projectId}`;
				promises.push(this.kvService.delete(orgProjectKey));
			}

			await Promise.all(promises);
		} catch (error) {
			console.warn(`Failed to clear project cache for ${projectId}:`, error);
		}
	}
}
