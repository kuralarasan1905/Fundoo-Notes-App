import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  ValidatorFn,
  AbstractControl,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { UserService } from 'src/app/services/user_service/user.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    MatCheckboxModule,
    RouterLink,
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent implements OnInit {
  userForm!: FormGroup;
  showPassword = false;

  ngOnInit(): void {
    this.userForm = new FormGroup(
      {
        firstName: new FormControl('', [Validators.required]),
        lastName: new FormControl('', [Validators.required]),
        email: new FormControl('', [Validators.required, Validators.email]),
        password: new FormControl('', [Validators.required]),
        confirm: new FormControl('', [Validators.required]),
      },
      { validators: this.passwordCheck }
    );
  }

  constructor(private userService: UserService, private router: Router) {}

  onSubmit() {
    if (this.userForm.valid) {
      const payload = {
        firstName: this.userForm.value.firstName,
        lastName: this.userForm.value.lastName,
        email: this.userForm.value.email,
        service: 'advance',
        password: this.userForm.value.password,
      };

      this.userService.register(payload).subscribe({
        next: (res: any) => {
          console.log('Signup Success:', res);
          this.router.navigate(['/login']);
        },
        error: (err) => {
          console.log('Signup Failed', err);
        },
      });
    } else {
      this.userForm.markAllAsTouched();
    }
  }

  visibility() {
    this.showPassword = !this.showPassword;
  }

  passwordCheck: ValidatorFn = (
    group: AbstractControl
  ): { [key: string]: any } | null => {
    const password = group.get('password');
    const confirm = group.get('confirm');

    if (!password || !confirm) return null;

    if (password.value !== confirm.value) {
      confirm.setErrors({ passMismatch: true });
      return { passMismatch: true };
    } else {
      if (confirm.hasError('passMismatch')) {
        confirm.setErrors(null);
      }
      return null;
    }
  };
}
