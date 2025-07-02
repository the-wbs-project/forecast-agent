import type { WeatherGuardApiService } from "../api-services/external-api.service";
import type { PagedResult, PaginationQuery, TaskDto } from "../dto";
import type { KVService } from "./kv.service";

export interface TaskDataService {
	getTaskById(taskId: string): Promise<TaskDto | null>;
	getTasksByProject(
		projectId: string,
		query?: PaginationQuery,
	): Promise<PagedResult<TaskDto>>;
	getTasksBySchedule(
		scheduleId: string,
		query?: PaginationQuery,
	): Promise<PagedResult<TaskDto>>;
	createTask(task: TaskDto): Promise<void>;
	updateTask(taskId: string, updates: Partial<TaskDto>): Promise<void>;
	deleteTask(taskId: string): Promise<void>;
	getTasksByParent(parentTaskId: string): Promise<TaskDto[]>;
	getWeatherSensitiveTasks(projectId: string): Promise<TaskDto[]>;
}

export class KVTaskDataService implements TaskDataService {
	constructor(
		private kvService: KVService,
		private apiService?: WeatherGuardApiService,
	) {}

	private getTaskKey(taskId: string): string {
		return `task:${taskId}`;
	}

	private getProjectTaskKey(projectId: string, taskId: string): string {
		return `task:project:${projectId}:${taskId}`;
	}

	private getScheduleTaskKey(scheduleId: string, taskId: string): string {
		return `task:schedule:${scheduleId}:${taskId}`;
	}

	private getParentTaskKey(parentTaskId: string, taskId: string): string {
		return `task:parent:${parentTaskId}:${taskId}`;
	}

	async getTaskById(taskId: string): Promise<TaskDto | null> {
		try {
			// Try KV first
			let task = await this.kvService.get<TaskDto>(this.getTaskKey(taskId));

			if (!task && this.apiService) {
				// Fallback to API
				try {
					task = await this.apiService.getTask(taskId);
					if (task) {
						// Cache in KV for future requests
						await this.createTask(task);
					}
				} catch (apiError) {
					console.warn(`API fallback failed for task ${taskId}:`, apiError);
				}
			}

			return task;
		} catch (error) {
			// On any error, clear KV cache to ensure consistency
			await this.clearTaskCache(taskId);
			throw error;
		}
	}

	async getTasksByProject(
		projectId: string,
		query?: PaginationQuery,
	): Promise<PagedResult<TaskDto>> {
		try {
			const pageNumber = query?.pageNumber || 1;
			const pageSize = query?.pageSize || 10;

			// Try KV first
			const taskListResult = await this.kvService.list(
				`task:project:${projectId}:`,
			);
			let taskIds = taskListResult.keys
				.map((key) => key.name.split(":").pop())
				.filter((id): id is string => id !== undefined);

			// If no data in KV and API service available, try API fallback
			if (taskIds.length === 0 && this.apiService) {
				try {
					const apiTasks = await this.apiService.getTasks(projectId, query);
					// Cache API results in KV
					if (apiTasks && Array.isArray(apiTasks.items)) {
						for (const task of apiTasks.items) {
							await this.createTask(task);
						}
						taskIds = apiTasks.items.map((t) => t.id);
					}
				} catch (apiError) {
					console.warn(
						`API fallback failed for project ${projectId} tasks:`,
						apiError,
					);
				}
			}

			const totalCount = taskIds.length;
			const totalPages = Math.ceil(totalCount / pageSize);
			const startIndex = (pageNumber - 1) * pageSize;
			const paginatedIds = taskIds.slice(startIndex, startIndex + pageSize);

			const tasks = await Promise.all(
				paginatedIds.map((id) => this.getTaskById(id)),
			);

			const validTasks = tasks.filter((t): t is TaskDto => t !== null);

			return {
				items: validTasks,
				totalCount,
				pageNumber,
				pageSize,
				totalPages,
			};
		} catch (error) {
			// Clear cache on errors
			await this.clearProjectTasksCache(projectId);
			throw error;
		}
	}

	async getTasksBySchedule(
		scheduleId: string,
		query?: PaginationQuery,
	): Promise<PagedResult<TaskDto>> {
		const pageNumber = query?.pageNumber || 1;
		const pageSize = query?.pageSize || 10;

		const taskListResult = await this.kvService.list(
			`task:schedule:${scheduleId}:`,
		);
		const taskIds = taskListResult.keys
			.map((key) => key.name.split(":").pop())
			.filter((id): id is string => id !== undefined);

		const totalCount = taskIds.length;
		const totalPages = Math.ceil(totalCount / pageSize);
		const startIndex = (pageNumber - 1) * pageSize;
		const paginatedIds = taskIds.slice(startIndex, startIndex + pageSize);

		const tasks = await Promise.all(
			paginatedIds.map((id) => this.getTaskById(id)),
		);

		const validTasks = tasks.filter((t): t is TaskDto => t !== null);

		return {
			items: validTasks,
			totalCount,
			pageNumber,
			pageSize,
			totalPages,
		};
	}

