import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskAllocationComponent } from '../task-allocation/task-allocation.component';

@Component({
  selector: 'app-people',
  standalone: true,
  imports: [CommonModule, TaskAllocationComponent],
  templateUrl: './people.component.html',
  styleUrls: ['./people.component.scss']
})
export class PeopleComponent {
  @Input() jobData: any;
}
