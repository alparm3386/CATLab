import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LinguistSearchComponent } from '../linguist-search/linguist-search.component';
import { LinguistAllocationComponent } from '../linguist-allocation/linguist-allocation.component';

@Component({
  selector: 'app-people',
  standalone: true,
  imports: [CommonModule, LinguistSearchComponent, LinguistAllocationComponent],
  templateUrl: './people.component.html',
  styleUrls: ['./people.component.scss']
})
export class PeopleComponent {
  @Input() jobData: any;
}
