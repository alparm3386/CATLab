import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MonitoringComponent } from './components/monitoring/monitoring.component';
import { DataService } from './services/data.service';
import { SpinnerService } from '../../../cat-common/services/spinner.service';
import { finalize } from 'rxjs';
import { ModalService } from '../../../cat-common/services/modal.service';

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

  constructor(private dataService: DataService, private spinnerService: SpinnerService,
    private modalService: ModalService) { }

  ngOnInit(): void {
    //the spinner
    this.dataService.data$.subscribe({
      next: data => {
        if (data) {
          // Handle data here if needed
        }
      },
      error: error => {
        this.modalService.alert("Unable to retrieve data from the server. Please try again.", "Error")
        this.spinnerService.hide();
      },
      complete: () => {
        this.spinnerService.hide();
      }
    });
    this.spinnerService.show();

    //get the data from the server
    this.dataService.getJobData();
  }
}
