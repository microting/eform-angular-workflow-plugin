export class WorkflowCaseModel {
  id: number;
  updatedAt: Date | string;
  dateOfIncident: Date | string;
  incidentType: string;
  incidentPlace: string;
  photosExists: boolean;
  description: string;
  deadline: Date | string;
  actionPlan: string;
  toBeSolvedBy: string;
  status: string;
}
