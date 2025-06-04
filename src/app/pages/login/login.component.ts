import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
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

  constructor(private userService: UserService, private router: Router) {}

  onSubmit() {
    if (this.userForm.valid) {
      this.userService.login(this.userForm.value).subscribe({
        next: (res: any) => {
          console.log('Login Success:', res);
          localStorage.setItem('token', res.id);
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          console.log('Login Failed', err);
        },
      });
    }
  }
}
