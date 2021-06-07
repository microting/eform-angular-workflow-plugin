import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { updateTableSort } from 'src/app/common/helpers';
import { getOffset } from 'src/app/common/helpers/pagination.helper';
import {
  CaseListModel,
  OperationDataResult,
  PaginationModel,
  SortModel,
} from 'src/app/common/models';
import { CasesService } from 'src/app/common/services';
import { WorkflowCasesQuery } from './workflow-cases.query';
import { WorkflowCasesStore } from './workflow-cases.store';


@Injectable({ providedIn: 'root' })
export class WorkflowCasesStateService {
  constructor(
    private store: WorkflowCasesStore,
    private service: CasesService,
    private query: WorkflowCasesQuery
  ) {}

  private templateId: number;

  getPageSize(): Observable<number> {
    return this.query.selectPageSize$;
  }

  getSort(): Observable<SortModel> {
    return this.query.selectSort$;
  }

  getCases(
  ): Observable<OperationDataResult<CaseListModel>> {
    return this.service
      .getCases({
        ...this.query.pageSetting.pagination,
        ...this.query.pageSetting.filters,
        templateId: this.templateId,
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
