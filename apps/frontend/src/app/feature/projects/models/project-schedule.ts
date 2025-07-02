export interface ProjectSchedule {
	id?: string;
	projectId: string;
	scheduleName: string;
	baselineStartDate: Date | string;
	baselineEndDate: Date | string;
	actualStartDate?: Date | string;
	actualEndDate?: Date | string;
	progress: number;
	criticalPathLength?: number;
	totalFloat?: number;
	isBaseline: boolean;
	createdAt?: Date | string;
	updatedAt?: Date | string;
}