	async createTask(task: TaskDto): Promise<void> {
		const taskKey = this.getTaskKey(task.id);
		const projectTaskKey = this.getProjectTaskKey(task.projectId, task.id);
		const scheduleTaskKey = this.getScheduleTaskKey(task.scheduleId, task.id);

		const metadata = {
			id: task.id,
			createdAt: task.createdAt,
			updatedAt: task.updatedAt,
			tags: [
				"task",
				task.taskType,
				task.projectId,
				task.scheduleId,
				...(task.weatherSensitive ? ["weather_sensitive"] : []),
				...(task.criticalPath ? ["critical_path"] : []),
			],
		};

		const promises = [
			this.kvService.put(taskKey, task, metadata),
			this.kvService.put(projectTaskKey, task.id, metadata),
			this.kvService.put(scheduleTaskKey, task.id, metadata),
		];

		// Create parent-child relationship index
		if (task.parentTaskId) {
			const parentTaskKey = this.getParentTaskKey(task.parentTaskId, task.id);
			promises.push(this.kvService.put(parentTaskKey, task.id, metadata));
		}

		await Promise.all(promises);
	}

	async updateTask(taskId: string, updates: Partial<TaskDto>): Promise<void> {
		try {
			const existingTask = await this.getTaskById(taskId);
			if (!existingTask) throw new Error("Task not found");

			const updatedTask = {
				...existingTask,
				...updates,
				updatedAt: new Date().toISOString(),
			};

			const metadata = {
				id: taskId,
				createdAt: existingTask.createdAt,
				updatedAt: updatedTask.updatedAt,
				tags: [
					"task",
					updatedTask.taskType,
					updatedTask.projectId,
					updatedTask.scheduleId,
					...(updatedTask.weatherSensitive ? ["weather_sensitive"] : []),
					...(updatedTask.criticalPath ? ["critical_path"] : []),
				],
			};

			await this.kvService.put(this.getTaskKey(taskId), updatedTask, metadata);

			// Handle parent task changes
			if (existingTask.parentTaskId !== updatedTask.parentTaskId) {
				// Remove old parent relationship
				if (existingTask.parentTaskId) {
					await this.kvService.delete(
						this.getParentTaskKey(existingTask.parentTaskId, taskId),
					);
				}

				// Add new parent relationship
				if (updatedTask.parentTaskId) {
					const parentTaskKey = this.getParentTaskKey(
						updatedTask.parentTaskId,
						taskId,
					);
					await this.kvService.put(parentTaskKey, taskId, metadata);
				}
			}
		} catch (error) {
			// Clear cache on update errors
			await this.clearTaskCache(taskId);
			throw error;
		}
	}

	async deleteTask(taskId: string): Promise<void> {
		const task = await this.getTaskById(taskId);
		if (!task) return;

		// Always clear from KV when deleting
		await this.clearTaskCache(taskId, task);
	}

	async getTasksByParent(parentTaskId: string): Promise<TaskDto[]> {
		const taskListResult = await this.kvService.list(
			`task:parent:${parentTaskId}:`,
		);
		const taskIds = taskListResult.keys
			.map((key) => key.name.split(":").pop())
			.filter((id): id is string => id !== undefined);

		const tasks = await Promise.all(taskIds.map((id) => this.getTaskById(id)));

		return tasks.filter((t): t is TaskDto => t !== null);
	}

	async getWeatherSensitiveTasks(projectId: string): Promise<TaskDto[]> {
		const projectTasks = await this.getTasksByProject(projectId, {
			pageNumber: 1,
			pageSize: 1000,
		});
		return projectTasks.items.filter((task) => task.weatherSensitive);
	}

	private async clearTaskCache(taskId: string, task?: TaskDto): Promise<void> {
		try {
			const promises = [this.kvService.delete(this.getTaskKey(taskId))];

			if (task) {
				promises.push(
					this.kvService.delete(this.getProjectTaskKey(task.projectId, taskId)),
					this.kvService.delete(
						this.getScheduleTaskKey(task.scheduleId, taskId),
					),
				);

				if (task.parentTaskId) {
					promises.push(
						this.kvService.delete(
							this.getParentTaskKey(task.parentTaskId, taskId),
						),
					);
				}
			}

			await Promise.all(promises);
		} catch (error) {
			console.warn(`Failed to clear task cache for ${taskId}:`, error);
		}
	}

	private async clearProjectTasksCache(projectId: string): Promise<void> {
		try {
			const taskListResult = await this.kvService.list(
				`task:project:${projectId}:`,
			);
			const deletePromises = taskListResult.keys.map((key) =>
				this.kvService.delete(key.name),
			);
			await Promise.all(deletePromises);
		} catch (error) {
			console.warn(
				`Failed to clear project tasks cache for ${projectId}:`,
				error,
			);
		}
	}
}
