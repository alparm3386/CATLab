import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { WorkflowComponent } from './components/workflow/workflow.component';
import { DataService } from './services/data.service';
import { Location } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  imports: [
     CommonModule, WorkflowComponent
  ],
})
export class AppComponent {
  title = 'monitoring details';

  public jobData = {
    workflowSteps: []
  };

  constructor(private location: Location, private dataService: DataService, private route: ActivatedRoute) {
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
