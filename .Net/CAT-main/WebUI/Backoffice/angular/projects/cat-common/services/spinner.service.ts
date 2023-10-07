import { Injectable, ComponentFactoryResolver, ApplicationRef, Injector, EmbeddedViewRef } from '@angular/core';
import { SpinnerComponent } from '../components/spinner/spinner.component';

@Injectable({
  providedIn: 'root'
})
export class SpinnerService {
 
  private spinnerComponentRef: any;
  private isShowing = false;

  constructor(
    private componentFactoryResolver: ComponentFactoryResolver,
    private appRef: ApplicationRef,
    private injector: Injector
  ) { }

  show() {
    if (this.isShowing)
      return;
    const factory = this.componentFactoryResolver.resolveComponentFactory(SpinnerComponent);
    this.spinnerComponentRef = factory.create(this.injector);
    this.appRef.attachView(this.spinnerComponentRef.hostView);
    const domElem = (this.spinnerComponentRef.hostView as EmbeddedViewRef<any>).rootNodes[0] as HTMLElement;
    document.body.appendChild(domElem);
    this.isShowing = true;
  }

  hide() {
    this.isShowing = false;
    if (this.spinnerComponentRef) {
      this.appRef.detachView(this.spinnerComponentRef.hostView);
      this.spinnerComponentRef.destroy();
    }
  }
}
