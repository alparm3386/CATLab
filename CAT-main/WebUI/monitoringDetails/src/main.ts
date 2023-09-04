/*
*  Protractor support is deprecated in Angular.
*  Protractor is used in this example for compatibility with Angular documentation tools.
*/
import { bootstrapApplication,provideProtractorTestingSupport } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { HttpClientModule } from "@angular/common/http";
import { importProvidersFrom } from "@angular/core";
import { routes } from './app/app.routes';
import { provideRouter } from '@angular/router';

bootstrapApplication(AppComponent,
  { providers: [importProvidersFrom(HttpClientModule), provideProtractorTestingSupport()]})
  .catch(err => console.error(err));
