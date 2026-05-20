import { Injectable } from '@angular/core';
import { NarrationBlockType } from '../models/narration';

@Injectable({ providedIn: 'root' })
export class NarrationService {
    narrations = [
        {
            id: "genesis-creation",
            title: "Genesis 1:1-5 - The Creation",
            category: "Bible Study",
            tags: "Genesis,Creation,Explanation",
            status: "Draft",
            dateCreated: new Date("2025-07-31T12:00:00Z"),
            dateUpdated: new Date("2025-07-31T12:10:00Z"),
            blocks: [
                {
                    id: "b1",
                    type: NarrationBlockType.Statement,
                    order: 1,
                    content: "In the beginning God created the heaven and the earth.",
                    speech: {
                        speechService: "Azure",
                        language: "en-US",
                        voice: "en-US-JohnNeural"
                    },
                    contentFontFamily: "Times New Roman",
                    contentFontSize: 20,
                    contentColor: "#1E90FF"
                },
                {
                    id: "e1",
                    type: NarrationBlockType.Note,
                    order: 2,
                    content: "This verse introduces God's role as Creator, establishing His sovereignty.",
                    speech: {
                        speechService: "Azure",
                        language: "en-US",
                        voice: "en-US-EmmaNeural"
                    },
                    contentFontFamily: "Segoe UI",
                    contentFontSize: 18,
                    contentColor: "#444444"
                },
                {
                    id: "b2",
                    type: NarrationBlockType.Statement,
                    order: 3,
                    content: "And the earth was without form, and void; and darkness was upon the face of the deep.",
                    speech: {
                        speechService: "Azure",
                        language: "en-US",
                        voice: "en-US-JohnNeural"
                    },
                    contentFontFamily: "Times New Roman",
                    contentFontSize: 20,
                    contentColor: "#1E90FF"
                },
                {
                    id: "e2",
                    type: NarrationBlockType.Note,
                    order: 4,
                    content: "The earth was in a chaotic state before God shaped it. Darkness represents the absence of His order.",
                    speech: {
                        speechService: "Azure",
                        language: "en-US",
                        voice: "en-US-EmmaNeural"
                    },
                    contentFontFamily: "Segoe UI",
                    contentFontSize: 18,
                    contentColor: "#444444"
                },
                {
                    id: "b3",
                    type: NarrationBlockType.Statement,
                    order: 5,
                    content: "And the Spirit of God moved upon the face of the waters.",
                    speech: {
                        speechService: "Azure",
                        language: "en-US",
                        voice: "en-US-JohnNeural"
                    },
                    contentFontFamily: "Times New Roman",
                    contentFontSize: 20,
                    contentColor: "#1E90FF"
                },
                {
                    id: "e3",
                    type: NarrationBlockType.Note,
                    order: 6,
                    content: "God’s Spirit is active, bringing life and movement to creation even before it takes shape.",
                    speech: {
                        speechService: "Azure",
                        language: "en-US",
                        voice: "en-US-EmmaNeural"
                    },
                    contentFontFamily: "Segoe UI",
                    contentFontSize: 18,
                    contentColor: "#444444"
                },
                {
                    id: "b4",
                    type: NarrationBlockType.Statement,
                    order: 7,
                    content: "And God said, Let there be light: and there was light.",
                    speech: {
                        speechService: "Azure",
                        language: "en-US",
                        voice: "en-US-JohnNeural"
                    },
                    contentFontFamily: "Times New Roman",
                    contentFontSize: 20,
                    contentColor: "#1E90FF"
                },
                {
                    id: "e4",
                    type: NarrationBlockType.Note,
                    order: 8,
                    content: "Light represents God’s presence and truth. His word brings transformation.",
                    speech: {
                        speechService: "Azure",
                        language: "en-US",
                        voice: "en-US-EmmaNeural"
                    },
                    contentFontFamily: "Segoe UI",
                    contentFontSize: 18,
                    contentColor: "#444444"
                },
                {
                    id: "b5",
                    type: NarrationBlockType.Statement,
                    order: 9,
                    content: "And God saw the light, that it was good: and God divided the light from the darkness.",
                    speech: {
                        speechService: "Azure",
                        language: "en-US",
                        voice: "en-US-JohnNeural"
                    },
                    contentFontFamily: "Times New Roman",
                    contentFontSize: 20,
                    contentColor: "#1E90FF"
                },
                {
                    id: "e5",
                    type: NarrationBlockType.Note,
                    order: 10,
                    content: "God evaluates and affirms His creation. The separation symbolizes moral order.",
                    speech: {
                        speechService: "Azure",
                        language: "en-US",
                        voice: "en-US-EmmaNeural"
                    },
                    contentFontFamily: "Segoe UI",
                    contentFontSize: 18,
                    contentColor: "#444444"
                }
            ]
        },
        {
            id: "genesis-creation",
            title: "Genesis 1:1-5 - The Creation",
            category: "Bible Study",
            tags: "Genesis,Creation,Explanation",
            status: "Draft",
            dateCreated: new Date("2025-07-31T12:00:00Z"),
            dateUpdated: new Date("2025-07-31T12:10:00Z"),
            blocks: [/* same blocks updated as above */]
        },
        {
            id: "psalm-23",
            title: "Psalm 23 - The Lord is My Shepherd",
            category: "Psalms",
            tags: "Psalm,Trust,Shepherd",
            status: "Published",
            dateCreated: new Date("2025-07-28T08:00:00Z"),
            dateUpdated: new Date("2025-07-29T10:15:00Z"),
            blocks: [
                {
                    id: "b1",
                    type: NarrationBlockType.Statement,
                    order: 1,
                    content: "The Lord is my shepherd; I shall not want.",
                    speech: {
                        speechService: "Azure",
                        language: "en-US",
                        voice: "en-US-JohnNeural"
                    },
                    contentFontFamily: "Georgia",
                    contentFontSize: 20,
                    contentColor: "#228B22"
                }
            ]
        },
        {
            id: "romans-8",
            title: "Romans 8:1 - No Condemnation",
            category: "New Testament",
            tags: "Romans,Grace,Freedom",
            status: "Draft",
            dateCreated: new Date("2025-07-15T11:30:00Z"),
            dateUpdated: new Date("2025-07-16T11:30:00Z"),
            blocks: [
                {
                    id: "b1",
                    type: NarrationBlockType.Statement,
                    order: 1,
                    content: "There is therefore now no condemnation to them which are in Christ Jesus, There is therefore now no condemnation to them which are in Christ Jesus, There is therefore now no condemnation to them which are in Christ Jesus",
                    speech: {
                        speechService: "Azure",
                        language: "en-US",
                        voice: "en-US-JohnNeural"
                    },
                    contentFontFamily: "Cambria",
                    contentFontSize: 19,
                    contentColor: "#000080"
                },
                {
                    id: "e1",
                    type: NarrationBlockType.Note,
                    order: 2,
                    content: "This verse emphasizes the believer’s freedom from guilt through faith in Christ.",
                    speech: {
                        speechService: "Azure",
                        language: "en-US",
                        voice: "en-US-EmmaNeural"
                    },
                    contentFontFamily: "Segoe UI",
                    contentFontSize: 18,
                    contentColor: "#333"
                }
            ]
        },
        {
            id: "proverbs-blank",
            title: "Proverbs Wisdom Series - Intro",
            category: "Proverbs",
            tags: "Wisdom",
            status: "Draft",
            dateCreated: new Date("2025-07-20T09:00:00Z"),
            dateUpdated: new Date("2025-07-20T09:00:00Z"),
            blocks: []
        },
        {
            id: "ecclesiastes",
            title: "Ecclesiastes Highlights",
            category: "Wisdom Literature",
            tags: "Ecclesiastes,Vanity",
            status: "Draft",
            dateCreated: new Date("2025-07-10T10:00:00Z"),
            dateUpdated: new Date("2025-07-10T10:30:00Z"),
            blocks: [
                {
                    id: "b1",
                    type: NarrationBlockType.Statement,
                    order: 1,
                    content: "Vanity of vanities, saith the Preacher, vanity of vanities; all is vanity.",
                    speech: {
                        speechService: "Azure",
                        language: "en-GB",
                        voice: "en-GB-RyanNeural"
                    },
                    contentFontFamily: "Palatino Linotype",
                    contentFontSize: 20,
                    contentColor: "#800000"
                }
            ]
        }
    ];

    constructor() { }

    async getAll(page = 1, pageSize = 30): Promise<any[]> {
        return this.narrations.slice((page - 1) * pageSize, page * pageSize);
    }

    async getById(id: string): Promise<any> {
        return this.narrations.find(narration => narration.id === id);
    }
}