import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LinguistSearchComponent, LinguistSearchParam } from '../linguist-search/linguist-search.component';
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
  @Input() allocation: any;

  public linguistSearchParam?: LinguistSearchParam;

  constructor(public activeModal: NgbActiveModal) {
  }

  ngOnInit(): void {
    this.linguistSearchParam = {
      sourceLang: this.jobData.sourceLanguage,
      targetLang: this.jobData.targetLanguage,
      speciality: this.jobData.speciality,
      task: this.allocation.task
    };
  }
  close(): void {
    this.activeModal.close();
  }
}
