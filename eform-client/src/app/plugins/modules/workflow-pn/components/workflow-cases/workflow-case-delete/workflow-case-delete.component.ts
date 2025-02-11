import {
  Component,
  EventEmitter,
  Inject,
  OnDestroy,
  OnInit,
} from '@angular/core';
import {AutoUnsubscribe} from 'ngx-auto-unsubscribe';
import {Subscription} from 'rxjs';
import {WorkflowCaseModel} from '../../../models';
import {WorkflowPnCasesService} from '../../../services';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';

@AutoUnsubscribe()
@Component({
    selector: 'app-workflow-case-delete',
    templateUrl: './workflow-case-delete.component.html',
    styleUrls: ['./workflow-case-delete.component.scss'],
    standalone: false
})
export class WorkflowCaseDeleteComponent implements OnInit, OnDestroy {
  workflowCaseDeleted: EventEmitter<void> = new EventEmitter<void>();
  deleteWorkflowCase$: Subscription;

  constructor(
    private workflowCaseService: WorkflowPnCasesService,
    public dialogRef: MatDialogRef<WorkflowCaseDeleteComponent>,
    @Inject(MAT_DIALOG_DATA) public workflowCaseModel: WorkflowCaseModel = new WorkflowCaseModel(),
  ) {
  }

  ngOnInit() {
  }

  deleteWorkflowCase() {
    this.deleteWorkflowCase$ = this.workflowCaseService
      .deleteWorkflowCase(this.workflowCaseModel.id)
      .subscribe((data) => {
        if (data && data.success) {
          this.workflowCaseDeleted.emit();
          this.hide();
        }
      });
  }

  hide() {
    this.dialogRef.close();
  }

  ngOnDestroy(): void {
  }
}
