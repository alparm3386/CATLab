import { Component, OnInit, NgModule, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import * as _ from 'underscore';
import { ModalService } from '../../../../../cat-common/services/modal.service';

export interface LinguistDetails {
  sourceLang: number;
  targetLang: number;
  speciality: number;
  task: number;
}

@Component({
  selector: 'app-linguist-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './linguist-search.component.html',
  styleUrls: ['./linguist-search.component.scss']
})
export class LinguistSearchComponent implements OnInit {

  @Input() linguistDetails?: LinguistDetails;

  public searchTerm: string = '';
  public linguists: any[] = [];
  throttledSearch: any;

  constructor(private http: HttpClient, private modalService: ModalService) {
    this.throttledSearch = _.throttle(this.searchLinguists, 300);
  }

  ngOnInit(): void { }


  searchLinguists() {
    if (this.searchTerm.trim().length < 3) {
      this.linguists = [];
      return;
    }

    // Get the filtered list of linguists from the server
    this.http.get<any[]>('/api/Common/GetFilteredLinguists', {
      params: {
        term: this.searchTerm,
        // Optionally, you can add the 'limit' parameter here.
      }
    }).subscribe({
        next: data => {
            this.linguists = data;
          },
      error: error => {
        this.modalService.alert("Unable to retrieve linguists from the server. Please try again later.", "Error");
          //this.dataSubject.error(error);
        }
      });
  }
}
