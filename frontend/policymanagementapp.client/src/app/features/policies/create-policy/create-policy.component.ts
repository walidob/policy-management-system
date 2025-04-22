import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { PolicyService } from '../../../core/services/policy.service';
import { finalize, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { PolicyTypeDropdownComponent } from '../../../shared/components/policy-type-dropdown/policy-type-dropdown.component';
import { AuthService } from '../../../core/services/auth.service';

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
  private tenantId: string | null = null;
  
  constructor(
    private fb: FormBuilder, 
    private router: Router,
    private policyService: PolicyService,
    private authService: AuthService
  ) {
    this.policyForm = this.createForm();
  }
  
  ngOnInit(): void {
    this.authService.user$.subscribe(user => {
      if (user) {
        this.tenantId = user.tenantId;
      }
    });
  }
  
  createForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(256)]],
      description: ['', [Validators.maxLength(1000)]],
      effectiveDate: [new Date().toISOString().split('T')[0], [Validators.required, this.effectiveDateValidator]],
      expiryDate: [this.getDefaultExpiryDate(), [Validators.required, this.expiryDateValidator]],
      isActive: [true],
      policyTypeId: ['', Validators.required],
      tenantId: ['']
    });
  }
  
  effectiveDateValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    
    const effectiveDate = new Date(control.value);
    effectiveDate.setHours(0, 0, 0, 0);
    
    if (effectiveDate < today) {
      return { effectiveBeforeToday: true };
    }
    
    const parent = control.parent;
    
    if (!parent) return null;
    
    return null;
  }
  
  expiryDateValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    
    const expiryDate = new Date(control.value);
    expiryDate.setHours(0, 0, 0, 0);
    
    const parent = control.parent;
    
    if (!parent) return null;
    
    const effectiveDateControl = parent.get('effectiveDate');
    
    if (!effectiveDateControl || !effectiveDateControl.value) return null;
    
    const effectiveDate = new Date(effectiveDateControl.value);
    effectiveDate.setHours(0, 0, 0, 0);
    
    if (expiryDate < effectiveDate) {
      return { expiryBeforeEffective: true };
    }
    
    return null;
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
    
    if (this.tenantId) {
      this.policyForm.get('tenantId')?.setValue(this.tenantId);
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
          if (error.error && error.error.errors) {
            const errorDetails = Object.entries(error.error.errors)
              .map(([field, messages]) => `${field}: ${(messages as string[]).join(', ')}`)
              .join('; ');
            this.errorMessage = `Validation error: ${errorDetails}`;
          }
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