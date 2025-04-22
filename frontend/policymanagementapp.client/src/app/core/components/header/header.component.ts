import { Component, OnInit, OnDestroy } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Subscription } from 'rxjs';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NavMenuComponent } from '../nav-menu/nav-menu.component';
import { MetadataService } from '../../services/metadata.service';
import { switchMap, tap } from 'rxjs/operators';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, NavMenuComponent],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent implements OnInit, OnDestroy {
  isLoggedIn = false;
  username = '';
  firstName = '';
  lastName = '';
  userRole: string = '';
  currentDate: Date = new Date();
  private userSubscription: Subscription | undefined;

  constructor(
    public authService: AuthService,
    private metadataService: MetadataService
  ) {}

  ngOnInit(): void {
    this.userSubscription = this.authService.user$.pipe(
      switchMap(user => {
        this.isLoggedIn = !!user;
        
        if (user) {
          this.username = user.username || '';
          this.firstName = user.firstName || '';
          this.lastName = user.lastName || '';
          
          if (user?.roles?.length) {
            const role = user.roles[0];
            
            return this.metadataService.getRoleDisplayName(role).pipe(
              tap(displayRole => {
                this.userRole = displayRole;
              })
            );
          }
        } else {
          this.firstName = '';
          this.lastName = '';
          this.userRole = '';
        }
        
        return [];
      })
    ).subscribe();
  }

  ngOnDestroy(): void {
    if (this.userSubscription) {
      this.userSubscription.unsubscribe();
    }
  }

  logout(): void {
    
    this.authService.logout().subscribe({
      next: () => {
      },
      error: (error) => {
        console.error('Logout error:', error);
      }
    });
  }
}
