import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {AdminGuard, AuthGuard, PermissionGuard} from 'src/app/common/guards';
import {WorkflowPnLayoutComponent} from './layouts';
import {
  WorkflowCasesContainerComponent,
  WorkflowSettingsComponent,
} from './components';
import {WorkflowPnClaims} from './enums';

export const routes: Routes = [
  {
    path: '',
    component: WorkflowPnLayoutComponent,
    canActivate: [PermissionGuard],
    data: {
      requiredPermission: WorkflowPnClaims.accessWorkflowPlugin,
    },
    children: [
      {
        path: 'cases',
        canActivate: [AuthGuard],
        component: WorkflowCasesContainerComponent,
      },
      {
        path: 'settings',
        canActivate: [AdminGuard],
        component: WorkflowSettingsComponent,
      }
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class WorkflowPnRouting {}
