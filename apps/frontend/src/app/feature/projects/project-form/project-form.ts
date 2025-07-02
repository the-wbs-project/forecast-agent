import { CommonModule } from "@angular/common";
import { Component, type OnInit, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { ButtonModule } from "@syncfusion/ej2-angular-buttons";
import { DatePickerModule } from "@syncfusion/ej2-angular-calendars";
import { DropDownListModule } from "@syncfusion/ej2-angular-dropdowns";
import { TextBoxModule } from "@syncfusion/ej2-angular-inputs";
import { NumericTextBoxModule } from "@syncfusion/ej2-angular-inputs";
import {
	type CreateProjectRequest,
	Project,
	ProjectStatus,
	type UpdateProjectRequest,
} from "../models";
import { ProjectService } from "../services/project";

@Component({
	selector: "app-project-form",
	standalone: true,
	imports: [
		CommonModule,
		FormsModule,
		TextBoxModule,
		DatePickerModule,
		NumericTextBoxModule,
		DropDownListModule,
		ButtonModule,
	],
	templateUrl: "./project-form.html",
	styleUrl: "./project-form.scss",
})
export class ProjectFormComponent implements OnInit {
	private router = inject(Router);
	private route = inject(ActivatedRoute);
	private projectService = inject(ProjectService);

	protected readonly isEditMode = signal(false);
	protected readonly projectId = signal<string | null>(null);
	protected readonly loading = signal(false);
	protected readonly error = signal<string | null>(null);
	protected readonly submitting = signal(false);

	formData: {
		name: string;
		description: string;
		startDate: Date;
		endDate: Date;
		budget: number | null;
		location: string;
		status: ProjectStatus;
	} = {
		name: "",
		description: "",
		startDate: new Date(),
		endDate: new Date(),
		budget: null,
		location: "",
		status: ProjectStatus.Planning,
	};

	protected readonly statusOptions = Object.entries(ProjectStatus).map(
		([key, value]) => ({
			text: value,
			value: value,
		}),
	);

	async ngOnInit() {
		const id = this.route.snapshot.params.id;
		if (id) {
			this.isEditMode.set(true);
			this.projectId.set(id);
			await this.loadProject(id);
		}
	}

	async loadProject(id: string) {
		this.loading.set(true);
		try {
			const project = await this.projectService.getProject(id);
			this.formData = {
				name: project.name,
				description: project.description || "",
				startDate: new Date(project.startDate),
				endDate: new Date(project.endDate),
				budget: project.budget || null,
				location: project.location || "",
				status: project.status,
			};
		} catch (error) {
			this.error.set(
				error instanceof Error ? error.message : "Failed to load project",
			);
		} finally {
			this.loading.set(false);
		}
	}

	async saveProject() {
		if (
			!this.formData.name ||
			!this.formData.startDate ||
			!this.formData.endDate
		) {
			this.error.set("Please fill in all required fields");
			return;
		}

		this.submitting.set(true);
		this.error.set(null);

		try {
			if (this.isEditMode() && this.projectId()) {
				const updateData: UpdateProjectRequest = {
					name: this.formData.name,
					description: this.formData.description || undefined,
					startDate: this.formData.startDate,
					endDate: this.formData.endDate,
					budget: this.formData.budget || undefined,
					location: this.formData.location || undefined,
					status: this.formData.status,
				};
				const projectId = this.projectId();
				if (projectId) {
					await this.projectService.updateProject(projectId, updateData);
				}
			} else {
				const createData: CreateProjectRequest = {
					name: this.formData.name,
					description: this.formData.description || undefined,
					startDate: this.formData.startDate,
					endDate: this.formData.endDate,
					budget: this.formData.budget || undefined,
					location: this.formData.location || undefined,
				};
				await this.projectService.createProject(createData);
			}

			this.router.navigate(["/projects"]);
		} catch (error) {
			this.error.set(
				error instanceof Error ? error.message : "An error occurred",
			);
		} finally {
			this.submitting.set(false);
		}
	}

	navigateBack() {
		this.router.navigate(["/projects"]);
	}
}
