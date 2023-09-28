import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LinguistSearchComponent } from '../linguist-search/linguist-search.component';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';


@Component({
  selector: 'app-linguist-allocation',
  standalone: true,
  imports: [CommonModule, LinguistSearchComponent],
  templateUrl: './linguist-allocation.component.html',
  styleUrls: ['./linguist-allocation.component.scss']
})
export class LinguistAllocationComponent {
  @Input() jobData: any;
  @Input() task: any;

  constructor(public activeModal: NgbActiveModal) { }

  close(): void {
    this.activeModal.close();
  }
}
