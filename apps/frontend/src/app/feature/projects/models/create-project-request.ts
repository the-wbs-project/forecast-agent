export interface CreateProjectRequest {
	name: string;
	description?: string;
	startDate: Date | string;
	endDate: Date | string;
	budget?: number;
	location?: string;
	firmId?: string;
}
