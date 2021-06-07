import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  OperationDataResult,
  OperationResult,
  TemplateDto,
} from 'src/app/common/models';
import { WorkflowBaseSettingsModel } from '../models';
import { ApiBaseService } from 'src/app/common/services';

export let WorkflowPnSettingsMethods = {
  WorkflowSettings: 'api/workflow-pn/settings',
  WorkflowSettingsTemplate: 'api/workflow-pn/settings/template',
};

@Injectable()
export class WorkflowPnSettingsService {
  constructor(private apiBaseService: ApiBaseService) {}

  getAllSettings(): Observable<OperationDataResult<WorkflowBaseSettingsModel>> {
    return this.apiBaseService.get(WorkflowPnSettingsMethods.WorkflowSettings);
  }

  getSelectedTemplate(): Observable<OperationDataResult<TemplateDto>> {
    return this.apiBaseService.get(
      WorkflowPnSettingsMethods.WorkflowSettingsTemplate
    );
  }

  updateSettings(
    model: WorkflowBaseSettingsModel
  ): Observable<OperationResult> {
    return this.apiBaseService.post(
      WorkflowPnSettingsMethods.WorkflowSettings,
      model
    );
  }
}
