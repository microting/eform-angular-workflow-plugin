import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  OperationDataResult,
  OperationResult,
  TemplateDto,
} from 'src/app/common/models';
import { WorkflowSettingsModel } from '../models';
import { ApiBaseService } from 'src/app/common/services';

export let WorkflowPnSettingsMethods = {
  WorkflowSettings: 'api/workflow-pn/settings',
  SettingsSites: 'api/workflow-pn/settings/sites',
  SettingsFolder: 'api/workflow-pn/settings/folder',
  SettingsTasksFolder: 'api/workflow-pn/settings/tasksfolder'
};

@Injectable()
export class WorkflowPnSettingsService {
  constructor(private apiBaseService: ApiBaseService) {}

  getAllSettings(): Observable<OperationDataResult<WorkflowSettingsModel>> {
    return this.apiBaseService.get(WorkflowPnSettingsMethods.WorkflowSettings);
  }
  addSiteToSettings(siteId: number): Observable<OperationResult> {
    return this.apiBaseService.post(WorkflowPnSettingsMethods.SettingsSites, siteId);
  }

  removeSiteFromSettings(id: number): Observable<OperationResult> {
    return this.apiBaseService.delete(WorkflowPnSettingsMethods.SettingsSites + '/' + id);
  }

  updateSettingsFolder(folderId: number): Observable<OperationResult> {
    return this.apiBaseService.post(WorkflowPnSettingsMethods.SettingsFolder, folderId);
  }

  updateSettingsTasksFolder(folderId: number): Observable<OperationResult> {
    return this.apiBaseService.post(WorkflowPnSettingsMethods.SettingsTasksFolder, folderId);
  }
}
