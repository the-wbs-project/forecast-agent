import { KVService } from './kv.service';
import { ProjectDto, PagedResult, PaginationQuery } from '../dto';

export interface ProjectDataService {
  getProjectById(projectId: string): Promise<ProjectDto | null>;
  getProjectsByOrganization(organizationId: string, query?: PaginationQuery): Promise<PagedResult<ProjectDto>>;
  createProject(project: ProjectDto): Promise<void>;
  updateProject(projectId: string, updates: Partial<ProjectDto>): Promise<void>;
  deleteProject(projectId: string): Promise<void>;
  searchProjects(query: string, organizationId?: string, pagination?: PaginationQuery): Promise<PagedResult<ProjectDto>>;
}

export class KVProjectDataService implements ProjectDataService {
  constructor(private kvService: KVService) {}

  private getProjectKey(projectId: string): string {
    return `project:${projectId}`;
  }

  private getOrganizationProjectsKey(organizationId: string): string {
    return `org_projects:${organizationId}`;
  }

  async getProjectById(projectId: string): Promise<ProjectDto | null> {
    return await this.kvService.get<ProjectDto>(this.getProjectKey(projectId));
  }

  async getProjectsByOrganization(organizationId: string, query?: PaginationQuery): Promise<PagedResult<ProjectDto>> {
    const pageNumber = query?.pageNumber || 1;
    const pageSize = query?.pageSize || 10;
    
    // Get list of project IDs for this organization
    const projectListResult = await this.kvService.list(`project:org:${organizationId}:`);
    const projectIds = projectListResult.keys.map(key => key.name.split(':').pop()!);
    
    // Calculate pagination
    const totalCount = projectIds.length;
    const totalPages = Math.ceil(totalCount / pageSize);
    const startIndex = (pageNumber - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedIds = projectIds.slice(startIndex, endIndex);
    
    // Fetch project details
    const projects = await Promise.all(
      paginatedIds.map(id => this.getProjectById(id))
    );
    
    const validProjects = projects.filter((p): p is ProjectDto => p !== null);
    
    return {
      items: validProjects,
      totalCount,
      pageNumber,
      pageSize,
      totalPages
    };
  }

  async createProject(project: ProjectDto): Promise<void> {
    const projectKey = this.getProjectKey(project.id);
    const orgProjectKey = `project:org:${project.organizationId}:${project.id}`;
    
    const metadata = {
      id: project.id,
      createdAt: project.createdAt,
      updatedAt: project.updatedAt,
      createdBy: project.createdBy,
      tags: ['project', project.status, project.organizationId]
    };

    await Promise.all([
      this.kvService.put(projectKey, project, metadata),
      this.kvService.put(orgProjectKey, project.id, metadata) // Index for organization
    ]);
  }

  async updateProject(projectId: string, updates: Partial<ProjectDto>): Promise<void> {
    const existingProject = await this.getProjectById(projectId);
    if (!existingProject) throw new Error('Project not found');

    const updatedProject = { 
      ...existingProject, 
      ...updates, 
      updatedAt: new Date().toISOString() 
    };
    
    const metadata = {
      id: projectId,
      createdAt: existingProject.createdAt,
      updatedAt: updatedProject.updatedAt,
      createdBy: existingProject.createdBy,
      tags: ['project', updatedProject.status, updatedProject.organizationId]
    };

    await this.kvService.put(this.getProjectKey(projectId), updatedProject, metadata);
  }

  async deleteProject(projectId: string): Promise<void> {
    const project = await this.getProjectById(projectId);
    if (!project) return;

    const orgProjectKey = `project:org:${project.organizationId}:${projectId}`;
    
    await Promise.all([
      this.kvService.delete(this.getProjectKey(projectId)),
      this.kvService.delete(orgProjectKey)
    ]);
  }

  async searchProjects(query: string, organizationId?: string, pagination?: PaginationQuery): Promise<PagedResult<ProjectDto>> {
    const pageNumber = pagination?.pageNumber || 1;
    const pageSize = pagination?.pageSize || 10;
    
    // Get all projects for organization or all projects
    const prefix = organizationId ? `project:org:${organizationId}:` : 'project:';
    const projectListResult = await this.kvService.list(prefix);
    
    // Get project details and filter by search query
    const projectPromises = projectListResult.keys.map(async (key) => {
      const projectId = organizationId ? key.name.split(':').pop()! : key.name.replace('project:', '');
      return await this.getProjectById(projectId);
    });
    
    const allProjects = (await Promise.all(projectPromises)).filter((p): p is ProjectDto => p !== null);
    
    // Filter by search query
    const queryLower = query.toLowerCase();
    const filteredProjects = allProjects.filter(project => 
      project.name.toLowerCase().includes(queryLower) ||
      (project.description && project.description.toLowerCase().includes(queryLower)) ||
      (project.location && project.location.toLowerCase().includes(queryLower))
    );
    
    // Apply pagination
    const totalCount = filteredProjects.length;
    const totalPages = Math.ceil(totalCount / pageSize);
    const startIndex = (pageNumber - 1) * pageSize;
    const paginatedProjects = filteredProjects.slice(startIndex, startIndex + pageSize);
    
    return {
      items: paginatedProjects,
      totalCount,
      pageNumber,
      pageSize,
      totalPages
    };
  }
}