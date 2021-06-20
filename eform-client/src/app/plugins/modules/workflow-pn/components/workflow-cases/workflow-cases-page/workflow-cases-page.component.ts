import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { Subject, Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import {
  Paged,
  SiteNameDto,
  TableHeaderElementModel,
} from 'src/app/common/models';
import { SitesService } from 'src/app/common/services';
import {
  WorkflowCaseModel,
  WorkflowCaseUpdateModel,
} from 'src/app/plugins/modules/workflow-pn/models';
import { WorkflowPnCasesService } from 'src/app/plugins/modules/workflow-pn/services';
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
  deviceUsersList: SiteNameDto[] = [];

  searchSubject = new Subject();
  getAllSub$: Subscription;
  getAllSitesSub$: Subscription;
  updateSub$: Subscription;

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
      sortable: true,
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

  constructor(
    public workflowCasesStateService: WorkflowCasesStateService,
    private workflowCasesService: WorkflowPnCasesService,
    private sitesService: SitesService
  ) {
    this.searchSubject.pipe(debounceTime(500)).subscribe((val) => {
      this.workflowCasesStateService.updateNameFilter(val.toString());
      this.getWorkflowCases();
    });
  }

  ngOnInit() {
    this.getWorkflowCases();
    this.getSites();
  }

  showDeleteWorkflowCaseModal(model: WorkflowCaseModel) {
    this.deleteWorkflowCaseModal.show(model);
  }

  getSites() {
    this.getAllSitesSub$ = this.sitesService.getAllSites().subscribe((data) => {
      if (data && data.success) {
        this.deviceUsersList = data.model;
      }
    });
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

  updateWorkflowCase(model: WorkflowCaseUpdateModel) {
    this.updateSub$ = this.workflowCasesService
      .updateCase(model)
      .subscribe((data) => {
        if (data && data.success) {
          this.editWorkflowCaseModal.hide();
          this.getWorkflowCases();
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

  workflowCaseUpdate(model: WorkflowCaseUpdateModel) {
    this.workflowCasesStateService.onDelete();
    this.getWorkflowCases();
  }

  showEditWorkflowCaseModal(model: WorkflowCaseModel) {
    this.deleteWorkflowCaseModal.show(model);
  }
}
