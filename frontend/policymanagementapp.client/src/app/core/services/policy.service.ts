import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Policy, PolicyResponse, PolicyType } from '../../shared/models/interfaces/policy.models';
import { switchMap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class PolicyService {
  constructor(private apiService: ApiService) { }

  getPoliciesByTenant(tenantId: string, pageNumber: number = 1, pageSize: number = 10): Observable<PolicyResponse> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    
    return this.apiService.get<PolicyResponse>(`policies`, params);
  }

  getAllPolicies(pageNumber: number = 1, pageSize: number = 10, sortColumn: string = 'id', sortDirection: string = 'asc'): Observable<PolicyResponse> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString())
      .set('sortColumn', sortColumn)
      .set('sortDirection', sortDirection);
    
    return this.apiService.get<PolicyResponse>(`policies`, params);
  }

  getPolicyById(id: number, tenantId?: string): Observable<Policy> {
    let params = new HttpParams();
    if (tenantId && tenantId.trim() !== '') {
      params = params.set('tenantId', tenantId);
    }
    
    return this.apiService.get<Policy>(`policies/${id}`, params);
  }

  createPolicy(policy: Partial<Policy>): Observable<Policy> {
    return this.apiService.post<Policy>('policies', policy);
  }

  updatePolicy(id: number, policy: Partial<Policy>): Observable<Policy> {
    const policyWithId = { ...policy, id };
    return this.apiService.put<Policy>('policies', policyWithId);
  }

  deletePolicy(id: number, pageInfo?: { pageNumber?: number, pageSize?: number, sortColumn?: string, sortDirection?: string, tenantId?: string }): Observable<boolean> {
    // The backend DeletePolicyDto extends PolicyDtoBase which requires Name, EffectiveDate, ExpiryDate, PolicyTypeId
    // We need to get the policy details first, then use that to create a valid DeletePolicyDto
    return this.getPolicyById(id, pageInfo?.tenantId).pipe(
      switchMap((policy: Policy) => {
        const deleteDto: any = {
          id,
          name: policy.name,
          description: policy.description,
          effectiveDate: policy.effectiveDate,
          expiryDate: policy.expiryDate,
          policyTypeId: policy.policyTypeId,
          isActive: policy.isActive,
          pageNumber: pageInfo?.pageNumber || 1,
          pageSize: pageInfo?.pageSize || 10,
          sortColumn: pageInfo?.sortColumn || 'id',
          sortDirection: pageInfo?.sortDirection || 'asc'
        };
        
        // Only add tenantId if it has a value
        if (pageInfo?.tenantId && pageInfo.tenantId.trim() !== '') {
          deleteDto.tenantId = pageInfo.tenantId;
        }
        
        return this.apiService.delete<boolean>('policies', undefined, deleteDto);
      })
    );
  }

  getLatestPolicies(): Observable<Policy[]> {
    return this.apiService.get<Policy[]>('policies/latest');
  }

  getPolicyTypes(): Observable<PolicyType[]> {
    return this.apiService.get<PolicyType[]>('policies/types');
  }
} 