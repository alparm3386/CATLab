// confirm.component.ts
import { Component, Input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-confirm',
  templateUrl: './confirm.component.html'
})
export class ConfirmComponent {

  @Input() message!: string;

  constructor(public activeModal: NgbActiveModal) { }

  confirm(): void {
    this.activeModal.close(true);  // when confirmed, return true
  }

  cancel(): void {
    this.activeModal.close(false);  // when cancelled, return false
  }
}
