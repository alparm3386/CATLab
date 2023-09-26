import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LinguistSearchComponent } from './linguist-search.component';

describe('LinguistSearchComponent', () => {
  let component: LinguistSearchComponent;
  let fixture: ComponentFixture<LinguistSearchComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [LinguistSearchComponent]
    });
    fixture = TestBed.createComponent(LinguistSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
