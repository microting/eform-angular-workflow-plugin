import {createAction} from '@ngrx/store';

export const updateWorkflowCasesFilters = createAction(
  '[WorkflowCases] Update filters',
  (payload: any) => ({payload})
);

export const updateWorkflowCasesPagination = createAction(
  '[WorkflowCases] Update pagination',
  (payload: any) => ({payload})
);
