import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { HttpClient } from '@angular/common/http';
import * as _ from 'underscore';
import { ModalService } from '../../../../../cat-common/services/modal.service';


@Component({
  selector: 'app-linguist-allocation',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './linguist-allocation.component.html',
  styleUrls: ['./linguist-allocation.component.scss']
})
export class LinguistAllocationComponent {
  @Input() jobData: any;
  @Input() allocation: any;

  public searchTerm: string = '';
  public linguists: any[] = [];
  public filteredLinguists: any[] = [];
  public isLoading: boolean = false;
  public throttledSearch: any;

  constructor(private http: HttpClient, public activeModal: NgbActiveModal, private modalService: ModalService) {
    this.throttledSearch = _.throttle(this.searchLinguists.bind(this), 300);
  }

  ngOnInit(): void {
    this.getLinguists();
  }

  searchLinguists(): void {
    if (this.searchTerm.trim().length < 3) {
      this.linguists = [];
      return;
    }
  }

  getLinguists(): void {
    // Get the filtered list of linguists from the server
    this.isLoading = true;
    this.http.get<any[]>('/api/Common/GetFilteredLinguists', {
      params: {
        sourceLanguageId: this.jobData.sourceLanguage,
        targetLanguageId: this.jobData.targetLanguage,
        speciality: this.jobData.speciality,
        task: this.allocation.task
      }
    }).subscribe({
      next: data => {
        this.isLoading = false;
        this.linguists = data;
      },
      error: error => {
        this.isLoading = false;
        this.modalService.alert("Unable to retrieve linguists from the server. Please try again later.", "Error");
        //this.dataSubject.error(error);
      }
    });
  }


  close(): void {
    this.activeModal.close();
  }
}
