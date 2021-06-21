import { Injectable } from '@angular/core';
import { persistState, Store, StoreConfig } from '@datorama/akita';
import { CommonPaginationState } from 'src/app/common/models/common-pagination-state';

export interface WorkflowCasesState {
  pagination: CommonPaginationState;
  filters: { nameFilter: string };
  total: number;
}

function createInitialState(): WorkflowCasesState {
  return <WorkflowCasesState>{
    pagination: {
      pageSize: 10,
      sort: 'Id',
      isSortDsc: false,
      offset: 0,
    },
    filters: {
      nameFilter: '',
    },
    total: 0,
  };
}

const workflowCasesPersistStorage = persistState({
  include: ['workflowCases'],
  key: 'workflowPn',
  preStorageUpdate(storeName, state: WorkflowCasesState) {
    return {
      pagination: state.pagination,
      filters: state.filters,
    };
  },
});

@Injectable({ providedIn: 'root' })
@StoreConfig({ name: 'workflowCases', resettable: true })
export class WorkflowCasesStore extends Store<WorkflowCasesState> {
  constructor() {
    super(createInitialState());
  }
}

export const workflowCasesPersistProvider = {
  provide: 'persistStorage',
  useValue: workflowCasesPersistStorage,
  multi: true,
};
