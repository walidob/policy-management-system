import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { Policy } from '../../../shared/models/interfaces/policy.models';
import { PolicyService } from '../../../core/services/policy.service';
import { catchError, finalize, of, Subject, takeUntil, switchMap, map } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { MetadataService } from '../../../core/services/metadata.service';

@Component({
  selector: 'app-all-policies',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './all-policies.component.html',
  styleUrls: ['./all-policies.component.css']
})
export class AllPoliciesComponent implements OnInit, OnDestroy {
  policies: Policy[] = [];
  loading = false;
  loadingInProgress = false;
  error = false;
  errorMessage = '';
  dataLoaded = false;
  totalItems = 0;
  page = 1;
  pageSize = 10;
  sortColumn = 'id';
  sortDirection = 'asc';
  isSuperAdmin = false;
  isAdmin = false;
  isClient = false;
  Math = Math;
  private destroy$ = new Subject<void>();

  constructor(
    private router: Router,
    private policyService: PolicyService,
    private authService: AuthService,
    private metadataService: MetadataService
  ) {}

  ngOnInit(): void {
    this.authService.user$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.isSuperAdmin = !!user?.isSuperAdmin;
        this.isAdmin = !!user?.roles?.includes('TenantAdmin');
        this.isClient = !this.isSuperAdmin && !this.isAdmin && !!user?.isAuthenticated;
        
        if (!this.dataLoaded) {
          this.loadPolicies();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  createNewPolicy(): void {
    if (this.isAdmin) {
      this.router.navigate(['/policies/create']);
    }
  }

  loadPolicies(): void {
    if (this.loadingInProgress) {
      return;
    }
    
    this.loading = true;
    this.loadingInProgress = true;
    this.error = false;
    
    this.policyService.getAllPolicies(this.page, this.pageSize, this.sortColumn, this.sortDirection)
      .pipe(
        takeUntil(this.destroy$),
        switchMap(response => {
          return this.metadataService.getPolicyTypes().pipe(
            map(policyTypes => {
              response.policies = response.policies.map(policy => {
                if (policy.policyTypeId !== undefined) {
                  const policyType = policyTypes.find(t => t.id === policy.policyTypeId);
                  if (policyType) {
                    return {
                      ...policy,
                      policyTypeName: policyType.displayName
                    };
                  }
                }
                return policy;
              });
              return response;
            })
          );
        }),
        catchError(error => {
          console.error('Error loading policies:', error);
          this.error = true;
          this.errorMessage = 'Failed to load policies: ' + (error.message || 'Unknown error');
          return of({ policies: [], totalCount: 0, pageNumber: 1, pageSize: this.pageSize });
        }),
        finalize(() => {
          this.loading = false;
          this.loadingInProgress = false;
        })
      )
      .subscribe(response => {
        this.policies = response.policies;
        this.totalItems = response.totalCount;
        this.page = response.pageNumber;
        this.dataLoaded = true;
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

  viewPolicy(id: number, tenantId?: string): void {
    const queryParams = tenantId ? { tenantId } : undefined;
    this.router.navigate(['/policies', id], { queryParams });
  }

  editPolicy(id: number, tenantId?: string): void {
    if (this.isSuperAdmin || this.isAdmin) {
      const queryParams = tenantId ? { tenantId } : undefined;
      this.router.navigate(['/policies/edit', id], { queryParams });
    }
  }

  deletePolicy(id: number, tenantId?: string): void {
    if (!this.isSuperAdmin) {
      return;
    }
    
    if (confirm('Are you sure you want to delete this policy?')) {
      this.policyService.getPolicyById(id, tenantId)
        .pipe(
          takeUntil(this.destroy$),
          switchMap(policy => {
            if (!policy) {
              throw new Error('Policy not found');
            }
            return this.policyService.deletePolicy(id, {
              pageNumber: this.page,
              pageSize: this.pageSize,
              sortColumn: this.sortColumn,
              sortDirection: this.sortDirection,
              tenantId: tenantId
            });
          }),
          catchError(error => {
            console.error('Error deleting policy:', error);
            this.error = true;
            this.errorMessage = 'Failed to delete policy: ' + (error.message || 'Unknown error');
            return of(null);
          })
        )
        .subscribe(_ => {
          this.loadPolicies();
        });
    }
  }

  canEdit(policy: Policy): boolean {
    return this.isSuperAdmin || this.isAdmin;
  }

  canDelete(policy: Policy): boolean {
    return this.isSuperAdmin;
  }

  onPageChange(pageNumber: number): void {
    if (pageNumber >= 1 && pageNumber <= this.getTotalPages()) {
      this.page = pageNumber;
      this.loadPolicies();
    }
  }

  getTotalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize);
  }

  getPages(): number[] {
    const totalPages = this.getTotalPages();
    const pages: number[] = [];
    
    if (totalPages <= 5) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      const startPage = Math.max(1, this.page - 2);
      const endPage = Math.min(totalPages, startPage + 4);
      
      for (let i = startPage; i <= endPage; i++) {
        pages.push(i);
      }
    }
    
    return pages;
  }

  sortBy(column: string): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    this.page = 1;
    this.loadPolicies();
  }

  getSortIcon(column: string): string {
    if (this.sortColumn !== column) {
      return 'bi bi-arrow-down-up';
    }
    return this.sortDirection === 'asc' ? 'bi bi-sort-down' : 'bi bi-sort-up';
  }
} 