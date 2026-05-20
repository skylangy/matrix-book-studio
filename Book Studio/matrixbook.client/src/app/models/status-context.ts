import { Book } from "./book";
import { EditorContext } from "./editor-context";

export interface StatusContext {
    book?: Book;
    editorContext?: EditorContext;
}