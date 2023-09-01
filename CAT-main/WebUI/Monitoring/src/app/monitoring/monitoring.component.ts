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
  orders = [
    {
      id: 1,
      client: "Apple",
      dateCreated: "01/09/2023",
      jobs:
        [{
          id: 1,
          orderId: 1,
          document: "test.docx",
          sourceLanguage: "en",
          targetLanguage: "fr",
          speciality: "Marketing",
          workflow: [{
            id: 1,
            task: "jobboard",
            stepOrder: 1,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 2,
            task: "jobboard",
            stepOrder: 2,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 3,
            task: "AI process",
            stepOrder: 3,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 4,
            task: "Completed",
            stepOrder: 4,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 5,
            task: "Delivery",
            stepOrder: 5,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 6,
            task: "Billing",
            stepOrder: 6,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },

          ]
        },
        {
          id: 1,
          orderId: 1,
          document: "test.docx",
          sourceLanguage: "en",
          targetLanguage: "fr",
          speciality: "Marketing",
          workflow: [{
            id: 1,
            task: "jobboard",
            stepOrder: 1,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 2,
            task: "jobboard",
            stepOrder: 2,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 3,
            task: "AI process",
            stepOrder: 3,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 4,
            task: "Completed",
            stepOrder: 4,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 5,
            task: "Delivery",
            stepOrder: 5,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 6,
            task: "Billing",
            stepOrder: 6,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },

          ]
        }]
    },
    {
      id: 2,
      client: "Apple",
      dateCreated: "01/09/2023",
      jobs:
        [{
          id: 3,
          orderId: 2,
          document: "test.docx",
          sourceLanguage: "en",
          targetLanguage: "fr",
          speciality: "Marketing",
          workflow: [{
            id: 1,
            task: "jobboard",
            stepOrder: 1,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 2,
            task: "jobboard",
            stepOrder: 2,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 3,
            task: "AI process",
            stepOrder: 3,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 4,
            task: "Completed",
            stepOrder: 4,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 5,
            task: "Delivery",
            stepOrder: 5,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 6,
            task: "Billing",
            stepOrder: 6,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },

          ]
        },
        {
          id: 1,
          orderId: 1,
          document: "test.docx",
          sourceLanguage: "en",
          targetLanguage: "fr",
          speciality: "Marketing",
          workflow: [{
            id: 1,
            task: "jobboard",
            stepOrder: 1,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 2,
            task: "jobboard",
            stepOrder: 2,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 3,
            task: "AI process",
            stepOrder: 3,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 4,
            task: "Completed",
            stepOrder: 4,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 5,
            task: "Delivery",
            stepOrder: 5,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },
          {
            id: 6,
            task: "Billing",
            stepOrder: 6,
            status: 0,
            StartDate: "01/09/2023",
            ScheduledDate: "01/09/2023",
            CompletedDate: "01/09/2023",
            Fee: 10.0
          },

          ]
        }]
    }
  ];
}
