import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LinguistDeallocationComponent } from './linguist-deallocation.component';

describe('LinguistDeallocationComponent', () => {
  let component: LinguistDeallocationComponent;
  let fixture: ComponentFixture<LinguistDeallocationComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [LinguistDeallocationComponent]
    });
    fixture = TestBed.createComponent(LinguistDeallocationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
