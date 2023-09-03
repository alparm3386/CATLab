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

  fetchData(idJob: any): void {
    this.http.get(`${this.apiUrl}/GetJobDetails?idJob` + idJob).subscribe(
      data => {
        this.dataSubject.next(data);
      },
      error => {
        console.error('Error fetching data:', error);
        this.dataSubject.error(error);
      }
    );
  }

  postData(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/GetMonitoringData`, data);
  }

  // Add more methods as needed for PUT, DELETE, etc.
}
