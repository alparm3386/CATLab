import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { WorkflowComponent } from './components/workflow/workflow.component';
import { DataService } from './services/data.service';

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
  title = 'monitoringDetails';

  constructor(private dataService: DataService, private route: ActivatedRoute) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const idJob = +params['idJob'];
      this.dataService.fetchData(idJob);
    });
  }
}
