import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalService } from '../../../../../cat-common/modal.service';

@Component({
  selector: 'app-linguist-allocation-menu',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="d-flex">
      <label> {{ task }}: </label> <span class="text-primary ms-2"> {{jobData.projectManager}} </span>
      <!-- Dropdown Menu -->
      <div class="dropdown ms-auto">
        <button class="btn dropdown-toggle" type="button" id="dropdownMenuButton" data-bs-toggle="dropdown" aria-expanded="false">
          ☰
        </button>
        <ul class="dropdown-menu" aria-labelledby="dropdownMenuButton">
          <li><a class="dropdown-item" href="#">Allocate linguist</a></li>
          <li><a class="dropdown-item" href="#" (click)=allocateToYorself($event)>Allocate to yourself</a></li>
          <li><a class="dropdown-item" href="#">Modify fee</a></li>
        </ul>
      </div>
    </div>
  `,
  styleUrls: ['./linguist-allocation-menu.component.scss']
})
export class LinguistAllocationMenuComponent {
  @Input() jobData: any;
  @Input() task: any;

  constructor(private modalService: ModalService) { }

  allocateToYorself(event: Event): void {
    event.preventDefault();
    this.modalService.alert("Allocate to yourself.", "Warning");
  }
}
