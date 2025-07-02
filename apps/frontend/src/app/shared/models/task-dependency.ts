import { DependencyType } from './dependency-type';

export interface TaskDependency {
  id?: string;
  taskId: string;
  dependentTaskId: string;
  dependencyType: DependencyType;
  lagTime: number;
}