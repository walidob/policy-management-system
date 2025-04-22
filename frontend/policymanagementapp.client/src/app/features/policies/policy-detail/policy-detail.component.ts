import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Policy } from '../../../shared/models/interfaces/policy.models';
import { PolicyService } from '../../../core/services/policy.service';
import { catchError, finalize, of, switchMap } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { UserInfo } from '../../../core/models/auth.model';
import { MetadataService } from '../../../core/services/metadata.service';

@Component({
  selector: 'app-policy-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './policy-detail.component.html',
  styleUrls: ['./policy-detail.component.css']
})
export class PolicyDetailComponent implements OnInit {
  policy: Policy | null = null;
  loading = false;
  error = false;
  errorMessage = '';
  policyId: number = 0;
  tenantId: string | null = null;
  policyTypeDisplayName: string = '';
  
  user: UserInfo | null = null;
  isSuperAdmin = false;
  isAdmin = false;
  isClient = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private policyService: PolicyService,
    private authService: AuthService,
    private metadataService: MetadataService
  ) {}

  ngOnInit(): void {
    this.authService.user$.subscribe(user => {
      this.user = user;
      this.isSuperAdmin = !!user?.isSuperAdmin;
      this.isAdmin = !!user?.roles?.includes('TenantAdmin');
      this.isClient = !this.isSuperAdmin && !this.isAdmin;
      
      if (!this.isSuperAdmin && user?.tenantId) {
        this.tenantId = user.tenantId;
      }
    });
    
    this.route.params.subscribe(params => {
      const id = +params['id'];
      if (id) {
        this.policyId = id;
        this.route.queryParams.subscribe(queryParams => {
          // For super admin, use tenantId from query params if available
          if (this.isSuperAdmin && queryParams['tenantId']) {
            this.tenantId = queryParams['tenantId'];
          } 
          else if (!this.isSuperAdmin && queryParams['tenantId'] && (!this.tenantId || this.tenantId.trim() === '')) {
            this.tenantId = queryParams['tenantId'];
          }
          
          this.loadPolicy(id);
        });
      } else {
        this.error = true;
        this.errorMessage = 'No policy ID provided';
        this.loading = false;
      }
    });
  }

  loadPolicy(id: number): void {
    this.loading = true;
    this.error = false;
    
    if (this.isSuperAdmin && (!this.tenantId || this.tenantId.trim() === '')) {
      console.log('Super admin flow without tenant ID');
      
      this.policyService.getAllPolicies(1, 100)
        .pipe(
          catchError(error => {
            this.error = true;
            this.errorMessage = 'Failed to retrieve policy information: ' + (error.error?.detail || error.message || 'Unknown error');
            console.error('Policy list error:', error);
            return of({ policies: [], totalCount: 0, pageNumber: 1, pageSize: 10 });
          })
        )
        .subscribe(response => {
          const foundPolicy = response.policies.find(p => p.id === id);
          
          if (foundPolicy && foundPolicy.tenantId) {
            console.log('Found policy with tenant ID:', foundPolicy.tenantId);
            this.tenantId = foundPolicy.tenantId;
            this.getPolicyWithTenantId(id);
          } else {
            this.error = true;
            this.errorMessage = `Could not find policy with ID ${id} or tenant information is missing`;
            this.loading = false;
          }
        });
    } else {
      this.getPolicyWithTenantId(id);
    }
  }

  private getPolicyWithTenantId(id: number): void {
    const tenantIdParam =  this.tenantId ;
    
    this.policyService.getPolicyById(id, tenantIdParam || undefined)
      .pipe(
        switchMap(policy => {
          this.policy = policy;
          
          if (policy && policy.tenantId) {
            this.tenantId = policy.tenantId;
          }
          
          if (policy && policy.policyTypeId) {
            return this.metadataService.getPolicyTypeDisplayName(policy.policyTypeId);
          }
          return of('');
        }),
        catchError(error => {
          this.error = true;
          if (error.status === 400 && error.error?.detail?.includes('tenant')) {
            this.errorMessage = 'Tenant ID is required to view this policy. Please contact your administrator.';
          } else {
            this.errorMessage = 'Failed to load policy details: ' + (error.error?.detail || error.message || 'Unknown error');
          }
          console.error('Policy detail error:', error);
          return of('');
        }),
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe(displayName => {
        if (displayName) {
          this.policyTypeDisplayName = displayName;
        } else if (this.policy?.policyTypeName) {
          this.policyTypeDisplayName = this.policy.policyTypeName;
        }
      });
  }

  getBadgeClass(policy: Policy): string {
    const currentDate = new Date();
    const expiryDate = new Date(policy.expiryDate);
    
    if (!policy.isActive) {
      return 'bg-secondary';
    } else if (expiryDate < currentDate) {
      return 'bg-danger';
    } else {
      const daysUntilExpiry = Math.ceil((expiryDate.getTime() - currentDate.getTime()) / (1000 * 60 * 60 * 24));
      if (daysUntilExpiry <= 30) {
        return 'bg-warning';
      } else {
        return 'bg-success';
      }
    }
  }

  getStatusText(policy: Policy): string {
    const currentDate = new Date();
    const expiryDate = new Date(policy.expiryDate);
    
    if (!policy.isActive) {
      return 'Inactive';
    } else if (expiryDate < currentDate) {
      return 'Expired';
    } else {
      const daysUntilExpiry = Math.ceil((expiryDate.getTime() - currentDate.getTime()) / (1000 * 60 * 60 * 24));
      if (daysUntilExpiry <= 30) {
        return 'Expiring Soon';
      } else {
        return 'Active';
      }
    }
  }

  cancel(): void {
    this.router.navigate(['/policies']);
  }
} 