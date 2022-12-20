import {Component, OnDestroy, OnInit} from '@angular/core';
import {AutoUnsubscribe} from 'ngx-auto-unsubscribe';
import {Subject, Subscription, zip} from 'rxjs';
import {debounceTime} from 'rxjs/operators';
import {DeleteModalSettingModel, Paged, PaginationModel,} from 'src/app/common/models';
import {WorkflowCaseModel} from '../../../models';
import {WorkflowCasesStateService} from '../store';
import {saveAs} from 'file-saver';
import {WorkflowPnCasesService} from '../../../services';
import {Sort} from '@angular/material/sort';
import {MtxGridColumn} from '@ng-matero/extensions/grid';
import {TranslateService} from '@ngx-translate/core';
import {MatDialog, MatDialogRef} from '@angular/material/dialog';
import {Overlay} from '@angular/cdk/overlay';
import {dialogConfigHelper} from 'src/app/common/helpers';
import {WorkflowCaseDeleteComponent} from '../';
import {DeleteModalComponent} from 'src/app/common/modules/eform-shared/components';

@AutoUnsubscribe()
@Component({
  selector: 'app-workflow-cases-page',
  templateUrl: './workflow-cases-page.component.html',
  styleUrls: ['./workflow-cases-page.component.scss'],
})
export class WorkflowCasesPageComponent implements OnInit, OnDestroy {
  workflowCasesModel: Paged<WorkflowCaseModel> = new Paged<WorkflowCaseModel>();
  searchSubject = new Subject();
  statuses = [
    {id: 2, text: 'Vælg status'}, // No status
    {id: 0, text: 'Igangværende'}, // Ongoing
    {id: 3, text: 'Ikke igangsat'}, // Not initiated
    {id: 1, text: 'Afsluttet'}, // Closed
    {id: 4, text: 'Annulleret'}, // Canceled
  ];
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
  workflowCaseDeletedSub$: Subscription;
  deleteWorkflowCase$: Subscription;
  getAllSub$: Subscription;
  translatesSub$: Subscription;

  constructor(
    public workflowCasesStateService: WorkflowCasesStateService,
    private service: WorkflowPnCasesService,
    private translateService: TranslateService,
    private dialog: MatDialog,
    private overlay: Overlay,
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
    // const workflowCaseDeleteModal =
    //   this.dialog.open(WorkflowCaseDeleteComponent, {...dialogConfigHelper(this.overlay, model), minWidth: 500});
    // this.workflowCaseDeletedSub$ = workflowCaseDeleteModal.componentInstance.workflowCaseDeleted
    //   .subscribe(() => this.workflowCaseDeleted());
    this.translatesSub$ = zip(
      this.translateService.stream('Are you sure you want to delete'),
      this.translateService.stream('Date of incident'),
      this.translateService.stream('Created by'),
      this.translateService.stream('Incident type'),
      this.translateService.stream('Incident place'),
      this.translateService.stream('Description'),
    ).subscribe(([headerText, dateOfIncident, createdBySiteName, incidentType, incidentPlace, description]) => {
      const settings: DeleteModalSettingModel = {
        model: model,
        settings: {
          headerText: `${headerText}?`,
          fields: [
            {header: 'ID', field: 'id'},
            {header: dateOfIncident, field: 'dateOfIncident', type: 'date', format: 'dd.MM.yyyy'},
            {header: createdBySiteName, field: 'createdBySiteName'},
            {header: incidentType, field: 'incidentType'},
            {header: incidentPlace, field: 'incidentPlace'},
            {header: description, field: 'description'},
          ]
        }
      }
      const workflowCaseDeleteModal =
        this.dialog.open(DeleteModalComponent, {...dialogConfigHelper(this.overlay, settings), minWidth: 500});
      this.workflowCaseDeletedSub$ = workflowCaseDeleteModal.componentInstance.delete
        .subscribe((workflowCaseModel: WorkflowCaseModel) => this.workflowCaseDelete(workflowCaseModel, workflowCaseDeleteModal));
    });
  }

  workflowCaseDelete(workflowCaseModel: WorkflowCaseModel, workflowCaseDeleteModal: MatDialogRef<DeleteModalComponent>) {
    this.deleteWorkflowCase$ = this.service
      .deleteWorkflowCase(workflowCaseModel.id)
      .subscribe((data) => {
        if (data && data.success) {
          this.workflowCasesStateService.onDelete();
          this.getWorkflowCases();
          workflowCaseDeleteModal.close();
        }
      });

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
