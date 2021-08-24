import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { Subject, Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { Paged, TableHeaderElementModel } from 'src/app/common/models';
import { WorkflowCaseModel } from '../../../models';
import { WorkflowCasesStateService } from '../store';

@AutoUnsubscribe()
@Component({
  selector: 'app-workflow-cases-page',
  templateUrl: './workflow-cases-page.component.html',
  styleUrls: ['./workflow-cases-page.component.scss'],
})
export class WorkflowCasesPageComponent implements OnInit, OnDestroy {
  @ViewChild('deleteWorkflowCaseModal', { static: false })
  deleteWorkflowCaseModal;
  @ViewChild('editWorkflowCaseModal', { static: false }) editWorkflowCaseModal;
  workflowCasesModel: Paged<WorkflowCaseModel> = new Paged<WorkflowCaseModel>();

  searchSubject = new Subject();
  getAllSub$: Subscription;

  tableHeaders: TableHeaderElementModel[] = [
    { name: 'Id', elementId: 'idTableHeader', sortable: true },
    {
      name: 'DateOfIncident',
      elementId: 'dateOfIncidentHeader',
      sortable: true,
      visibleName: 'Date of incident',
    },
    {
      name: 'CreatedBySiteName',
      elementId: 'createdByHeader',
      sortable: true,
      visibleName: 'Created by',
    },
    {
      name: 'IncidentType',
      elementId: 'incidentTypeHeader',
      sortable: true,
      visibleName: 'Incident type',
    },
    {
      name: 'IncidentPlace',
      elementId: 'incidentPlaceHeader',
      sortable: true,
      visibleName: 'Incident place',
    },
    {
      name: 'NumberOfPhotos',
      elementId: 'photosExistsHeader',
      sortable: true,
      visibleName: 'Photo',
    },
    {
      name: 'Description',
      elementId: 'descriptionHeader',
      sortable: true,
    },
    {
      name: 'Deadline',
      elementId: 'deadlineHeader',
      sortable: true,
    },
    {
      name: 'ActionPlan',
      elementId: 'actionPlanHeader',
      sortable: true,
      visibleName: 'Action plan',
    },
    {
      name: 'SolvedBy',
      elementId: 'toBeSolvedByHeader',
      sortable: true,
      visibleName: 'To be solved by',
    },
    {
      name: 'Status',
      elementId: 'statusHeader',
      sortable: true,
    },
    { name: 'Actions', elementId: '', sortable: false },
  ];

  constructor(public workflowCasesStateService: WorkflowCasesStateService) {
    this.searchSubject.pipe(debounceTime(500)).subscribe((val) => {
      this.workflowCasesStateService.updateNameFilter(val.toString());
      this.getWorkflowCases();
    });
  }

  ngOnInit() {
    this.getWorkflowCases();
  }

  ngOnDestroy(): void {}

  getWorkflowCases() {
    this.getAllSub$ = this.workflowCasesStateService
      .getWorkflowCases()
      .subscribe((data) => {
        if (data && data.success) {
          this.workflowCasesModel = data.model;
        }
      });
  }

  sortTable(sort: string) {
    this.workflowCasesStateService.onSortTable(sort);
    this.getWorkflowCases();
  }

  changePage(e: any) {
    this.workflowCasesStateService.changePage(e);
    this.getWorkflowCases();
  }

  onSearchInputChanged(e: string) {
    this.searchSubject.next(e);
  }

  onPageSizeChanged(newPageSize: number) {
    this.workflowCasesStateService.updatePageSize(newPageSize);
    this.getWorkflowCases();
  }

  showDeleteWorkflowCaseModal(model: WorkflowCaseModel) {
    this.deleteWorkflowCaseModal.show(model);
  }

  workflowCaseDeleted() {
    this.workflowCasesStateService.onDelete();
    this.getWorkflowCases();
  }
}
