
import { Routes } from '@angular/router';
import { AuthorEditorComponent } from './components/author-editor/author-editor.component';
import { AuthorManageComponent } from './components/author-manage/author-manage.component';
import { AuthorsComponent } from './components/authors/authors.component';
import { BookEditorComponent } from './components/book-editor/book.editor.component';
import { BookImporterComponent } from './components/book-importer/book.import.component';
import { CategoriesComponent } from './components/categories/categories.component';
import { NarrationEditorComponent } from './components/narration/narration.editor.component';
import { NarrationManagerComponent } from './components/narration/narration.manager.component';
import { PublishQueueComponent } from './components/publish-queue/publish-queue.component';
import { SettingsComponent } from './components/settings/settings.component';
import { StatisticsComponent } from './components/statistics/statistics.component';
import { StatusBooksComponent } from './components/status-books/status.books.component';
import { TagsViewComponent } from './components/tags/tags-view.component';
import { VideoTemplateEditorComponent } from './components/video-template/video.template.editor.component';
import { VideoTemplateManageComponent } from './components/video-template/video.template.manage.component';
import { VideoEditorComponent } from './components/video/video.editor.component';
import { VideoManageComponent } from './components/video/video.manage.component';
import { WelcomeComponent } from './components/welcome/welcome.component';

export const routes: Routes = [
    { path: '', redirectTo: '/welcome', pathMatch: 'full' },
    { path: 'welcome', component: WelcomeComponent },
    { path: 'categories', component: CategoriesComponent },
    { path: 'tags', component: TagsViewComponent },
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
    { path: 'publish-queue', component: PublishQueueComponent },
    { path: 'videos', component: VideoManageComponent },
    { path: 'videos/new', component: VideoEditorComponent },
    { path: 'videos/edit/:id?', component: VideoEditorComponent },
    { path: 'narrations', component: NarrationManagerComponent },
    { path: 'narrations/new', component: NarrationEditorComponent },
    { path: 'narrations/edit/:id?', component: NarrationEditorComponent },
    { path: 'video/templates', component: VideoTemplateManageComponent },
    { path: 'video/templates/new', component: VideoTemplateEditorComponent },
    { path: 'video/templates/edit/:id?', component: VideoTemplateEditorComponent },
    { path: '**', redirectTo: '/welcome' }
];