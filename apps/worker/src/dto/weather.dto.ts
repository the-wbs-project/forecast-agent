export interface WeatherForecastDto {
  date: string;
  temperature: {
    min: number;
    max: number;
    average: number;
  };
  humidity: number;
  precipitation: {
    probability: number;
    amount: number;
  };
  windSpeed: number;
  windDirection: number;
  condition: string;
  visibility: number;
  uvIndex: number;
  pressure: number;
}

export interface WeatherRiskAnalysisDto {
  id: string;
  projectId: string;
  taskId?: string;
  analysisDate: string;
  latitude: number;
  longitude: number;
  startDate: string;
  endDate: string;
  riskLevel: string;
  riskScore: number;
  weatherCondition: string;
  impactDescription: string;
  recommendedActions: string[];
  alternativeSchedule?: string;
  costImpact?: number;
  delayRisk: number;
  createdBy: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateWeatherRiskAnalysisRequest {
  projectId: string;
  taskId?: string;
  latitude: number;
  longitude: number;
  startDate: string;
  endDate: string;
  weatherCondition?: string;
}

export interface GenerateAnalysisRequest {
  startDate: string;
  endDate: string;
  includeTaskAnalysis?: boolean;
  weatherSensitiveOnly?: boolean;
}

export interface WeatherRiskSummaryDto {
  totalAnalyses: number;
  highRiskCount: number;
  mediumRiskCount: number;
  lowRiskCount: number;
  averageRiskScore: number;
  totalDelayRisk: number;
  totalCostImpact: number;
}