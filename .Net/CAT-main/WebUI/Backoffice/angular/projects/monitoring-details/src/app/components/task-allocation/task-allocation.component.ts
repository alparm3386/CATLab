import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { LinguistAllocationComponent } from '../linguist-allocation/linguist-allocation.component';
import { ModalService } from '../../../../../cat-common/services/modal.service';
import { TaskDisplayName } from '../../../../../cat-common/enums/task.enum';
import { DataService } from '../../services/data.service';
import * as _ from 'underscore';
import { LinguistDeallocationComponent } from '../linguist-deallocation/linguist-deallocation.component';
import { SpinnerService } from '../../../../../cat-common/services/spinner.service';
import { timeout } from 'rxjs';


@Component({
  selector: 'app-task-allocation',
  standalone: true,
  imports: [CommonModule, FormsModule],
  providers: [ NgbActiveModal ],
  templateUrl: `./task-allocation.component.html`,
  styleUrls: ['./task-allocation.component.scss']
})

export class TaskAllocationComponent {
  @Input() jobData: any;
  @Input() task: any;

  public allocation: any;

  constructor(private dataService: DataService, public activeModal: NgbActiveModal, private modalService: ModalService,
    private spinnerService: SpinnerService) {
  }

  ngOnInit(): void {
    this.allocation = _.find(this.jobData.allocations, allocation => {
      return allocation.taskId == this.task && !allocation.returnUnsatisfactory;
    });
  }

  allocateToYorself(event: Event): void {
    event.preventDefault();
    this.modalService.confirm("Are you sure that you want to allocate this job to yourself?", "Confirm").result.then((result) => {
      if (result) {
        this.spinnerService.show();
        //setTimeout((param: string) => { alert(param); this.spinnerService.hide(); }, 1000, "John");
        this.dataService.allocateJob(this.jobData.jobId, this.task, '0').subscribe({
          next: data => {
            this.spinnerService.hide();
            this.modalService.alert(`The job #${this.jobData.jobId} is allocated to you.`, "Allocation").result.then(() => {
              // Refresh the whole page
              window.location.reload();
            });
            this.activeModal.close();
          },
          error: error => {
            this.spinnerService.hide();
            this.modalService.alert(`There was an error allocating the job #${this.jobData.jobId} to you.`, "Error");
          }
        });
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

  deallocateLinguist(event: Event): void {
    event.preventDefault();
    this.modalService.open(LinguistDeallocationComponent, { jobData: this.jobData, task: this.task }).result.then((result) => {
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

  getDisplayNameForTask(taskId: number): string {
    return TaskDisplayName[taskId];
  }
}
