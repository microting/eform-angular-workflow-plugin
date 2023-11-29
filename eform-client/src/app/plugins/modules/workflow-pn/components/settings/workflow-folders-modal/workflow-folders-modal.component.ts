import {
  Component,
  EventEmitter,
  Inject,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { AuthService, FoldersService} from 'src/app/common/services';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { FolderDto } from 'src/app/common/models';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';

@AutoUnsubscribe()
@Component({
  selector: 'app-workflow-folders-modal',
  templateUrl: './workflow-folders-modal.component.html',
  styleUrls: ['./workflow-folders-modal.component.scss']
})
export class WorkflowFoldersModalComponent implements OnInit, OnDestroy {
  folderSelected: EventEmitter<FolderDto> = new EventEmitter<FolderDto>();
  folders: FolderDto[] = [];
  selectedFolderId: number;

  constructor(
    private folderService: FoldersService,
    private authService: AuthService,
    @Inject(MAT_DIALOG_DATA) model: {folders: FolderDto[], selectedFolderId?: number},
    public dialogRef: MatDialogRef<WorkflowFoldersModalComponent>,
    ) {
    this.folders = model.folders;
    this.selectedFolderId = model.selectedFolderId;
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
