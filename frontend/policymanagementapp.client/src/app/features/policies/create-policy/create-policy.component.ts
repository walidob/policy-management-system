import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { PolicyService } from '../../../core/services/policy.service';
import { finalize, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { PolicyTypeDropdownComponent } from '../../../shared/components/policy-type-dropdown/policy-type-dropdown.component';

@Component({
  selector: 'app-create-policy',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PolicyTypeDropdownComponent],
  templateUrl: './create-policy.component.html',
  styleUrls: ['./create-policy.component.css']
})
export class CreatePolicyComponent implements OnInit {
  policyForm: FormGroup;
  submitted = false;
  loading = false;
  error = false;
  errorMessage = '';
  
  constructor(
    private fb: FormBuilder, 
    private router: Router,
    private policyService: PolicyService
  ) {
    this.policyForm = this.createForm();
  }
  
  ngOnInit(): void {
  }
  
  createForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(256)]],
      description: ['', [Validators.maxLength(1000)]],
      effectiveDate: [new Date().toISOString().split('T')[0], Validators.required],
      expiryDate: [this.getDefaultExpiryDate(), Validators.required],
      isActive: [true],
      policyTypeId: ['', Validators.required]
    });
  }
  
  getDefaultExpiryDate(): string {
    const date = new Date();
    date.setFullYear(date.getFullYear() + 1);
    return date.toISOString().split('T')[0];
  }
  
  onSubmit(): void {
    this.submitted = true;
    
    if (this.policyForm.invalid) {
      return;
    }
    
    const policyData = {
      ...this.policyForm.value
    };
    
    this.loading = true;
    
    this.policyService.createPolicy(policyData)
      .pipe(
        catchError(error => {
          this.error = true;
          this.errorMessage = 'Failed to create policy. Please try again later.';
          return of(null);
        }),
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe(result => {
        if (result) {
          this.router.navigate(['/policies']);
        }
      });
  }
  
  cancel(): void {
    this.router.navigate(['/policies']);
  }
} 