import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderComponent } from './order/order.component';

@Component({
  selector: 'monitoring',
  standalone: true,
  imports: [CommonModule, OrderComponent],
  templateUrl: './monitoring.component.html',
  styleUrls: ['./monitoring.component.scss']
})
export class MonitoringComponent {
}
