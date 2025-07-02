import type { Context } from "../../config";
import type {
	ApiResponse,
	CreateProjectRequest,
	PagedResult,
	PaginationQuery,
	ProjectDto,
	UpdateProjectRequest,
} from "../../dto";

export class ProjectsHttpService {
	async getProjectsAsync(ctx: Context): Promise<Response> {
		try {
			const user = ctx.var.user;
			const query = ctx.req.query();

			console.log("query", query);

			const pagination: PaginationQuery = {
				pageNumber: Number.parseInt(query.pageNumber || "1"),
				pageSize: Math.min(Number.parseInt(query.pageSize || "10"), 100),
			};

			const search = query.search;
			const organizationId = query.organizationId || user.organizationId;

			let result: PagedResult<ProjectDto>;

			if (search) {
				result = await ctx.var.projectService.searchProjects(
					search,
					organizationId,
					pagination,
				);
			} else {
				result = await ctx.var.projectService.getProjectsByOrganization(
					organizationId,
					pagination,
				);
			}

			return ctx.json<ApiResponse<PagedResult<ProjectDto>>>({
				success: true,
				data: result,
			});
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error ? error.message : "Failed to get projects",
				},
				500,
			);
		}
	}

	async getProjectByIdAsync(ctx: Context): Promise<Response> {
		try {
			const projectId = ctx.req.param("id");
			const project = await ctx.var.projectService.getProjectById(projectId);

			if (!project) {
				return ctx.json(
					{
						success: false,
						message: "Project not found",
					},
					404,
				);
			}

			return ctx.json<ApiResponse<ProjectDto>>({
				success: true,
				data: project,
			});
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error ? error.message : "Failed to get project",
				},
				500,
			);
		}
	}

	async createProjectAsync(ctx: Context): Promise<Response> {
		try {
			const user = ctx.var.user;
			const request = await ctx.req.json<CreateProjectRequest>();

			if (!request.organizationId) {
				request.organizationId = user.organizationId;
			}

			if (!request.name || !request.organizationId) {
				return ctx.json(
					{
						success: false,
						message: "Project name and organization ID are required",
					},
					400,
				);
			}

			const projectId = crypto.randomUUID();
			const now = new Date().toISOString();

			const project: ProjectDto = {
				id: projectId,
				name: request.name,
				description: request.description,
				location: request.location,
				latitude: request.latitude,
				longitude: request.longitude,
				startDate: request.startDate,
				endDate: request.endDate,
				status: "planning",
				organizationId: request.organizationId,
				createdBy: user.userId,
				createdAt: now,
				updatedAt: now,
				bimEnabled: request.bimEnabled || false,
				coordinateSystemId: request.coordinateSystemId,
				northDirection: request.northDirection,
				buildingHeight: request.buildingHeight,
				grossFloorArea: request.grossFloorArea,
				numberOfStoreys: request.numberOfStoreys,
			};

			await ctx.var.projectService.createProject(project);

			return ctx.json<ApiResponse<ProjectDto>>(
				{
					success: true,
					data: project,
				},
				201,
			);
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error ? error.message : "Failed to create project",
				},
				500,
			);
		}
	}

	async updateProjectAsync(ctx: Context): Promise<Response> {
		try {
			const projectId = ctx.req.param("id");
			const updates = await ctx.req.json<UpdateProjectRequest>();

			const existingProject =
				await ctx.var.projectService.getProjectById(projectId);
			if (!existingProject) {
				return ctx.json(
					{
						success: false,
						message: "Project not found",
					},
					404,
				);
			}

			await ctx.var.projectService.updateProject(projectId, updates);

			const updatedProject =
				await ctx.var.projectService.getProjectById(projectId);

			if (!updatedProject) {
				return ctx.json(
					{
						success: false,
						message: "Failed to retrieve updated project",
					},
					500,
				);
			}

			return ctx.json<ApiResponse<ProjectDto>>({
				success: true,
				data: updatedProject,
			});
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error ? error.message : "Failed to update project",
				},
				500,
			);
		}
	}

	async deleteProjectAsync(ctx: Context): Promise<Response> {
		try {
			const projectId = ctx.req.param("id");

			const existingProject =
				await ctx.var.projectService.getProjectById(projectId);
			if (!existingProject) {
				return ctx.json(
					{
						success: false,
						message: "Project not found",
					},
					404,
				);
			}

			await ctx.var.projectService.deleteProject(projectId);

			return ctx.json({
				success: true,
				message: "Project deleted successfully",
			});
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error ? error.message : "Failed to delete project",
				},
				500,
			);
		}
	}
}
