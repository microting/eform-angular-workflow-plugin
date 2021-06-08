import {
  Component,
  OnDestroy,
  OnInit,
  QueryList,
  ViewChild,
  ViewChildren,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { Observable, Subscription } from 'rxjs';
import { UserClaimsEnum } from 'src/app/common/const';
import {
  CaseEditRequest,
  EformPermissionsSimpleModel,
  ReplyElementDto,
  ReplyRequest,
  TemplateDto,
} from 'src/app/common/models';
import { CaseEditElementComponent } from 'src/app/common/modules/eform-cases/components';
import {
  AuthService,
  CasesService,
  EFormService,
  SecurityGroupEformsPermissionsService,
} from 'src/app/common/services';
import { AuthStateService } from 'src/app/common/store';
import {
  WorkflowPnCasesService,
  WorkflowPnSettingsService,
} from '../../../services';

@AutoUnsubscribe()
@Component({
  selector: 'app-workflow-case-edit',
  templateUrl: './workflow-case-edit.component.html',
  styleUrls: ['./workflow-case-edit.component.scss'],
})
export class WorkflowCaseEditComponent implements OnInit, OnDestroy {
  @ViewChildren(CaseEditElementComponent)
  editElements: QueryList<CaseEditElementComponent>;
  @ViewChild('caseConfirmation', { static: true }) caseConfirmation;
  id: number;
  currentTemplate: TemplateDto = new TemplateDto();
  replyElement: ReplyElementDto = new ReplyElementDto();

  requestModels: Array<CaseEditRequest> = [];
  replyRequest: ReplyRequest = new ReplyRequest();
  eformPermissionsSimpleModel: EformPermissionsSimpleModel = new EformPermissionsSimpleModel();

  isNoSaveExitAllowed = false;
  isSaveClicked = false;
  reverseRoute: string;

  getCase$: Subscription;
  getSelectedTemplate$: Subscription;
  updateCase$: Subscription;
  getTemplatePermissions$: Subscription;

  get userClaims() {
    return this.authStateService.currentUserClaims;
  }

  get userClaimsEnum() {
    return UserClaimsEnum;
  }

  constructor(
    private activateRoute: ActivatedRoute,
    private casesService: CasesService,
    private workflowCasesService: WorkflowPnCasesService,
    private settingsService: WorkflowPnSettingsService,
    private eFormService: EFormService,
    private router: Router,
    private authService: AuthService,
    private securityGroupEformsService: SecurityGroupEformsPermissionsService,
    private authStateService: AuthStateService
  ) {
    const activatedRouteSub = this.activateRoute.params.subscribe((params) => {
      this.id = +params['id'];
    });
    this.activateRoute.queryParams.subscribe((params) => {
      this.reverseRoute = params['reverseRoute'];
    });
  }

  ngOnInit() {
    this.loadTemplateInfo();
  }

  ngOnDestroy() {}

  loadCase(templateId: number) {
    if (!this.id || this.id === 0) {
      return;
    }
    this.getCase$ = this.casesService
      .getById(this.id, templateId)
      .subscribe((operation) => {
        if (operation && operation.success) {
          this.replyElement = operation.model;
        }
      });
  }

  saveCase() {
    this.requestModels = [];
    this.editElements.forEach((x) => {
      x.extractData();
      this.requestModels.push(x.requestModel);
    });
    this.replyRequest.id = this.replyElement.id;
    this.replyRequest.label = this.replyElement.label;
    this.replyRequest.elementList = this.requestModels;
    this.updateCase$ = this.workflowCasesService
      .updateCase(this.replyRequest)
      .subscribe((operation) => {
        if (operation && operation.success) {
          this.replyElement = new ReplyElementDto();
          this.isNoSaveExitAllowed = true;
          this.navigateToReverse();
        }
      });
  }

  loadTemplateInfo() {
    this.getSelectedTemplate$ = this.settingsService
      .getSelectedTemplate()
      .subscribe((operation) => {
        if (operation && operation.success) {
          this.currentTemplate = operation.model;
          this.loadEformPermissions(this.currentTemplate.id);
          this.loadCase(this.currentTemplate.id);
        }
      });
  }

  goToSection(location: string): void {
    window.location.hash = location;
    setTimeout(() => {
      document.querySelector(location).parentElement.scrollIntoView();
      return true;
    });
  }

  confirmExit(keepData: boolean): void {
    this.caseConfirmation.navigateAwaySelection$.next(true);
    if (keepData) {
      this.saveCase();
    } else {
      this.isNoSaveExitAllowed = true;
    }
  }

  canDeactivate(): Observable<boolean> | boolean {
    if (
      !this.isNoSaveExitAllowed &&
      this.checkEformPermissions(UserClaimsEnum.caseUpdate)
    ) {
      this.caseConfirmation.show();
      return this.caseConfirmation.navigateAwaySelection$;
    }
    return true;
  }

  navigateToReverse() {
    if (this.reverseRoute) {
      this.router.navigate([this.reverseRoute]).then();
    } else {
      this.router.navigate(['../']).then();
    }
  }

  loadEformPermissions(templateId: number) {
    if (this.securityGroupEformsService.mappedPermissions.length) {
      this.eformPermissionsSimpleModel = this.securityGroupEformsService.mappedPermissions.find(
        (x) => x.templateId === templateId
      );
    } else {
      this.getTemplatePermissions$ = this.securityGroupEformsService
        .getEformsSimplePermissions()
        .subscribe((data) => {
          if (data && data.success) {
            const foundTemplates = this.securityGroupEformsService.mapEformsSimplePermissions(
              data.model
            );
            if (foundTemplates.length) {
              this.eformPermissionsSimpleModel = foundTemplates.find(
                (x) => x.templateId === templateId
              );
            }
            // // This is commented as loadCase is in 99% of the time the slowest
          }
        });
    }
  }

  checkEformPermissions(permissionIndex: number) {
    if (this.eformPermissionsSimpleModel.templateId) {
      return this.eformPermissionsSimpleModel.permissionsSimpleList.find(
        (x) => x === UserClaimsEnum[permissionIndex].toString()
      );
    } else {
      return this.userClaims[UserClaimsEnum[permissionIndex].toString()];
    }
  }
}
