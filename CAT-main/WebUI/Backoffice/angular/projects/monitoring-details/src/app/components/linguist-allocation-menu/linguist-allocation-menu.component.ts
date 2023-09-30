import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LinguistAllocationComponent } from '../linguist-allocation/linguist-allocation.component';
import { ModalService } from '../../../../../cat-common/services/modal.service';


@Component({
  selector: 'app-linguist-allocation-menu',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="d-flex">
      <label> {{ allocation.description }}: </label> <span class="text-primary ms-2"> {{jobData.projectManager}} </span>
      <!-- Dropdown Menu -->
      <div class="dropdown ms-auto">
        <button class="btn dropdown-toggle" type="button" id="dropdownMenuButton" data-bs-toggle="dropdown" aria-expanded="false">
          ☰
        </button>
        <ul class="dropdown-menu" aria-labelledby="dropdownMenuButton">
          <li><a class="dropdown-item" href="#" (click)=allocateLinguist($event)>Allocate linguist</a></li>
          <li><a class="dropdown-item" href="#" (click)=allocateToYorself($event)>Allocate to yourself</a></li>
          <li><a class="dropdown-item" href="#" (click)=modifyFee($event)>Modify fee</a></li>
        </ul>
      </div>
    </div>
  `,
  styleUrls: ['./linguist-allocation-menu.component.scss']
})

export class LinguistAllocationMenuComponent {
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
