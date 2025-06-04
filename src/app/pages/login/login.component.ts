import { Component, OnInit, inject } from '@angular/core';
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
    private snackBar: MatSnackBar
  ) {}

  onSubmit() {
    if (this.userForm.valid) {
      this.userService.login(this.userForm.value).subscribe({
        next: (res: any) => {
          console.log('Login Success:', res);
          localStorage.setItem('token', res.id);
          this.router.navigate(['/dashboard']);
          this.snackBar.open('Login Successful!', 'Close', {
            duration: 1000,
            verticalPosition: 'top',
          });
        },
        error: (err) => {
          console.log('Login Failed', err);
          this.snackBar.open('Login Failed. Try again.', 'Close', {
            duration: 1000,
            verticalPosition: 'top',
          });
        },
      });
    }
  }
}
