import { Injectable } from '@angular/core';

export interface AccessTokenDto {
  accessToken: string;
  refreshToken?: string | null;
  expiresIn?: number;
  refreshExpiresIn?: number;
}

export interface UserInfoDto {
  id?: string | null;
  username?: string | null;
  userName?: string | null;
  roles?: string[] | null;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly authKeys = ['accessToken', 'refreshToken', 'username'];
  isLogin = false;
  isAdmin = false;
  userName?: string | null = null;
  id?: string | null = null;
  constructor() {
    this.updateUserLoginState();
  }

  saveToken(token: AccessTokenDto): void {
    this.isLogin = true;
    localStorage.setItem("accessToken", token.accessToken);
    if (token.refreshToken) {
      localStorage.setItem("refreshToken", token.refreshToken);
    }

  }

  saveUserInfo(userinfo: UserInfoDto): void {
    this.isLogin = true;
    this.userName = userinfo.username ?? userinfo.userName ?? null;
    if (this.userName) {
      localStorage.setItem("username", this.userName);
    }
  }

  updateUserLoginState(): void {
    const username = localStorage.getItem('username');
    const token = localStorage.getItem('accessToken');
    if (token && username) {
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
    this.isLogin = false;
  }
}
