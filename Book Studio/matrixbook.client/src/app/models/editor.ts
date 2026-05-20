
export interface IEditor {
    save(): Promise<void>;
    finishBook(): Promise<void>;
    showRightSidePane(paneName: string): void;
    showBottomPane(paneName: string): void;
    insert(text: string, atBegin?: true, atEnd?: false): void;
    paste(): void;
    setContent(actionName: string, content: string): void;
}