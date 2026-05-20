export interface KeyHandler {
    handleKeyDown(event: KeyboardEvent, context?: any): void;

    canHandle(event: KeyboardEvent): boolean;
}

export class CtrlSHandler implements KeyHandler {
    handleKeyDown(event: KeyboardEvent, context?: any): void {
        event.preventDefault();
        context?.editor?.save();
    }

    canHandle(event: KeyboardEvent): boolean {
        return event.ctrlKey && event.key === 's';
    }
}

export class CtrlQHandler implements KeyHandler {
    handleKeyDown(event: KeyboardEvent, context?: any): void {
        event.preventDefault();
        context?.router.navigate(['/']);
    }

    canHandle(event: KeyboardEvent): boolean {
        return event.ctrlKey && event.key === 'q';
    }
}

export class CtrlNHandler implements KeyHandler {
    handleKeyDown(event: KeyboardEvent, context?: any): void {
        event.preventDefault();
        context?.router.navigate(['/new']);
    }

    canHandle(event: KeyboardEvent): boolean {
        return event.ctrlKey && event.key === 'n';
    }
}

export class CtrlJHandler implements KeyHandler {
    handleKeyDown(event: KeyboardEvent, context?: any): void {
        event.preventDefault();
        context?.editor?.insert('第七章 ');
    }

    canHandle(event: KeyboardEvent): boolean {
        return event.ctrlKey && event.key === 'j';
    }
}

export class CtrlOHandler implements KeyHandler {
    handleKeyDown(event: KeyboardEvent, context?: any): void {
        event.preventDefault();
        context?.editor?.insert('。', false, true);
    }

    canHandle(event: KeyboardEvent): boolean {
        return event.ctrlKey && event.key === 'o';
    }
}

export class CtrlAltVHandler implements KeyHandler {
    handleKeyDown(event: KeyboardEvent, context?: any): void {
        event.preventDefault();
        context?.editor?.paste();
    }

    canHandle(event: KeyboardEvent): boolean {
        return event.ctrlKey && event.altKey && event.key === 'v';
    }
}

export class VideoCtrlSHandler implements KeyHandler {
    handleKeyDown(event: KeyboardEvent, context?: any): void {
        event.preventDefault();
        context?.save();
    }

    canHandle(event: KeyboardEvent): boolean {
        return event.ctrlKey && event.key === 's';
    }
}