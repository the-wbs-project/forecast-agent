export interface ProjectDto {
	id: string;
	name: string;
	description?: string;
	location?: string;
	latitude?: number;
	longitude?: number;
	startDate?: string;
	endDate?: string;
	status: string;
	organizationId: string;
	organizationName?: string;
	createdBy: string;
	creatorName?: string;
	createdAt: string;
	updatedAt: string;

	// BIM fields
	bimEnabled: boolean;
	coordinateSystemId?: string;
	northDirection?: number;
	buildingHeight?: number;
	grossFloorArea?: number;
	numberOfStoreys?: number;

	// Computed fields
	durationInDays?: number;
	scheduleCount?: number;
	taskCount?: number;
	analysisCount?: number;
}

export interface CreateProjectRequest {
	name: string;
	description?: string;
	location?: string;
	latitude?: number;
	longitude?: number;
	startDate?: string;
	endDate?: string;
	organizationId: string;
	bimEnabled?: boolean;
	coordinateSystemId?: string;
	northDirection?: number;
	buildingHeight?: number;
	grossFloorArea?: number;
	numberOfStoreys?: number;
}

export interface UpdateProjectRequest {
	name?: string;
	description?: string;
	location?: string;
	latitude?: number;
	longitude?: number;
	startDate?: string;
	endDate?: string;
	status?: string;
	bimEnabled?: boolean;
	coordinateSystemId?: string;
	northDirection?: number;
	buildingHeight?: number;
	grossFloorArea?: number;
	numberOfStoreys?: number;
}

export interface ProjectStatsDto {
	totalTasks: number;
	completedTasks: number;
	pendingTasks: number;
	overdueTasks: number;
	totalSchedules: number;
	totalAnalyses: number;
	completionPercentage: number;
	estimatedCompletionDate?: string;
}
