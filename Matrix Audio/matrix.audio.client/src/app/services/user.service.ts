import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { ILogger } from '../models/logger';
import { LoggingService } from './logging.service';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { User, UserSubscription } from '../models/user';
import { Result } from '../models/result';
import { UserSettings } from '../models/user-setting';
import { IInitializable } from '../models/initializable';
import { SubscriptionPlan } from '../models/subscription-plan';
import { Message } from '../models/message';
import { SettingNames } from '../models/app-setting';

@Injectable({
    providedIn: 'root'
})
export class UserService implements IInitializable {
    private static DefautLanguage = 'en-US';
    private static DefaultTheme = 'dark';
    private static SupportedThemes = ['dark', 'light'];
    private logger: ILogger
    private userSubscription: UserSubscription | undefined;
    private _userSettings: UserSettings | undefined;

    avatarUpdated = new Subject<string>();

    constructor(
        private apiService: ApiService,
        private authService: AuthService,
        loggingService: LoggingService) {
        this.logger = loggingService.getLogger('UserService');
    }

    get userId(): string | undefined {
        return this.authService.user?.id;
    }

    get subscription(): UserSubscription | undefined {
        return this.userSubscription;
    }

    get hasSubscription(): boolean {
        return this.userSubscription?.isActive ?? false;
    }

    get userSettings(): UserSettings | undefined {
        return this._userSettings;
    }

    async initialize(): Promise<void> {
        this.userSubscription = await this.getCurrentUserSubscription();
        this._userSettings = await this.getUserSettings();

        let theme = this.getTheme();
        this.applyTheme(theme);
    }

    canPlayAlbum(albumLevel: number): boolean {
        if (!this.hasSubscription)
            return false;

        let userLevel = this.authService.user?.level || 1000;

        return userLevel >= albumLevel;
    }

    getSetting(name: string, defaultValue = ''): string {
        if (!this._userSettings)
            return defaultValue;
        let setting = this._userSettings!.settings?.find(s => s.name === name);
        return setting?.value ?? defaultValue;
    }

    getLanguage(): string {
        if (this._userSettings) {
            return this._userSettings!.settings?.find(s => s.name === SettingNames.Language)?.value || UserService.DefautLanguage;
        }
        return UserService.DefautLanguage;
    }

    getTheme(): string {
        if (this._userSettings) {
            return this._userSettings!.settings?.find(s => s.name === SettingNames.Theme)?.value || UserService.DefaultTheme;
        }
        return UserService.DefaultTheme;
    }

    async searchUsers(keyword: string, page = 1, pageSize = 10): Promise<User[]> {
        return this.apiService.get(`user/search/${keyword}/${page}/${pageSize}`);
    }

    async getUsers(page = 1, pageSize = 10): Promise<User[]> {
        return this.apiService.get(`user/${page}/${pageSize}`);
    }

    async getOnlineUsers(page = 1, pageSize = 10): Promise<User[]> {
        return this.apiService.get(`user/online/${page}/${pageSize}`);
    }

    async getCurrentUser(): Promise<User | undefined> {
        return this.getUser(this.userId);
    }

    async getUser(userId?: string): Promise<User | undefined> {
        if (!userId)
            return undefined;
        return this.apiService.get(`auth/user/${userId}`);
    }

    async updateProfile(profile: any) {
        return this.apiService.post(`auth/user/profile`, profile);
    }

    async favoriteAlbum(albumId?: string) {
        if (!albumId || !this.userId)
            return;
        return this.apiService.post(`user/favorite/album/${this.userId}/${albumId}`, {});
    }

    async unfavoriteAlbum(albumId?: string) {
        if (!albumId || !this.userId)
            return;
        return this.apiService.post(`user/unfavorite/album/${this.userId}/${albumId}`, {});
    }

    async likeAlbum(albumId?: string) {
        if (!albumId || !this.userId)
            return;
        return this.apiService.post(`user/like/album/${this.userId}/${albumId}`, {});
    }

