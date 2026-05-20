using AudioBookStudio.Common.Models;

namespace AudioBookStudio.Common.Abstracts;

public interface IWorkProgressService
{
    void AddHandler(string name, Action<WorkProgress> handler);

    void UpdateProgress(WorkProgress progress);
}