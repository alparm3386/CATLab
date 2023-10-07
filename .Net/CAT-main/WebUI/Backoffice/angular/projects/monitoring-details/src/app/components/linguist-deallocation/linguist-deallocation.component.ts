import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TaskDisplayName } from '../../../../../cat-common/enums/task.enum';
import { DataService } from '../../services/data.service';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ModalService } from '../../../../../cat-common/services/modal.service';

@Component({
  selector: 'app-linguist-deallocation',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './linguist-deallocation.component.html',
  styleUrls: ['./linguist-deallocation.component.scss']
})
export class LinguistDeallocationComponent {
  @Input() jobData: any;
  @Input() task: any;

  public isLoading: boolean = false;
  public taskName: string = '';
  public deallocationReason: string = '';

  constructor(private dataService: DataService, public activeModal: NgbActiveModal, private modalService: ModalService) {
  }

  ngOnInit() {
    this.taskName = TaskDisplayName[this.task];
  }

  deallocateTask() {
    if (!this.deallocationReason) {
      this.modalService.alert("Please fill the reason.", "Alert");
      return;
    }

    this.dataService.deallocateJob(this.jobData.jobId, this.task, this.deallocationReason).subscribe({
      next: data => {
        this.isLoading = false;
        this.modalService.alert("The task is deallocated.", "Success").result.then(() => {
          // Refresh the whole page
          window.location.reload();
        });
        this.activeModal.close();
      },
      error: error => {
        this.isLoading = false;
        this.modalService.alert("Unable to deallocate linguists.", "Error");
        //this.dataSubject.error(error);
      }
    });
  }

  close(): void {
    this.activeModal.close();
  }
}
