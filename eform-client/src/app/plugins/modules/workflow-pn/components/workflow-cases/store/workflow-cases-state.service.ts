import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { getOffset, updateTableSort } from 'src/app/common/helpers';
import {
  CommonPaginationState,
  OperationDataResult,
  Paged,
  PaginationModel,
} from 'src/app/common/models';
import { WorkflowCaseModel } from '../../../models';
import { WorkflowPnCasesService } from '../../../services';
import {Store} from '@ngrx/store';
import {
  selectWorkflowCasesFilters,
  selectWorkflowCasesPagination
} from '../../../state/workflow-cases/workflow-cases.selector';

@Injectable({ providedIn: 'root' })
export class WorkflowCasesStateService {
  private selectWorkflowCasesPagination$ = this.store.select(selectWorkflowCasesPagination);
  private selectWorkflowCasesFilters$ = this.store.select(selectWorkflowCasesFilters);
  constructor(
    private store: Store,
    private service: WorkflowPnCasesService,
  ) {}

  // getPageSize(): Observable<number> {
  //   return this.query.selectPageSize$;
  // }
  //
  // /*  getSort(): Observable<SortModel> {
  //   return this.query.selectSort$;
  // }*/
  //
  // getActiveSort(): Observable<string> {
  //   return this.query.selectActiveSort$;
  // }
  //
  // getActiveSortDirection(): Observable<'asc' | 'desc'> {
  //   return this.query.selectActiveSortDirection$;
  // }

  getWorkflowCases(): Observable<
    OperationDataResult<Paged<WorkflowCaseModel>>
  > {
    let currentPagination: CommonPaginationState;
    let currentFilters = {};
    this.selectWorkflowCasesPagination$.subscribe((x) => currentPagination = x);
    this.selectWorkflowCasesFilters$.subscribe((x) => currentFilters = x);
    return this.service
      .getWorkflowCases({
        ...currentPagination,
        ...currentFilters,
      })
      .pipe(
        map((response) => {
          if (response && response.success && response.model) {
            this.store.dispatch({
              type: '[WorkflowCases] Update pagination',
              payload: {
                ...currentPagination,
                total: response.model.total,
              }
            });
            // this.store.update(() => ({
            //   total: response.model.total,
            // }));
          }
          return response;
        })
      );
    // return this.service
    //   .getWorkflowCases({
    //     ...this.query.pageSetting.pagination,
    //     ...this.query.pageSetting.filters,
    //   })
    //   .pipe(
    //     map((response) => {
    //       if (response && response.success && response.model) {
    //         this.store.update(() => ({
    //           total: response.model.total,
    //         }));
    //       }
    //       return response;
    //     })
    //   );
  }

  updateNameFilter(nameFilter: string) {
    this.store.dispatch({
      type: '[WorkflowCases] Update filters',
      payload: {
        nameFilter: nameFilter,
      }
    })
    // this.store.update((state) => ({
    //   pagination: {
    //     ...state.pagination,
    //     offset: 0,
    //   },
    //   filters: {
    //     ...state.filters,
    //     nameFilter: nameFilter,
    //   },
    // }));
  }
  //
  // updatePageSize(pageSize: number) {
  //   this.store.update((state) => ({
  //     pagination: {
  //       ...state.pagination,
  //       pageSize: pageSize,
  //     },
  //   }));
  //   this.checkOffset();
  // }
  //
  // changePage(offset: number) {
  //   this.store.update((state) => ({
  //     pagination: {
  //       ...state.pagination,
  //       offset: offset,
  //     },
  //   }));
  // }

  onDelete() {
    // this.store.update((state) => ({
    //   total: state.total - 1,
    // }));
    // this.checkOffset();
  }

  onSortTable(sort: string) {
    let currentPagination: CommonPaginationState;
    this.selectWorkflowCasesPagination$.subscribe((x) => currentPagination = x);
    const localPageSettings = updateTableSort(
      sort,
      currentPagination.sort,
      currentPagination.isSortDsc
    );
    this.store.dispatch({
      type: '[WorkflowCases] Update pagination',
      payload: {
        ...currentPagination,
        isSortDsc: localPageSettings.isSortDsc,
        sort: localPageSettings.sort,
      }
    })
    // const localPageSettings = updateTableSort(
    //   sort,
    //   this.query.pageSetting.pagination.sort,
    //   this.query.pageSetting.pagination.isSortDsc
    // );
    // this.store.update((state) => ({
    //   pagination: {
    //     ...state.pagination,
    //     isSortDsc: localPageSettings.isSortDsc,
    //     sort: localPageSettings.sort,
    //   },
    // }));
  }

  // checkOffset() {
  //   const newOffset = getOffset(
  //     this.query.pageSetting.pagination.pageSize,
  //     this.query.pageSetting.pagination.offset,
  //     this.query.pageSetting.total
  //   );
  //   if (newOffset !== this.query.pageSetting.pagination.offset) {
  //     this.store.update((state) => ({
  //       pagination: {
  //         ...state.pagination,
  //         offset: newOffset,
  //       },
  //     }));
  //   }
  // }

  // getPagination(): Observable<PaginationModel> {
  //   return this.query.selectPagination$;
  // }

  updatePagination(pagination: PaginationModel) {
    let currentPagination: CommonPaginationState;
    this.selectWorkflowCasesPagination$.subscribe((x) => currentPagination = x);
    this.store.dispatch({
      type: '[WorkflowCases] Update pagination',
      payload: {
        ...currentPagination,
        pageSize: pagination.pageSize,
        offset: pagination.offset,
      }
    })
    // this.store.update((state) => ({
    //   pagination: {
    //     ...state.pagination,
    //     pageSize: pagination.pageSize,
    //     offset: pagination.offset,
    //   },
    // }));
    // this.checkOffset();
  }

  // getNameFilter(): Observable<string> {
  //   return this.query.selectNameFilter$;
  // }
}
