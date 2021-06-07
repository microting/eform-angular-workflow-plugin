import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { OperationResult, ReplyRequest } from 'src/app/common/models';
import { ApiBaseService } from 'src/app/common/services';

export let WorkflowPnCasesMethods = {
  Cases: 'api/workflow-pn/cases',
};

@Injectable({
  providedIn: 'root',
})
export class WorkflowPnCasesService {
  constructor(private apiBaseService: ApiBaseService) {}

  updateCase(model: ReplyRequest): Observable<OperationResult> {
    return this.apiBaseService.post<ReplyRequest>(
      `${WorkflowPnCasesMethods.Cases}`,
      model
    );
  }
}
