import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-linguist-allocation',
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
          <li><a class="dropdown-item" href="#">Allocate to yourself</a></li>
          <li><a class="dropdown-item" href="#">Modify fee</a></li>
        </ul>
      </div>
    </div>
  `,
  styleUrls: ['./linguist-allocation.component.scss']
})
export class LinguistAllocationComponent {
  @Input() jobData: any;
  @Input() task: any;
}
