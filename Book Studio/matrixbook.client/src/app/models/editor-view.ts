import { EditorContext } from "./editor-context";
import { EditorPane } from "./editor-pane";
import { GroupedNavItem, NavItem } from "./nav-item";

export interface EditorView {
    panes?: EditorPane[];
    leftPanes?: EditorPane[];
    rightPanes?: EditorPane[];
    bottomPanes?: EditorPane[];
    navItems?: NavItem[];
    fileNavItems?: NavItem[];
    mainNav?: GroupedNavItem[];
    context?: EditorContext;
}