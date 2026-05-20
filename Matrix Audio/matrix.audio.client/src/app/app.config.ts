import { ApplicationConfig, importProvidersFrom, inject, provideAppInitializer } from '@angular/core';
import { provideRouter } from '@angular/router';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { JwtModule } from "@auth0/angular-jwt";
import { routes } from './app.routes';
import { AuthInterceptor } from './services/auth.interceptor';
import { InitializerService } from './services/initializer.service';

async function initializeApp(initializer: InitializerService) {
  await initializer.initialize();
  return;
}

export function tokenGetter() {
  return localStorage.getItem("token");
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    provideAppInitializer(() => initializeApp(inject(InitializerService))),
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    importProvidersFrom(JwtModule.forRoot({
      config: {
        tokenGetter: tokenGetter,
        allowedDomains: ['localhost:8960'],
        disallowedRoutes: ['localhost:8960/auth/login']
      }
    }))
  ]
};
