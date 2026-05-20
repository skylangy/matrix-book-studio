import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import { JwtModule } from "@auth0/angular-jwt";

@NgModule({
    declarations: [

    ],
    imports: [
        BrowserModule,
        CommonModule,
        FormsModule,
        RouterModule,
        BrowserAnimationsModule,
        JwtModule
    ],
    providers: [],
    bootstrap: []
})
export class AppModule { }