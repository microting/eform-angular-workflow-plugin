import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { EformSharedModule } from 'src/app/common/modules/eform-shared/eform-shared.module';
import {
  SettingsAddSiteModalComponent,
  /*SettingsRemoveSiteModalComponent,*/
  WorkflowCaseDeleteComponent,
  WorkflowCaseEditComponent,
  WorkflowCasesPageComponent,
  WorkflowSettingsComponent, WorkflowFoldersModalComponent,
} from './components';
import { WorkflowPnLayoutComponent } from './layouts';
import { WorkflowPnCasesService, WorkflowPnSettingsService } from './services';
import { WorkflowPnRouting } from './workflow-pn.routing';
import {EformImportedModule} from 'src/app/common/modules/eform-imported/eform-imported.module';
import {EformCasesModule} from 'src/app/common/modules/eform-cases/eform-cases.module';
import {MtxGridModule} from '@ng-matero/extensions/grid';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatInputModule} from '@angular/material/input';
import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {MatTooltipModule} from '@angular/material/tooltip';
import {MatDialogModule} from '@angular/material/dialog';
import {MatCardModule} from '@angular/material/card';
import {MtxSelectModule} from '@ng-matero/extensions/select';
import {StoreModule} from '@ngrx/store';
import * as workflowCasesReducer from './state/workflow-cases/workflow-cases.reducer';
import {MatDatepickerModule} from '@angular/material/datepicker';

@NgModule({
  imports: [
    CommonModule,
    TranslateModule,
    FormsModule,
    EformSharedModule,
    RouterModule,
    WorkflowPnRouting,
    EformImportedModule,
    EformCasesModule,
    MtxGridModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatDialogModule,
    MatCardModule,
    MtxSelectModule,
    StoreModule.forFeature('workflowPn', {
      workflowCasesState: workflowCasesReducer.reducer,
    }),
    MatDatepickerModule,
  ],
  declarations: [
    WorkflowPnLayoutComponent,
    WorkflowSettingsComponent,
    WorkflowCasesPageComponent,
    WorkflowCaseDeleteComponent,
    WorkflowCaseEditComponent,
    SettingsAddSiteModalComponent,
    // SettingsRemoveSiteModalComponent,
    WorkflowFoldersModalComponent,
  ],
  providers: [
    WorkflowPnSettingsService,
    WorkflowPnCasesService,
  ],
})
export class WorkflowPnModule {}
