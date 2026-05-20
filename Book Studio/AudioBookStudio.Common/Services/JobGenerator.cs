using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Extensions;
using AudioBookStudio.Common.Models;
using AudioBookStudio.Common.Shared;
using AudioBookStudio.Models.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace AudioBookStudio.Common.Services;
public class JobGenerator : IJobGenerator
{
    private readonly Dictionary<string, Func<ExportModel, ConcurrentQueue<JobDescriptor>>> _jobGenerators = [];
    private readonly AppConfiguration _bookConfig;
    private readonly ILogger<JobGenerator> _logger;
    private readonly IEntityRepository _entityRepository;
    private readonly ITextToVoiceService _textToVoiceService;
    private readonly IWavToMp3Converter _wavToMp3Converter;
    private readonly IVideoGenerator _videoGenerator;
    private readonly IVideoCombiner _videoCombiner;
    private readonly ISubtitleGenerator _subtitleGenerator;
    private readonly IMetadataProcessor _metadataProcessor;
    private readonly IContentWriter _contentWriter;
    private readonly IWorkProgressService _workProgressService;
    private readonly ICommandInvoker _commandInvoker;

    public JobGenerator(IOptions<AppConfiguration> bookConfig,
                            IEntityRepository entityRepository,
                            ITextToVoiceService textToVoiceService,
                            IWavToMp3Converter wavToMp3Converter,
                            IVideoGenerator videoGenerator,
                            IVideoCombiner videoCombiner,
                            ISubtitleGenerator subtitleGenerator,
                            IMetadataProcessor metadataProcessor,
                            IContentWriter contentWriter,
                            IWorkProgressService workProgressService,
                            ICommandInvoker commandInvoker,
                            ILogger<JobGenerator> logger)
    {
        _entityRepository = entityRepository;
        _textToVoiceService = textToVoiceService;
        _wavToMp3Converter = wavToMp3Converter;
        _videoGenerator = videoGenerator;
        _videoCombiner = videoCombiner;
        _subtitleGenerator = subtitleGenerator;
        _metadataProcessor = metadataProcessor;
        _contentWriter = contentWriter;
        _workProgressService = workProgressService;
        _bookConfig = bookConfig.Value;
        _commandInvoker = commandInvoker;
        _logger = logger;

        InitializeJobs();
    }

    public ConcurrentQueue<JobDescriptor> GetJobs(ExportModel exportModel)
    {
        _jobGenerators.TryGetValue(exportModel.Type!, out var generator);

        return generator(exportModel);
    }

    private void InitializeJobs()
    {
        _jobGenerators.Add(ResourceTypes.Txt, (model) => new ConcurrentQueue<JobDescriptor>([GenerateExportTextJob(model)]));
        _jobGenerators.Add(ResourceTypes.Wav, (model) => new ConcurrentQueue<JobDescriptor>([GenerateExportWavJob(model)]));
        _jobGenerators.Add(ResourceTypes.Mp3, (model) => new ConcurrentQueue<JobDescriptor>([GenerateWavToMp3Job(model)]));
        _jobGenerators.Add(ResourceTypes.Mp4, (model) => new ConcurrentQueue<JobDescriptor>([GenerateMp3ToMp4Job(model)]));
        _jobGenerators.Add(ResourceTypes.Srt, (model) => new ConcurrentQueue<JobDescriptor>([GenerateSrtJob(model)]));
        _jobGenerators.Add(ResourceTypes.Metadata, (model) => new ConcurrentQueue<JobDescriptor>([GenerateMetadataJob(model)]));
        _jobGenerators.Add(ResourceTypes.CombineVideos, (model) => new ConcurrentQueue<JobDescriptor>([GenerateCombineVideosJob(model)]));

        _jobGenerators.Add(ResourceTypes.All, (model) => new ConcurrentQueue<JobDescriptor>([ GenerateExportTextJob(model),
                                                                                              GenerateExportWavJob(model, false),
                                                                                              GenerateWavToMp3Job(model, false),
                                                                                              GenerateMp3ToMp4Job(model),
                                                                                              GenerateSrtJob(model),
                                                                                              GenerateMetadataJob(model)

                                                                                                     ]));
    }

