export interface TaskDto {
	id: string;
	projectId: string;
	scheduleId: string;
	externalId?: string;
	name: string;
	description?: string;
	startDate?: string;
	endDate?: string;
	duration?: number;
	predecessorIds?: string;
	weatherSensitive: boolean;
	weatherCategories?: string[];

	// Hierarchy fields
	parentTaskId?: string;
	wbsCode?: string;
	taskLevel: number;
	sortOrder?: number;
	taskType: string;
	outlineNumber?: string;
	isSummaryTask: boolean;
	isMilestone: boolean;

	// Project management fields
	percentComplete: number;
	actualStartDate?: string;
	actualEndDate?: string;
	baselineStartDate?: string;
	baselineEndDate?: string;
	baselineDuration?: number;
	criticalPath: boolean;
	totalFloat: number;
	freeFloat: number;

	// Cost tracking
	plannedCost?: number;
	actualCost?: number;
	remainingCost?: number;

	// BIM integration
	bimElementIds?: string[];
	locationElementId?: string;

	// Computed properties
	durationInDays?: number;
	isOverdue: boolean;
	isInProgress: boolean;

	createdAt: string;
	updatedAt: string;
}

export interface CreateTaskRequest {
	projectId: string;
	scheduleId: string;
	externalId?: string;
	name: string;
	description?: string;
	startDate?: string;
	endDate?: string;
	duration?: number;
	predecessorIds?: string;
	weatherSensitive?: boolean;
	weatherCategories?: string[];
	parentTaskId?: string;
	wbsCode?: string;
	taskLevel?: number;
	sortOrder?: number;
	taskType?: string;
	outlineNumber?: string;
	isSummaryTask?: boolean;
	isMilestone?: boolean;
	plannedCost?: number;
	bimElementIds?: string[];
	locationElementId?: string;
}

export interface UpdateTaskRequest {
	name?: string;
	description?: string;
	startDate?: string;
	endDate?: string;
	duration?: number;
	predecessorIds?: string;
	weatherSensitive?: boolean;
	weatherCategories?: string[];
	percentComplete?: number;
	actualStartDate?: string;
	actualEndDate?: string;
	plannedCost?: number;
	actualCost?: number;
	remainingCost?: number;
	bimElementIds?: string[];
	locationElementId?: string;
}
