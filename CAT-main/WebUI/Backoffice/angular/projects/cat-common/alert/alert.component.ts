import { Component, Input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser'

@Component({
  selector: 'app-alert',
  templateUrl: './alert.component.html'
})
export class AlertComponent {

  @Input() message!: string;

  constructor(public activeModal: NgbActiveModal, private sanitizer: DomSanitizer) { }

  get safeHtml(): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(this.message);
  }

  close(): void {
    this.activeModal.close();
  }
}
