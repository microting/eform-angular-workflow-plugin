import { Injectable } from '@angular/core';
import { CasesService } from 'src/app/common/services';
import { WorkflowCasesStore, WorkflowCasesQuery } from './';
import { Observable } from 'rxjs';
import {
  CaseListModel,
  OperationDataResult,
  Paged,
  PaginationModel,
  SortModel,
} from 'src/app/common/models';
import { updateTableSort } from 'src/app/common/helpers';
import { getOffset } from 'src/app/common/helpers/pagination.helper';
import { map } from 'rxjs/operators';
import { WorkflowPnCasesService } from '../../../services';

@Injectable({ providedIn: 'root' })
export class WorkflowCasesStateService {
  constructor(
    private store: WorkflowCasesStore,
    private service: CasesService,
    private query: WorkflowCasesQuery
  ) {}

  getPageSize(): Observable<number> {
    return this.query.selectPageSize$;
  }

  getSort(): Observable<SortModel> {
    return this.query.selectSort$;
  }

  getAllCases(
    templateId: number
  ): Observable<OperationDataResult<CaseListModel>> {
    return this.service
      .getCases({
        ...this.query.pageSetting.pagination,
        ...this.query.pageSetting.filters,
        templateId,
      })
      .pipe(
        map((response) => {
          if (response && response.success && response.model) {
            this.store.update(() => ({
              totalCases: response.model.numOfElements,
            }));
          }
          return response;
        })
      );
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
      totalCases: state.totalCases - 1,
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
      this.query.pageSetting.totalCases
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
}
