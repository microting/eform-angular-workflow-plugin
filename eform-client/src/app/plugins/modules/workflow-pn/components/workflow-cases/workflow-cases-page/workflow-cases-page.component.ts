import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { Subscription } from 'rxjs';
import { UserClaimsEnum } from 'src/app/common/const';
import { composeCasesTableHeaders } from 'src/app/common/helpers';
import {
  CaseListModel,
  CaseModel,
  EformPermissionsSimpleModel,
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
import { WorkflowCasesStateService } from '../store';
import { WorkflowPnSettingsService } from '../../../services';

@AutoUnsubscribe()
@Component({
  selector: 'app-workflow-cases-page',
  templateUrl: './workflow-cases-page.component.html',
  styleUrls: ['./workflow-cases-page.component.scss'],
})
export class WorkflowCasesPageComponent implements OnInit, OnDestroy {
  @ViewChild('modalRemoveCase', { static: true }) modalRemoveCase;
  currentTemplate: TemplateDto = new TemplateDto();
  eformPermissionsSimpleModel: EformPermissionsSimpleModel = new EformPermissionsSimpleModel();
  caseListModel: CaseListModel = new CaseListModel();
  localPageSettings: PageSettingsModel = new PageSettingsModel();

  getCases$: Subscription;
  getTemplate$: Subscription;

  get userClaims() {
    return this.authStateService.currentUserClaims;
  }

  get userClaimsEnum() {
    return UserClaimsEnum;
  }

  tableHeaders: TableHeaderElementModel[];

  constructor(
    private activateRoute: ActivatedRoute,
    private casesService: CasesService,
    private eFormService: EFormService,
    public authStateService: AuthStateService,
    private securityGroupEformsService: SecurityGroupEformsPermissionsService,
    public workflowCaseStateService: WorkflowCasesStateService,
    private settingsService: WorkflowPnSettingsService
  ) {}

  ngOnDestroy(): void {}

  ngOnInit() {
    this.loadTemplateData();
  }

  onLabelInputChanged(label: string) {
    this.workflowCaseStateService.updateNameFilter(label);
    this.loadAllCases();
  }

  onDeleteClicked(caseModel: CaseModel) {
    this.modalRemoveCase.show(caseModel, this.currentTemplate.id);
  }

  sortTable(sort: string) {
    this.workflowCaseStateService.onSortTable(sort);
    this.loadAllCases();
  }

  loadAllCases() {
    this.getCases$ = this.workflowCaseStateService
      .getCases()
      .subscribe((operation) => {
        if (operation && operation.success) {
          this.caseListModel = operation.model;
          composeCasesTableHeaders(
            this.currentTemplate,
            this.authStateService.isAdmin
          );
        }
      });
  }

  loadTemplateData() {
    this.getTemplate$ = this.settingsService
      .getSelectedTemplate()
      .subscribe((operation) => {
        if (operation && operation.success) {
          this.currentTemplate = operation.model;
          this.loadEformPermissions(this.currentTemplate.id);
          this.loadAllCases();
        }
      });
  }

  loadEformPermissions(templateId: number) {
    if (this.securityGroupEformsService.mappedPermissions.length) {
      this.eformPermissionsSimpleModel = this.securityGroupEformsService.mappedPermissions.find(
        (x) => x.templateId === templateId
      );
    } else {
      this.securityGroupEformsService
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

  changePage(offset: number) {
    this.workflowCaseStateService.changePage(offset);
    this.loadAllCases();
  }

  onPageSizeChanged(newPageSize: number) {
    this.workflowCaseStateService.updatePageSize(newPageSize);
    this.loadAllCases();
  }

  onCaseDeleted() {
    this.workflowCaseStateService.onDelete();
    this.loadAllCases();
  }
}
