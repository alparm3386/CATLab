import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LinguistSearchComponent } from '../linguist-search/linguist-search.component';

@Component({
  selector: 'app-people',
  standalone: true,
  imports: [CommonModule, LinguistSearchComponent],
  template: `
    <div>
      <div class="btn btn-danger btn-lg">
        <label>Project manager: </label> {{jobData.projectManager}}
      </div>
      people works!
      <app-linguist-search></app-linguist-search>
    </div>
  `,
  styleUrls: ['./people.component.scss']
})
export class PeopleComponent {
  @Input() jobData: any;
}
