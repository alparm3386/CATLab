import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Task, TaskDisplayName } from '../../../../../cat-common/enums/task.enum';

@Component({
  selector: 'app-workflow',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './workflow.component.html',
  styleUrls: ['./workflow.component.scss']
})
export class WorkflowComponent {
  private _workflowSteps: any[] = [];

  @Input()
  set workflowSteps(steps: any) {
    if (!steps)
      return;
    // Sort the input array when it is set
    this._workflowSteps = steps.sort((a: any, b: any) => {
      return a.stepOrder - b.stepOrder;
    });

    //filter the steps
    this._workflowSteps = this._workflowSteps.filter((step: { task: any; }) =>
      step.task !== 0 && step.task !== 100
    );
  }

  get workflowSteps(): any[] {
    return this._workflowSteps;
  }

  getDisplayNameForTask(taskId: number): string {
    return TaskDisplayName[taskId];
  }
}
