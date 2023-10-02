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
  @Input() workflowSteps: any; // Declare an input property

  getTaskDisplayName(taskId: number): string {
    return TaskDisplayName[taskId];
  }
}
