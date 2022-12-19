import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { getOffset, updateTableSort } from 'src/app/common/helpers';
import {
  OperationDataResult,
  Paged,
  PaginationModel,
} from 'src/app/common/models';
import { WorkflowCaseModel } from '../../../models';
import { WorkflowPnCasesService } from '../../../services';
import { WorkflowCasesQuery, WorkflowCasesStore } from './';

@Injectable({ providedIn: 'root' })
export class WorkflowCasesStateService {
  constructor(
    private store: WorkflowCasesStore,
    private service: WorkflowPnCasesService,
    private query: WorkflowCasesQuery
  ) {}

  getPageSize(): Observable<number> {
    return this.query.selectPageSize$;
  }

  /*  getSort(): Observable<SortModel> {
    return this.query.selectSort$;
  }*/

  getActiveSort(): Observable<string> {
    return this.query.selectActiveSort$;
  }

  getActiveSortDirection(): Observable<'asc' | 'desc'> {
    return this.query.selectActiveSortDirection$;
  }

  getWorkflowCases(): Observable<
    OperationDataResult<Paged<WorkflowCaseModel>>
  > {
    return this.service
      .getWorkflowCases({
        ...this.query.pageSetting.pagination,
        ...this.query.pageSetting.filters,
      })
      .pipe(
        map((response) => {
          if (response && response.success && response.model) {
            this.store.update(() => ({
              total: response.model.total,
            }));
          }
          return response;
        })
      );
  }

  updateNameFilter(nameFilter: string) {
    this.store.update((state) => ({
      pagination: {
        ...state.pagination,
        offset: 0,
      },
      filters: {
        ...state.filters,
        nameFilter: nameFilter,
      },
    }));
  }

  updatePageSize(pageSize: number) {
    this.store.update((state) => ({
      pagination: {
        ...state.pagination,
        pageSize: pageSize,
      },
    }));
    this.checkOffset();
  }

  changePage(offset: number) {
    this.store.update((state) => ({
      pagination: {
        ...state.pagination,
        offset: offset,
      },
    }));
  }

  onDelete() {
    this.store.update((state) => ({
      total: state.total - 1,
    }));
    this.checkOffset();
  }

  onSortTable(sort: string) {
    const localPageSettings = updateTableSort(
      sort,
      this.query.pageSetting.pagination.sort,
      this.query.pageSetting.pagination.isSortDsc
    );
    this.store.update((state) => ({
      pagination: {
        ...state.pagination,
        isSortDsc: localPageSettings.isSortDsc,
        sort: localPageSettings.sort,
      },
    }));
  }

  checkOffset() {
    const newOffset = getOffset(
      this.query.pageSetting.pagination.pageSize,
      this.query.pageSetting.pagination.offset,
      this.query.pageSetting.total
    );
    if (newOffset !== this.query.pageSetting.pagination.offset) {
      this.store.update((state) => ({
        pagination: {
          ...state.pagination,
          offset: newOffset,
        },
      }));
    }
  }

  getPagination(): Observable<PaginationModel> {
    return this.query.selectPagination$;
  }

  updatePagination(pagination: PaginationModel) {
    this.store.update((state) => ({
      pagination: {
        ...state.pagination,
        pageSize: pagination.pageSize,
        offset: pagination.offset,
      },
    }));
    // this.checkOffset();
  }

  getNameFilter(): Observable<string> {
    return this.query.selectNameFilter$;
  }
}
