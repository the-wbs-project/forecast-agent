import { KVService } from './kv.service';
import { TaskDto, PagedResult, PaginationQuery } from '../dto';

export interface TaskDataService {
  getTaskById(taskId: string): Promise<TaskDto | null>;
  getTasksByProject(projectId: string, query?: PaginationQuery): Promise<PagedResult<TaskDto>>;
  getTasksBySchedule(scheduleId: string, query?: PaginationQuery): Promise<PagedResult<TaskDto>>;
  createTask(task: TaskDto): Promise<void>;
  updateTask(taskId: string, updates: Partial<TaskDto>): Promise<void>;
  deleteTask(taskId: string): Promise<void>;
  getTasksByParent(parentTaskId: string): Promise<TaskDto[]>;
  getWeatherSensitiveTasks(projectId: string): Promise<TaskDto[]>;
}

export class KVTaskDataService implements TaskDataService {
  constructor(private kvService: KVService) {}

  private getTaskKey(taskId: string): string {
    return `task:${taskId}`;
  }

  private getProjectTaskKey(projectId: string, taskId: string): string {
    return `task:project:${projectId}:${taskId}`;
  }

  private getScheduleTaskKey(scheduleId: string, taskId: string): string {
    return `task:schedule:${scheduleId}:${taskId}`;
  }

  private getParentTaskKey(parentTaskId: string, taskId: string): string {
    return `task:parent:${parentTaskId}:${taskId}`;
  }

  async getTaskById(taskId: string): Promise<TaskDto | null> {
    return await this.kvService.get<TaskDto>(this.getTaskKey(taskId));
  }

  async getTasksByProject(projectId: string, query?: PaginationQuery): Promise<PagedResult<TaskDto>> {
    const pageNumber = query?.pageNumber || 1;
    const pageSize = query?.pageSize || 10;
    
    const taskListResult = await this.kvService.list(`task:project:${projectId}:`);
    const taskIds = taskListResult.keys.map(key => key.name.split(':').pop()!);
    
    const totalCount = taskIds.length;
    const totalPages = Math.ceil(totalCount / pageSize);
    const startIndex = (pageNumber - 1) * pageSize;
    const paginatedIds = taskIds.slice(startIndex, startIndex + pageSize);
    
    const tasks = await Promise.all(
      paginatedIds.map(id => this.getTaskById(id))
    );
    
    const validTasks = tasks.filter((t): t is TaskDto => t !== null);
    
    return {
      items: validTasks,
      totalCount,
      pageNumber,
      pageSize,
      totalPages
    };
  }

  async getTasksBySchedule(scheduleId: string, query?: PaginationQuery): Promise<PagedResult<TaskDto>> {
    const pageNumber = query?.pageNumber || 1;
    const pageSize = query?.pageSize || 10;
    
    const taskListResult = await this.kvService.list(`task:schedule:${scheduleId}:`);
    const taskIds = taskListResult.keys.map(key => key.name.split(':').pop()!);
    
    const totalCount = taskIds.length;
    const totalPages = Math.ceil(totalCount / pageSize);
    const startIndex = (pageNumber - 1) * pageSize;
    const paginatedIds = taskIds.slice(startIndex, startIndex + pageSize);
    
    const tasks = await Promise.all(
      paginatedIds.map(id => this.getTaskById(id))
    );
    
    const validTasks = tasks.filter((t): t is TaskDto => t !== null);
    
    return {
      items: validTasks,
      totalCount,
      pageNumber,
      pageSize,
      totalPages
    };
  }

  async createTask(task: TaskDto): Promise<void> {
    const taskKey = this.getTaskKey(task.id);
    const projectTaskKey = this.getProjectTaskKey(task.projectId, task.id);
    const scheduleTaskKey = this.getScheduleTaskKey(task.scheduleId, task.id);
    
    const metadata = {
      id: task.id,
      createdAt: task.createdAt,
      updatedAt: task.updatedAt,
      tags: [
        'task', 
        task.taskType, 
        task.projectId,
        task.scheduleId,
        ...(task.weatherSensitive ? ['weather_sensitive'] : []),
        ...(task.criticalPath ? ['critical_path'] : [])
      ]
    };

    const promises = [
      this.kvService.put(taskKey, task, metadata),
      this.kvService.put(projectTaskKey, task.id, metadata),
      this.kvService.put(scheduleTaskKey, task.id, metadata)
    ];

    // Create parent-child relationship index
    if (task.parentTaskId) {
      const parentTaskKey = this.getParentTaskKey(task.parentTaskId, task.id);
      promises.push(this.kvService.put(parentTaskKey, task.id, metadata));
    }

    await Promise.all(promises);
  }

  async updateTask(taskId: string, updates: Partial<TaskDto>): Promise<void> {
    const existingTask = await this.getTaskById(taskId);
    if (!existingTask) throw new Error('Task not found');

    const updatedTask = { 
      ...existingTask, 
      ...updates, 
      updatedAt: new Date().toISOString() 
    };
    
    const metadata = {
      id: taskId,
      createdAt: existingTask.createdAt,
      updatedAt: updatedTask.updatedAt,
      tags: [
        'task', 
        updatedTask.taskType, 
        updatedTask.projectId,
        updatedTask.scheduleId,
        ...(updatedTask.weatherSensitive ? ['weather_sensitive'] : []),
        ...(updatedTask.criticalPath ? ['critical_path'] : [])
      ]
    };

    await this.kvService.put(this.getTaskKey(taskId), updatedTask, metadata);

    // Handle parent task changes
    if (existingTask.parentTaskId !== updatedTask.parentTaskId) {
      // Remove old parent relationship
      if (existingTask.parentTaskId) {
        await this.kvService.delete(this.getParentTaskKey(existingTask.parentTaskId, taskId));
      }
      
      // Add new parent relationship
      if (updatedTask.parentTaskId) {
        const parentTaskKey = this.getParentTaskKey(updatedTask.parentTaskId, taskId);
        await this.kvService.put(parentTaskKey, taskId, metadata);
      }
    }
  }

  async deleteTask(taskId: string): Promise<void> {
    const task = await this.getTaskById(taskId);
    if (!task) return;

    const promises = [
      this.kvService.delete(this.getTaskKey(taskId)),
      this.kvService.delete(this.getProjectTaskKey(task.projectId, taskId)),
      this.kvService.delete(this.getScheduleTaskKey(task.scheduleId, taskId))
    ];

    if (task.parentTaskId) {
      promises.push(this.kvService.delete(this.getParentTaskKey(task.parentTaskId, taskId)));
    }

    await Promise.all(promises);
  }

  async getTasksByParent(parentTaskId: string): Promise<TaskDto[]> {
    const taskListResult = await this.kvService.list(`task:parent:${parentTaskId}:`);
    const taskIds = taskListResult.keys.map(key => key.name.split(':').pop()!);
    
    const tasks = await Promise.all(
      taskIds.map(id => this.getTaskById(id))
    );
    
    return tasks.filter((t): t is TaskDto => t !== null);
  }

  async getWeatherSensitiveTasks(projectId: string): Promise<TaskDto[]> {
    const projectTasks = await this.getTasksByProject(projectId, { pageNumber: 1, pageSize: 1000 });
    return projectTasks.items.filter(task => task.weatherSensitive);
  }
}