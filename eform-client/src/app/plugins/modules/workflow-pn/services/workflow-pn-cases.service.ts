import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  OperationDataResult,
  OperationResult,
  Paged,
} from 'src/app/common/models';
import { ApiBaseService } from 'src/app/common/services';
import { WorkflowCaseModel, WorkflowCaseUpdateModel } from '../models';

export let WorkflowPnCasesMethods = {
  Cases: 'api/workflow-pn/cases',
};

@Injectable({
  providedIn: 'root',
})
export class WorkflowPnCasesService {
  constructor(private apiBaseService: ApiBaseService) {}

  getWorkflowCases(
    model: any
  ): Observable<OperationDataResult<Paged<WorkflowCaseModel>>> {
    return this.apiBaseService.post(WorkflowPnCasesMethods.Cases, model);
  }

  updateCase(model: WorkflowCaseUpdateModel): Observable<OperationResult> {
    return this.apiBaseService.put(WorkflowPnCasesMethods.Cases, model);
  }

  deleteWorkflowCase(id: number): Observable<OperationResult> {
    return this.apiBaseService.delete(
      WorkflowPnCasesMethods.Cases + '?id=' + id
    );
  }
}
