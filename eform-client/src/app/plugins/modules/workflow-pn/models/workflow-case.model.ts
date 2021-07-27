import {FieldValueDto} from 'src/app/common/models';

export class WorkflowCaseModel {
  id: number;
  updatedAt: Date | string;
  dateOfIncident: Date | string;
  incidentType: string;
  incidentPlace: string;
  photosExist: boolean;
  description: string;
  deadline: Date | string;
  actionPlan: string;
  toBeSolvedBy: string;
  toBeSolvedById: number;
  statusName: string;
  status: number;
  picturesOfTask: FieldValueDto;
}
