import {Injectable} from '@angular/core';
import {tap} from 'rxjs';
import {updateTableSort} from 'src/app/common/helpers';
import {
  CommonPaginationState,
  PaginationModel,
} from 'src/app/common/models';
import {WorkflowPnCasesService} from '../../../services';
import {Store} from '@ngrx/store';
import {
  selectWorkflowCasesFilters,
  selectWorkflowCasesPagination,
  updateWorkflowCasesFilters,
  updateWorkflowCasesPagination,
  updateWorkflowCasesTotal,
  WorkflowCasesFiltration
} from '../../../state';

@Injectable({providedIn: 'root'})
export class WorkflowCasesStateService {
  private selectWorkflowCasesPagination$ = this.store.select(selectWorkflowCasesPagination);
  private selectWorkflowCasesFilters$ = this.store.select(selectWorkflowCasesFilters);
  currentPagination: CommonPaginationState;
  currentFilters: WorkflowCasesFiltration;

  constructor(
    private store: Store,
    private service: WorkflowPnCasesService,
  ) {
    this.selectWorkflowCasesPagination$.subscribe((x) => this.currentPagination = x);
    this.selectWorkflowCasesFilters$.subscribe((x) => this.currentFilters = x);
  }

  getWorkflowCases() {
    return this.service
      .getWorkflowCases({
        ...this.currentPagination,
        ...this.currentFilters,
      })
      .pipe(
        tap((response) => {
          if (response && response.success && response.model) {
            this.store.dispatch(updateWorkflowCasesTotal(response.model.total));
          }
        })
      );
  }

  updateNameFilter(nameFilter: string) {
    this.store.dispatch(updateWorkflowCasesFilters({...this.currentFilters, nameFilter: nameFilter}));
  }

  onDelete() {
    this.store.dispatch(updateWorkflowCasesTotal(this.currentPagination.total - 1));
  }

  onSortTable(sort: string) {
    const localPageSettings = updateTableSort(
      sort,
      this.currentPagination.sort,
      this.currentPagination.isSortDsc
    );
    this.store.dispatch(updateWorkflowCasesPagination({...this.currentPagination, ...localPageSettings}));
  }

  updatePagination(pagination: PaginationModel) {
    this.store.dispatch(updateWorkflowCasesPagination({
      ...this.currentPagination,
      pageSize: pagination.pageSize,
      offset: pagination.offset,
    }));
  }
}
