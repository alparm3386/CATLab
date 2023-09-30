import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DataService {

  private apiUrl = 'https://localhost:7096/api/Monitoring';
  private dataSubject = new BehaviorSubject<any>(null);
  public data$ = this.dataSubject.asObservable();

  constructor(private http: HttpClient) { }

  fetchData(): void {
    this.http.get(`${this.apiUrl}/GetMonitoringData`).subscribe({
      next: data => {
        this.dataSubject.next(data);
      },
      error: error => {
        console.error('Error fetching data:', error);
        // Optionally, you can push the error to a new subject or handle it differently
        // this.errorSubject.next(error);
        // But for this case, just log it without pushing it to dataSubject.
      },
      complete: () => {
        this.dataSubject.complete();
        // handle completion if necessary
      }
    });
  }


  postData(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/GetMonitoringData`, data);
  }

  // Add more methods as needed for PUT, DELETE, etc.
}
