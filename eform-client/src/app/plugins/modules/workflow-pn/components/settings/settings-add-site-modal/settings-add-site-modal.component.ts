import {Component, EventEmitter, Inject, OnDestroy, OnInit} from '@angular/core';
import {SiteNameDto} from 'src/app/common/models';
import {AutoUnsubscribe} from 'ngx-auto-unsubscribe';
import {Subscription} from 'rxjs';
import {WorkflowPnSettingsService} from '../../../services';
import {eqBy, prop, symmetricDifferenceWith} from 'ramda';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';

@AutoUnsubscribe()
@Component({
  selector: 'app-settings-add-site-modal',
  templateUrl: './settings-add-site-modal.component.html',
  styleUrls: ['./settings-add-site-modal.component.scss'],
})
export class SettingsAddSiteModalComponent implements OnInit, OnDestroy {
  siteAdded: EventEmitter<void> = new EventEmitter<void>();
  availableSites: any[] = [];
  selectedSiteId: number;
  addSiteSub$: Subscription;

  constructor(
    private settingsService: WorkflowPnSettingsService,
    public dialogRef: MatDialogRef<SettingsAddSiteModalComponent>,
    @Inject(MAT_DIALOG_DATA) model: { sites: SiteNameDto[], assignedSites: SiteNameDto[] }
  ) {
    // Removing assigned sites from all sites by id
    const propEqual = eqBy(prop('siteUId'));
    this.availableSites = symmetricDifferenceWith(propEqual, model.sites, model.assignedSites);
  }

  ngOnInit(): void {
  }

  hide() {
    this.dialogRef.close();
  }

  assignSite() {
    this.addSiteSub$ = this.settingsService
      .addSiteToSettings(this.selectedSiteId)
      .subscribe((data) => {
        if (data && data.success) {
          this.hide();
          this.selectedSiteId = null;
          this.availableSites = [];
          this.siteAdded.emit();
        }
      });
  }

  ngOnDestroy(): void {
  }
}
