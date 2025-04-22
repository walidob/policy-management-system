import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { PolicyService } from '../../../core/services/policy.service';
import { finalize, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { PolicyTypeDropdownComponent } from '../../../shared/components/policy-type-dropdown/policy-type-dropdown.component';

@Component({
  selector: 'app-edit-policy',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PolicyTypeDropdownComponent],
  templateUrl: './edit-policy.component.html',
  styleUrls: ['./edit-policy.component.css']
})
export class EditPolicyComponent implements OnInit {
  policyForm: FormGroup;
  submitted = false;
  policyId: number = 0;
  loading = false;
  error = false;
  errorMessage = '';
  tenantId: string | null = null;
  isSuperAdmin = false;
  
  constructor(
    private fb: FormBuilder, 
    private router: Router,
    private route: ActivatedRoute,
    private policyService: PolicyService,
    private authService: AuthService
  ) {
    this.policyForm = this.createForm();
  }
  
  ngOnInit(): void {
    this.authService.user$.subscribe(user => {
      this.isSuperAdmin = !!user?.isSuperAdmin;
      
      if (!this.isSuperAdmin && user?.tenantId) {
        this.tenantId = user.tenantId;
      }
    });
    
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.policyId = +params['id'];
        
        this.route.queryParams.subscribe(queryParams => {
          if (this.isSuperAdmin && queryParams['tenantId']) {
            this.tenantId = queryParams['tenantId'];
          }
          
          this.loadPolicy(this.policyId);
        });
      } else {
        this.router.navigate(['/policies']);
      }
    });
  }
  
  createForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(256)]],
      description: ['', [Validators.maxLength(1000)]],
      creationDate: ['', Validators.required],
      effectiveDate: ['', Validators.required],
      expiryDate: ['', Validators.required],
      isActive: [true],
      policyTypeId: ['', Validators.required],
      tenantId: ['']
    });
  }
  
  loadPolicy(id: number): void {
    this.loading = true;
    this.policyService.getPolicyById(id, this.tenantId || undefined)
      .pipe(
        catchError(error => {
          console.error(`Error loading policy with ID: ${id}`, error);
          this.error = true;
          this.errorMessage = 'Failed to load policy. Please try again later.';
          return of(null);
        }),
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe(policy => {
        if (policy) {
          this.policyForm.patchValue({
            name: policy.name,
            description: policy.description,
            creationDate: policy.creationDate ? new Date(policy.creationDate).toISOString().split('T')[0] : new Date().toISOString().split('T')[0],
            effectiveDate: new Date(policy.effectiveDate).toISOString().split('T')[0],
            expiryDate: new Date(policy.expiryDate).toISOString().split('T')[0],
            isActive: policy.isActive,
            policyTypeId: policy.policyTypeId,
            tenantId: policy.tenantId || this.tenantId
          });
        } else {
          this.router.navigate(['/policies']);
        }
      });
  }
  
  onSubmit(): void {
    this.submitted = true;
    
    if (this.policyForm.invalid) {
      return;
    }
    
    const policyData = {
      ...this.policyForm.value,
      id: this.policyId
    };
    
    if (this.tenantId && !policyData.tenantId) {
      policyData.tenantId = this.tenantId;
    }
    
    this.loading = true;
    
    this.policyService.updatePolicy(this.policyId, policyData)
      .pipe(
        catchError(error => {
          console.error('Error updating policy:', error);
          this.error = true;
          this.errorMessage = 'Failed to update policy. Please try again later.';
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
