import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LinguistAllocationComponent } from '../linguist-allocation/linguist-allocation.component';
import { ModalService } from '../../../../../cat-common/services/modal.service';
import { TaskDisplayName } from '../../../../../cat-common/enums/task.enum';


@Component({
  selector: 'app-task-allocation',
  standalone: true,
  imports: [CommonModule],
  templateUrl: `./task-allocation.component.html`,
  styleUrls: ['./task-allocation.component.scss']
})

export class TaskAllocationComponent {
  @Input() jobData: any;
  @Input() task: any;

  constructor(private modalService: ModalService) { }

  ngOnInit(): void {
    console.log(this.task);
  }

  allocateToYorself(event: Event): void {
    event.preventDefault();
    this.modalService.confirm("Are you sure that you want to allocate this job to yourself?", "Confirm").result.then((result) => {
      if (result) {
        console.log('The user was sure.');
      }
    });
  }

  allocateLinguist(event: Event): void {
    event.preventDefault();
    this.modalService.open(LinguistAllocationComponent, { jobData: this.jobData, task: this.task }).result.then((result) => {
      if (result) {
        console.log('The user was sure.');
      }
    },
      (reason) => {
        // Handle the modal dismissal reason. (e.g., click outside, ESC key, etc.)
        console.log('Dismissed with:', reason);
      });
  }

  modifyFee(event: Event): void {
    event.preventDefault();
    this.modalService.confirm("Modify fee", "Confirm").result.then((result) => {
      if (result) {
        console.log('The user was sure.');
      }
    });
  }

  getTaskDisplayName(taskId: number): string {
    return TaskDisplayName[taskId];
  }
}
