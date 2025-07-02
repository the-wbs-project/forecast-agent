import { CommonModule } from "@angular/common";
import { Component, type OnInit, inject, signal } from "@angular/core";
import { ActivatedRoute, RouterModule } from "@angular/router";
import { ButtonModule } from "@syncfusion/ej2-angular-buttons";
import {
	AccumulationChartModule,
	ChartModule,
} from "@syncfusion/ej2-angular-charts";
import {
	FilterService,
	GridModule,
	PageService,
	SortService,
} from "@syncfusion/ej2-angular-grids";
import { TabModule } from "@syncfusion/ej2-angular-navigations";
import type { Task } from "../../../shared/models";
import { TaskService } from "../../../shared/services/task";
import { type Project, ProjectStatus } from "../models";
import { ProjectService } from "../services/project";

@Component({
	selector: "app-project-detail",
	standalone: true,
	imports: [
		CommonModule,
		RouterModule,
		TabModule,
		ButtonModule,
		GridModule,
		ChartModule,
		AccumulationChartModule,
	],
	providers: [PageService, SortService, FilterService],
	templateUrl: "./project-detail.html",
	styleUrl: "./project-detail.scss",
})
export class ProjectDetailComponent implements OnInit {
	private readonly route = inject(ActivatedRoute);
	protected readonly projectService = inject(ProjectService);
	protected readonly taskService = inject(TaskService);

	protected readonly project = signal<Project | null>(null);
	protected readonly tasks = signal<Task[]>([]);
	protected readonly loading = signal(true);
	protected readonly error = signal<string | null>(null);

	protected readonly taskStats = signal({
		total: 0,
		completed: 0,
		inProgress: 0,
		progress: 0,
	});

	async ngOnInit() {
		const projectId = this.route.snapshot.params.id;
		if (projectId) {
			await this.loadProjectData(projectId);
		}
	}

	async loadProjectData(projectId: string) {
		this.loading.set(true);
		try {
			const [project, taskResult] = await Promise.all([
				this.projectService.getProject(projectId),
				this.taskService.getTasksByProject(projectId),
			]);

			this.project.set(project);
			this.tasks.set(taskResult.items);

			this.calculateTaskStats(taskResult.items);
		} catch (error) {
			this.error.set(
				error instanceof Error ? error.message : "Failed to load project data",
			);
		} finally {
			this.loading.set(false);
		}
	}

	calculateTaskStats(tasks: Task[]) {
		const stats = {
			total: tasks.length,
			completed: tasks.filter((t) => t.status === "Completed").length,
			inProgress: tasks.filter((t) => t.status === "InProgress").length,
			progress: 0,
		};

		if (stats.total > 0) {
			const totalProgress = tasks.reduce((sum, task) => sum + task.progress, 0);
			stats.progress = Math.round(totalProgress / stats.total);
		}

		this.taskStats.set(stats);
	}

	protected getStatusClass(status: ProjectStatus): string {
		const statusClasses: Record<ProjectStatus, string> = {
			[ProjectStatus.Planning]: "badge badge-planning",
			[ProjectStatus.InProgress]: "badge badge-in-progress",
			[ProjectStatus.OnHold]: "badge badge-on-hold",
			[ProjectStatus.Completed]: "badge badge-completed",
			[ProjectStatus.Cancelled]: "badge badge-cancelled",
		};
		return statusClasses[status] || "badge badge-secondary";
	}

	protected formatDate(date: Date | string): string {
		return new Date(date).toLocaleDateString();
	}

	protected formatCurrency(amount: number): string {
		return new Intl.NumberFormat("en-US", {
			style: "currency",
			currency: "USD",
		}).format(amount);
	}

	async deleteProject() {
		if (
			confirm(
				"Are you sure you want to delete this project? This action cannot be undone.",
			)
		) {
			try {
				const project = this.project();
				if (project?.id) {
					await this.projectService.deleteProject(project.id);
				}
				window.location.href = "/projects";
			} catch (error) {
				this.error.set(
					error instanceof Error ? error.message : "Failed to delete project",
				);
			}
		}
	}
}
