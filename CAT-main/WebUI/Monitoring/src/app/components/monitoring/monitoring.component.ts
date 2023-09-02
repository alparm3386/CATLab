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
  public monitoringData = {
    orders: []
  };

  constructor(private dataService: DataService) { // Injecting the service here
    this.dataService.data$.subscribe(
      data => {
        if (data) {
          this.monitoringData = data;
        }
      }
    );
} 

  onSearchClicked() {
    // Your search logic here
    console.log('Search button clicked!');
  }
}
