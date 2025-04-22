import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { Policy } from '../../models/interfaces/policy.models';
import { MetadataService } from '../../../core/services/metadata.service';
import { take } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { UserInfo } from '../../../core/models/auth.model';

@Component({
  selector: 'app-policies-table',
  standalone: true,
  imports: [CommonModule, NgbPaginationModule],
  templateUrl: './policies-table.component.html',
  styleUrl: './policies-table.component.css'
})
export class PoliciesTableComponent implements OnInit {
  @Input() policies: Policy[] = [];
  @Input() loading = false;
  @Input() error = false;
  @Input() errorMessage = '';
  @Input() dataLoaded = false;
  @Input() totalItems = 0;
  @Input() page = 1;
  @Input() pageSize = 10;
  
  @Output() pageChange = new EventEmitter<number>();
  @Output() viewPolicyEvent = new EventEmitter<Policy>();
  @Output() deletePolicyEvent = new EventEmitter<Policy>();
  @Output() retryLoad = new EventEmitter<void>();
  
  user: UserInfo | null = null;
  isSuperAdmin = false;
  isAdmin = false;
  
  constructor(
    private metadataService: MetadataService,
    private authService: AuthService
  ) {}
  
  ngOnInit(): void {
    this.metadataService.getAllEnums().pipe(take(1)).subscribe();
    
    this.authService.user$.subscribe(user => {
      this.user = user;
      this.isSuperAdmin = !!user?.isSuperAdmin;
      this.isAdmin = !!user?.roles?.includes('TenantAdmin');
    });
  }
  
  onPageChange(page: number): void {
    this.pageChange.emit(page);
  }
  
  viewPolicy(policy: Policy): void {
    this.viewPolicyEvent.emit(policy);
  }
  

  deletePolicy(policy: Policy): void {
    this.deletePolicyEvent.emit(policy);
  }
  
  retry(): void {
    this.retryLoad.emit();
  }
  
  getExpirationStatus(expirationDate: string | Date | undefined): { status: string, class: string } {
    if (!expirationDate) {
      return { status: 'Unknown', class: 'bg-secondary' };
    }
    
    const today = new Date();
    const expDate = new Date(expirationDate);
    
    if (isNaN(expDate.getTime())) {
      return { status: 'Invalid Date', class: 'bg-secondary' };
    }
    
    const diffTime = expDate.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    if (diffDays < 0) {
      return { status: 'Expired', class: 'bg-danger' };
    } else {
      return { status: 'Valid', class: 'bg-success' };
    }
  }
  
  getPolicyTypeName(typeId: number | string | undefined): string {
    if (typeId === undefined) {
      return 'Unknown Type';
    }
    
    const numericTypeId = typeof typeId === 'string' ? parseInt(typeId, 10) : typeId;
    const policy = this.policies.find(p => p.policyTypeId === numericTypeId);
    
    if (policy && policy.policyTypeName) {
      return policy.policyTypeName;
    }
    
    return `Type ${typeId}`;
  }
} 
