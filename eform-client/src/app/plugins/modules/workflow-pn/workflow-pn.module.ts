import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { NgSelectModule } from '@ng-select/ng-select';
import { TranslateModule } from '@ngx-translate/core';
import { MDBBootstrapModule } from 'angular-bootstrap-md';
import { EformCasesModule } from 'src/app/common/modules/eform-cases/eform-cases.module';
import { EformSharedTagsModule } from 'src/app/common/modules/eform-shared-tags/eform-shared-tags.module';
import { EformSharedModule } from 'src/app/common/modules/eform-shared/eform-shared.module';
import { CasesModule } from 'src/app/modules';
import {
  WorkflowCaseDeleteComponent,
  WorkflowCaseEditComponent,
  WorkflowCasesPageComponent,
  WorkflowSettingsComponent,
} from './components';
import { WorkflowPnLayoutComponent } from './layouts';
import { WorkflowPnCasesService, WorkflowPnSettingsService } from './services';
import { workflowStoreProviders } from './store/store-providers.config';
import { WorkflowPnRouting } from './workflow-pn.routing';

@NgModule({
  imports: [
    CommonModule,
    TranslateModule,
    FormsModule,
    NgSelectModule,
    EformSharedModule,
    FontAwesomeModule,
    RouterModule,
    WorkflowPnRouting,
    ReactiveFormsModule,
    CasesModule,
    EformSharedTagsModule,
    EformCasesModule,
    MDBBootstrapModule,
  ],
  declarations: [
    WorkflowPnLayoutComponent,
    WorkflowSettingsComponent,
    WorkflowCaseEditComponent,
    WorkflowCaseEditComponent,
    WorkflowCasesPageComponent,
    WorkflowCaseDeleteComponent
  ],
  providers: [
    WorkflowPnSettingsService,
    WorkflowPnCasesService,
    ...workflowStoreProviders,
  ],
})
export class WorkflowPnModule {}
