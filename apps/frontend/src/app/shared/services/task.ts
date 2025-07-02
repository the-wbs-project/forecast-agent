import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable, inject, signal } from "@angular/core";
import { firstValueFrom } from "rxjs";
import { environment } from "../../environments/environment";
import type { Task, TaskRiskDetail } from "../models";
import type { ApiResponse, PagedResult } from "../models";

@Injectable({
	providedIn: "root",
})
export class TaskService {
	private http = inject(HttpClient);
	private apiUrl = `${environment.apiUrl}/tasks`;

	tasks = signal<Task[]>([]);
	currentTask = signal<Task | null>(null);
	loading = signal(false);
	error = signal<string | null>(null);

	async getTasksByProject(
		projectId: string,
		pageNumber = 1,
		pageSize = 50,
	): Promise<PagedResult<Task>> {
		this.loading.set(true);
		this.error.set(null);

		try {
			const params = new HttpParams()
				.set("pageNumber", pageNumber.toString())
				.set("pageSize", pageSize.toString());

			const response = await firstValueFrom(
				this.http.get<ApiResponse<PagedResult<Task>>>(
					`${this.apiUrl}/project/${projectId}`,
					{ params },
				),
			);

			if (response?.success && response.data) {
				this.tasks.set(response.data.items);
				return response.data;
			}

			throw new Error(response?.message || "Failed to fetch tasks");
		} catch (error) {
			this.error.set(
				error instanceof Error ? error.message : "An error occurred",
			);
			throw error;
		} finally {
			this.loading.set(false);
		}
	}

	async getTask(id: string): Promise<Task> {
		this.loading.set(true);
		this.error.set(null);

		try {
			const response = await this.http
				.get<ApiResponse<Task>>(`${this.apiUrl}/${id}`)
				.toPromise();

			if (response?.success && response.data) {
				this.currentTask.set(response.data);
				return response.data;
			}

			throw new Error(response?.message || "Failed to fetch task");
		} catch (error) {
			this.error.set(
				error instanceof Error ? error.message : "An error occurred",
			);
			throw error;
		} finally {
			this.loading.set(false);
		}
	}

	async createTask(task: Partial<Task>): Promise<Task> {
		this.loading.set(true);
		this.error.set(null);

		try {
			const response = await this.http
				.post<ApiResponse<Task>>(this.apiUrl, task)
				.toPromise();

			if (response?.success && response.data) {
				const currentTasks = this.tasks();
				this.tasks.set([...currentTasks, response.data]);
				return response.data;
			}

			throw new Error(response?.message || "Failed to create task");
		} catch (error) {
			this.error.set(
				error instanceof Error ? error.message : "An error occurred",
			);
			throw error;
		} finally {
			this.loading.set(false);
		}
	}

	async updateTask(id: string, task: Partial<Task>): Promise<Task> {
		this.loading.set(true);
		this.error.set(null);

		try {
			const response = await this.http
				.put<ApiResponse<Task>>(`${this.apiUrl}/${id}`, task)
				.toPromise();

			if (response?.success && response.data) {
				const currentTasks = this.tasks();
				const index = currentTasks.findIndex((t) => t.id === id);
				if (index !== -1) {
					currentTasks[index] = response.data;
					this.tasks.set([...currentTasks]);
				}

				if (this.currentTask()?.id === id) {
					this.currentTask.set(response.data);
				}

				return response.data;
			}

			throw new Error(response?.message || "Failed to update task");
		} catch (error) {
			this.error.set(
				error instanceof Error ? error.message : "An error occurred",
			);
			throw error;
		} finally {
			this.loading.set(false);
		}
	}

	async deleteTask(id: string): Promise<void> {
		this.loading.set(true);
		this.error.set(null);

		try {
			const response = await this.http
				.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
				.toPromise();

			if (response?.success) {
				const currentTasks = this.tasks();
				this.tasks.set(currentTasks.filter((t) => t.id !== id));

				if (this.currentTask()?.id === id) {
					this.currentTask.set(null);
				}

				return;
			}

			throw new Error(response?.message || "Failed to delete task");
		} catch (error) {
			this.error.set(
				error instanceof Error ? error.message : "An error occurred",
			);
			throw error;
		} finally {
			this.loading.set(false);
		}
	}

	async getTaskRisks(taskId: string): Promise<TaskRiskDetail[]> {
		this.loading.set(true);
		this.error.set(null);

		try {
			const response = await this.http
				.get<ApiResponse<TaskRiskDetail[]>>(`${this.apiUrl}/${taskId}/risks`)
				.toPromise();

			if (response?.success && response.data) {
				return response.data;
			}

			throw new Error(response?.message || "Failed to fetch task risks");
		} catch (error) {
			this.error.set(
				error instanceof Error ? error.message : "An error occurred",
			);
			throw error;
		} finally {
			this.loading.set(false);
		}
	}
}
