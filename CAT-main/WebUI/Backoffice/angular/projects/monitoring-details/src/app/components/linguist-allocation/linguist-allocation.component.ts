import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LinguistSearchComponent } from '../linguist-search/linguist-search.component';


@Component({
  selector: 'app-linguist-allocation',
  standalone: true,
  imports: [CommonModule, LinguistSearchComponent],
  templateUrl: './linguist-allocation.component.html',
  styleUrls: ['./linguist-allocation.component.scss']
})
export class LinguistAllocationComponent {

}
