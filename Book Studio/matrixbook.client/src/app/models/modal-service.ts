import { IModal } from "./modal";

export interface IModalService {
    openModal(modal: IModal): void;
}