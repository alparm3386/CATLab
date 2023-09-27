import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AlertComponent } from '../../../../../cat-common/src/lib/components/alert/alert.component';


@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './documents.component.html',
  styleUrls: ['./documents.component.scss']
})
export class DocumentsComponent {
  @Input() jobData: any;

  constructor(private modalService: NgbModal) { }

  showAlert(message: string): void {
    const modalRef = this.modalService.open(AlertComponent);
    modalRef.componentInstance.message = message;
  }

  rectifyOriginalDocument(event: Event): void {
    event.preventDefault();
    alert("rectifyOriginalDocument");
    this.showAlert('This is an alert message.');
    // Your function logic here
    console.log('Rectify original document clicked.');
  }
}
