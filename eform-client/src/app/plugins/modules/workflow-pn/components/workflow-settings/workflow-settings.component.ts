import {
  ChangeDetectorRef,
  Component,
  EventEmitter,
  OnInit,
} from '@angular/core';
import { Router } from '@angular/router';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { Subscription } from 'rxjs';
import { debounceTime, switchMap } from 'rxjs/operators';
import { TemplateListModel, TemplateRequestModel } from 'src/app/common/models';
import { EFormService } from 'src/app/common/services';
import { WorkflowPnSettingsService } from 'src/app/plugins/modules/workflow-pn/services';
import { WorkflowBaseSettingsModel } from '../../models/workflow-base-settings.model';

@AutoUnsubscribe()
@Component({
  selector: 'app-workflow-settings',
  templateUrl: './workflow-settings.component.html',
  styleUrls: ['./workflow-settings.component.scss'],
})
export class WorkflowSettingsComponent implements OnInit {
  typeahead = new EventEmitter<string>();
  settingsModel: WorkflowBaseSettingsModel = new WorkflowBaseSettingsModel();
  templatesModel: TemplateListModel = new TemplateListModel();
  templateRequestModel: TemplateRequestModel = new TemplateRequestModel();

  getSettings$: Subscription;
  updateSettings$: Subscription;

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
          this.templateRequestModel.nameFilter = term;
          return this.eFormService.getAll(this.templateRequestModel);
        })
      )
      .subscribe((items) => {
        this.templatesModel = items.model;
        this.cd.markForCheck();
      });
  }

  ngOnInit() {
    this.getSettings();
  }

  getSettings() {
    this.getSettings$ = this.workflowPnSettingsService
      .getAllSettings()
      .subscribe((data) => {
        if (data && data.success) {
          this.settingsModel = data.model;
        }
      });
  }

  updateSettings() {
    this.updateSettings$ = this.workflowPnSettingsService
      .updateSettings(this.settingsModel)
      .subscribe((data) => {});
  }
}
