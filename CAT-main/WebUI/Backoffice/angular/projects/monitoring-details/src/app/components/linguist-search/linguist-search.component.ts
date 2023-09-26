import { Component, OnInit, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import * as _ from 'underscore';

@Component({
  selector: 'app-linguist-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
      linguist-search works!
      <div class="autocomplete-container">
        <input 
          type="text" 
          class="form-control" 
          [(ngModel)]="searchTerm" 
          (input)="throttledSearch()" 
          placeholder="Search for linguist...">

        <div *ngIf="linguists.length" class="dropdown-menu show">
          <a *ngFor="let linguist of linguists" class="dropdown-item">
            {{ linguist.User.FirstName }} {{ linguist.User.LastName }}
          </a>
        </div>
      </div>
  `,
  styleUrls: ['./linguist-search.component.scss']
})
export class LinguistSearchComponent implements OnInit {
  public searchTerm: string = '';
  public linguists: any[] = [];
  throttledSearch: any;

  constructor(private http: HttpClient) {
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
    }).subscribe(data => {
      this.linguists = data;
    });
  }
}
