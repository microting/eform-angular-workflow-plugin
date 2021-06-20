import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { SiteNameDto } from 'src/app/common/models';
import { WorkflowCaseModel, WorkflowCaseUpdateModel } from '../../../models';

@Component({
  selector: 'app-workflow-case-edit-modal',
  templateUrl: './workflow-case-edit-modal.component.html',
  styleUrls: ['./workflow-case-edit-modal.component.scss'],
})
export class WorkflowCaseEditModalComponent implements OnInit {
  @Input() deviceUsersList: SiteNameDto[] = [];
  @Output()
  workflowCaseUpdated: EventEmitter<WorkflowCaseUpdateModel> = new EventEmitter<WorkflowCaseUpdateModel>();
  @ViewChild('frame', { static: true }) frame;
  workflowCaseUpdateModel: WorkflowCaseUpdateModel = new WorkflowCaseUpdateModel();

  statuses = [{ id: 1, text: 'Open' }];

  constructor() {}

  ngOnInit() {}

  show(model: WorkflowCaseModel) {
    this.workflowCaseUpdateModel = {
      id: model.id,
      status: model.status,
      toBeSolvedById: model.toBeSolvedById,
    };
    this.frame.show();
  }

  hide() {
    this.frame.hide();
  }

  updateWorkflowCase() {
    this.workflowCaseUpdated.emit(this.workflowCaseUpdateModel);
  }
}
