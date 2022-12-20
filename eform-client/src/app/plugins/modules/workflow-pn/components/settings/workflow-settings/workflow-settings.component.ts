import {
  Component,
  EventEmitter,
  OnDestroy,
  OnInit, ViewChild,
} from '@angular/core';
import {Router} from '@angular/router';
import {AutoUnsubscribe} from 'ngx-auto-unsubscribe';
import {Subscription, take, zip} from 'rxjs';
import {CommonDictionaryModel, DeleteModalSettingModel, FolderDto, SiteNameDto} from 'src/app/common/models';
import {FoldersService, SitesService} from 'src/app/common/services';
import {WorkflowPnSettingsService} from '../../../services';
import {WorkflowSettingsModel} from '../../../models';
import {composeFolderName, dialogConfigHelper} from 'src/app/common/helpers';
import {
  SettingsAddSiteModalComponent,
  WorkflowFoldersModalComponent
} from '../../../components';
import {MatDialog} from '@angular/material/dialog';
import {Overlay} from '@angular/cdk/overlay';
import {MtxGridColumn} from '@ng-matero/extensions/grid';
import {TranslateService} from '@ngx-translate/core';
import {DeleteModalComponent} from 'src/app/common/modules/eform-shared/components';

@AutoUnsubscribe()
@Component({
  selector: 'app-workflow-settings',
  templateUrl: './workflow-settings.component.html',
  styleUrls: ['./workflow-settings.component.scss'],
})
export class WorkflowSettingsComponent implements OnInit, OnDestroy {
  @ViewChild('addSiteModal') addSiteModal: SettingsAddSiteModalComponent;
  typeahead = new EventEmitter<string>();
  settingsModel: WorkflowSettingsModel = new WorkflowSettingsModel();
  templatesModel: CommonDictionaryModel[] = new Array<CommonDictionaryModel>();

