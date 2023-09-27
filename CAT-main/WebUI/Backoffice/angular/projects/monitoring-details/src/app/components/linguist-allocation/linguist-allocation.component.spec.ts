import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LinguistAllocationComponent } from './linguist-allocation.component';

describe('LinguistAllocationComponent', () => {
  let component: LinguistAllocationComponent;
  let fixture: ComponentFixture<LinguistAllocationComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [LinguistAllocationComponent]
    });
    fixture = TestBed.createComponent(LinguistAllocationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
