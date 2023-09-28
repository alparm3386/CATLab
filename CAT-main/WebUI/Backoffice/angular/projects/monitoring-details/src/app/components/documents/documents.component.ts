import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AlertComponent } from '../../../../../cat-common/alert/alert.component';
import { ModalService } from '../../../../../cat-common/modal.service';
import { AnalysisComponent } from '../analysis/analysis.component';


@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [CommonModule, AnalysisComponent],
  templateUrl: './documents.component.html',
  styleUrls: ['./documents.component.scss']
})
export class DocumentsComponent {
  @Input() jobData: any;

  constructor(private modalService: ModalService) { }

  rectifyOriginalDocument(event: Event): void {
    event.preventDefault();
    //this.modalService.alert("<b>My alert</b>");
    //return;
    const modalRef = this.modalService.confirm("<b>Are you sure?</b>").result.then((result) => {
      if (result) {
        // Handle the confirmation (i.e., when user clicked "OK")
        console.log('User confirmed');
      } else {
        // Handle the cancellation (i.e., when user clicked "Cancel" or closed the modal)
        console.log('User cancelled');
      }
    },
      (reason) => {
        // Handle the modal dismissal reason. (e.g., click outside, ESC key, etc.)
        console.log('Dismissed with:', reason);
      });
    // Your function logic here
    console.log('Rectify original document clicked.');
  }

  uploadDocument(event: Event): void {
    event.preventDefault();
    alert("uploadDocument");
  }


  showAnalysis(event: Event): void {
    event.preventDefault();

    this.modalService.open(AnalysisComponent, { jobData: this.jobData });
  }
}
