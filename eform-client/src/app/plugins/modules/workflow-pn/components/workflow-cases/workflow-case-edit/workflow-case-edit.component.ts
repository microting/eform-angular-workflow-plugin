import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import {
  CaseEditRequest,
  SiteNameDto,
  TemplateDto,
} from 'src/app/common/models';
import { WorkflowCaseModel } from '../../../models';
import { Subscription } from 'rxjs';
import { UserClaimsEnum } from 'src/app/common/const';
import { ActivatedRoute, Router } from '@angular/router';
import {
  AuthService,
  SecurityGroupEformsPermissionsService,
  SitesService,
} from 'src/app/common/services';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { WorkflowPnCasesService } from '../../../services';
import { DateTimeAdapter } from '@danielmoncada/angular-datetime-picker';
import { AuthStateService } from 'src/app/common/store';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Location } from '@angular/common';
import { format } from 'date-fns';

@AutoUnsubscribe()
@Component({
  selector: 'app-workflow-case-edit',
  templateUrl: './workflow-case-edit.component.html',
  styleUrls: ['./workflow-case-edit.component.scss'],
})
export class WorkflowCaseEditComponent implements OnInit, OnDestroy {
  deviceUsersList: SiteNameDto[] = [];
  @ViewChild('frame', { static: true }) frame;
  workflowCaseModel: WorkflowCaseModel = new WorkflowCaseModel();

  @ViewChild('caseConfirmation', { static: true }) caseConfirmation;
  id: number;
  templateId: number;
  currentTemplate: TemplateDto = new TemplateDto();

  requestModels: Array<CaseEditRequest> = [];
  places: { id: number; name: string }[] = new Array<{
    id: number;
    name: string;
  }>();
  incidentTypes: { id: number; name: string }[] = new Array<{
    id: number;
    name: string;
  }>();

  isSaveClicked = false;
  reverseRoute: string;
  isNoSaveExitAllowed = false;

  activatedRouteSub$: Subscription;
  updateSub$: Subscription;
  getAllSitesSub$: Subscription;

  dataForm: FormGroup;

  statuses = [
    { id: 0, text: 'Ongoing' },
    { id: 1, text: 'Closed' },
    { id: 2, text: 'No status' },
    { id: 3, text: 'Not initiated' },
    { id: 4, text: 'Canceled' },
  ];

  constructor(
    private activateRoute: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private securityGroupEformsService: SecurityGroupEformsPermissionsService,
    dateTimeAdapter: DateTimeAdapter<any>,
    private workflowPnCasesService: WorkflowPnCasesService,
    authStateService: AuthStateService,
    private formBuilder: FormBuilder,
    private location: Location,
    private sitesService: SitesService
  ) {
    dateTimeAdapter.setLocale(authStateService.currentUserLocale);
    this.activatedRouteSub$ = this.activateRoute.params.subscribe((params) => {
      this.id = +params['id'];
    });
    this.activateRoute.queryParams.subscribe((params) => {
      this.reverseRoute = params['reverseRoute'];
    });
  }

  goBack() {
    this.location.back();
  }

  updateWorkflowCase() {
    this.workflowCaseModel = {
      ...this.workflowCaseModel,
      deadline: format(this.dataForm.value.deadline, 'yyyy-MM-dd'),
      dateOfIncident: format(this.dataForm.value.dateOfIncident, 'yyyy-MM-dd'),
    };
    this.updateSub$ = this.workflowPnCasesService
      .updateCase(this.workflowCaseModel)
      .subscribe((data) => {
        if (data && data.success) {
          this.goBack();
        }
      });
  }

  get userClaimsEnum() {
    return UserClaimsEnum;
  }

  ngOnInit() {
    this.dataForm = this.formBuilder.group({
      deadline: ['', Validators.required],
      dateOfIncident: ['', Validators.required],
    });
    this.getSites();
    this.loadCase();
    this.loadPlaces();
  }

  ngOnDestroy() {}

  loadCase() {
    if (!this.id || this.id === 0) {
      return;
    }
    this.workflowPnCasesService.readCase(this.id).subscribe((operation) => {
      if (operation && operation.success) {
        this.workflowCaseModel = operation.model;
      }
    });
  }

  getSites() {
    this.getAllSitesSub$ = this.sitesService.getAllSites().subscribe((data) => {
      if (data && data.success) {
        this.deviceUsersList = data.model;
      }
    });
  }

  loadPlaces() {
    this.workflowPnCasesService.getPlaces().subscribe((operation) => {
      if (operation && operation.success) {
        this.places = operation.model;
      }
    });
  }
}
