import {
  Component,
  EventEmitter,
  OnDestroy,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { AutoUnsubscribe } from 'ngx-auto-unsubscribe';
import { Subscription } from 'rxjs';
import { WorkflowCaseModel } from '../../../models/workflow-case.model';
import { WorkflowPnCasesService } from '../../../services';

@AutoUnsubscribe()
@Component({
  selector: 'app-workflow-case-delete',
  templateUrl: './workflow-case-delete.component.html',
  styleUrls: ['./workflow-case-delete.component.scss'],
})
export class WorkflowCaseDeleteComponent implements OnInit, OnDestroy {
  @ViewChild('frame', { static: false }) frame;
  @Output() workflowCaseDeleted: EventEmitter<void> = new EventEmitter<void>();
  workflowCaseModel: WorkflowCaseModel = new WorkflowCaseModel();

  deleteWorkflowCase$: Subscription;

  constructor(private workflowCaseService: WorkflowPnCasesService) {}

  ngOnInit() {}

  show(model: WorkflowCaseModel) {
    this.workflowCaseModel = model;
    this.frame.show();
  }

  deletePlanning() {
    this.deleteWorkflowCase$ = this.workflowCaseService
      .deleteWorkflowCase(this.workflowCaseModel.id)
      .subscribe((data) => {
        if (data && data.success) {
          this.workflowCaseDeleted.emit();
          this.frame.hide();
        }
      });
  }

  ngOnDestroy(): void {}
}
