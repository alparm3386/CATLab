import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { HttpClient } from '@angular/common/http';
import * as _ from 'underscore';
import { ModalService } from '../../../../../cat-common/services/modal.service';
import { DataService } from '../../services/data.service';


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

  constructor(private dataService: DataService, public activeModal: NgbActiveModal, private modalService: ModalService) {
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
    const searchParams = {
      sourceLanguageId: this.jobData.sourceLanguage,
      targetLanguageId: this.jobData.targetLanguage,
      speciality: this.jobData.speciality,
      task: this.allocation.task
    }

    // Get the list of linguists from the server
    this.isLoading = true;
    this.dataService.getLinguists(searchParams).subscribe({
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

  allocateLinguist(linguist: any): void {
    this.modalService.confirm("Are you sure that you want to allocate " + linguist.user.fullName + "?",
      "Allocate " + this.allocation.description)
      .result.then((result) => {
        if (result) {
          console.log(result);
        }
        console.log(result);
        this.activeModal.close();
      });
  }

  close(): void {
    this.activeModal.close();
  }
}
