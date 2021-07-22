import {SiteNameDto} from 'src/app/common/models';

export class WorkflowSettingsModel {
  folderId?: number;
  folderTasksId?: number;
  folderName: string;
  folderTasksName: string;
  assignedSites: SiteNameDto[] = [];
}
