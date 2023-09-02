import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderComponent } from './order/order.component';
import { DataService } from '../../services/data.service';

@Component({
  selector: 'app-monitoring',
  standalone: true,
  imports: [CommonModule, OrderComponent],
  templateUrl: './monitoring.component.html',
  styleUrls: ['./monitoring.component.scss']
})
export class MonitoringComponent {
  orders = [];

  constructor(private dataService: DataService) { } // Injecting the service here

  onSearchClicked() {
    // Your search logic here
    console.log('Search button clicked!');
    this.dataService.fetchData().subscribe(
      responseData => {
        console.log(responseData);
      },
      error => {
        console.error('Error fetching data:', error);
      }
    );

  }
}
