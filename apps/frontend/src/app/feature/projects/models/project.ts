import { ProjectStatus } from './project-status';

export interface Project {
  id?: string;
  name: string;
  description?: string;
  startDate: Date | string;
  endDate: Date | string;
  budget?: number;
  location?: string;
  status: ProjectStatus;
  firmId?: string;
  createdBy?: string;
  createdAt?: Date | string;
  updatedAt?: Date | string;
}