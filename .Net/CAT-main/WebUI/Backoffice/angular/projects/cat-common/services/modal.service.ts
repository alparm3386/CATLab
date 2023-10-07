import { Component, Type, Injectable } from "@angular/core";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { AlertComponent } from "../components/alert/alert.component";
import { ConfirmComponent } from "../components/confirm/confirm.component";

// modal.service.ts
@Injectable({
  providedIn: 'root'
})

export class ModalService {
  open(component: Type<any>, inputs?: { [key: string]: any }) {
    const modalRef = this.modalService.open(component);

    if (inputs) {
      for (const [key, value] of Object.entries(inputs)) {
        modalRef.componentInstance[key] = value;
      }
    }
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
