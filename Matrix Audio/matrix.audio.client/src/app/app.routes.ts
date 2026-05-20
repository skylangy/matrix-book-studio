import { Routes } from '@angular/router';
import { AboutComponent } from './components/about/about.component';
import { AccountComponent } from './components/account/account.component';
import { AdminAlbumCollectionEditorComponent } from './components/admin/albums/admin.album.collection.editor.component';
import { AdminAlbumCollectionsComponent } from './components/admin/albums/admin.album.collections.component';
import { AdminAlbumEditorComponent } from './components/admin/albums/admin.album.editor.component';
import { AdminAlbumsComponent } from './components/admin/albums/admin.albums.component';
import { AdminArtistEditorComponent } from './components/admin/artists/admin.artist.editor.component';
import { AdminArtistsComponent } from './components/admin/artists/admin.artists.component';
import { DashboardComponent } from './components/admin/dashboard/dashboard.component';
import { AdminFaqEditorComponent } from './components/admin/faq/admin.faq.editor.component';
import { AdminFaqsComponent } from './components/admin/faq/admin.faqs.component';
import { AdminHomeComponent } from './components/admin/home/admin.home.component';
import { AdminLoginComponent } from './components/admin/login/admin.login.component';
import { AdminLogoutComponent } from './components/admin/logout/admin.logout.component';
import { AdminMaintenanceComponent } from './components/admin/maintenance/admin.maintenance.component';
import { AdminMessageViewComponent } from './components/admin/messages/admin.message.view.component';
import { AdminMessagesComponent } from './components/admin/messages/admin.messages.component';
import { AdminOrderEditorComponent } from './components/admin/orders/admin.order.editor.component';
import { AdminOrdersComponent } from './components/admin/orders/admin.orders.component';
import { AdminPostEditorComponent } from './components/admin/posts/admin.post.editor.component';
import { AdminPostsComponent } from './components/admin/posts/admin.posts.component';
import { AdminPromotionEditorComponent } from './components/admin/promotions/admin.promotion.editor.component';
import { AdminPromotionsComponent } from './components/admin/promotions/admin.promotions.component';
import { AdminSettingsComponent } from './components/admin/settings/admin.settings.component';
import { AdminSubscriptionEditorComponent } from './components/admin/subscriptions/admin.subscription.component';
import { AdminSubscriptionsComponent } from './components/admin/subscriptions/admin.subscriptions.component';
import { AdminOnlineUsersComponent } from './components/admin/users/admin.online.users.component';
import { AdminUserEditorComponent } from './components/admin/users/admin.user.editor.component';
import { AdminUsersComponent } from './components/admin/users/admin.users.component';
import { AlbumCollectionDetailsComponent } from './components/album-collection/album.collection.details.component';
import { AlbumCollectionsComponent } from './components/album-collection/album.collections.component';
import { AlbumDetailsComponent } from './components/album-details/album-details.component';
import { AlbumsComponent } from './components/albums/albums.component';
import { ArtistsComponent } from './components/artists/artists.component';
import { AuthorDetailsComponent } from './components/author-details/author-details.component';
import { CheckoutComponent } from './components/checkout/checkout.component';
import { StripeCheckoutComponent } from './components/checkout/stripe.checkout.component';
import { ContactComponent } from './components/contact/contact.component';
import { DiscoveryComponent } from './components/discover/discover.component';
import { EpisodeDetailsComponent } from './components/episode-details/episode-details.component';
import { ErrorComponent } from './components/error/error.component';
import { ForgotPasswordComponent } from './components/forgot-password/forgot-password.component';
import { HomeComponent } from './components/home/home.component';
import { LibraryComponent } from './components/library/library.component';
import { LoginComponent } from './components/login/login.component';
import { LogoutComponent } from './components/logout/logout.component';
import { PlansComponent } from './components/plans/plans.component';
import { PostDetailsComponent } from './components/post-details/post-details.component';
import { PostsComponent } from './components/posts/posts.component';
import { ProfileComponent } from './components/profile/profile.component';
import { RegisterComponent } from './components/register/register.component';
import { SearchComponent } from './components/search/search.component';
import { SettingsComponent } from './components/settings/settings.component';
import { ShoppingCartComponent } from './components/shopping-cart/shopping-cart.component';
import { WelcomeComponent } from './components/welcome/welcome.component';
import { AdminAuthGuard } from './services/admin.guard.service';
import { AuthGuard } from './services/auth.guard.service';

