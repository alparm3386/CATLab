import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppComponent } from './app.component';
import { CatCommonModule } from 'cat-common';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule, CatCommonModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
