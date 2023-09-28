import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LinguistAllocationMenuComponent } from './linguist-allocation-menu.component';

describe('LinguistAllocationMenuComponent', () => {
  let component: LinguistAllocationMenuComponent;
  let fixture: ComponentFixture<LinguistAllocationMenuComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [LinguistAllocationMenuComponent]
    });
    fixture = TestBed.createComponent(LinguistAllocationMenuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
