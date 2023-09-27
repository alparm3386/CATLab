import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AlertComponent } from '../../../../../cat-common/alert/alert.component';
import { ModalService } from '../../../../../cat-common/modal.service';


@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './documents.component.html',
  styleUrls: ['./documents.component.scss']
})
export class DocumentsComponent {
  @Input() jobData: any;

  constructor(private modalService: ModalService) { }

  showAlert(message: string): void {
    const modalRef = this.modalService.show("aaaa");
  }

  rectifyOriginalDocument(event: Event): void {
    event.preventDefault();
    alert("rectifyOriginalDocument");
    this.showAlert('This is an alert message.');
    // Your function logic here
    console.log('Rectify original document clicked.');
  }
}
