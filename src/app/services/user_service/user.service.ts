import { Injectable } from '@angular/core';
import { HttpService } from '../http_service/http.service';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  constructor(private http: HttpService) {}

  login(payload: any) {
    return this.http.postApi('/user/login/', payload);
  }

  register(payload: any) {
    return this.http.postApi('/user/userSignUp/', payload);
  }
}
