import { RiskSeverity } from './risk-severity';

export interface TaskRiskDetail {
  id?: string;
  taskId: string;
  riskType: string;
  severity: RiskSeverity;
  probability: number;
  impact: string;
  mitigationStrategy?: string;
  weatherCondition?: string;
  threshold?: number;
}