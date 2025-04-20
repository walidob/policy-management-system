export interface PolicyDto {
  id: number;
  name: string;
  description: string;
  creationDate: Date;
  effectiveDate: Date;
  expiryDate: Date;
  policyTypeId: number;
  policyTypeName: string;
} 