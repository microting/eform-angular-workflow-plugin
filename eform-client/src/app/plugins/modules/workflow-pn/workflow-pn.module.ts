import { NgModule } from '@angular/core';
import {MatDialogModule} from '@angular/material/dialog';
import { StoreModule } from '@ngrx/store';
import { WorkflowPnCasesService, WorkflowPnSettingsService } from './services';
import { WorkflowPnRouting } from './workflow-pn.routing';
import * as workflowCasesReducer from './state/workflow-cases/workflow-cases.reducer';

@NgModule({
  imports: [
    MatDialogModule,
    WorkflowPnRouting,
    StoreModule.forFeature('workflowPn', {
      workflowCasesState: workflowCasesReducer.reducer,
    }),
  ],
  providers: [
    WorkflowPnSettingsService,
    WorkflowPnCasesService,
  ],
})
export class WorkflowPnModule {}