    public JobDescriptor GenerateExportTextJob(ExportModel model)
    {
        var taskName = "Generate Text content";
        var job = new JobDescriptor
        {
            Name = $"Generate text for {model.BookName}",
            PrepareTask = new TaskDescriptor
            {
                Name = $"Prepare text for {model.BookName}",
                Group = $"{ResourceTypes.Text}-{model.BookName}",
                TaskProvider = token =>
                {
                    _workProgressService.UpdateProgress(taskName, $"Prepare generating text for {model.BookName}", model.BookName, BookStatus.InProgress);

                    return Task.CompletedTask;
                }
            },
            DoneTask = new TaskDescriptor
            {
                Name = $"Finish Text for {model.BookName}",
                Group = $"{ResourceTypes.Text}-{model.BookName}",
                TaskProvider = async token =>
                {
                    // update book
                    var book = await _entityRepository.GetByNameAsync(model.BookName).DefaultAwait();
                    book.TextGenerated = true;
                    await _entityRepository.UpdateAsync(book).DefaultAwait();

                    _workProgressService.UpdateProgress(taskName, $"Finish generating text for {model.BookName}", model.BookName, BookStatus.Finished);
                }
            },
            MainTasks = GenerateExportTextTasks(model)
        };

        return job;
    }

    public JobDescriptor GenerateExportWavJob(ExportModel model, bool validate = true)
    {
        var taskName = "Generate WAV";
        var job = new JobDescriptor
        {
            Name = $"Generate WAV for {model.BookName}",
            PrepareTask = new TaskDescriptor
            {
                Name = $"Prepare WAV for {model.BookName}",
                Group = $"{ResourceTypes.Wav}-{model.BookName}",
                TaskProvider = token =>
                {
                    _workProgressService.UpdateProgress(taskName, $"Prepare generating wav for {model.BookName}", model.BookName, BookStatus.InProgress);

                    var exportFolder = GetExportFolder(model, ResourceTypes.Wav);
                    exportFolder.EnsureDirectoryExists();

                    return Task.CompletedTask;
                }
            },
            DoneTask = new TaskDescriptor
            {
                Name = $"Finish WAV for {model.BookName}",
                Group = $"{ResourceTypes.Wav}-{model.BookName}",
                TaskProvider = async token =>
                {
                    // update book
                    var book = await _entityRepository.GetByNameAsync(model.BookName).DefaultAwait();
                    book.WavGenerated = true;
                    await _entityRepository.UpdateAsync(book).DefaultAwait();
                    var chunkCount = model.Chapters.Sum(x => x.Chunks.Count);

                    _workProgressService.UpdateProgress(taskName, $"Finish generating wav for {model.BookName}", model.BookName, BookStatus.Finished, chunkCount, chunkCount);
                }
            },
            MainTasks = GenerateExportWavTasks(model, validate)
        };

        return job;
    }

    public JobDescriptor GenerateWavToMp3Job(ExportModel model, bool validate = true)
    {
        var job = new JobDescriptor
        {
            Name = $"Generate MP3 for {model.BookName}",
            PrepareTask = new TaskDescriptor
            {
                Name = $"Prepare MP3 for {model.BookName}",
                Group = $"{ResourceTypes.Mp3}-{model.BookName}",
                TaskProvider = token =>
                {
                    _workProgressService.UpdateProgress("Generate MP3", $"Prepare generate mp3 for {model.BookName}", model.BookName, BookStatus.InProgress);

                    var exportFolder = GetExportFolder(model, ResourceTypes.Mp3);
                    exportFolder.EnsureDirectoryExists();

                    return Task.CompletedTask;
                }
            },
            DoneTask = new TaskDescriptor
            {
                Name = $"Finish MP3 for {model.BookName}",
                Group = $"{ResourceTypes.Mp3}-{model.BookName}",
                TaskProvider = async token =>
                {
                    // update book
                    var book = await _entityRepository.GetByNameAsync(model.BookName).DefaultAwait();
                    book.Mp3Generated = true;
                    await _entityRepository.UpdateAsync(book).DefaultAwait();

                    var chapterCount = model.Chapters.Count;

                    _workProgressService.UpdateProgress("Generate MP3", $"Finish generate mp3 for {model.BookName}", model.BookName, BookStatus.Finished, chapterCount, chapterCount);
                }
            },
            MainTasks = GenerateWavToMp3Tasks(model, validate)
        };

        return job;
    }

