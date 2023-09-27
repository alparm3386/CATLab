import { TestBed } from '@angular/core/testing';

import { CatCommonService } from './cat-common.service';

describe('CatCommonService', () => {
  let service: CatCommonService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CatCommonService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
