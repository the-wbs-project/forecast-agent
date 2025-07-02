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
		private apiService?: WeatherGuardApiService,
	) {}

	private getProjectKey(projectId: string): string {
		return `project:${projectId}`;
	}

	private getOrganizationProjectsKey(organizationId: string): string {
		return `org_projects:${organizationId}`;
	}

	async getProjectById(projectId: string): Promise<ProjectDto | null> {
		try {
			// Try KV first
			let project = await this.kvService.get<ProjectDto>(
				this.getProjectKey(projectId),
			);

			if (!project && this.apiService) {
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
		const projectIds = projectListResult.keys
			.map((key) => key.name.split(":").pop())
			.filter((id): id is string => id !== undefined);

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
	}

	async updateProject(
		projectId: string,
		updates: Partial<ProjectDto>,
	): Promise<void> {
		try {
			const existingProject = await this.getProjectById(projectId);
			if (!existingProject) throw new Error("Project not found");

			const updatedProject = {
				...existingProject,
				...updates,
				updatedAt: new Date().toISOString(),
			};

			const metadata = {
				id: projectId,
				createdAt: existingProject.createdAt,
				updatedAt: updatedProject.updatedAt,
				createdBy: existingProject.createdBy,
				tags: ["project", updatedProject.status, updatedProject.organizationId],
			};

			await this.kvService.put(
				this.getProjectKey(projectId),
				updatedProject,
				metadata,
			);
		} catch (error) {
			// Clear cache on update errors to ensure consistency
			await this.clearProjectCache(projectId);
			throw error;
		}
	}

	async deleteProject(projectId: string): Promise<void> {
		const project = await this.getProjectById(projectId);
		if (!project) return;

		// Always clear from KV when deleting
		await this.clearProjectCache(projectId, project.organizationId);
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

		// Get project details and filter by search query
		const projectPromises = projectListResult.keys.map(async (key) => {
			const projectId = organizationId
				? key.name.split(":").pop()
				: key.name.replace("project:", "");
			if (!projectId) return null;
			return await this.getProjectById(projectId);
		});

		const allProjects = (await Promise.all(projectPromises)).filter(
			(p: ProjectDto | null): p is ProjectDto => p !== null,
		);

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
