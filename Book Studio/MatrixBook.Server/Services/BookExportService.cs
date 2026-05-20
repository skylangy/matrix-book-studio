using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Extensions;
using AudioBookStudio.Common.Models;
using AudioBookStudio.Models.Data;
using AudioBookStudio.Models.Extensions;
using MatrixBook.Server.Common;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace MatrixBook.Server.Services;

public class BookExportService : IBookExportService
{
    private const string Explorer = "explorer.exe";
    private readonly AppConfiguration _bookConfig;

    private readonly IWorkProgressService _workProgressService;
    private readonly IWorkProgressRepository _workProgressRepository;
    private readonly IBackgroundWorkerQueue _backgroundWorkerQueue;
    private readonly IJobGenerator _jobGenerator;
    private readonly IHubContext<WorkProgressHub> _hubContext;
    private readonly IHubContext<CommandHub> _commandHub;
    private readonly ILogger<BookExportService> _logger;


    public BookExportService(IOptions<AppConfiguration> bookConfig,
                            IJobGenerator jobGenerator,
                            IWorkProgressService workProgressService,
                            IWorkProgressRepository workProgressRepository,
                            IBackgroundWorkerQueue backgroundWorkerQueue,
                            IHubContext<WorkProgressHub> hubContext,
                            IHubContext<CommandHub> commandHub,
                            ILogger<BookExportService> logger)
    {
        _jobGenerator = jobGenerator;
        _workProgressService = workProgressService;
        _workProgressRepository = workProgressRepository;
        _bookConfig = bookConfig.Value;
        _backgroundWorkerQueue = backgroundWorkerQueue;
        _hubContext = hubContext;
        _commandHub = commandHub;
        _logger = logger;

        InitializeWorkProgressHandler();
    }

    public void ExportBook(ExportModel model)
    {
        var exportFolder = Path.Combine(_bookConfig.BooksLocation, model.Author!, model.BookName!);
        exportFolder.EnsureDirectoryExists();

        var jobs = _jobGenerator.GetJobs(model);
        var cancellationTokenSource = new CancellationTokenSource();
        _backgroundWorkerQueue.EnqueueJob(jobs, cancellationTokenSource);
    }

    public async Task OpenBookFolderAsync(string bookFolder)
    {
        _logger.LogInformation("Opening book folder: {}", bookFolder);
        await _commandHub.RunCommand(new CommandModel()
        {
            Command = Explorer,
            Arguments = bookFolder
        });
    }

    public async Task OpenBookFolder(string bookFolder)
    {
        _logger.LogInformation("Opening book folder: {}", bookFolder);
        await _commandHub.RunCommand(new CommandModel()
        {
            Command = Explorer,
            Arguments = bookFolder
        });
    }

    public async Task SelectFile(string fileName)
    {
        _logger.LogInformation("Selecting file: {}", fileName);
        await _commandHub.RunCommand(new CommandModel()
        {
            Command = Explorer,
            Arguments = $"/select,\"{fileName}\""
        });
    }

    public Task EnhanceMp3(Book book)
    {
        var model = new ExportModel()
        {
            BookName = book.Title,
            Author = book.Author,
            Type = ResourceTypes.Mp3
        };
        var jobs = _jobGenerator.GenerateEhanceMp3Job(model);
        var cancellationTokenSource = new CancellationTokenSource();
        _backgroundWorkerQueue.EnqueueJob(jobs, cancellationTokenSource);

        return Task.CompletedTask;
    }

    private void InitializeWorkProgressHandler()
    {
        _workProgressService.AddHandler("LogHandler", (workProgress) =>
        {
            _logger.LogInformation("[{}] [{}] [{}] [{}] - {}/{} - {}", workProgress.Timestamp, workProgress.Category, workProgress.Name, workProgress.Status, workProgress.Current, workProgress.Total, workProgress.Description);
            if (workProgress.Status == BookStatus.Finished)
            {
                _logger.LogInformation("");
            }
        });

        _workProgressService.AddHandler("RepoHandler", async (workProgress) =>
        {
            await _workProgressRepository.AddAsync(workProgress).DefaultAwait();
        });

        _workProgressService.AddHandler("PublishHandler", async (workProgress) =>
        {
            await _hubContext.Publish(workProgress);
        });
    }
}
