import { Injectable, Renderer2, RendererFactory2 } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SpinnerService {

  private renderer: Renderer2;
  private spinnerElement?: HTMLElement;

  constructor(private rendererFactory: RendererFactory2) {
    this.renderer = this.rendererFactory.createRenderer(null, null);
  }

  show() {
    this.spinnerElement = this.renderer.createElement('div');

    // Apply styles and classes for the spinner here
    this.renderer.addClass(this.spinnerElement, 'spinner-border');
    this.renderer.setStyle(this.spinnerElement, 'position', 'fixed');
    this.renderer.setStyle(this.spinnerElement, 'top', '50%');
    this.renderer.setStyle(this.spinnerElement, 'left', '50%');
    this.renderer.setStyle(this.spinnerElement, 'z-index', '1000');
    // ... (add other necessary styles or classes)

    this.renderer.appendChild(document.body, this.spinnerElement);
  }

  hide() {
    if (this.spinnerElement) {
      this.renderer.removeChild(document.body, this.spinnerElement);
    }
  }
}
