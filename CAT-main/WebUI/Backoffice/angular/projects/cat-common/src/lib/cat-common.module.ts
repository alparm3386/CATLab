import { NgModule } from '@angular/core';
import { CatCommonComponent } from './cat-common.component';
import { AlertComponent } from './components/alert/alert.component';
import { ConfirmComponent } from './components/confirm/confirm.component';



@NgModule({
  declarations: [
    CatCommonComponent,
    AlertComponent,
    ConfirmComponent
  ],
  imports: [
  ],
  exports: [
    CatCommonComponent
  ]
})
export class CatCommonModule { }
