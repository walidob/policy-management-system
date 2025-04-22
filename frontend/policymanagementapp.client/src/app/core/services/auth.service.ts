import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of, tap,  catchError } from 'rxjs';
import { ApiService } from './api.service';
import { Router } from '@angular/router';
import { LoginRequest,  UserInfo } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private userSubject = new BehaviorSubject<UserInfo | null>(null);
  public user$ = this.userSubject.asObservable();
  private authChecked = false;

  constructor(
    private apiService: ApiService,
    private router: Router
  ) {
    this.checkAuth().subscribe();
  }

  checkAuth(): Observable<any> {
    return this.apiService.get<any>('auth/check').pipe(
      tap(response => {
        if (response && response.id) {
          const userInfo: UserInfo = {
            userId: response.id.toString(),
            email: response.email,
            username: response.username,
            firstName: response.firstName,
            lastName: response.lastName,
            tenantId: response.tenantId || '',
            roles: response.roles || [],
            isAuthenticated: true,
            isSuperAdmin: response.isSuperAdmin || false
          };
          this.userSubject.next(userInfo);
          this.authChecked = true;
        } else {
          this.userSubject.next(null);
          this.authChecked = true;
        }
      }),
      catchError(error => {
        this.userSubject.next(null);
        this.authChecked = true;
        return of(null);
      })
    );
  }

  login(credentials: LoginRequest): Observable<any> {
    return this.apiService.post<any>('auth/login', credentials).pipe(
      tap(response => {
        const userInfo: UserInfo = {
          userId: response.id.toString(),
          email: response.email,
          username: response.username,
          firstName: response.firstName,
          lastName: response.lastName,
          tenantId: response.tenantId || '',
          roles: response.roles || [],
          isAuthenticated: true,
          isSuperAdmin: response.isSuperAdmin || false
        };
        this.userSubject.next(userInfo);
        this.authChecked = true;
      }),
      catchError(error => {
        throw error;
      })
    );
  }

  logout(): Observable<any> {
    return this.apiService.post<any>('auth/logout', {}).pipe(
      tap(_ => {
        this.clearUserSession();
      }),
      catchError(error => {
        this.clearUserSession();
        return of(null);
      })
    );
  }

  private clearUserSession(): void {
    this.userSubject.next(null);
    this.authChecked = true;
    this.router.navigate(['/login']);
  }

  isLoggedIn(): boolean {
    const isLoggedIn = this.userSubject.value !== null && this.userSubject.value.isAuthenticated === true;
    
    if (!this.authChecked) {
      this.checkAuth().subscribe();
    }
    
    return isLoggedIn;
  }
}