export const routes: Routes = [
    { path: '', redirectTo: 'public', pathMatch: 'full' },
    {
        path: 'public', component: HomeComponent,
        children: [
            { path: '', redirectTo: 'welcome', pathMatch: 'full' },
            { path: 'welcome', component: WelcomeComponent },
            { path: 'search', component: SearchComponent },
            { path: 'library', component: LibraryComponent },
            { path: 'discover', component: DiscoveryComponent },
            { path: 'album/:id', component: AlbumDetailsComponent },
            { path: 'albums/:source/:layout/:cardlayout/:title/:count/:paging', component: AlbumsComponent },
            { path: 'albums/:source/:layout/:cardlayout/:title/:count/:paging/:groupId', component: AlbumsComponent },
            { path: 'album-collections/:source/:layout/:cardlayout/:title/:count/:paging', component: AlbumCollectionsComponent },
            { path: 'album-collection/:id', component: AlbumCollectionDetailsComponent },
            { path: 'episode/:albumId/:episodeId', component: EpisodeDetailsComponent },
            { path: 'author/:id', component: AuthorDetailsComponent },
            { path: 'posts', component: PostsComponent },
            { path: 'post/:id', component: PostDetailsComponent },
            { path: 'artists/:source/:layout/:cardsize/:title/:page/:pageSize', component: ArtistsComponent },
            { path: 'contact', component: ContactComponent },
            { path: 'about', component: AboutComponent },
            { path: 'plans', component: PlansComponent },
            { path: 'account', component: AccountComponent, canActivate: [AuthGuard] },
            { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard] },
            { path: 'settings', component: SettingsComponent, canActivate: [AuthGuard] },
            { path: 'shopping-cart', component: ShoppingCartComponent, canActivate: [AuthGuard] },
            { path: 'checkout', component: CheckoutComponent, canActivate: [AuthGuard] },
            { path: 'secure/checkout', component: StripeCheckoutComponent, canActivate: [AuthGuard] },
            { path: '**', redirectTo: 'welcome' }
        ]
    },
    {
        path: 'control-tower', component: DashboardComponent, canActivate: [AdminAuthGuard],
        children: [
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: AdminHomeComponent },
            { path: 'albums', component: AdminAlbumsComponent },
            { path: 'edit/album/:id', component: AdminAlbumEditorComponent },
            { path: 'album-collections', component: AdminAlbumCollectionsComponent },
            { path: 'edit/album/collection/:id', component: AdminAlbumCollectionEditorComponent },
            { path: 'new/album-collection', component: AdminAlbumCollectionEditorComponent },
            { path: 'artists', component: AdminArtistsComponent },
            { path: 'edit/artist/:id', component: AdminArtistEditorComponent },
            { path: 'posts', component: AdminPostsComponent },
            { path: 'edit/post/:id', component: AdminPostEditorComponent },
            { path: 'new/post', component: AdminPostEditorComponent },
            { path: 'faqs', component: AdminFaqsComponent },
            { path: 'edit/faq/:id', component: AdminFaqEditorComponent },
            { path: 'new/faq', component: AdminFaqEditorComponent },
            { path: 'orders', component: AdminOrdersComponent },
            { path: 'edit/order/:id', component: AdminOrderEditorComponent },
            { path: 'subscriptions', component: AdminSubscriptionsComponent },
            { path: 'edit/subscription/:id', component: AdminSubscriptionEditorComponent },
            { path: 'new/subscription', component: AdminSubscriptionEditorComponent },
            { path: 'promotions', component: AdminPromotionsComponent },
            { path: 'edit/promotion/:id', component: AdminPromotionEditorComponent },
            { path: 'new/promotion', component: AdminPromotionEditorComponent },
            { path: 'messages', component: AdminMessagesComponent },
            { path: 'view/message/:id', component: AdminMessageViewComponent },
            { path: 'users', component: AdminUsersComponent },
            { path: 'online-users', component: AdminOnlineUsersComponent },
            { path: 'edit/user/:id', component: AdminUserEditorComponent },
            { path: 'settings', component: AdminSettingsComponent },
            { path: 'maintenance', component: AdminMaintenanceComponent },
            { path: 'logout', component: AdminLogoutComponent },
            { path: '**', redirectTo: 'home' }
        ]
    },
    {
        path: 'tower', component: AdminLoginComponent,
        children: [
            { path: 'login', component: AdminLoginComponent }
        ]
    },
    { path: 'login', component: LoginComponent },
    { path: 'logout', component: LogoutComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'forgot-password', component: ForgotPasswordComponent },
    { path: 'error', component: ErrorComponent },
    { path: '**', redirectTo: 'public' }
];
