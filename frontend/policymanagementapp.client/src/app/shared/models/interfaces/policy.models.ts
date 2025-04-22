export interface Policy {
  id: number;
  name: string;
  description: string;
  type?: string;
  creationDate?: string | Date;
  effectiveDate: string | Date;
  expiryDate: string | Date;
  expirationDate?: string | Date;
  isActive: boolean;
  policyTypeId: number;
  policyTypeName?: string;
  tenantId?: string;
  tenantName?: string;
}

export interface PolicyType {
  id: number;
  name: string;
}

export interface PolicyResponse {
  policies: Policy[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  tenantId?: string;
  tenantName?: string;
}

export interface DeletePolicyDto {
  id?: number;
  tenantId?: string;
  pageNumber?: number;
  pageSize?: number;
  sortColumn?: string;
  sortDirection?: string;
} 