    public JobDescriptor GenerateMp3ToMp4Job(ExportModel model, bool validate = true)
    {
        var chapterCount = model.Chapters.Count;

        var job = new JobDescriptor
        {
            Name = $"Generate MP4 for {model.BookName}",
            PrepareTask = new TaskDescriptor
            {
                Name = $"Prepare MP4 for {model.BookName}",
                Group = $"{ResourceTypes.Mp4}-{model.BookName}",
                TaskProvider = token =>
                {
                    _workProgressService.UpdateProgress("Generate MP4", $"Prepare generate mp4 for {model.BookName}", model.BookName, BookStatus.InProgress);
                    var exportFolder = GetExportFolder(model, ResourceTypes.Mp4);
                    exportFolder.EnsureDirectoryExists();

                    return Task.CompletedTask;
                }
            },
            DoneTask = new TaskDescriptor
            {
                Name = $"Finish MP4 for {model.BookName}",
                Group = $"{ResourceTypes.Mp4}-{model.BookName}",
                TaskProvider = async token =>
                {
                    // update book
                    var book = await _entityRepository.GetByNameAsync(model.BookName).DefaultAwait();
                    book.Mp4Generated = true;
                    await _entityRepository.UpdateAsync(book).DefaultAwait();

                    var chapterCount = model.Chapters.Count;

                    _workProgressService.UpdateProgress("Generate MP4", $"Finish generate mp4 for {model.BookName}", model.BookName, BookStatus.Finished, chapterCount, chapterCount);

                    await OpenBookFolder(model);
                }
            },
            MainTasks = GenerateMp3ToVideoTasks(model, validate)
        };

        return job;
    }

    public JobDescriptor GenerateCombineVideosJob(ExportModel model)
    {
        var job = new JobDescriptor
        {
            Name = $"Combine videos for {model.BookName}",
            PrepareTask = new TaskDescriptor
            {
                Name = $"Prepare combine videos for {model.BookName}",
                Group = $"{ResourceTypes.CombineVideos}-{model.BookName}",
                TaskProvider = token =>
                {
                    _workProgressService.UpdateProgress("Combine videos", $"Prepare combine videos for {model.BookName}", model.BookName, BookStatus.InProgress);
                    var exportFolder = GetExportFolder(model, ResourceTypes.Mp4);
                    exportFolder.EnsureDirectoryExists();

                    return Task.CompletedTask;
                }
            },
            DoneTask = new TaskDescriptor
            {
                Name = $"Finish MP4 for {model.BookName}",
                Group = $"{ResourceTypes.CombineVideos}-{model.BookName}",
                TaskProvider = async token =>
                {
                    _workProgressService.UpdateProgress("Combine videos", $"Finish combine videos for {model.BookName}", model.BookName, BookStatus.Finished);

                    await OpenBookFolder(model);
                }
            },
            MainTasks = GenerateCombineVideosTasks(model)
        };
        return job;
    }

    public JobDescriptor GenerateSrtJob(ExportModel model, bool validate = true)
    {
        var chapterCount = model.Chapters.Count;
        var job = new JobDescriptor
        {
            Name = $"Generate SRT for {model.BookName}",
            PrepareTask = new TaskDescriptor
            {
                Name = $"Prepare SRT for {model.BookName}",
                Group = $"{ResourceTypes.Srt}-{model.BookName}",
                TaskProvider = token =>
                {
                    _workProgressService.UpdateProgress("Generate SRT", $"Prepare generate srt for {model.BookName}", model.BookName, BookStatus.InProgress);

                    var exportFolder = GetExportFolder(model, ResourceTypes.Srt);
                    exportFolder.EnsureDirectoryExists();

                    return Task.CompletedTask;
                }
            },
            DoneTask = new TaskDescriptor
            {
                Name = $"Finish SRT for {model.BookName}",
                Group = $"{ResourceTypes.Srt}-{model.BookName}",
                TaskProvider = async token =>
                {
                    // update book
                    var book = await _entityRepository.GetByNameAsync(model.BookName).DefaultAwait();
                    book.SrtGenerated = true;
                    await _entityRepository.UpdateAsync(book).DefaultAwait();

                    _workProgressService.UpdateProgress("Generate SRT", $"Finish generate srt for {model.BookName}", model.BookName, BookStatus.Finished, chapterCount, chapterCount);

                    await RunValidateTasks(model, validate).DefaultAwait();
                }
            },
            MainTasks = GenerateSrtForFilesTasks(model)
        };

        return job;
    }

