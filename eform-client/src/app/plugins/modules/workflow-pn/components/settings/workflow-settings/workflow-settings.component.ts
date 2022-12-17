import {
  // ChangeDetectorRef,
  Component,
  EventEmitter,
  OnDestroy,
  OnInit, ViewChild,
} from '@angular/core';
import { Router } from '@angular/router';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { Subscription } from 'rxjs';
// import { debounceTime, switchMap } from 'rxjs/operators';
import {CommonDictionaryModel, FolderDto, SiteNameDto} from 'src/app/common/models';
import {EFormService, FoldersService, SitesService} from 'src/app/common/services';
import { WorkflowPnSettingsService } from '../../../services';
import { WorkflowSettingsModel } from '../../../models';
import {composeFolderName} from 'src/app/common/helpers';
import {
  SettingsAddSiteModalComponent,
  SettingsRemoveSiteModalComponent,
  WorkflowFoldersModalComponent
} from 'src/app/plugins/modules/workflow-pn/components';

@AutoUnsubscribe()
@Component({
  selector: 'app-workflow-settings',
  templateUrl: './workflow-settings.component.html',
  styleUrls: ['./workflow-settings.component.scss'],
})
export class WorkflowSettingsComponent implements OnInit, OnDestroy {
  @ViewChild('removeSiteModal') removeSiteModal: SettingsRemoveSiteModalComponent;
  @ViewChild('addSiteModal') addSiteModal: SettingsAddSiteModalComponent;
  @ViewChild('foldersModal', {static: false}) foldersModal: WorkflowFoldersModalComponent;
  typeahead = new EventEmitter<string>();
  settingsModel: WorkflowSettingsModel = new WorkflowSettingsModel();
  templatesModel: CommonDictionaryModel[] = new Array<CommonDictionaryModel>();
  templatesModelForFirst: CommonDictionaryModel[] = new Array<CommonDictionaryModel>();
  templatesModelForSecond: CommonDictionaryModel[] = new Array<CommonDictionaryModel>();

  getSettings$: Subscription;
  sites: SiteNameDto[] = [];
  foldersTreeDto: FolderDto[] = [];
  foldersDto: FolderDto[] = [];
  settingsSub$: Subscription;
  sitesSub$: Subscription;
  foldersSubTree$: Subscription;
  foldersSub$: Subscription;
  folderUpdateSub$: Subscription;
  tasksFolder: boolean;

  constructor(
    private workflowPnSettingsService: WorkflowPnSettingsService,
    private router: Router,
    private foldersService: FoldersService,
    private sitesService: SitesService
  ) {}

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

  ngOnDestroy(): void {}

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
    this.addSiteModal.show(this.sites, this.settingsModel.assignedSites);
  }

  showRemoveSiteModal(selectedSite: SiteNameDto) {
    this.removeSiteModal.show(selectedSite);
  }

  openFoldersModal() {
    this.tasksFolder = false;
    this.foldersModal.show(this.settingsModel.folderId);
  }
  openTasksFoldersModal() {
    this.tasksFolder = true;
    this.foldersModal.show(this.settingsModel.folderTasksId);
  }

  onFolderSelected(folderDto: FolderDto) {
    if (this.tasksFolder) {
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
