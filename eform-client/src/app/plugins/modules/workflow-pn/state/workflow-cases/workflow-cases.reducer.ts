import {CommonPaginationState} from 'src/app/common/models';
import {Action, createReducer, on} from '@ngrx/store';
import {
  updateWorkflowCasesFilters,
  updateWorkflowCasesPagination, updateWorkflowCasesTotal
} from './workflow-cases.actions';

export interface WorkflowCasesFiltration {
  nameFilter: string;
}

export interface WorkflowCasesState {
  pagination: CommonPaginationState;
  filters: WorkflowCasesFiltration;
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
};

export const _reducer = createReducer(
  initialWorkflowCasesState,
  on(updateWorkflowCasesFilters, (state, {payload}) => ({
      ...state,
      filters: {...state.filters, ...payload,}
    }
  )),
  on(updateWorkflowCasesPagination, (state, {payload}) => ({
      ...state,
      pagination: {...state.pagination, ...payload},
    }
  )),
  on(updateWorkflowCasesTotal, (state, {payload}) => ({
      ...state,
      total: payload,
      pagination: {...state.pagination, total: payload},
    }
  )),
);

export function reducer(state: WorkflowCasesState | undefined, action: Action) {
  return _reducer(state, action);
}
