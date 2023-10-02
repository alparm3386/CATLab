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

  getJobData(jobId: any): void {
    this.http.get(`${this.apiUrl}/GetJobData?jobId=` + jobId).subscribe({
      next: data => {
        this.dataSubject.next(data);
      },
      error: error => {
        this.dataSubject.error(error);
        //console.error('Error fetching data:', error);
        //this.dataSubject.complete();
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
    return this.http.post(`${this.apiUrl}/GetJobData`, data);
  }

  getLinguists(searchParams: any): Observable<any[]> {
    // Get the filtered list of linguists from the server
    return this.http.get<any[]>('/api/Common/GetLinguists', {
      params: searchParams
    });
  }
}
