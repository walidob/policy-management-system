export interface LoginRequest {
  Email: string;
  Password: string;
}

export interface LoginResponse {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  jwtToken?: string;
  tokenExpires?: string;
  roles?: string[];
  isSuperAdmin?: boolean;
  isAuthenticated?: boolean;
}

export interface UserInfo {
  userId: string;
  email: string;
  username: string;
  firstName: string;
  lastName: string;
  tenantId: string;
  roles: string[];
  isAuthenticated: boolean;
  isSuperAdmin: boolean;
} 