import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { OperationResult } from 'src/app/common/models/operation.models';

import { ReplyRequest } from 'src/app/common/models';
import { ApiBaseService } from 'src/app/common/services';

export let WorkflowPnCasesMethods = {
  Cases: 'api/workflow-pn/cases',
};

@Injectable({
  providedIn: 'root',
})
export class WorkflowPnCasesService {
  constructor(private apiBaseService: ApiBaseService) {}

  updateCase(
    model: ReplyRequest,
    templateId: number
  ): Observable<OperationResult> {
    return this.apiBaseService.post<ReplyRequest>(
      `${WorkflowPnCasesMethods.Cases}/${templateId}`,
      model
    );
  }
}
