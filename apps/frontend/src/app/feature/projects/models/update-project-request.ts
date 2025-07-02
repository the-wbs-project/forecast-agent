import type { ProjectStatus } from "./project-status";

export interface UpdateProjectRequest {
	name?: string;
	description?: string;
	startDate?: Date | string;
	endDate?: Date | string;
	budget?: number;
	location?: string;
	status?: ProjectStatus;
}
