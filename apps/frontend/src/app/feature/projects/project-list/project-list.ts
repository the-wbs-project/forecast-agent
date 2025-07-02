import { CommonModule } from "@angular/common";
import { Component, type OnInit, inject } from "@angular/core";
import { RouterModule } from "@angular/router";
import { ButtonModule } from "@syncfusion/ej2-angular-buttons";
import {
	FilterService,
	GridModule,
	PageService,
	SortService,
	ToolbarService,
} from "@syncfusion/ej2-angular-grids";
import { Project, ProjectStatus } from "../models";
import { ProjectService } from "../services/project";

@Component({
	selector: "app-project-list",
	standalone: true,
	imports: [CommonModule, RouterModule, GridModule, ButtonModule],
	providers: [PageService, SortService, FilterService, ToolbarService],
	templateUrl: "./project-list.html",
	styleUrl: "./project-list.scss",
})
export class ProjectListComponent implements OnInit {
	protected readonly projectService = inject(ProjectService);

	protected readonly pageSettings = { pageSize: 10 };

	async ngOnInit() {
		await this.projectService.getProjects();
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

	protected async confirmDelete(id: string): Promise<void> {
		if (confirm("Are you sure you want to delete this project?")) {
			try {
				await this.projectService.deleteProject(id);
			} catch (error) {
				console.error("Error deleting project:", error);
			}
		}
	}
}
