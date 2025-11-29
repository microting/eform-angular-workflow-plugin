import {
  Component,
  EventEmitter,
  OnDestroy,
  OnInit,
  inject
} from '@angular/core';
import { AuthService, FoldersService} from 'src/app/common/services';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { FolderDto } from 'src/app/common/models';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';

@AutoUnsubscribe()
@Component({
    selector: 'app-workflow-folders-modal',
    templateUrl: './workflow-folders-modal.component.html',
    styleUrls: ['./workflow-folders-modal.component.scss'],
    standalone: false
})
export class WorkflowFoldersModalComponent implements OnInit, OnDestroy {
  private folderService = inject(FoldersService);
  private authService = inject(AuthService);
  private model = inject<{folders: FolderDto[], selectedFolderId?: number}>(MAT_DIALOG_DATA);
  public dialogRef = inject(MatDialogRef<WorkflowFoldersModalComponent>);

  folderSelected: EventEmitter<FolderDto> = new EventEmitter<FolderDto>();
  folders: FolderDto[] = [];
  selectedFolderId: number;

  constructor() {
    this.folders = this.model.folders;
    this.selectedFolderId = this.model.selectedFolderId;
  }

  ngOnInit() {
  }

  select(folder: FolderDto) {
    this.folderSelected.emit(folder);
    this.dialogRef.close();
  }


  ngOnDestroy(): void {
  }
}
