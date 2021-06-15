import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import {Subject, Subscription} from 'rxjs';
import {debounceTime} from 'rxjs/operators';
import { UserClaimsEnum } from 'src/app/common/const';
import { composeCasesTableHeaders } from 'src/app/common/helpers';
import {
  CaseListModel,
  CaseModel,
  EformPermissionsSimpleModel, Paged,
  PageSettingsModel,
  TableHeaderElementModel,
  TemplateDto,
} from 'src/app/common/models';
import {
  CasesService,
  EFormService,
  SecurityGroupEformsPermissionsService,
} from 'src/app/common/services';
import { AuthStateService } from 'src/app/common/store';
import {WorkflowCaseModel} from 'src/app/plugins/modules/workflow-pn/models';
import {WorkordersStateService} from 'src/app/plugins/modules/workorders-pn/components/workorders/store';
import {WorkOrderModel, WorkOrdersModel} from 'src/app/plugins/modules/workorders-pn/models';
import { WorkflowCasesStateService } from '../store';
import { WorkflowPnSettingsService } from '../../../services';

@AutoUnsubscribe()
@Component({
  selector: 'app-workflow-cases-page',
  templateUrl: './workflow-cases-page.component.html',
  styleUrls: ['./workflow-cases-page.component.scss'],
})
export class WorkflowCasesPageComponent implements OnInit, OnDestroy {
  @ViewChild('deleteWorkflowCaseModal', { static: false }) deleteWorkflowCaseModal;
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
      name: 'UpdatedAt',
      elementId: 'updatedAtHeader',
      sortable: false,
      visibleName: 'Updated at',
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
    { name: 'Photo', elementId: 'photosExistsHeader', sortable: true },
    {
      name: 'Deadline',
      elementId: 'deadlineHeader',
      sortable: true
    },
    {
      name: 'ActionPlan',
      elementId: 'actionPlanHeader',
      sortable: true,
      visibleName: 'Action plan',
    },
    {
      name: 'ToBeSolvedBy',
      elementId: 'toBeSolvedByHeader',
      sortable: false,
      visibleName: 'To be solved by',
    },
    {
      name: 'Status',
      elementId: 'statusHeader',
      sortable: false,
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

  showDeleteWorkflowCaseModal(model: WorkflowCaseModel) {
    this.deleteWorkflowCaseModal.show(model);
  }

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

  ngOnDestroy(): void {}

  onPageSizeChanged(newPageSize: number) {
    this.workflowCasesStateService.updatePageSize(newPageSize);
    this.getWorkflowCases();
  }

  workflowCaseDeleted() {
    this.workflowCasesStateService.onDelete();
    this.getWorkflowCases();
  }
}
