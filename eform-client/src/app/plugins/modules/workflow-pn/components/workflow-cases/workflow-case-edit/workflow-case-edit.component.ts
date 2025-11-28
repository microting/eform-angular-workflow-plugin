import {Component, OnDestroy, OnInit, inject} from '@angular/core';
import {
  CommonDictionaryTextModel,
  SiteNameDto,
} from 'src/app/common/models';
import {WorkflowCaseModel} from '../../../models';
import {Subscription} from 'rxjs';
import {UserClaimsEnum} from 'src/app/common/const';
import {ActivatedRoute, Router} from '@angular/router';
import {
  EntitySelectService,
  SitesService,
} from 'src/app/common/services';
import {AutoUnsubscribe} from 'ngx-auto-unsubscribe';
import {WorkflowPnCasesService} from '../../../services';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import {format} from 'date-fns';
import {Store} from '@ngrx/store';
import {MatDatepickerInputEvent} from '@angular/material/datepicker';

@AutoUnsubscribe()
@Component({
    selector: 'app-workflow-case-edit',
    templateUrl: './workflow-case-edit.component.html',
    styleUrls: ['./workflow-case-edit.component.scss'],
    standalone: false
})
export class WorkflowCaseEditComponent implements OnInit, OnDestroy {
  private activateRoute = inject(ActivatedRoute);
  private router = inject(Router);
  private authStore = inject(Store);
  private workflowPnCasesService = inject(WorkflowPnCasesService);
  private formBuilder = inject(FormBuilder);
  private sitesService = inject(SitesService);
  private entitySelectService = inject(EntitySelectService);

  deviceUsersList: SiteNameDto[] = [];
  places: Array<CommonDictionaryTextModel> = [];
  incidentTypes: Array<CommonDictionaryTextModel> = [];
  workflowCaseModel: WorkflowCaseModel = new WorkflowCaseModel();

  id: number;
  templateId: number;
  reverseRoute: string;

  activatedRouteSub$: Subscription;
  updateSub$: Subscription;
  getAllSitesSub$: Subscription;

  dataForm: FormGroup;

  statuses = [
    {id: 2, text: 'Vælg status'}, // No status
    {id: 0, text: 'Igangværende'}, // Ongoing
    {id: 3, text: 'Ikke igangsat'}, // Not initiated
    {id: 1, text: 'Afsluttet'}, // Closed
    {id: 4, text: 'Annulleret'}, // Canceled
  ];

  

  ngOnInit() {
    this.activatedRouteSub$ = this.activateRoute.params.subscribe((params) => {
      this.id = +params['id'];
    });
    this.activateRoute.queryParams.subscribe((params) => {
      this.reverseRoute = params['reverseRoute'];
    });

    this.dataForm = this.formBuilder.group({
      deadline: ['', Validators.required],
      dateOfIncident: ['', Validators.required],
    });
    this.getSites();
    this.loadCase();
  }

  ngOnDestroy() {
  }

  loadCase() {
    if (!this.id || this.id === 0) {
      return;
    }
    this.workflowPnCasesService.readCase(this.id).subscribe((operation) => {
      if (operation && operation.success) {
        this.workflowCaseModel = operation.model;
        this.loadPlaces();
        this.loadTypes();
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
    this.entitySelectService.getEntitySelectableGroupDictionary(this.workflowCaseModel.incidentPlaceListId).subscribe((operation => {
      if (operation && operation.success) {
        this.places = operation.model;
      }
    }));
  }

  loadTypes() {
    this.entitySelectService.getEntitySelectableGroupDictionary(this.workflowCaseModel.incidentTypeListId).subscribe((operation => {
      if (operation && operation.success) {
        this.incidentTypes = operation.model;
      }
    }));
  }

  onSelectedChangedPlace(e: any) {
    this.workflowCaseModel.incidentPlaceId = e.id;
    this.workflowCaseModel.incidentPlace = e.text;
  }

  onSelectedChangedType(e: any) {
    this.workflowCaseModel.incidentTypeId = e.id;
    this.workflowCaseModel.incidentType = e.text;
  }

  onDateSelectedIncidentDate(e: MatDatepickerInputEvent<any, any>) {
    this.workflowCaseModel.dateOfIncident = format(e.value, 'yyyy-MM-dd');
  }

  onDateSelectedDeadline(e: MatDatepickerInputEvent<any, any>) {
    this.workflowCaseModel.deadline = format(e.value, 'yyyy-MM-dd');
  }
}
