import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CatCommonModule } from 'cat-common';


@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './documents.component.html',
  styleUrls: ['./documents.component.scss']
})
export class DocumentsComponent {
  @Input() jobData: any;

  rectifyOriginalDocument(): void {
    alert("rectifyOriginalDocument");
    // Your function logic here
    console.log('Rectify original document clicked.');
  }
}
