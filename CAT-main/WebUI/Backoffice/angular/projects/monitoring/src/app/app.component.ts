import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MonitoringComponent } from './components/monitoring/monitoring.component';
import { DataService } from './services/data.service';
import { SpinnerService } from '../../../cat-common/services/spinner.service';

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
  title = 'Monitoring';

  constructor(private dataService: DataService, private spinnerService: SpinnerService) { }

  ngOnInit(): void {
    //the spinner
    this.dataService.data$.subscribe(data => {
      this.spinnerService.hide();
      if (data) {
      }
    });
    this.spinnerService.show();

    //get the data from the server
    this.dataService.fetchData();
  }
}
