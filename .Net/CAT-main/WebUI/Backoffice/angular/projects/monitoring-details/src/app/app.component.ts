import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WorkflowComponent } from './components/workflow/workflow.component';
import { DocumentsComponent } from './components/documents/documents.component';
import { PeopleComponent } from './components/people/people.component';
import { DataService } from './services/data.service';
import { Location } from '@angular/common';
import { SpinnerService } from '../../../cat-common/services/spinner.service';
import { ModalService } from '../../../cat-common/services/modal.service';

@Component({
  standalone: true,
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  imports: [
    CommonModule, WorkflowComponent, DocumentsComponent, PeopleComponent
  ],
  providers: [],
})
export class AppComponent {
  title = 'monitoring details';

  public jobData: any = {
    // initial properties here
  };

  constructor(private location: Location, private dataService: DataService, private spinnerService: SpinnerService,
    private modalService: ModalService) {
  }

  ngOnInit(): void {
    this.dataService.data$.subscribe({
      next: data => {
        if (data) {
          this.jobData = data;
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

    const currentUrl = this.location.path();
    const urlParams = new URLSearchParams(currentUrl.split('?')[1]);
    const jobId = +urlParams.get('jobId')! || -1;

    this.dataService.getJobData(jobId);
  }
}
