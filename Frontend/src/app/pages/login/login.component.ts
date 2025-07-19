import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { UserService } from 'src/app/services/user_service/user.service';
import { LoadingService } from 'src/app/services/loading.service';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    RouterLink,
    MatSnackBarModule,
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit {
  userForm!: FormGroup;

  ngOnInit(): void {
    this.userForm = new FormGroup({
      email: new FormControl('', [Validators.email, Validators.required]),
      password: new FormControl('', Validators.required),
    });
  }

  constructor(
    private userService: UserService,
    private router: Router,
    private snackBar: MatSnackBar,
    private loadingService: LoadingService
  ) {}

  onSubmit() {
    if (this.userForm.valid) {
      // Show loading spinner
      this.loadingService.show();

      this.userService.login(this.userForm.value).subscribe({
        next: (res: any) => {
          console.log('Login Success:', res);
          console.log(' Login Response Structure:', res);

          //  FIXED: Store the actual JWT token (handle case sensitivity)
          const jwtToken = res.token || res.Token || res.accessToken || res.AccessToken || res.jwt || res.authToken;

          if (jwtToken) {
            localStorage.setItem('token', jwtToken);
            console.log(' JWT Token stored successfully');
            console.log(' Token preview:', jwtToken.substring(0, 50) + '...');
          } else {
            console.error(' No JWT token found in login response!');
            console.error(' Available properties:', Object.keys(res));
            console.error(' Check your backend login endpoint - it should return a JWT token');

            // Fallback: store whatever token-like property exists
            const fallbackToken = res.id || res.userId || res.data?.token;
            if (fallbackToken) {
              localStorage.setItem('token', fallbackToken);
              console.warn(' Using fallback token:', fallbackToken);
            }
          }

          // Hide loading spinner
          this.loadingService.hide();

          // Navigate to dashboard
          this.router.navigate(['/dashboard']);

          // Show success message
          this.snackBar.open('Login Successful!', 'Close', {
            duration: 1000,
            verticalPosition: 'top',
          });

          // Test JWT authentication after login
          setTimeout(() => {
            console.log(' Testing JWT authentication after login...');
            (window as any).testJwtFlow?.();
          }, 1000);
        },
        error: (err) => {
          console.log('Login Failed', err);

          // Hide loading spinner
          this.loadingService.hide();

          this.snackBar.open('Login Failed. Try again.', 'Close', {
            duration: 1000,
            verticalPosition: 'top',
          });
        },
      });
    }
  }
}
