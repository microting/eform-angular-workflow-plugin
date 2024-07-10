import {FieldValueDto} from 'src/app/common/models';

export class WorkflowCaseModel {
  id: number;
  updatedAt: Date | string;
  dateOfIncident: Date | string;
  createdBySiteName: string;
  incidentType: string;
  incidentTypeId: number;
  incidentTypeListId: string;
  incidentPlace: string;
  incidentPlaceId: number;
  incidentPlaceListId: string;
  photosExist: boolean;
  numberOfPhotos: number;
  description: string;
  deadline: Date | string;
  actionPlan: string;
  solvedBy: string;
  toBeSolvedById: number;
  statusName: string;
  status: number;
  picturesOfTask: Array<FieldValueDto>;
  picturesOfTaskDone: Array<FieldValueDto>;
  fieldIdPicturesOfTaskDone: number;
}
