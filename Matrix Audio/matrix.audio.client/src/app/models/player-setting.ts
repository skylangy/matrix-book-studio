import { NamedSelectableValue } from "./namevalue";

export class PlayerSetting {
    static readonly speedKey = 'player:speed';
    static readonly compactKey = 'player:isCompact';

    sleepSettings: NamedSelectableValue[] = [
        { name: 'No Delay', value: 0, selected: true },
        { name: '1 min', value: 60, selected: false },
        { name: '15 min', value: 15 * 60, selected: false },
        { name: '30 min', value: 30 * 60, selected: false },
        { name: '45 min', value: 45 * 60, selected: false },
        { name: '1 hour', value: 60 * 60, selected: false }
    ];
    speedSettings: NamedSelectableValue[] = [
        { name: '50%', value: 0.5, selected: false },
        { name: '75%', value: 0.75, selected: false },
        { name: '80%', value: 0.8, selected: false },
        { name: '90%', value: 0.9, selected: false },
        { name: '100%', value: 1, selected: true },
        { name: '110%', value: 1.1, selected: false },
        { name: '125%', value: 1.25, selected: false },
        { name: '150%', value: 1.5, selected: false },
        { name: '175%', value: 1.75, selected: false },
        { name: '200%', value: 2, selected: false }
    ];

    getSleepIcon(isPlaying: boolean): string {
        return isPlaying ? 'bi-moon-stars text-success-emphasis' : 'bi-moon-stars';
    }

    getSpeedIcon(speed: number): string {
        return speed === 1 ? 'bi-speedometer2 text-success-emphasis' : 'bi-speedometer2';
    }
}