import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-workflow',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './workflow.component.html',
  styleUrls: ['./workflow.component.scss']
})
export class WorkflowComponent {
  private _workflowSteps: any; // Private variable to store the input data

  @Input()
  set workflowSteps(steps: any) {
    // Sort the input array when it is set
    this._workflowSteps = steps.sort((a: any, b: any) => {
      return a.stepOrder - b.stepOrder;
    });
  }

  get workflowSteps(): any[] {
    return this._workflowSteps;
  }

}
