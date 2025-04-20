import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { PolicyDto } from './models/policy-dto';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  public policies: PolicyDto[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.getPolicies();
  }

  getPolicies() {
    this.http.get<PolicyDto[]>('api/policy').subscribe(
      (result) => {
        this.policies = result;
      },
      (error) => {
        console.error(error);
      }
    );
  }

  title = 'policymanagementapp.client';
}
