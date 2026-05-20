import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ClipboardModule } from '@angular/cdk/clipboard';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { AppRoutingModule } from './app-routing.module';
import { HighchartsChartModule } from 'highcharts-angular';

import { ScrollingModule } from '@angular/cdk/scrolling';

import { MONACO_EDITOR_CONFIG, MonacoEditorConfig } from './components/monaco-editor/models';

const monacoConfig: MonacoEditorConfig = {
  baseUrl: 'assets',
  defaultOptions: { scrollBeyondLastLine: true },
  onMonacoLoad: () => {
    // console.log((<any>window).monaco);
  }
};


@NgModule({
  exports: [],
  imports: [
    CommonModule,
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    ClipboardModule,
    DragDropModule,
    AppRoutingModule,
    HighchartsChartModule,
    ScrollingModule,],

  providers: [
    { provide: MONACO_EDITOR_CONFIG, useValue: monacoConfig },
    provideHttpClient(withInterceptorsFromDi())
  ]
})
export class AppModule { }
