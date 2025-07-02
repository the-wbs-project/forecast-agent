import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Project, CreateProjectRequest, UpdateProjectRequest, ProjectSchedule } from '../models';
import { ApiResponse, PagedResult } from '../../../shared/models';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/projects`;

  projects = signal<Project[]>([]);
  currentProject = signal<Project | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  async getProjects(pageNumber = 1, pageSize = 10): Promise<PagedResult<Project>> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const params = new HttpParams()
        .set('pageNumber', pageNumber.toString())
        .set('pageSize', pageSize.toString());

      const response = await firstValueFrom(this.http.get<ApiResponse<PagedResult<Project>>>(
        this.apiUrl,
        { params }
      ));

      console.log('response', response);

      if (response?.success && response.data) {
        this.projects.set(response.data.items);
        return response.data;
      }

      throw new Error(response?.message || 'Failed to fetch projects');
    } catch (error) {
      this.error.set(error instanceof Error ? error.message : 'An error occurred');
      throw error;
    } finally {
      this.loading.set(false);
    }
  }

  async getProject(id: string): Promise<Project> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const response = await this.http.get<ApiResponse<Project>>(
        `${this.apiUrl}/${id}`
      ).toPromise();

      if (response?.success && response.data) {
        this.currentProject.set(response.data);
        return response.data;
      }

      throw new Error(response?.message || 'Failed to fetch project');
    } catch (error) {
      this.error.set(error instanceof Error ? error.message : 'An error occurred');
      throw error;
    } finally {
      this.loading.set(false);
    }
  }

  async createProject(project: CreateProjectRequest): Promise<Project> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const response = await this.http.post<ApiResponse<Project>>(
        this.apiUrl,
        project
      ).toPromise();

      if (response?.success && response.data) {
        const currentProjects = this.projects();
        this.projects.set([...currentProjects, response.data]);
        return response.data;
      }

      throw new Error(response?.message || 'Failed to create project');
    } catch (error) {
      this.error.set(error instanceof Error ? error.message : 'An error occurred');
      throw error;
    } finally {
      this.loading.set(false);
    }
  }

  async updateProject(id: string, project: UpdateProjectRequest): Promise<Project> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const response = await this.http.put<ApiResponse<Project>>(
        `${this.apiUrl}/${id}`,
        project
      ).toPromise();

      if (response?.success && response.data) {
        const currentProjects = this.projects();
        const index = currentProjects.findIndex(p => p.id === id);
        if (index !== -1) {
          currentProjects[index] = response.data;
          this.projects.set([...currentProjects]);
        }

        if (this.currentProject()?.id === id) {
          this.currentProject.set(response.data);
        }

        return response.data;
      }

      throw new Error(response?.message || 'Failed to update project');
    } catch (error) {
      this.error.set(error instanceof Error ? error.message : 'An error occurred');
      throw error;
    } finally {
      this.loading.set(false);
    }
  }

  async deleteProject(id: string): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const response = await this.http.delete<ApiResponse<void>>(
        `${this.apiUrl}/${id}`
      ).toPromise();

      if (response?.success) {
        const currentProjects = this.projects();
        this.projects.set(currentProjects.filter(p => p.id !== id));

        if (this.currentProject()?.id === id) {
          this.currentProject.set(null);
        }

        return;
      }

      throw new Error(response?.message || 'Failed to delete project');
    } catch (error) {
      this.error.set(error instanceof Error ? error.message : 'An error occurred');
      throw error;
    } finally {
      this.loading.set(false);
    }
  }

  async importProject(file: File, projectName: string, description?: string): Promise<Project> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const formData = new FormData();
      formData.append('file', file);
      formData.append('projectName', projectName);
      if (description) {
        formData.append('description', description);
      }

      const response = await this.http.post<ApiResponse<Project>>(
        `${this.apiUrl}/import`,
        formData
      ).toPromise();

      if (response?.success && response.data) {
        const currentProjects = this.projects();
        this.projects.set([...currentProjects, response.data]);
        return response.data;
      }

      throw new Error(response?.message || 'Failed to import project');
    } catch (error) {
      this.error.set(error instanceof Error ? error.message : 'An error occurred');
      throw error;
    } finally {
      this.loading.set(false);
    }
  }

  async getProjectSchedules(projectId: string): Promise<ProjectSchedule[]> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const response = await this.http.get<ApiResponse<ProjectSchedule[]>>(
        `${this.apiUrl}/${projectId}/schedules`
      ).toPromise();

      if (response?.success && response.data) {
        return response.data;
      }

      throw new Error(response?.message || 'Failed to fetch project schedules');
    } catch (error) {
      this.error.set(error instanceof Error ? error.message : 'An error occurred');
      throw error;
    } finally {
      this.loading.set(false);
    }
  }
}