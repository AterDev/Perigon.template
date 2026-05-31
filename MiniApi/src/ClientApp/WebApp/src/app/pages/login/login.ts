import { Component, Inject, OnInit, AfterViewInit, signal } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { Router } from '@angular/router';
import { CommonFormModules } from 'src/app/share/shared-modules';
import { AccessTokenDto, AuthService, UserInfoDto } from 'src/app/services/auth.service';
import { TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
import { initStarfield } from './starfield';

interface SystemLoginDto {
  userName: string;
  password: string;
}


@Component({
  selector: 'app-login',
  imports: [CommonFormModules, MatCardModule],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class Login implements OnInit, AfterViewInit {
  i18nKeys = I18N_KEYS;
  isLoading = signal(false);

  constructor(
    private authService: AuthService,
    private router: Router,
    private http: HttpClient,
    private translate: TranslateService,
    @Inject('ADMIN_BASE_URL') private adminBaseUrl: string
  ) {
    if (authService.isLogin) {
      this.router.navigate(['/index']);
    }
  }

  loginForm = new FormGroup({
    userName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(4), Validators.maxLength(60)]
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(6), Validators.maxLength(60)]
    })
  });

  get userName(): FormControl<string> {
    return this.loginForm.controls.userName;
  }

  get password(): FormControl<string> {
    return this.loginForm.controls.password;
  }

  ngOnInit(): void {
  }

  ngAfterViewInit(): void {
    const canvas = document.getElementById('starfield') as HTMLCanvasElement | null;
    if (canvas) {
      initStarfield(canvas);
    }
  }

  getValidatorMessage(control: AbstractControl): string {
    if (!control.errors) {
      return '';
    }

    if (control.hasError('required')) {
      return this.translate.instant(this.i18nKeys.validation.required);
    }
    if (control.hasError('minlength')) {
      return this.translate.instant(this.i18nKeys.validation.minlength, control.getError('minlength'));
    }
    if (control.hasError('maxlength')) {
      return this.translate.instant(this.i18nKeys.validation.maxlength, control.getError('maxlength'));
    }
    return this.translate.instant(this.i18nKeys.common.formInvalid);
  }

  doLogin(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const formValue = this.loginForm.getRawValue();
    const loginData = {
      userName: formValue.userName,
      password: formValue.password
    };

    this.http.post<AccessTokenDto>(`${this.adminBaseUrl}/api/systemUser/authorize`, loginData)
      .subscribe({
        next: (res) => {
          this.authService.saveToken(res);
          this.getUserInfo();
        },
        error: (error: any) => {
          this.isLoading.set(false);
          console.error('Login failed:', error);
          // 这里可以添加错误提示处理
        }
      });
  }

  getUserInfo(): void {
    this.http.get<UserInfoDto>(`${this.adminBaseUrl}/api/systemUser/userinfo`)
      .subscribe({
        next: (res) => {
          this.isLoading.set(false);
          this.authService.saveUserInfo(res);
          this.router.navigate(['/index']);
        },
        error: (error: any) => {
          this.isLoading.set(false);
          console.error('Get user info failed:', error);
        }
      });
  }

  logout(): void {
    this.authService.logout();
  }
}
