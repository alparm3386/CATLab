import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LinguistAllocationComponent } from '../linguist-allocation/linguist-allocation.component';
import { ModalService } from '../../../../../cat-common/services/modal.service';


@Component({
  selector: 'app-task-allocation',
  standalone: true,
  imports: [CommonModule],
  templateUrl: `./task-allocation.component.html`,
  styleUrls: ['./task-allocation.component.scss']
})

export class TaskAllocationComponent {
  @Input() jobData: any;
  @Input() allocation: any;

  constructor(private modalService: ModalService) { }

  ngOnInit(): void {
    console.log(this.allocation);
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
    this.modalService.open(LinguistAllocationComponent, { jobData: this.jobData, allocation: this.allocation }).result.then((result) => {
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
}
