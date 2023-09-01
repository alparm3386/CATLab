import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { JobComponent } from './job/job.component';

@Component({
  selector: '[order]',
  standalone: true,
  imports: [CommonModule, JobComponent],
  templateUrl: './order.component.html',
  styleUrls: ['./order.component.scss']
})
export class OrderComponent {
  readonly baseUrl = 'https://angular.io/assets/images/tutorials/faa';

  @Input() order: any; // Declare an input property

//  @Input() housingLocation!: HousingLocation;
//  = {
//    id: 9999,
//    name: 'Test Home',
//    city: 'Test city',
//    state: 'ST',
//    photo: `${this.baseUrl}/example-house.jpg`,
//    availableUnits: 99,
//    wifi: true,
//    laundry: false,
//  };
}
