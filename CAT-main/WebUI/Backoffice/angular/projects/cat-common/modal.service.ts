import { ComponentRef, Injectable } from "@angular/core";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { AlertComponent } from './alert/alert.component';

// modal.service.ts
@Injectable({
  providedIn: 'root'
})

export class ModalService {
  constructor(private modalService: NgbModal) { }

  public show(message: string) {
    const modalRef = this.modalService.open(AlertComponent);
    modalRef.componentInstance.message = message;
  }
}