    public JobDescriptor GenerateMetadataJob(ExportModel model)
    {
        var chapterCount = model.Chapters.Count;
        var job = new JobDescriptor
        {
            Name = $"Generate metadata for {model.BookName}",
            PrepareTask = new TaskDescriptor
            {
                Name = $"Prepare metadata for {model.BookName}",
                Group = $"{ResourceTypes.Metadata}-{model.BookName}",
                TaskProvider = token =>
                {
                    _workProgressService.UpdateProgress("Generate metadata", $"Prepare generate metadata for {model.BookName}", model.BookName, BookStatus.InProgress);

                    return Task.CompletedTask;
                }
            },
            DoneTask = new TaskDescriptor
            {
                Name = $"Finish metadata for {model.BookName}",
                Group = $"{ResourceTypes.Metadata}-{model.BookName}",
                TaskProvider = token =>
                {
                    _workProgressService.UpdateProgress("Generate metadata", $"Finish generate metadata for {model.BookName}", model.BookName, BookStatus.Finished, chapterCount, chapterCount);
                    return Task.CompletedTask;
                }
            },
            MainTasks = GenerateMetadataTasks(model)
        };

        return job;
    }

    public JobDescriptor GenerateValidateJob(ExportModel model)
    {
        var job = new JobDescriptor
        {
            Name = $"Validate {model.BookName}",
            PrepareTask = new TaskDescriptor
            {
                Name = "Prepare Validate",
                Group = $"{ResourceTypes.Validation}-{model.BookName}",
                TaskProvider = token => Task.CompletedTask
            },
            DoneTask = new TaskDescriptor
            {
                Name = "Finish Validate",
                Group = $"{ResourceTypes.Validation}-{model.BookName}",
                TaskProvider = token => Task.CompletedTask
            },
            MainTasks = GenerateValidateTasks(model)
        };

        return job;
    }

    public JobDescriptor GenerateEhanceMp3Job(ExportModel model)
    {
        var job = new JobDescriptor
        {
            Name = $"Enhance MP3 for {model.BookName}",
            PrepareTask = new TaskDescriptor
            {
                Name = "Prepare Enhance MP3",
                Group = $"{ResourceTypes.Mp3}-{model.BookName}",
                TaskProvider = token => Task.CompletedTask
            },
            DoneTask = new TaskDescriptor
            {
                Name = "Finish Enhance MP3",
                Group = $"{ResourceTypes.Mp3}-{model.BookName}",
                TaskProvider = token => Task.CompletedTask
            },
            MainTasks = new ConcurrentQueue<TaskDescriptor>([
                new TaskDescriptor
                {
                    Name = $"Enhance MP3 for {model.BookName}",
                    Group = $"{ResourceTypes.Mp3}-{model.BookName}",
                    TaskProvider = async token =>
                    {
                        var bookName = model.BookName!;
                        var mp3Folder = GetExportFolder(model, ResourceTypes.Mp3);
                        var files = Directory.GetFiles(mp3Folder, "*.mp3").OrderBy(x => x, ChapterNameComparer.Instance);
                        var context = new ConvertContext
                        {
                            Book = bookName,
                            Sources = files,
                            Destination = mp3Folder
                        };
                        await _wavToMp3Converter.Enhance(context).DefaultAwait();
                    }
                }
            ])
        };
        return job;
    }

    private ConcurrentQueue<TaskDescriptor> GenerateExportTextTasks(ExportModel model)
    {
        var chapterCount = model.Chapters.Count;
        var index = 0;
        var tasks = model.Chapters
                    .Select(x =>
                    {
                        var taksName = "Generate text";
                        return new TaskDescriptor
                        {
                            Name = $"Convert to text for {x.Title}",
                            Group = $"{ResourceTypes.Text}-{model.BookName}",
                            TaskProvider = async token =>
                            {
                                var bookName = model.BookName!;
                                var destinationFolder = GetExportFolder(model, ResourceTypes.Txt);
                                _logger.LogInformation("Export text to folder: {} {}, {}", model.Author, model.BookName, destinationFolder);
                                destinationFolder.EnsureDirectoryExists();

                                var context = new WriteContentContext
                                {
                                    Content = x.Content,
                                    Path = Path.Combine(destinationFolder, $"{bookName}-{x.Title}.{ResourceTypes.Txt}")
                                };

                                _workProgressService.UpdateProgress(taksName, $"Start generating text {x.Title}", model.BookName, BookStatus.InProgress, chapterCount, index);

                                var result = await _contentWriter.WriteAsync(context).DefaultAwait();

                                _workProgressService.UpdateProgress(taksName, $"Finish generating text {x.Title}", model.BookName, BookStatus.Finished, chapterCount, index++, result);
                            }
                        };
                    });

        return new ConcurrentQueue<TaskDescriptor>(tasks);
    }

