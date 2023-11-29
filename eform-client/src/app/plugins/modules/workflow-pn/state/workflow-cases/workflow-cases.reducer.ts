import {CommonPaginationState} from 'src/app/common/models';
import {Action, createReducer, on} from '@ngrx/store';
import {
  updateWorkflowCasesFilters, updateWorkflowCasesPagination
} from './workflow-cases.actions';

export interface WorkflowCasesState {
  pagination: CommonPaginationState;
  filters: { nameFilter: string };
  total: number;
}

export const initialWorkflowCasesState: WorkflowCasesState = {
  pagination: {
    pageSize: 10,
    sort: 'Id',
    isSortDsc: false,
    offset: 0,
    pageIndex: 0,
    total: 0,
  },
  filters: {
    nameFilter: '',
  },
  total: 0,
}

export const _reducer = createReducer(
  initialWorkflowCasesState,
  on(updateWorkflowCasesFilters, (state, {payload}) => ({
    ...state,
    filters: {
      nameFilter: payload.nameFilter,
    }
    }
  )),
  on(updateWorkflowCasesPagination, (state, {payload}) => ({
    ...state,
    pagination: payload,
    }
  ))
)

export function reducer(state: WorkflowCasesState | undefined, action: Action) {
  return _reducer(state, action);
}
