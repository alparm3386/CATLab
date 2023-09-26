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
          (input)="searchLinguists()" 
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

  constructor(private http: HttpClient) { }

  ngOnInit(): void { }

  searchLinguists() {
    if (!this.searchTerm.trim()) {
      this.linguists = [];
      return;
    }

    // Get the filtered list of linguists from the server
    this.http.get<any[]>('/api/GetFilteredLinguists', {
      params: {
        term: this.searchTerm,
        // Optionally, you can add the 'limit' parameter here.
      }
    }).pipe(
      catchError(error => {
        console.log('Error occurred:', error);
        return throwError(error); // This re-throws the error so you can handle it further downstream if needed.
      })
    ).subscribe(data => {
      this.linguists = data;
    });
  }
}