    private ConcurrentQueue<TaskDescriptor> GenerateExportWavTasks(ExportModel model, bool validate = true)
    {
        var chunkCount = model.Chapters.Sum(x => x.Chunks.Count);
        var index = 0;
        var tasks = model
                     .Chapters
                     .SelectMany(chapter => chapter.Chunks, (chapter, chunk) => new
                     {
                         ChapterTitle = chapter.Title,
                         ChunkIndex = chunk.Index,
                         ChunkContent = chunk.Content
                     })
                     .Select(x =>
                     {
                         var taskName = "Generate WAV";

                         return new TaskDescriptor
                         {
                             Name = $"Generate WAV for {x.ChapterTitle.Trim()}-{x.ChunkIndex}",
                             Group = $"{ResourceTypes.Wav}-{model.BookName}",
                             TaskProvider = async token =>
                             {
                                 var context = new TextToVoiceServiceContext
                                 {
                                     SpeechService = model.SpeechService,
                                     Language = model.Language,
                                     VoiceName = model.VoiceName,
                                     FileName = $"{x.ChapterTitle.Trim()}_{x.ChunkIndex}.{ResourceTypes.Wav}",
                                     OutputPath = GetExportFolder(model, ResourceTypes.Wav)
                                 };
                                 context.OutputPath.EnsureDirectoryExists();

                                 _workProgressService.UpdateProgress(taskName, $"Start generating wav for {x.ChapterTitle}, chunk {x.ChunkIndex}, {x.ChunkContent.Length} characters", model.BookName, BookStatus.InProgress, chunkCount, index);

                                 var result = await _textToVoiceService.Convert(x.ChunkContent, context).DefaultAwait();

                                 if (result.Success)
                                 {
                                     _workProgressService.UpdateProgress(taskName, $"Finish generating wav for {x.ChapterTitle}, chunk: {x.ChunkIndex}", model.BookName, BookStatus.Finished, chunkCount, index++, result.Success);
                                 }
                                 else
                                 {
                                     if (result.Error.Contains("Error code: 4429"))
                                     {
                                         await Task.Delay(1000 * 30).DefaultAwait();
                                     }
                                     _workProgressService.UpdateProgress(taskName, $"Finish generating wav for {x.ChapterTitle}, chunk: {x.ChunkIndex} with error: {result.Error}", model.BookName, BookStatus.Finished, chunkCount, index++, result.Success);
                                 }

                                 if (index == chunkCount - 1)
                                 {
                                     await RunValidateTasks(model, validate).DefaultAwait();
                                 }
                             }
                         };
                     });

        return new ConcurrentQueue<TaskDescriptor>(tasks);
    }

    private ConcurrentQueue<TaskDescriptor> GenerateWavToMp3Tasks(ExportModel model, bool validate = true)
    {
        var chapterCount = model.Chapters.Count;
        var index = 0;
        var tasks = model.Chapters
                     .Select(x =>
                     {
                         var taskName = "Generate MP3";
                         return new TaskDescriptor
                         {
                             Name = $"Convert to mp3 for {x.Title}",
                             Group = $"{ResourceTypes.Mp3}-{model.BookName}",
                             TaskProvider = async token =>
                             {
                                 var bookName = model.BookName!;
                                 var chapterName = x.Title.Trim();
                                 var extension = ResourceTypes.Wav;
                                 var wavFolder = GetExportFolder(model, extension);
                                 var destinationFolder = GetExportFolder(model, ResourceTypes.Mp3);
                                 destinationFolder.EnsureDirectoryExists();

                                 var context = new ConvertContext
                                 {
                                     Book = bookName,
                                     Chapter = chapterName,
                                     Destination = Path.Combine(destinationFolder, $"{bookName}-{chapterName}.{ResourceTypes.Mp3}"),
                                     Sources = Directory.GetFiles(wavFolder, $"{chapterName}*.{extension}")
                                                        .OrderBy(x => x, NaturalComparer.Instance)
                                 };

                                 _workProgressService.UpdateProgress(taskName, $"Start convert {chapterName} to mp3", model.BookName, BookStatus.InProgress, chapterCount, index);

                                 var result = await _wavToMp3Converter.Convert(context).DefaultAwait();

                                 _workProgressService.UpdateProgress(taskName, $"Finish convert {chapterName} to mp3", model.BookName, BookStatus.Finished, chapterCount, index++, result.Success);

                                 if (index == chapterCount - 1)
                                 {
                                     await RunValidateTasks(model, validate).DefaultAwait();
                                 }
                             }
                         };
                     });

        return new ConcurrentQueue<TaskDescriptor>(tasks);
    }

