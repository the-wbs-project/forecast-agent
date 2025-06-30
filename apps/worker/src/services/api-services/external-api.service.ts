export interface ExternalApiService {
  get<T>(url: string, headers?: Record<string, string>): Promise<T>;
  post<T>(url: string, data: any, headers?: Record<string, string>): Promise<T>;
  put<T>(url: string, data: any, headers?: Record<string, string>): Promise<T>;
  delete<T>(url: string, headers?: Record<string, string>): Promise<T>;
}

export class HttpService implements ExternalApiService {
  constructor(private baseUrl?: string, private defaultHeaders?: Record<string, string>) {}

  async get<T>(url: string, headers?: Record<string, string>): Promise<T> {
    return this.request<T>('GET', url, undefined, headers);
  }

  async post<T>(url: string, data: any, headers?: Record<string, string>): Promise<T> {
    return this.request<T>('POST', url, data, headers);
  }

  async put<T>(url: string, data: any, headers?: Record<string, string>): Promise<T> {
    return this.request<T>('PUT', url, data, headers);
  }

  async delete<T>(url: string, headers?: Record<string, string>): Promise<T> {
    return this.request<T>('DELETE', url, undefined, headers);
  }

  private async request<T>(
    method: string, 
    url: string, 
    data?: any, 
    headers?: Record<string, string>
  ): Promise<T> {
    const fullUrl = this.baseUrl ? `${this.baseUrl}${url}` : url;
    
    const requestHeaders = {
      'Content-Type': 'application/json',
      ...this.defaultHeaders,
      ...headers
    };

    const requestInit: RequestInit = {
      method,
      headers: requestHeaders
    };

    if (data && (method === 'POST' || method === 'PUT' || method === 'PATCH')) {
      requestInit.body = JSON.stringify(data);
    }

    const response = await fetch(fullUrl, requestInit);

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText}`);
    }

    // Handle empty responses
    const contentType = response.headers.get('content-type');
    if (!contentType || !contentType.includes('application/json')) {
      return {} as T;
    }

    return await response.json();
  }
}

// Service for calling the main WeatherGuard API
export class WeatherGuardApiService extends HttpService {
  constructor(apiBaseUrl: string, apiToken?: string) {
    const headers: Record<string, string> = {};
    if (apiToken) {
      headers['Authorization'] = `Bearer ${apiToken}`;
    }
    
    super(apiBaseUrl, headers);
  }

  // Project operations
  async getProjects(params?: Record<string, any>) {
    const queryParams = params ? '?' + new URLSearchParams(params).toString() : '';
    return this.get(`/api/projects${queryParams}`);
  }

  async getProject(projectId: string) {
    return this.get(`/api/projects/${projectId}`);
  }

  async createProject(projectData: any) {
    return this.post('/api/projects', projectData);
  }

  async updateProject(projectId: string, updates: any) {
    return this.put(`/api/projects/${projectId}`, updates);
  }

  async deleteProject(projectId: string) {
    return this.delete(`/api/projects/${projectId}`);
  }

  // Task operations
  async getTasks(projectId: string, params?: Record<string, any>) {
    const queryParams = params ? '?' + new URLSearchParams(params).toString() : '';
    return this.get(`/api/tasks/project/${projectId}${queryParams}`);
  }

  async getTask(taskId: string) {
    return this.get(`/api/tasks/${taskId}`);
  }

  async createTask(taskData: any) {
    return this.post('/api/tasks', taskData);
  }

  async updateTask(taskId: string, updates: any) {
    return this.put(`/api/tasks/${taskId}`, updates);
  }

  async deleteTask(taskId: string) {
    return this.delete(`/api/tasks/${taskId}`);
  }

  // Weather operations
  async getWeatherAnalyses(projectId: string, params?: Record<string, any>) {
    const queryParams = params ? '?' + new URLSearchParams(params).toString() : '';
    return this.get(`/api/weatherriskanalysis/project/${projectId}${queryParams}`);
  }

  async generateWeatherAnalysis(projectId: string, analysisData: any) {
    return this.post(`/api/weatherriskanalysis/project/${projectId}/generate`, analysisData);
  }

  async getWeatherForecast(latitude: number, longitude: number, days: number = 7) {
    return this.get(`/api/weatherriskanalysis/forecast?latitude=${latitude}&longitude=${longitude}&days=${days}`);
  }
}