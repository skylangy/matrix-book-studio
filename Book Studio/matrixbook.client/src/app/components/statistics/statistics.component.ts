import { Component, OnInit } from '@angular/core';
import { BookInfo, DashboardModel } from 'src/app/models/dashboard-model';
import { ILogger } from 'src/app/models/logger';
import { BookService } from 'src/app/services/book-service';
import { LoggingService } from 'src/app/services/logging-services';
import { WorkViewComponent } from '../work-view/work.view.component';
// import * as Highcharts from 'highcharts';
import { DecimalPipe } from '@angular/common';
import { HighchartsChartModule } from 'highcharts-angular';
import Highcharts from 'highcharts/es-modules/masters/highcharts.src.js';
import { HeaderComponent } from '../header/header.component';


@Component({
    selector: 'mtx-statistics',
    templateUrl: './statistics.component.html',

    imports: [
        HeaderComponent,
        HighchartsChartModule,
        DecimalPipe,
    ],
})
export class StatisticsComponent extends WorkViewComponent implements OnInit {
    model?: DashboardModel;

    Highcharts: typeof Highcharts = Highcharts;
    updateFlag = false;
    chartOptions: Highcharts.Options = {
        chart: {
            type: 'column',
            backgroundColor: '#212529',
            style: {
                color: '#fff'
            },
            plotBorderColor: '#606063'
        },
        title: {
            text: 'Published Books by month',
            style: {
                color: '#E0E0E3',
            }
        },
        yAxis: {
            min: 0,
            title: {
                text: 'Books',
                style: {
                    color: '#A0A0A3'
                }
            },
            gridLineColor: '#707073',
            labels: {
                style: {
                    color: '#E0E0E3'
                }
            },
            lineColor: '#707073',
            minorGridLineColor: '#505053',
            tickColor: '#707073',
            tickWidth: 1,

        },
        xAxis: {
            gridLineColor: '#707073',
            labels: {
                style: {
                    color: '#E0E0E3'
                }
            },
            lineColor: '#707073',
            minorGridLineColor: '#505053',
            tickColor: '#707073',
            title: {
                style: {
                    color: '#A0A0A3'

                }
            }
        },
        plotOptions: {
            column: {
                pointPadding: 0.2,
                borderWidth: 0
            }
        },
        series: [
            {
                type: 'column',
                name: 'Books in Progress',
                data: [1, 2, 3, 4, 5],
            },
            {
                type: 'column',
                name: 'Books Finished',
                data: [1, 2, 3, 4, 5],
            },
        ],
        legend: {
            backgroundColor: 'rgba(0, 0, 0, 0.5)',
            itemStyle: {
                color: '#E0E0E3'
            },
            itemHoverStyle: {
                color: '#FFF'
            },
            itemHiddenStyle: {
                color: '#606063'
            },
            title: {
                style: {
                    color: '#C0C0C0'
                }
            }
        },
    };
    private readonly logger?: ILogger;

    constructor(
        private readonly bookService: BookService,
        private readonly loggingService: LoggingService) {
        super();
        this.title = 'Statistics';
        this.subtitle = 'Book library statistics';
        this.bannerImage = './assets/images/splashes/photo-13.jpg';

        this.logger = this.loggingService?.getLogger('StatisticsComponent');
    }

    override async ngOnInit() {
        super.ngOnInit();

        this.model = await this.bookService.getDashboard();

        let bookData = this.getBooksByMonth(this.model.books || []);

        let finishedBooks = this.getFinishedBooksByMonth(this.model.books || [], 'Finished');
        let inProgressBooks = this.getFinishedBooksByMonth(this.model.books || [], 'In Progress');

        this.logger?.log('Dashboard model', bookData, finishedBooks, inProgressBooks);

        this.chartOptions.xAxis = {
            categories: Array.from(bookData.keys())
        };
        this.chartOptions.series = [{
            type: 'column',
            name: 'Books in Progress',
            data: Array.from(inProgressBooks.values())
        },
        {
            type: 'column',
            name: 'Books Finished',
            data: Array.from(finishedBooks.values())
        }
        ];

        this.updateFlag = true;
    }

    private getMonthKey(date: any): string {

        if (!(date instanceof Date)) {
            throw new Error('Invalid date object');
        }

        const year = date.getFullYear();
        const month = (date.getMonth() + 1).toString().padStart(2, '0');
        return `${year}-${month}`;
    }

    getBooksByMonth(books: BookInfo[]): Map<string, BookInfo[]> {
        const groupedBooks = new Map<string, BookInfo[]>();

        const sortedBooks = books?.sort((a, b) => {
            const dateA = new Date(a.dateUpdated || 0).getTime();
            const dateB = new Date(b.dateUpdated || 0).getTime();
            return dateA - dateB;
        });

        sortedBooks?.forEach((book) => {
            let date = new Date(book.dateUpdated!);
            const monthKey = this.getMonthKey(date);
            const currentMonth = groupedBooks.get(monthKey) || [];
            currentMonth.push(book);
            groupedBooks.set(monthKey, currentMonth);
        });

        return groupedBooks;
    }

    getFinishedBooksByMonth(books: BookInfo[], status: string): Map<string, number> {
        const groupedBooks = new Map<string, number>();

        books?.forEach((book) => {
            let date = new Date(book.dateUpdated!);
            const monthKey = this.getMonthKey(date);
            const currentCount = groupedBooks.get(monthKey) || 0;
            if (book.status === status) {
                groupedBooks.set(monthKey, currentCount + 1);
            } else {
                groupedBooks.set(monthKey, currentCount);
            }
        });

        return groupedBooks;
    }
}
