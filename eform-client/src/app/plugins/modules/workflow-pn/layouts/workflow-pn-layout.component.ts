import { AfterContentInit, Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { translates } from './../i18n/translates';
import { AuthStateService } from 'src/app/common/store';
import {Store} from '@ngrx/store';
import {selectCurrentUserLocale} from 'src/app/state/auth/auth.selector';

@Component({
  selector: 'app-workflow-pn-layout',
  template: ` <router-outlet></router-outlet>`,
})
export class WorkflowPnLayoutComponent implements AfterContentInit, OnInit {
  private selectCurrentUserLocale$ = this.store.select(selectCurrentUserLocale);
  constructor(
    private store: Store,
    private translateService: TranslateService,
    private authStateService: AuthStateService
  ) {}

  ngOnInit() {}

  ngAfterContentInit() {
    this.selectCurrentUserLocale$.subscribe((locale) => {
      const i18n = translates[locale];
      this.translateService.setTranslation(locale, i18n, true);
    });
  }
}
