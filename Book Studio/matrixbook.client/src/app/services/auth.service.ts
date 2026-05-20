import { Injectable } from '@angular/core';
import { ApiService } from './api-service';
import { User } from '../models/user';


@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private logInUser: User | undefined = undefined;

    constructor(private apiService: ApiService
    ) { }

    get user(): User | undefined {
        return this.logInUser;
    }

    public isLoggedIn(): boolean {
        return localStorage.getItem('token') !== null &&
            this.logInUser !== undefined &&
            this.logInUser.id !== undefined;
    }

    async checkAuth() {
        let token = localStorage.getItem('token');
        if (token) {
            let user = localStorage.getItem('user') ? JSON.parse(localStorage.getItem('user')!) : undefined;
            if (user) {
                this.logInUser = user;
            }
        }
    }

    public async login(email: string, password: string): Promise<boolean> {
        let result = await this.apiService.post('auth/login', { email, password });

        if (result && result.token) {
            localStorage.setItem('token', result.token);
            localStorage.setItem('user', JSON.stringify(result.user));
            this.logInUser = result.user;
            return true;
        }

        return false;
    }

    async register(email: string, name: string, password: string): Promise<boolean> {
        let result = await this.apiService.post('auth/register', { email, name, password });
        return result.success;
    }

    async resetPassword(email: string): Promise<boolean> {
        return await this.apiService.post('auth/reset', { email });
    }

    public logout() {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
    }
}