import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { LayoutComponent } from './core/components/layout/layout.component';
import { authGuard } from './core/guards/auth.guard';
import { AllPoliciesComponent } from './features/policies/all-policies/all-policies.component';
import { EditPolicyComponent } from './features/policies/edit-policy/edit-policy.component';
import { PolicyDetailComponent } from './features/policies/policy-detail/policy-detail.component';
import { CreatePolicyComponent } from './features/policies/create-policy/create-policy.component';
export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'login', component: LoginComponent },
      { path: 'policies', component: AllPoliciesComponent },
      { path: 'policies/create', component: CreatePolicyComponent },
      { path: 'policies/:id', component: PolicyDetailComponent },
      { path: 'policies/edit/:id', component: EditPolicyComponent },
    ]
  },
  { path: '**', redirectTo: 'login' }
]; 