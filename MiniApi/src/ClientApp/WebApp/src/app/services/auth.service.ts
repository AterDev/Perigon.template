import { Injectable } from '@angular/core';

export interface UserInfoDto {
  id: string;
  userName: string;
  nickName: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly authKeys = ['userId', 'username'];
  isLogin = false;
  isAdmin = false;
  userName?: string | null = null;
  id?: string | null = null;
  constructor() {
    this.updateUserLoginState();
  }

  saveUserInfo(userinfo: UserInfoDto): void {
    this.isLogin = true;
    this.id = userinfo.id;
    this.userName = userinfo.userName;
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.setItem('userId', userinfo.id);
    if (this.userName) {
      localStorage.setItem('username', this.userName);
    }
  }

  updateUserLoginState(): void {
    const username = localStorage.getItem('username');
    const id = localStorage.getItem('userId');
    if (username && id) {
      this.id = id;
      this.userName = username;
      this.isLogin = true;
    } else {
      this.isLogin = false;
    }
  }
  logout(): void {
    for (const key of this.authKeys) {
      localStorage.removeItem(key);
    }
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    this.isLogin = false;
    this.id = null;
    this.userName = null;
  }
}
