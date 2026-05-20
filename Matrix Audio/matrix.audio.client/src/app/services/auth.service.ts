import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { User } from '../models/user';
import { Result } from '../models/result';
import { IInitializable } from '../models/initializable';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class AuthService implements IInitializable {
    private static readonly TokenKey = 'token';
    private static readonly RefreshTokenKey = 'refreshToken';
    private static readonly UserKey = 'user';

    private logInUser: User | undefined = undefined;

    constructor(
        private readonly apiService: ApiService,
        private readonly jwtHelper: JwtHelperService
    ) { }

    get user(): User | undefined {
        return this.logInUser;
    }

    get accessToken(): string | null {
        return localStorage.getItem(AuthService.TokenKey);
    }

    set accessToken(token: string | null) {
        localStorage.setItem(AuthService.TokenKey, token!);
    }

    get refreshToken(): string | null {
        return localStorage.getItem(AuthService.RefreshTokenKey);
    }

    set refreshToken(token: string | null) {
        localStorage.setItem(AuthService.RefreshTokenKey, token!);
    }

    isLoggedIn(): boolean {
        return localStorage.getItem(AuthService.TokenKey) !== null &&
            this.logInUser !== undefined &&
            this.logInUser.id !== undefined;
    }

    isTokenExpired(token: string): boolean {
        try {
            return this.jwtHelper.isTokenExpired(token);
        } catch (e) {
            return true;
        }
    }

    isAdmin(): boolean {
        return localStorage.getItem(AuthService.TokenKey) !== null &&
            this.logInUser !== undefined &&
            this.logInUser.id !== undefined;
    }

    async initialize(): Promise<void> {
        let token = localStorage.getItem(AuthService.TokenKey);
        if (token) {
            let user = localStorage.getItem(AuthService.UserKey) ? JSON.parse(localStorage.getItem(AuthService.UserKey)!) : undefined;
            if (user) {
                this.logInUser = user;
            }
        }
    }

    async login(email: string, password: string): Promise<boolean> {
        let result = await this.apiService.post('auth/login', { email, password });

        if (result && result.token) {
            let decodedToken = this.jwtHelper.decodeToken(result.token);
            let role: string = decodedToken['role'];
            if (decodedToken && role && role.toLowerCase() === 'user') {
                localStorage.setItem(AuthService.TokenKey, result.token);
                localStorage.setItem(AuthService.RefreshTokenKey, result.refreshToken);
                localStorage.setItem(AuthService.UserKey, JSON.stringify(result.user));
                this.logInUser = result.user;
                return true;
            }
        }

        return false;
    }

    async loginTower(email: string, password: string, token: string): Promise<boolean> {
        let result = await this.apiService.post('auth/login/tower', { email, password, token });

        if (result && result.token) {
            let decodedToken = this.jwtHelper.decodeToken(result.token);
            let role: string = decodedToken['role'];
            if (decodedToken && role && role.toLowerCase() === 'admin') {
                localStorage.setItem(AuthService.TokenKey, result.token);
                localStorage.setItem(AuthService.RefreshTokenKey, result.refreshToken);
                localStorage.setItem(AuthService.UserKey, JSON.stringify(result.user));
                this.logInUser = result.user;
                return true;
            }
        }

        return false;
    }

    async register(email: string, name: string, password: string): Promise<boolean> {
        let result = await this.apiService.post('auth/register', { email, name, password });
        return result.success;
    }

    async resetPassword(email: string): Promise<boolean> {
        return await this.apiService.post('auth/password/reset', { email });
    }

    async updatePassword(resetViewModel: any): Promise<Result> {
        return await this.apiService.post('auth/password/update', resetViewModel);
    }

    logout() {
        localStorage.removeItem(AuthService.TokenKey);
        localStorage.removeItem(AuthService.RefreshTokenKey);
        localStorage.removeItem(AuthService.UserKey);
    }

    renewToken(): Observable<any> {
        let refreshToken = localStorage.getItem(AuthService.RefreshTokenKey);
        let accessToken = localStorage.getItem(AuthService.TokenKey);

        return this.apiService.postRaw('auth/refresh', { token: accessToken, refreshToken });
    }
}