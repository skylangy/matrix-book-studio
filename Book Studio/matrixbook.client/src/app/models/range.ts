
export interface IRange {
    startLineNumber: number;
    startColumn: number;
    endLineNumber: number;
    endColumn: number;

    textCount: number;
    match?: string;
    suggestion?: string;
    body?: string;
    tag?: any;
}