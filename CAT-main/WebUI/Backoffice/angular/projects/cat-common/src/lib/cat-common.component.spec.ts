import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CatCommonComponent } from './cat-common.component';

describe('CatCommonComponent', () => {
  let component: CatCommonComponent;
  let fixture: ComponentFixture<CatCommonComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [CatCommonComponent]
    });
    fixture = TestBed.createComponent(CatCommonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
