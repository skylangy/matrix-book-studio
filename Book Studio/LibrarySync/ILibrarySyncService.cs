namespace LibrarySync;
public interface ILibrarySyncService
{
    Task SyncLibrary(string source, string destination);
}