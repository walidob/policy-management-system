import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { tap, map, catchError } from 'rxjs/operators';
import { ApiService } from './api.service';

export interface EnumValue {
  id: number;
  name: string;
  displayName: string;
}

export interface AllEnums {
  policyTypes: EnumValue[];
  claimStatuses: EnumValue[];
  roles: EnumValue[];
}

@Injectable({
  providedIn: 'root'
})
export class MetadataService {
  private cachedEnums: AllEnums | null = null;
  private cachedPolicyTypes: EnumValue[] | null = null;
  private cachedClaimStatuses: EnumValue[] | null = null;
  private cachedRoles: EnumValue[] | null = null;

  constructor(private apiService: ApiService) { }

  /**
   * Get all enum values at once (cached)
   */
  getAllEnums(): Observable<AllEnums> {
    if (this.cachedEnums) {
      return of(this.cachedEnums);
    }

    return this.apiService.get<AllEnums>('metadata/enums').pipe(
      tap(enums => this.cachedEnums = enums),
      catchError(error => {
        return of({
          policyTypes: [],
          claimStatuses: [],
          roles: []
        });
      })
    );
  }

  /**
   * Get policy types (cached)
   */
  getPolicyTypes(): Observable<EnumValue[]> {
    if (this.cachedPolicyTypes) {
      return of(this.cachedPolicyTypes);
    }

    if (this.cachedEnums) {
      return of(this.cachedEnums.policyTypes);
    }

    return this.apiService.get<EnumValue[]>('metadata/enums/policytype').pipe(
      tap(types => this.cachedPolicyTypes = types),
      catchError(error => {
        return of([]);
      })
    );
  }

  /**
   * Get claim statuses (cached)
   */
  getClaimStatuses(): Observable<EnumValue[]> {
    if (this.cachedClaimStatuses) {
      return of(this.cachedClaimStatuses);
    }

    if (this.cachedEnums) {
      return of(this.cachedEnums.claimStatuses);
    }

    return this.apiService.get<EnumValue[]>('metadata/enums/claimstatus').pipe(
      tap(statuses => this.cachedClaimStatuses = statuses),
      catchError(error => {
        return of([]);
      })
    );
  }

  /**
   * Get roles (cached)
   */
  getRoles(): Observable<EnumValue[]> {
    if (this.cachedRoles) {
      return of(this.cachedRoles);
    }

    if (this.cachedEnums) {
      return of(this.cachedEnums.roles);
    }

    return this.apiService.get<EnumValue[]>('metadata/enums/role').pipe(
      tap(roles => this.cachedRoles = roles),
      catchError(error => {
        return of([]);
      })
    );
  }

  /**
   * Get the display name for a policy type by ID
   */
  getPolicyTypeDisplayName(typeId: number): Observable<string> {
    return this.getPolicyTypes().pipe(
      map(types => {
        const found = types.find(t => t.id === typeId);
        return found ? found.displayName : `Unknown Type (${typeId})`;
      })
    );
  }

  /**
   * Get the display name for a claim status by ID
   */
  getClaimStatusDisplayName(statusId: number): Observable<string> {
    return this.getClaimStatuses().pipe(
      map(statuses => {
        const found = statuses.find(s => s.id === statusId);
        return found ? found.displayName : `Unknown Status (${statusId})`;
      })
    );
  }

  /**
   * Get the display name for a role by name
   */
  getRoleDisplayName(roleName: string): Observable<string> {
    return this.getRoles().pipe(
      map(roles => {
        const found = roles.find(r => r.name === roleName);
        return found ? found.displayName : roleName;
      })
    );
  }
} 