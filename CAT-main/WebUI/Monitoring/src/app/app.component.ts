import { Component } from '@angular/core';
import { MonitoringComponent } from './monitoring/monitoring.component';

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  imports: [
    MonitoringComponent,
  ],
})
export class AppComponent {
  title = 'Monitoring';
}
