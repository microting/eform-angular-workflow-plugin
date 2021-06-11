import {
  ChangeDetectorRef,
  Component,
  EventEmitter,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { Router } from '@angular/router';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { Subscription } from 'rxjs';
import { debounceTime, switchMap } from 'rxjs/operators';
import { CommonDictionaryModel } from 'src/app/common/models';
import { EFormService } from 'src/app/common/services';
import { WorkflowPnSettingsService } from '../../services';
import { WorkflowBaseSettingsModel } from '../../models';

@AutoUnsubscribe()
@Component({
  selector: 'app-workflow-settings',
  templateUrl: './workflow-settings.component.html',
  styleUrls: ['./workflow-settings.component.scss'],
})
export class WorkflowSettingsComponent implements OnInit, OnDestroy {
  typeahead = new EventEmitter<string>();
  settingsModel: WorkflowBaseSettingsModel = new WorkflowBaseSettingsModel();
  templatesModel: CommonDictionaryModel[] = new Array<CommonDictionaryModel>();

  getSettings$: Subscription;
  updateSettings$: Subscription;
  getDictionaryTemplates$: Subscription;

  constructor(
    private workflowPnSettingsService: WorkflowPnSettingsService,
    private router: Router,
    private eFormService: EFormService,
    private cd: ChangeDetectorRef
  ) {
    this.typeahead
      .pipe(
        debounceTime(200),
        switchMap((term) => {
          return this.eFormService.getTemplatesDictionary(term);
        })
      )
      .subscribe((data) => {
        if (data && data.success && data.model) {
          this.templatesModel = data.model;
          this.cd.markForCheck();
        }
      });
  }

  ngOnInit() {
    this.getSettings();
  }

  templatesModelForFirst() {
    return this.templatesModel.filter(
      (x) => x.id !== this.settingsModel.secondEformId
    );
  }

  templatesModelForSecond() {
    return this.templatesModel.filter(
      (x) => x.id !== this.settingsModel.firstEformId
    );
  }

  getSettings() {
    this.getSettings$ = this.workflowPnSettingsService
      .getAllSettings()
      .subscribe((data) => {
        if (data && data.success) {
          this.settingsModel = data.model;
          this.getEforms();
        }
      });
  }

  updateSettings() {
    this.updateSettings$ = this.workflowPnSettingsService
      .updateSettings(this.settingsModel)
      .subscribe((data) => {});
  }

  ngOnDestroy(): void {}

  private getEforms() {
    this.getDictionaryTemplates$ = this.eFormService
      .getTemplatesDictionary('')
      .subscribe((data) => (this.templatesModel = [...data.model]));
  }
}
