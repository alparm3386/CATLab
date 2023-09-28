import { Component, Type, Injectable } from "@angular/core";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { AlertComponent } from './alert/alert.component';
import { ConfirmComponent } from './confirm/confirm.component';

// modal.service.ts
@Injectable({
  providedIn: 'root'
})

export class ModalService {
  open(component: Type<any>) {
    const modalRef = this.modalService.open(component);

    return modalRef;
  }
  constructor(private modalService: NgbModal) { }

  public alert(message: string, title: string = "Alert") {
    const modalRef = this.modalService.open(AlertComponent);
    modalRef.componentInstance.message = message;
    modalRef.componentInstance.title = title;

    return modalRef;
  }

  public confirm(message: string, title: string = "Confirm") {
    const modalRef = this.modalService.open(ConfirmComponent);
    modalRef.componentInstance.message = message;
    modalRef.componentInstance.title = title;

    return modalRef;
  }
}
