import {createAction} from '@ngrx/store';
import {CommonPaginationState} from 'src/app/common/models';
import {WorkflowCasesFiltration} from './';

export const updateWorkflowCasesFilters = createAction(
  '[WorkflowCases] Update filters',
  (payload: WorkflowCasesFiltration) => ({payload})
);

export const updateWorkflowCasesPagination = createAction(
  '[WorkflowCases] Update pagination',
  (payload: CommonPaginationState) => ({payload})
);
export const updateWorkflowCasesTotal = createAction(
  '[WorkflowCases] Update total',
  (payload: number) => ({payload})
);
