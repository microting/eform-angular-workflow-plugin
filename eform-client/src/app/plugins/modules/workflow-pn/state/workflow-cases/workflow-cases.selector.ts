import {WorkflowState} from 'src/app/plugins/modules/workflow-pn/state/workflow.state';
import {createSelector} from '@ngrx/store';

export const selectWorkflowState = (state: { workflowPn: WorkflowState }) => state.workflowPn;
export const selectWorkflowCases =
  createSelector(selectWorkflowState, (state) => state.workflowCasesState);
export const selectWorkflowCasesPagination =
  createSelector(selectWorkflowCases, (state) => state.pagination);
export const selectWorkflowCasesPaginationIsSortDsc =
  createSelector(selectWorkflowCasesPagination, (state) => state.isSortDsc ? 'desc' : 'asc');
export const selectWorkflowCasesPaginationSort =
  createSelector(selectWorkflowCasesPagination, (state) => state.sort);
export const selectWorkflowCasesFilters =
  createSelector(selectWorkflowCases, (state) => state.filters);
export const selectWorkflowCasesFiltersName =
  createSelector(selectWorkflowCasesFilters, (state) => state.nameFilter);
