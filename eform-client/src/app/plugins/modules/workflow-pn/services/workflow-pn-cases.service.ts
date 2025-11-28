import { Injectable, inject } from '@angular/core';
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
  Places: 'api/workflow-pn/cases/places',
  DownloadPDF: 'api/workflow-pn/cases/download-case-pdf',
  DownloadExcel: 'api/workflow-pn/cases/download-cases-as-xlsx',
};

@Injectable({
  providedIn: 'root',
})
export class WorkflowPnCasesService {
  private apiBaseService = inject(ApiBaseService);

  getWorkflowCases(
    model: any
  ): Observable<OperationDataResult<Paged<WorkflowCaseModel>>> {
    return this.apiBaseService.post(WorkflowPnCasesMethods.Cases, model);
  }

  updateCase(model: WorkflowCaseModel): Observable<OperationResult> {
    return this.apiBaseService.put(WorkflowPnCasesMethods.Cases, model);
  }

  deleteWorkflowCase(id: number): Observable<OperationResult> {
    return this.apiBaseService.delete(
      WorkflowPnCasesMethods.Cases + '?id=' + id
    );
  }

  readCase(id: number): Observable<OperationDataResult<WorkflowCaseModel>> {
    return this.apiBaseService.get(WorkflowPnCasesMethods.Cases, { id: id });
  }

  getPlaces(): Observable<OperationDataResult<{ id: number; name: string }[]>> {
    return this.apiBaseService.get(WorkflowPnCasesMethods.Places);
  }

  downloadEformPDF(
    caseId: number,
    fileType: string
  ): Observable<any> {
    return this.apiBaseService.getBlobData(
      WorkflowPnCasesMethods.DownloadPDF +
      '/?id=' +
      caseId +
      '&fileType=' +
      fileType
    );
  }

  downloadExcelFile(): Observable<any> {
    return this.apiBaseService.getBlobData(
      WorkflowPnCasesMethods.DownloadExcel
    );
  }
}
