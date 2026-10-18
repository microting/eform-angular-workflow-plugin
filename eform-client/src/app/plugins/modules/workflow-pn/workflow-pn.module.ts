import { NgModule } from '@angular/core';
import { StoreModule } from '@ngrx/store';
import { WorkflowPnCasesService, WorkflowPnSettingsService } from './services';
import { WorkflowPnRouting } from './workflow-pn.routing';
import * as workflowCasesReducer from './state/workflow-cases/workflow-cases.reducer';

@NgModule({
  imports: [
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
