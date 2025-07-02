import type { Context } from "../../config";
import type {
	ApiResponse,
	CreateTaskRequest,
	PagedResult,
	PaginationQuery,
	TaskDto,
	UpdateTaskRequest,
} from "../../dto";

export class TasksHttpService {
	async getTasksAsync(ctx: Context): Promise<Response> {
		try {
			const projectId = ctx.req.param("projectId");
			const query = ctx.req.query();

			const pagination: PaginationQuery = {
				pageNumber: Number.parseInt(query.pageNumber || "1"),
				pageSize: Math.min(Number.parseInt(query.pageSize || "10"), 100),
			};

			const result = await ctx.var.taskService.getTasksByProject(
				projectId,
				pagination,
			);

			return ctx.json<ApiResponse<PagedResult<TaskDto>>>({
				success: true,
				data: result,
			});
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error ? error.message : "Failed to get tasks",
				},
				500,
			);
		}
	}

	async getTaskByIdAsync(ctx: Context): Promise<Response> {
		try {
			const taskId = ctx.req.param("id");
			const task = await ctx.var.taskService.getTaskById(taskId);

			if (!task) {
				return ctx.json(
					{
						success: false,
						message: "Task not found",
					},
					404,
				);
			}

			return ctx.json<ApiResponse<TaskDto>>({
				success: true,
				data: task,
			});
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error ? error.message : "Failed to get task",
				},
				500,
			);
		}
	}

	async getTasksByScheduleAsync(ctx: Context): Promise<Response> {
		try {
			const scheduleId = ctx.req.param("scheduleId");
			const query = ctx.req.query();

			const pagination: PaginationQuery = {
				pageNumber: Number.parseInt(query.pageNumber || "1"),
				pageSize: Math.min(Number.parseInt(query.pageSize || "10"), 100),
			};

			const result = await ctx.var.taskService.getTasksBySchedule(
				scheduleId,
				pagination,
			);

			return ctx.json<ApiResponse<PagedResult<TaskDto>>>({
				success: true,
				data: result,
			});
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error ? error.message : "Failed to get tasks",
				},
				500,
			);
		}
	}

	async getTasksByParentAsync(ctx: Context): Promise<Response> {
		try {
			const parentTaskId = ctx.req.param("id");
			const childTasks =
				await ctx.var.taskService.getTasksByParent(parentTaskId);

			return ctx.json<ApiResponse<TaskDto[]>>({
				success: true,
				data: childTasks,
			});
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error
							? error.message
							: "Failed to get child tasks",
				},
				500,
			);
		}
	}

	async getWeatherSensitiveTasksAsync(ctx: Context): Promise<Response> {
		try {
			const projectId = ctx.req.param("projectId");
			const weatherSensitiveTasks =
				await ctx.var.taskService.getWeatherSensitiveTasks(projectId);

			return ctx.json<ApiResponse<TaskDto[]>>({
				success: true,
				data: weatherSensitiveTasks,
			});
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error
							? error.message
							: "Failed to get weather sensitive tasks",
				},
				500,
			);
		}
	}

	async createTaskAsync(ctx: Context): Promise<Response> {
		try {
			const request = await ctx.req.json<CreateTaskRequest>();

			if (!request.name || !request.projectId || !request.scheduleId) {
				return ctx.json(
					{
						success: false,
						message: "Task name, project ID, and schedule ID are required",
					},
					400,
				);
			}

			const taskId = crypto.randomUUID();
			const now = new Date().toISOString();

			const task: TaskDto = {
				id: taskId,
				projectId: request.projectId,
				scheduleId: request.scheduleId,
				externalId: request.externalId,
				name: request.name,
				description: request.description,
				startDate: request.startDate,
				endDate: request.endDate,
				duration: request.duration,
				predecessorIds: request.predecessorIds,
				weatherSensitive: request.weatherSensitive || false,
				weatherCategories: request.weatherCategories,
				parentTaskId: request.parentTaskId,
				wbsCode: request.wbsCode,
				taskLevel: request.taskLevel || 0,
				sortOrder: request.sortOrder,
				taskType: request.taskType || "Task",
				outlineNumber: request.outlineNumber,
				isSummaryTask: request.isSummaryTask || false,
				isMilestone: request.isMilestone || false,
				percentComplete: 0,
				criticalPath: false,
				totalFloat: 0,
				freeFloat: 0,
				plannedCost: request.plannedCost,
				bimElementIds: request.bimElementIds,
				locationElementId: request.locationElementId,
				isOverdue: false,
				isInProgress: false,
				createdAt: now,
				updatedAt: now,
			};

			// Calculate computed properties
			if (task.startDate && task.endDate) {
				const start = new Date(task.startDate);
				const end = new Date(task.endDate);
				task.durationInDays = Math.ceil(
					(end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24),
				);
				task.isOverdue = end < new Date() && task.percentComplete < 100;
			}

			await ctx.var.taskService.createTask(task);

			return ctx.json<ApiResponse<TaskDto>>(
				{
					success: true,
					data: task,
				},
				201,
			);
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error ? error.message : "Failed to create task",
				},
				500,
			);
		}
	}

	async updateTaskAsync(ctx: Context): Promise<Response> {
		try {
			const taskId = ctx.req.param("id");
			const updates = await ctx.req.json<UpdateTaskRequest>();

			const existingTask = await ctx.var.taskService.getTaskById(taskId);
			if (!existingTask) {
				return ctx.json(
					{
						success: false,
						message: "Task not found",
					},
					404,
				);
			}

			// Update computed properties if dates change
			const updatedTask = { ...existingTask, ...updates };
			if (updatedTask.startDate && updatedTask.endDate) {
				const start = new Date(updatedTask.startDate);
				const end = new Date(updatedTask.endDate);
				updatedTask.durationInDays = Math.ceil(
					(end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24),
				);
				updatedTask.isOverdue =
					end < new Date() && updatedTask.percentComplete < 100;
			}

			updatedTask.isInProgress =
				!!updatedTask.actualStartDate && !updatedTask.actualEndDate;

			await ctx.var.taskService.updateTask(taskId, updatedTask);

			const finalTask = await ctx.var.taskService.getTaskById(taskId);

			if (!finalTask) {
				return ctx.json(
					{
						success: false,
						message: "Failed to retrieve updated task",
					},
					500,
				);
			}

			return ctx.json<ApiResponse<TaskDto>>({
				success: true,
				data: finalTask,
			});
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error ? error.message : "Failed to update task",
				},
				500,
			);
		}
	}

	async deleteTaskAsync(ctx: Context): Promise<Response> {
		try {
			const taskId = ctx.req.param("id");

			const existingTask = await ctx.var.taskService.getTaskById(taskId);
			if (!existingTask) {
				return ctx.json(
					{
						success: false,
						message: "Task not found",
					},
					404,
				);
			}

			await ctx.var.taskService.deleteTask(taskId);

			return ctx.json({
				success: true,
				message: "Task deleted successfully",
			});
		} catch (error) {
			return ctx.json(
				{
					success: false,
					message:
						error instanceof Error ? error.message : "Failed to delete task",
				},
				500,
			);
		}
	}
}