    private ConcurrentQueue<TaskDescriptor> GenerateMp3ToVideoTasks(ExportModel model, bool validate = true)
    {
        var chapterCount = model.Chapters.Count;
        var index = 0;
        var tasks = model.Chapters
                    .Select(x =>
                    {
                        var taksName = "Generate mp4";
                        return new TaskDescriptor
                        {
                            Name = $"Convert to mp4 for {x.Title}",
                            Group = $"{ResourceTypes.Mp4}-{model.BookName}",
                            TaskProvider = async token =>
                            {
                                var bookName = model.BookName!;
                                var chapterName = x.Title.Trim();
                                var mp3Folder = GetExportFolder(model, ResourceTypes.Mp3);
                                var destinationFolder = GetExportFolder(model, ResourceTypes.Mp4);
                                destinationFolder.EnsureDirectoryExists();

                                var context = new VideoGeneratorContext
                                {
                                    AudioFile = Path.Combine(mp3Folder, $"{bookName}-{chapterName}.{ResourceTypes.Mp3}"),
                                    VideoFile = Path.Combine(destinationFolder, $"{bookName}-{chapterName}.{ResourceTypes.Mp4}"),
                                    MetadataTitle = bookName,
                                    Book = bookName,
                                    Chapter = chapterName,
                                };

                                var imageFolder = Path.Combine(_bookConfig.BooksLocation, model.Author, bookName, ResourceTypes.Images);

                                context.Images.Add(bookName, new NamedImageFile(bookName, Path.Combine(imageFolder, model.Image!)));

                                var imageFiles = Directory.GetFiles(imageFolder, $"*{ResourceTypes.SplashType}");
                                foreach (var file in imageFiles)
                                {
                                    var fileName = Path.GetFileNameWithoutExtension(file);
                                    context.Images.Add(fileName, new NamedImageFile(fileName, file));
                                }

                                _workProgressService.UpdateProgress(taksName, $"Start generating video {x.Title}", model.BookName, BookStatus.InProgress, chapterCount, index);

                                var result = await _videoGenerator.Generate(context).DefaultAwait();

                                _workProgressService.UpdateProgress(taksName, $"Finish generating video {x.Title}", model.BookName, BookStatus.Finished, chapterCount, index++, result.Success);

                                if (index == chapterCount)
                                {
                                    _logger.LogInformation("Finish generating video for {BookName} {Chapter}, start validation", model.BookName, x.Title);
                                    await RunValidateTasks(model, validate).DefaultAwait();
                                }
                            }
                        };
                    });

        return new ConcurrentQueue<TaskDescriptor>(tasks);
    }

    private ConcurrentQueue<TaskDescriptor> GenerateCombineVideosTasks(ExportModel model)
    {
        var tasks = new TaskDescriptor[] {
         new() {
             Name = $"Combine videos for {model.BookName}",
             Group = $"{ResourceTypes.CombineVideos}-{model.BookName}",
             TaskProvider = async token => {
                  var bookName = model.BookName!;
                  var sourceFolder = GetExportFolder(model, ResourceTypes.Mp4);
                  var destinationFolder = GetExportFolder(model, ResourceTypes.CombineVideos);
                  destinationFolder.EnsureDirectoryExists();

                 var context = new VideoCombineContext
                    {
                        Name = model.BookName,
                        SourceFiles = Directory.GetFiles(sourceFolder, "*.mp4").OrderBy(x => x, ChapterNameComparer.Instance),
                        OutputFolder = destinationFolder,
                        OutputFile = Path.Combine(destinationFolder, $"{bookName}.{ResourceTypes.Mp4}")
                    };
                 await _videoCombiner.Combine(context);
             }
         }
        };

        return new ConcurrentQueue<TaskDescriptor>(tasks);
    }

