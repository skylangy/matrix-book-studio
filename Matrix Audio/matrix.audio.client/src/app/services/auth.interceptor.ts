import { Injectable } from '@angular/core';
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';
import { ILogger } from '../models/logger';
import { LoggingService } from './logging.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    private readonly logger: ILogger;
    private skipUrls = ['login', 'auth/refresh'];

    constructor(
        private authService: AuthService,
        private router: Router,
        loggingService: LoggingService
    ) {
        this.logger = loggingService.getLogger('AuthInterceptor');
    }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        if (this.shouldSkip(request.url)) {
            return next.handle(request)
                .pipe(
                    catchError((error: HttpErrorResponse) => {
                        this.logger.error('Error occurred', error);
                        if (error.status === 401) {
                            this.authService.logout();
                            this.router.navigate(['/login']);
                        }
                        return throwError(error);
                    })
                );
        }

        const token = this.authService.accessToken;
        if (token) {
            if (this.authService.isTokenExpired(token)) {
                this.logger.info('Token expired, renewing token', request);
                return this.authService
                    .renewToken()
                    .pipe(
                        switchMap((response: any) => {
                            this.authService.accessToken = response.token;
                            this.authService.refreshToken = response.refreshToken;

                            this.logger.info('Token renewed successfully');
                            const clonedRequest = request.clone({
                                setHeaders: {
                                    Authorization: `Bearer ${response.accessToken}`,
                                },
                            });
                            this.logger.info('Token renewed successfully');
                            return next.handle(clonedRequest);
                        }),
                        catchError((error) => {
                            this.logger.error('Token renewal failed', error);
                            this.authService.logout(); // Logout if token renewal fails
                            this.router.navigate(['/login']);
                            return throwError(() => error);
                        }));
            } else {
                request = request.clone({
                    setHeaders: {
                        Authorization: `Bearer ${token}`
                    }
                });
            }
        } else {
            // this.logger.info('Token is not valid or does not exist, will logout');
            // this.authService.logout();
            // this.router.navigate(['/login']);
        }

        return next.handle(request).pipe(
            catchError((error: HttpErrorResponse) => {
                this.logger.error('Error occurred', error);
                if (error.status === 401) {
                    this.authService.logout();
                    this.router.navigate(['/login']);
                }
                return throwError(error);
            })
        );
    }

    private shouldSkip(url: string): boolean {
        return this.skipUrls.some(skipUrl => url.includes(skipUrl));
    }
}