    async addToPlayList(albumId?: string) {
        if (!albumId || !this.userId)
            return;
        return this.apiService.post(`user/playlist/add/${this.userId}/${albumId}`, {});
    }

    async removeFromPlayList(albumId?: string) {
        if (!albumId || !this.userId)
            return;
        return this.apiService.post(`user/playlist/remove/${this.userId}/${albumId}`, {});
    }

    async downloadEpisode(albumId?: string, episodeId?: string, fileName?: string) {
        if (!episodeId || !this.userId)
            return;
        return this.apiService.downloadFile(`user/download/episode/${this.userId}/${albumId}/${episodeId}`, {}, fileName);
    }

    async downloadAlbum(albumId?: string) {
        if (!albumId || !this.userId)
            return;
        return this.apiService.post(`user/download/album/${this.userId}/${albumId}`, {});
    }

    async playAlbum(albumId?: string, episodeId?: string) {
        if (!albumId || !this.userId)
            return;
        return this.apiService.post(`user/play/album/${this.userId}/${albumId}/${episodeId}`, {});
    }

    async updatePlayRecord(albumId?: string, episodeId?: string, position?: number, duration?: number) {
        if (!albumId || !episodeId || !this.userId)
            return;
        this.logger.info(`Updating play record for ${position?.toFixed(2)}s, ${duration?.toFixed(2)}s`);
        return this.apiService.post(`user/record/play`, { albumId, episodeId, userId: this.userId, position, duration });
    }

    async leaveMessage(message: any): Promise<Result> {
        return this.apiService.post(`usermessage/send`, message);
    }

    async getMessages(page = 1, pageSize = 10): Promise<Message[]> {
        return this.apiService.get(`usermessage/${page}/${pageSize}`);
    }

    async getMessage(id: string): Promise<Message> {
        return this.apiService.get(`usermessage/${id}`);
    }

    async searchMessages(keyword: string, page = 1, pageSize = 10): Promise<any[]> {
        return this.apiService.get(`usermessage/search/${keyword}/${page}/${pageSize}`);
    }

    async subscribe(email: string): Promise<Result> {
        return this.apiService.post(`user/subscribe/${email}`, {});
    }

    async getUserSettings(): Promise<UserSettings> {
        return this.apiService.get(`user/settings/${this.userId}`);
    }

    async updateUserSettings(settings: UserSettings) {
        return this.apiService.post(`user/settings`, settings);
    }

    async getAvatars() {
        return this.apiService.get(`image/avatars`);
    }

    async updateAvatar(avatar: string) {
        let result = await this.apiService.post(`user/avatar/${this.userId}/${avatar}`, { userId: this.userId, avatar });

        if (result.success)
            this.avatarUpdated.next(avatar);
        return result
    }

    async getCurrentUserSubscription(): Promise<UserSubscription> {
        return this.getUserSubscription(this.userId!);
    }

    async getUserSubscription(userId: string): Promise<UserSubscription> {
        return this.apiService.get(`user/subscription/${userId}`);
    }

    async getSubscriptions(page = 1, pageSize = 10): Promise<SubscriptionPlan[]> {
        return this.apiService.get(`subscriptionplan/${page}/${pageSize}`);
    }

    async getSubscription(id: string): Promise<SubscriptionPlan> {
        return this.apiService.get(`subscriptionplan/${id}`);
    }

    async getAssignableSubscriptions(): Promise<SubscriptionPlan[]> {
        return this.apiService.get(`subscriptionplan/assignable`);
    }

    async assignSubscription(userId: string, subscriptionId: string, periodInDays: number): Promise<Result> {
        return this.apiService.post(`user/assign/subscription`, { userId: userId, subscriptionId: subscriptionId, periodInDays: periodInDays });
    }

    async populateSubscriptionFromConfig(): Promise<Result> {
        return this.apiService.post(`subscriptionplan/populate/from/config`, {});
    }

    applyTheme(theme: string) {
        if (theme && UserService.SupportedThemes.includes(theme)) {
            document.documentElement.setAttribute('data-bs-theme', theme);
        }
    }
}