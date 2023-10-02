import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskAllocationComponent } from '../task-allocation/task-allocation.component';
import * as _ from 'underscore';
import { Task } from '../../../../../cat-common/enums/task.enum';

@Component({
  selector: 'app-people',
  standalone: true,
  imports: [CommonModule, TaskAllocationComponent],
  templateUrl: './people.component.html',
  styleUrls: ['./people.component.scss']
})
export class PeopleComponent {
  @Input() jobData: any;
  TaskEnum = Task;  // Add this line

  hasTask(task: any): boolean {
    const hasTask = _.any(this.jobData.workflowSteps, workflowStep => {
      return workflowStep.task == task;
    });

    return hasTask;
  }
}
