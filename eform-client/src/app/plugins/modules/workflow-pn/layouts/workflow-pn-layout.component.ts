import {AfterContentInit, Component, OnInit} from '@angular/core';
import {TranslateService} from '@ngx-translate/core';
import {translates} from './../i18n/translates';
import {Store} from '@ngrx/store';
import {take} from 'rxjs';
import {addPluginToVisited, selectPluginsVisitedPlugins} from 'src/app/state';

@Component({
  selector: 'app-workflow-pn-layout',
  template: `
    <router-outlet></router-outlet>`,
})
export class WorkflowPnLayoutComponent implements AfterContentInit, OnInit {
  private pluginName = 'workflow';

  constructor(
    private translateService: TranslateService,
    store: Store
  ) {
    store.select(selectPluginsVisitedPlugins)
      .pipe(take(1))
      .subscribe(x => {
        // check current plugin in activated plugin
        if (x.findIndex(y => y === this.pluginName) === -1) {
          // add all plugin translates one time
          Object.keys(translates).forEach(locale => {
            this.translateService.setTranslation(locale, translates[locale], true);
          });
          // add plugin to visited plugins
          store.dispatch(addPluginToVisited(this.pluginName));
        }
      });
  }

  ngOnInit() {
  }

  ngAfterContentInit() {
  }
}
