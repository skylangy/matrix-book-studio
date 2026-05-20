

export interface BookCardConfig {
    showEdit?: boolean;
    showDelete?: boolean;
    showDone?: boolean;
    showMore?: boolean;
    showPublish?: boolean;
    showRank?: boolean;
    showCategory?: boolean;
    showTag?: boolean;
    showAuthor?: boolean;
    showSubtitle?: boolean;
    showTitle?: boolean;
    showImage?: boolean;
    showStatus?: boolean;
    showPublishOrder?: boolean;
}


const DefaultBookCardConfig: BookCardConfig = {
    showEdit: true,
    showDelete: true,
    showDone: true,
    showMore: true,
    showPublish: true,
    showRank: true,
    showCategory: true,
    showTag: true,
    showAuthor: true,
    showSubtitle: true,
    showTitle: true,
    showImage: true,
    showStatus: true,
    showPublishOrder: false
};

export default DefaultBookCardConfig;