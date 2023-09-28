import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LinguistAllocationMenuComponent } from '../linguist-allocation-menu/linguist-allocation-menu.component';

@Component({
  selector: 'app-people',
  standalone: true,
  imports: [CommonModule, LinguistAllocationMenuComponent],
  templateUrl: './people.component.html',
  styleUrls: ['./people.component.scss']
})
export class PeopleComponent {
  @Input() jobData: any;
}
