import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root',
})
export class BrowserDetectionService {
    getBrowserInfo(): string {
        const userAgent = window.navigator.userAgent;

        if (userAgent.includes('Firefox')) {
            return 'Firefox';
        } else if (userAgent.includes('Edg')) {
            return 'Edge';
        } else if (userAgent.includes('Chrome')) {
            return 'Chrome';
        } else if (userAgent.includes('Safari')) {
            return 'Safari';
        } else {
            return 'Unknown';
        }
    }

    getOSInfo(): string {
        const userAgent = window.navigator.userAgent;

        if (userAgent.includes('Win')) {
            return 'Windows';
        } else if (userAgent.includes('Mac')) {
            return 'MacOS';
        } else if (userAgent.includes('Linux')) {
            return 'Linux';
        } else if (/Android/.test(userAgent)) {
            return 'Android';
        } else if (/iPhone|iPad|iPod/.test(userAgent)) {
            return 'iOS';
        } else {
            return 'Unknown';
        }
    }

    isMediaSourceSupported(): boolean {
        try {
            return 'MediaSource' in window;
        }
        catch (e) {
            return false;
        }
    }
}
