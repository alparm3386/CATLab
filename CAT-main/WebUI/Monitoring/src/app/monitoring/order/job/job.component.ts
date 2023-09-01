import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WorkflowComponent } from './workflow/workflow.component';


@Component({
  selector: 'job',
  standalone: true,
  imports: [CommonModule, WorkflowComponent],
  templateUrl: './job.component.html',
  styleUrls: ['./job.component.scss']
})
export class JobComponent {

}