  getSettings$: Subscription;
  sites: SiteNameDto[] = [];
  foldersTreeDto: FolderDto[] = [];
  foldersDto: FolderDto[] = [];
  sitesSub$: Subscription;
  foldersSubTree$: Subscription;
  foldersSub$: Subscription;
  folderUpdateSub$: Subscription;
  folderSelectedSub$: Subscription;
  folderTaskSelectedSub$: Subscription;
  siteDeletedSub$: Subscription;
  removeSub$: Subscription;
  translatesSub$: Subscription;
  tableHeaders: MtxGridColumn[] = [
    {header: 'ID', field: 'siteUId'},
    {header: this.translateService.stream('Site name'), field: 'siteName'},
    {
      header: this.translateService.stream('Actions'),
      field: 'actions',
      type: 'button',
      buttons: [
        {
          tooltip: this.translateService.stream('Remove site'),
          click: (siteDto: SiteNameDto) => this.showRemoveSiteModal(siteDto),
          icon: 'delete',
          color: 'warn',
          type: 'icon',
          class: 'removeSiteBtn',
        }
      ],
    },
  ];
  siteSelectedSub$: Subscription;
  constructor(
    private workflowPnSettingsService: WorkflowPnSettingsService,
    private router: Router,
    private foldersService: FoldersService,
    private sitesService: SitesService,
    public dialog: MatDialog,
    private overlay: Overlay,
    private translateService: TranslateService,
  ) {
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
          this.loadAllFoldersTree();
        }
      });
  }

  ngOnDestroy(): void {
  }

  getSites() {
    this.sitesSub$ = this.sitesService.getAllSites().subscribe((data) => {
      if (data && data.success) {
        this.sites = data.model;
        this.loadFlatFolders();
      }
    });
  }

  loadAllFoldersTree() {
    this.foldersSubTree$ = this.foldersService.getAllFolders().subscribe((operation) => {
      if (operation && operation.success) {
        this.foldersTreeDto = operation.model;

        this.getSites();
      }
    });
  }

  loadFlatFolders() {
    this.foldersSub$ = this.foldersService.getAllFoldersList().subscribe((operation) => {
      if (operation && operation.success) {
        this.foldersDto = operation.model;
        this.settingsModel.folderId === null ?
          this.settingsModel.folderName = null :
          this.settingsModel.folderName = composeFolderName(this.settingsModel.folderId, this.foldersDto);
        this.settingsModel.folderTasksId === null ?
          this.settingsModel.folderTasksName = null :
          this.settingsModel.folderTasksName = this.foldersDto.find(x =>
            x.id === this.settingsModel.folderTasksId).name;
      }
    });
  }

  showAddNewSiteModal() {
    const siteAddModal = this.dialog.open(SettingsAddSiteModalComponent,
      {
        ...dialogConfigHelper(this.overlay, {sites: this.sites, assignedSites: this.settingsModel.assignedSites}),
        // minWidth: 500,
      });
    this.siteSelectedSub$ = siteAddModal.componentInstance.siteAdded.subscribe(x => this.getSettings());
  }

  showRemoveSiteModal(selectedSite: SiteNameDto) {
    this.translatesSub$ = zip(
      this.translateService.stream('Are you sure you want to remove this site'),
      this.translateService.stream('Company name'),
      this.translateService.stream('Site Uid'),
      this.translateService.stream('Remove'),
    ).subscribe(([headerText, siteName, siteUId, remove]) => {
      const settings: DeleteModalSettingModel = {
        model: selectedSite,
        settings: {
          headerText: `${headerText}?`,
          fields: [
            {header: siteName, field: 'siteName'},
            {header: siteUId, field: 'siteUId'},
          ],
          deleteButtonId: 'removeSiteSaveBtn',
          cancelButtonId: 'removeSiteSaveCancelBtn',
          deleteButtonText: remove,
        }
      }
      const removeSiteModal =
        this.dialog.open(DeleteModalComponent, {...dialogConfigHelper(this.overlay, settings), minWidth: 500});
      this.siteDeletedSub$ = removeSiteModal.componentInstance.delete
        .subscribe((selectedSite: SiteNameDto) => {
          this.removeSub$ = this.workflowPnSettingsService
            .removeSiteFromSettings(selectedSite.siteUId)
            .subscribe((data) => {
              if(data && data.success) {
                this.getSettings();
                removeSiteModal.close();
              }
            });
        });
    });
  }

  openFoldersModal() {
    const foldersModal = this.dialog.open(WorkflowFoldersModalComponent,
      {
        ...dialogConfigHelper(this.overlay, {folders: this.foldersTreeDto, selectedFolderId: this.settingsModel.folderId}),
        hasBackdrop: true,
      });
    foldersModal.backdropClick().pipe(take(1)).subscribe(_ => foldersModal.close());
    this.folderSelectedSub$ = foldersModal.componentInstance.folderSelected.subscribe(x => this.onFolderSelected(x, false));
  }

  openTasksFoldersModal() {
    const foldersModal = this.dialog.open(WorkflowFoldersModalComponent,
      {
        ...dialogConfigHelper(this.overlay, {folders: this.foldersTreeDto, selectedFolderId: this.settingsModel.folderTasksId}),
        hasBackdrop: true,
      });
    foldersModal.backdropClick().pipe(take(1)).subscribe(_ => foldersModal.close());
    this.folderTaskSelectedSub$ = foldersModal.componentInstance.folderSelected.subscribe(x => this.onFolderSelected(x, true));
  }

  onFolderSelected(folderDto: FolderDto, tasksFolder: boolean) {
    if (tasksFolder) {
      this.folderUpdateSub$ = this.workflowPnSettingsService.updateSettingsTasksFolder(folderDto.id).subscribe((operation) => {
        if (operation && operation.success) {
          this.getSettings();
        }
      });
    } else {
      this.folderUpdateSub$ = this.workflowPnSettingsService.updateSettingsFolder(folderDto.id).subscribe((operation) => {
        if (operation && operation.success) {
          this.getSettings();
        }
      });
    }
  }
}
