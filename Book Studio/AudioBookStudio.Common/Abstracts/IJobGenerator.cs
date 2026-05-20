using AudioBookStudio.Common.Models;
using System.Collections.Concurrent;


namespace AudioBookStudio.Common.Abstracts;
public interface IJobGenerator
{
    ConcurrentQueue<JobDescriptor> GetJobs(ExportModel exportModel);

    JobDescriptor GenerateEhanceMp3Job(ExportModel model);
}
