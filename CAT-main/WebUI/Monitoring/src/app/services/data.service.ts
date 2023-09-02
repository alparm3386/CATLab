import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DataService {

  private apiUrl = 'https://localhost:7096/api/Monitoring';

  constructor(private http: HttpClient) { }

  fetchData(): Observable<any> {
    return this.http.get(`${this.apiUrl}/GetMonitoringData`);
  }

  postData(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/GetMonitoringData`, data);
  }

  // Add more methods as needed for PUT, DELETE, etc.
}
