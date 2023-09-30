import { Component, OnInit, NgModule, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import * as _ from 'underscore';
import { ModalService } from '../../../../../cat-common/services/modal.service';

export interface LinguistSearchParam {
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

  @Input() linguistSearchParam?: LinguistSearchParam;

  public searchTerm: string = '';
  public linguists: any[] = [];
  public throttledSearch: any;
  public isLoading: boolean = false;

  constructor(private http: HttpClient, private modalService: ModalService) {
    this.throttledSearch = _.throttle(this.searchLinguists, 300);
  }

  ngOnInit(): void {
    this.getLinguists();
  }

  searchLinguists() {
    if (this.searchTerm.trim().length < 3) {
      this.linguists = [];
      return;
    }
  }

  getLinguists() {
    if (!this.linguistSearchParam)
      return;
    // Get the filtered list of linguists from the server
    this.isLoading = true;
    this.http.get<any[]>('/api/Common/GetFilteredLinguists', {
      params: {
        sourceLanguageId: this.linguistSearchParam!.sourceLang,
        targetLanguageId: this.linguistSearchParam!.targetLang,
        speciality: this.linguistSearchParam!.speciality,
        task: this.linguistSearchParam!.task
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
}
