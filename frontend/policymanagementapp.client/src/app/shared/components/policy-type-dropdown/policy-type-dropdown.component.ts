import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormControl, ReactiveFormsModule } from '@angular/forms';
import { MetadataService, EnumValue } from '../../../core/services/metadata.service';
import { finalize, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-policy-type-dropdown',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="form-group">
      <select 
        [id]="id" 
        class="form-select"
        [formControl]="control"
        [attr.disabled]="readonly ? true : null"
        [class.is-invalid]="showErrors && control.errors">
        <option value="">Select Policy Type</option>
        <option *ngFor="let type of policyTypes" [value]="type.id">{{ type.displayName }}</option>
      </select>
      <div *ngIf="showErrors && control.errors?.['required']" class="text-danger mt-1">
        <small>Policy type is required</small>
      </div>
      <div *ngIf="error" class="text-danger mt-1">
        <small>{{ errorMessage }}</small>
      </div>
    </div>
  `,
  styles: [`
    .form-select {
      padding: 0.5rem 1rem;
      border-radius: 0.375rem;
      border: 1px solid #ced4da;
      transition: border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out;
    }
    
    .form-select:focus {
      border-color: #86b7fe;
      box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
      outline: 0;
    }
    
    .form-select.is-invalid {
      border-color: #dc3545;
    }
    
    .form-select:disabled {
      background-color: #e9ecef;
      opacity: 1;
    }
  `]
})
export class PolicyTypeDropdownComponent implements OnInit {
  @Input() control!: FormControl;
  @Input() id: string = 'policyTypeId';
  @Input() label: string = 'Policy Type';
  @Input() showErrors: boolean = false;
  @Input() readonly: boolean = false;
  
  policyTypes: EnumValue[] = [];
  loading = false;
  error = false;
  errorMessage = '';
  
  constructor(private metadataService: MetadataService) {}
  
  ngOnInit(): void {
    this.loadPolicyTypes();
    
    if (!this.control) {
      this.control = new FormControl();
    }
  }
  
  loadPolicyTypes(): void {
    this.loading = true;
    this.metadataService.getPolicyTypes()
      .pipe(
        catchError(error => {
          this.error = true;
          this.errorMessage = 'Failed to load policy types';
          return of([]);
        }),
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe(types => {
        this.policyTypes = types;
      });
  }
  
  getDisplayName(id: number): string {
    const type = this.policyTypes.find(t => t.id === id);
    return type ? type.displayName : '';
  }
} 