    private ConcurrentQueue<TaskDescriptor> GenerateSrtTasks(ExportModel model)
    {
        var task = new TaskDescriptor
        {
            Name = $"Generate subtitle for {model.BookName}",
            Group = $"{ResourceTypes.Srt}-{model.BookName}",
            TaskProvider = async token =>
            {
                var mp3Folder = GetExportFolder(model, ResourceTypes.Mp3);
                var srtFolder = GetExportFolder(model, ResourceTypes.Srt);
                var context = new SubtitleGenerateContext
                {
                    AudioFolder = mp3Folder,
                    SrtFolder = srtFolder
                };
                _workProgressService.UpdateProgress("Generate Subtitle", $"Start {model.BookName} ", model.BookName, BookStatus.InProgress, 1, 0);
                await _subtitleGenerator.GenerateSubtitlesFromFolderAsync(context).DefaultAwait();
                //await _subtitleGenerator.GenerateSubtitlesAsync(context).DefaultAwait();
                _workProgressService.UpdateProgress("Generate Subtitle", $"Start  {model.BookName} ", model.BookName, BookStatus.Finished, 1, 1);
            }
        };
        return new ConcurrentQueue<TaskDescriptor>([task]);
    }

    private ConcurrentQueue<TaskDescriptor> GenerateSrtForFilesTasks(ExportModel model)
    {
        var chapterCount = model.Chapters.Count;
        var index = 0;
        var tasks = model.Chapters
            .Select(x =>
            {
                var mp3Folder = GetExportFolder(model, ResourceTypes.Mp3);
                var srtFolder = GetExportFolder(model, ResourceTypes.Srt);

                return new TaskDescriptor
                {
                    Name = $"Generate Subtitle for {x.Title}",
                    Group = $"{ResourceTypes.Srt}-{model.BookName}",
                    TaskProvider = async token =>
                    {
                        var context = new SubtitleGenerateContext
                        {
                            Chapter = x,
                            AudioFilePath = Path.Combine(mp3Folder, $"{model.BookName}-{x.Title.Trim()}.{ResourceTypes.MP3}"),
                            SubtitleFilePath = Path.Combine(srtFolder, $"{model.BookName}-{x.Title.Trim()}.{ResourceTypes.Srt}"),
                            AudioFolder = mp3Folder,
                            SrtFolder = srtFolder
                        };
                        await _subtitleGenerator.GenerateSubtitlesAsync(context).DefaultAwait();
                        _workProgressService.UpdateProgress("Generate Subtitle", $"Finish {x.Title} ", model.BookName, BookStatus.Finished, chapterCount, index++);
                    }
                };
            });

        return new ConcurrentQueue<TaskDescriptor>(tasks);
    }

    private ConcurrentQueue<TaskDescriptor> GenerateMetadataTasks(ExportModel model)
    {
        var tasks = new TaskDescriptor[] {
         new() {
             Name = $"Generate metadata for {model.BookName}",
             Group = $"{ResourceTypes.Metadata}-{model.BookName}",
             TaskProvider = async token => {
                 var book = await _entityRepository.GetByNameAsync(model.BookName).DefaultAwait();
                 await _metadataProcessor.GenerateBookMeta(book);
             }
         }
        };

        return new ConcurrentQueue<TaskDescriptor>(tasks);
    }

