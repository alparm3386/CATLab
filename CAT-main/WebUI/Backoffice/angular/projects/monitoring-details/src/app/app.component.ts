import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WorkflowComponent } from './components/workflow/workflow.component';
import { DocumentsComponent } from './components/documents/documents.component';
import { PeopleComponent } from './components/people/people.component';
import { DataService } from './services/data.service';
import { Location } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  imports: [
    CommonModule, WorkflowComponent, DocumentsComponent, PeopleComponent
  ],
})
export class AppComponent {
  title = 'monitoring details';

  public jobData = {
    jobId: {},
    orderId: {},
    dateProcessed: {},
    sourceLanguage: '',
    targetLanguage: '',
    speciality: {},
    specialityName: '',
    speed: '',
    service: '',
    serviceName: '',
    documents: [],
    words: {},
    fee: {},
    workflowSteps: [],
    companyName: '',
   companyId: '',
  };

  constructor(private location: Location, private dataService: DataService) {
    this.dataService.data$.subscribe(
      data => {
        if (data) {
          this.jobData = data;
        }
      });
  }

    ngOnInit(): void {
  //  this.route.queryParams.subscribe(params => {
  //    const idJob = +params['idJob'];
  //    this.dataService.fetchData(idJob);
  //  });

    const currentUrl = this.location.path();
    const urlParams = new URLSearchParams(currentUrl.split('?')[1]);
    const jobId = +urlParams.get('jobId')! || -1;

    this.dataService.fetchData(jobId);
  }
}
