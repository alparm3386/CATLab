// confirm.component.ts
import { Component, Input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-confirm',
  templateUrl: './confirm.component.html'
})
export class ConfirmComponent {

  @Input() message!: string;
  @Input() title!: string;

  constructor(public activeModal: NgbActiveModal, private sanitizer: DomSanitizer) { }

  get safeHtml(): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(this.message);
  }

  confirm(): void {
    this.activeModal.close(true);  // when confirmed, return true
  }

  cancel(): void {
    this.activeModal.close(false);  // when cancelled, return false
  }
}
