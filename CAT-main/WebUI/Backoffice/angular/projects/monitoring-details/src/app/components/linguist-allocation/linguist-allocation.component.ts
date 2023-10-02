import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { HttpClient } from '@angular/common/http';
import * as _ from 'underscore';
import { ModalService } from '../../../../../cat-common/services/modal.service';
import { DataService } from '../../services/data.service';
import { TaskDisplayName } from '../../../../../cat-common/enums/task.enum';


@Component({
  selector: 'app-linguist-allocation',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './linguist-allocation.component.html',
  styleUrls: ['./linguist-allocation.component.scss']
})
export class LinguistAllocationComponent {
  @Input() jobData: any;
  @Input() task: any;

  public searchTerm: string = '';
  public linguists: any[] = [];
  public filteredLinguists: any[] = [];
  public isLoading: boolean = false;
  public throttledSearch: any;

  constructor(private dataService: DataService, public activeModal: NgbActiveModal, private modalService: ModalService) {
    this.throttledSearch = _.throttle(this.searchLinguists.bind(this), 300);
  }

  ngOnInit(): void {
    this.getLinguists();
  }

  searchLinguists(): void {
    if (this.searchTerm.trim().length < 3) {
      this.linguists = [];
      return;
    }
  }

  getLinguists(): void {
    const searchParams = {
      sourceLanguageId: this.jobData.sourceLanguage,
      targetLanguageId: this.jobData.targetLanguage,
      speciality: this.jobData.speciality,
      task: this.task
    }

    // Get the list of linguists from the server
    this.isLoading = true;
    this.dataService.getLinguists(searchParams).subscribe({
      next: data => {
        this.isLoading = false;
        this.linguists = data;
      },
      error: error => {
        this.isLoading = false;
        this.modalService.alert("Unable to retrieve linguists from the server. Please try again later.", "Error");
        //this.dataSubject.error(error);
      }
    });
  }

  allocateLinguist(linguist: any): void {
    this.modalService.confirm(`Are you sure that you want to allocate ${linguist.user.fullName}?`,
      `Allocate ${this.getTaskDisplayName(this.task)}`)
      .result.then((result) => {
        if (result) {
          this.isLoading = true; // Start the loader before the async operation.
          this.dataService.allocateJob(this.jobData.jobId, this.task, linguist.user.id).subscribe({
            next: data => {
              this.isLoading = false;
              this.modalService.alert(`${linguist.user.fullName} is allocated to the job #${this.jobData.jobId}`, "Allocation");
              this.activeModal.close();
            },
            error: error => {
              this.isLoading = false;
              this.modalService.alert(`There was an error allocating ${linguist.user.fullName} to the job #${this.jobData.jobId}`, "Error");
            }
          });
        }
      });
  }

  getTaskDisplayName(taskId: number): string {
    return TaskDisplayName[taskId];
  }

  close(): void {
    this.activeModal.close();
  }
}
