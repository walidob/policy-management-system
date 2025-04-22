import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  get<T>(url: string, params: HttpParams = new HttpParams(), options: any = {}): Observable<T> {
    const requestOptions = { 
      params,
      ...options
    };
    return this.http.get<T>(`${this.baseUrl}/${url}`, requestOptions) as Observable<T>;
  }

  post<T>(url: string, body: any, options: any = {}): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}/${url}`, body, options) as Observable<T>;
  }

  put<T>(url: string, body: any, options: any = {}): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}/${url}`, body, options) as Observable<T>;
  }

  delete<T>(url: string, params: HttpParams = new HttpParams(), body?: any, options: any = {}): Observable<T> {
    const requestOptions = {
      params,
      body,
      ...options
    };
    return this.http.delete<T>(`${this.baseUrl}/${url}`, requestOptions) as Observable<T>;
  }
}
