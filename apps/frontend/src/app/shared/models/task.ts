import type { TaskDependency } from "./task-dependency";
import type { TaskPriority } from "./task-priority";
import type { TaskStatus } from "./task-status";

export interface Task {
	id?: string;
	projectId: string;
	name: string;
	description?: string;
	startDate: Date | string;
	endDate: Date | string;
	duration: number;
	progress: number;
	priority: TaskPriority;
	status: TaskStatus;
	assignedTo?: string;
	dependencies?: TaskDependency[];
	weatherSensitive: boolean;
	indoorTask: boolean;
	criticalPathTask: boolean;
	createdAt?: Date | string;
	updatedAt?: Date | string;
}
