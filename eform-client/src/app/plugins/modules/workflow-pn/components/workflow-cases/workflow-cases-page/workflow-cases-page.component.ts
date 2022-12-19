import {Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {AutoUnsubscribe} from 'ngx-auto-unsubscribe';
import {Subject, Subscription} from 'rxjs';
import {debounceTime} from 'rxjs/operators';
import {Paged, PaginationModel,} from 'src/app/common/models';
import {WorkflowCaseModel} from '../../../models';
import {WorkflowCasesStateService} from '../store';
import {saveAs} from 'file-saver';
import {WorkflowPnCasesService} from '../../../services';
import {Sort} from '@angular/material/sort';
import {MtxGridColumn} from '@ng-matero/extensions/grid';
import {TranslateService} from '@ngx-translate/core';

@AutoUnsubscribe()
@Component({
  selector: 'app-workflow-cases-page',
  templateUrl: './workflow-cases-page.component.html',
  styleUrls: ['./workflow-cases-page.component.scss'],
})
export class WorkflowCasesPageComponent implements OnInit, OnDestroy {
  @ViewChild('deleteWorkflowCaseModal', {static: false}) deleteWorkflowCaseModal;
  workflowCasesModel: Paged<WorkflowCaseModel> = new Paged<WorkflowCaseModel>();

  searchSubject = new Subject();
  getAllSub$: Subscription;

  tableHeaders: MtxGridColumn[] = [
    {header: this.translateService.stream('Id'), field: 'id', sortProp: {id: 'Id'}, sortable: true},
    {
      header: this.translateService.stream('Date of incident'),
      field: 'dateOfIncident',
      sortProp: {id: 'DateOfIncident'},
      sortable: true,
      type: 'date',
      typeParameter: {format: 'dd.MM.yyyy'}
    },
    {header: this.translateService.stream('Created by'), field: 'createdBySiteName', sortProp: {id: 'CreatedBySiteName'}, sortable: true},
    {header: this.translateService.stream('Incident type'), field: 'incidentType', sortProp: {id: 'IncidentType'}, sortable: true},
    {header: this.translateService.stream('Incident place'), field: 'incidentPlace', sortProp: {id: 'IncidentPlace'}, sortable: true},
    {
      header: this.translateService.stream('Photo'),
      field: 'numberOfPhotos',
      sortProp: {id: 'NumberOfPhotos'},
      sortable: true,
      formatter: (rowData: WorkflowCaseModel) => `(${rowData.numberOfPhotos})`
    },
    {header: this.translateService.stream('Description'), field: 'description', sortProp: {id: 'Description'}, sortable: true},
    {
      header: this.translateService.stream('Deadline'),
      field: 'deadline',
      sortProp: {id: 'Deadline'},
      sortable: true,
      type: 'date',
      typeParameter: {format: 'dd.MM.y'}
    },
    {header: this.translateService.stream('Action plan'), field: 'actionPlan', sortProp: {id: 'ActionPlan'}, sortable: true},
    {header: this.translateService.stream('To be solved by'), field: 'solvedBy', sortProp: {id: 'SolvedBy'}, sortable: true},
    {
      header: this.translateService.stream('Status'),
      field: 'status',
      sortProp: {id: 'Status'},
      sortable: true,
      formatter: (rowData: WorkflowCaseModel) => this.getStatusText(rowData.status)
    },
    {header: this.translateService.stream('Actions'), field: 'actions'},
  ];

  statuses = [
    {id: 2, text: 'Vælg status'}, // No status
    {id: 0, text: 'Igangværende'}, // Ongoing
    {id: 3, text: 'Ikke igangsat'}, // Not initiated
    {id: 1, text: 'Afsluttet'}, // Closed
    {id: 4, text: 'Annulleret'}, // Canceled
  ];

  constructor(
    public workflowCasesStateService: WorkflowCasesStateService,
    private service: WorkflowPnCasesService,
    private translateService: TranslateService,
  ) {
    this.searchSubject.pipe(debounceTime(500)).subscribe((val) => {
      this.workflowCasesStateService.updateNameFilter(val.toString());
      this.getWorkflowCases();
    });
  }

  ngOnInit() {
    this.getWorkflowCases();
  }

  ngOnDestroy(): void {
  }


  getStatusText(id: number) {
    if (id === null) {
      return '';
    }
    if (this.statuses.length > 0) {
      return this.statuses.find(x => x.id === id).text;
    } else {
      return '';
    }
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

  sortTable(sort: Sort) {
    this.workflowCasesStateService.onSortTable(sort.active);
    this.getWorkflowCases();
  }

  changePage(e: any) {
    this.workflowCasesStateService.changePage(e);
    this.getWorkflowCases();
  }

  onSearchInputChanged(e: string) {
    this.searchSubject.next(e);
  }

  showDeleteWorkflowCaseModal(model: WorkflowCaseModel) {
    this.deleteWorkflowCaseModal.show(model);
  }

  workflowCaseDeleted() {
    this.workflowCasesStateService.onDelete();
    this.getWorkflowCases();
  }


  downloadFile(caseId: number, fileType: string) {
    this.service
      .downloadEformPDF(caseId, fileType)
      .subscribe((data) => {
        const blob = new Blob([data]);
        saveAs(blob, `sag_nr_${caseId}.${fileType}`);
      });
  }

  onPaginationChanged(paginationModel: PaginationModel) {
    this.workflowCasesStateService.updatePagination(paginationModel);
    this.getWorkflowCases();
  }
}
