using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;

namespace AudioBookStudio.Common.Services;

public static class TextToVoiceServiceExtensions
{
    public static void UpdateProgress(this IWorkProgressService workProgressService, string name, string description,
        string category, string status, int total = 0, int current = 0, bool success = true)
    {
        workProgressService.UpdateProgress(new WorkProgress
        {
            Id = Guid.NewGuid().ToString(),
            Timestamp = DateTime.Now,
            Name = name,
            Description = description,
            Status = status,
            Total = total,
            Current = current,
            Category = category,
            Success = success
        });
    }
}