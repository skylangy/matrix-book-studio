import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { WelcomeComponent } from './components/welcome/welcome.component';
import { CategoriesComponent } from './components/categories/categories.component';
import { AuthorsComponent } from './components/authors/authors.component';
import { SettingsComponent } from './components/settings/settings.component';
import { BookImporterComponent } from './components/book-importer/book.import.component';
import { BookEditorComponent } from './components/book-editor/book.editor.component';
import { StatusBooksComponent } from './components/status-books/status.books.component';
import { StatisticsComponent } from './components/statistics/statistics.component';
import { AuthorManageComponent } from './components/author-manage/author-manage.component';
import { AuthorEditorComponent } from './components/author-editor/author-editor.component';

const routes: Routes = [
  { path: '', redirectTo: '/welcome', pathMatch: 'full' },
  { path: 'welcome', component: WelcomeComponent },
  { path: 'categories', component: CategoriesComponent },
  { path: 'authors', component: AuthorsComponent },
  { path: 'status/:status?', component: StatusBooksComponent },
  { path: 'settings', component: SettingsComponent },
  { path: 'import', component: BookImporterComponent },
  { path: 'new', component: BookEditorComponent, canDeactivate: [(component: BookEditorComponent) => !component.isDirty] },
  { path: 'edit/:id?/:name?', component: BookEditorComponent, canDeactivate: [(component: BookEditorComponent) => !component.isDirty] },
  { path: 'statistics', component: StatisticsComponent },
  { path: 'author-manage', component: AuthorManageComponent },
  { path: 'new-author', component: AuthorEditorComponent },
  { path: 'edit/author/:id?', component: AuthorEditorComponent },
  { path: '**', redirectTo: '/welcome' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
