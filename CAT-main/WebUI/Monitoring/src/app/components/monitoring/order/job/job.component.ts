import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WorkflowComponent } from './workflow/workflow.component';


@Component({
  selector: 'app-job',
  standalone: true,
  imports: [CommonModule, WorkflowComponent],
  templateUrl: './job.component.html',
  styleUrls: ['./job.component.scss']
})
export class JobComponent {
  @Input() job: any; // Declare an input property

}
