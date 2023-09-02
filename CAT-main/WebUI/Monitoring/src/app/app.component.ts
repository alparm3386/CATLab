import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MonitoringComponent } from './components/monitoring/monitoring.component';
import { DataService } from './services/data.service';

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  imports: [
    MonitoringComponent, CommonModule
  ],
})
export class AppComponent implements OnInit {
  data: any;

  constructor(private dataService: DataService) { }

  ngOnInit(): void {
    this.dataService.fetchData().subscribe(
      responseData => {
        this.data = responseData;
      },
      error => {
        console.error('Error fetching data:', error);
      }
    );
  }
  title = 'Monitoring';
}
