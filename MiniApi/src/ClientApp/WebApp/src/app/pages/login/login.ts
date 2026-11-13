import { Component, OnInit, AfterViewInit, signal, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { Router } from '@angular/router';
import { CommonFormModules } from 'src/app/modules/share/shared-modules';
import { AuthService, UserInfoDto } from 'src/app/services/auth.service';
import { TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/modules/share/i18n-keys';
import { initStarfield } from './starfield';

interface UserLoginDto {
  userName: string;
  password: string;
}


@Component({
  selector: 'app-login',
  imports: [...CommonFormModules, MatCardModule],
  templateUrl: './login.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./login.scss']
})
export class Login implements OnInit, AfterViewInit {
  i18nKeys = I18N_KEYS;
  isLoading = signal(false);

  constructor(
    private authService: AuthService,
    private router: Router,
    private http: HttpClient,
    private translate: TranslateService
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
    const loginData: UserLoginDto = {
      userName: formValue.userName,
      password: formValue.password
    };

    this.http.post<UserInfoDto>('/api/users/login', loginData)
      .subscribe({
        next: (res) => {
          this.isLoading.set(false);
          this.authService.saveUserInfo(res);
          this.router.navigate(['/index']);
        },
        error: (error: any) => {
          this.isLoading.set(false);
          console.error('Login failed:', error);
          // 这里可以添加错误提示处理
        }
      });
  }

  logout(): void {
    this.authService.logout();
  }
}