    private ConcurrentQueue<TaskDescriptor> GenerateValidateTasks(ExportModel model)
    {
        var tasks = new ConcurrentQueue<TaskDescriptor>();

        var chapterCount = model.Chapters.Count;
        var chunkCount = model.Chapters.Sum(x => x.Chunks.Count);
        var bookFolder = GetBookFolder(model);

        if (model.Type == ResourceTypes.Wav || model.Type == ResourceTypes.All)
        {
            tasks.Enqueue(new TaskDescriptor
            {
                Name = $"Validate WAV for {model.BookName}",
                Group = $"{ResourceTypes.Validation}-{model.BookName}",
                TaskProvider = token =>
                {
                    var folder = Path.Combine(bookFolder, ResourceTypes.Wav);
                    var files = Directory.GetFiles(folder, "*.wav");

                    _logger.LogInformation("Found {} wav files, expect {}", files.Length, chunkCount);
                    _workProgressService.UpdateProgress("Validate Wav", $"Validated WAV files expects {chunkCount}, actual {files.Length}", model.BookName, BookStatus.Finished, chapterCount, chapterCount, chunkCount == files.Length);

                    return Task.CompletedTask;
                }
            });
        }

        if (model.Type == ResourceTypes.Mp3 || model.Type == ResourceTypes.All)
        {
            tasks.Enqueue(new TaskDescriptor
            {
                Name = $"Validate MP3 for {model.BookName}",
                Group = $"{ResourceTypes.Validation}-{model.BookName}",
                TaskProvider = token =>
                {
                    var folder = Path.Combine(bookFolder, ResourceTypes.Mp3);
                    var files = Directory.GetFiles(folder, "*.mp3");

                    _logger.LogInformation("Found {} mp3 files, expect {}", files.Length, chapterCount);
                    _workProgressService.UpdateProgress("Validate Mp3", $"Validated Mp3 files expects {chapterCount}, actual {files.Length}", model.BookName, BookStatus.Finished, chapterCount, chapterCount, chapterCount == files.Length);
                    return Task.CompletedTask;
                }
            });
        }

        if (model.Type == ResourceTypes.Mp4 || model.Type == ResourceTypes.All)
        {
            tasks.Enqueue(new TaskDescriptor
            {
                Name = $"Validate MP4 for {model.BookName}",
                Group = $"{ResourceTypes.Validation}-{model.BookName}",
                TaskProvider = token =>
                {
                    var folder = Path.Combine(bookFolder, ResourceTypes.Mp4);
                    var files = Directory.GetFiles(folder, "*.mp4");

                    _logger.LogInformation("Found {} mp4 files, expect {}", files.Length, chapterCount);
                    _workProgressService.UpdateProgress("Validate Mp4", $"Validated Mp4 files expects {chapterCount}, actual {files.Length}", model.BookName, BookStatus.Finished, chapterCount, chapterCount, chapterCount == files.Length);

                    return Task.CompletedTask;
                }
            });
        }

        if (model.Type == ResourceTypes.Srt || model.Type == ResourceTypes.All)
        {
            tasks.Enqueue(new TaskDescriptor
            {
                Name = $"Validate subtitle for {model.BookName}",
                Group = $"{ResourceTypes.Validation}-{model.BookName}",
                TaskProvider = token =>
                {
                    var folder = Path.Combine(bookFolder, ResourceTypes.Srt);
                    var files = Directory.GetFiles(folder, "*.srt");

                    _logger.LogInformation("Found {} srt files, expect {}", files.Length, chapterCount);
                    _workProgressService.UpdateProgress("Validate Subtitle", $"Validated srt files expects {chapterCount}, actual {files.Length}", model.BookName, BookStatus.Finished, chapterCount, chapterCount, chapterCount == files.Length);

                    return Task.CompletedTask;
                }
            });
        }

        return tasks;
    }

    private async Task RunValidateTasks(ExportModel model, bool append)
    {
        if (append)
        {
            _logger.LogInformation("Run validate tasks for {BookName}", model.BookName);

            var cancelationTokenSource = new CancellationTokenSource();
            var tasks = GenerateValidateTasks(model).Select(x => x.TaskProvider(cancelationTokenSource));

            await Task.Delay(1000 * 3).DefaultAwait();
            Task.WaitAll([.. tasks]);
        }
        await Task.CompletedTask;
    }

    private string GetBookFolder(ExportModel model)
    {
        return Path.Combine(_bookConfig.BooksLocation, model.Author, model.BookName!);
    }

    private string GetExportFolder(ExportModel model, string type)
    {
        return Path.Combine(_bookConfig.BooksLocation, model.Author, model.BookName, type);
    }

    private async Task OpenBookFolder(ExportModel model)
    {
        var bookFolder = GetBookFolder(model);

        var command = new CommandModel
        {
            Command = "explorer.exe",
            Arguments = bookFolder
        };

        await _commandInvoker.InvokeAsync(command).DefaultAwait();
    }
}

