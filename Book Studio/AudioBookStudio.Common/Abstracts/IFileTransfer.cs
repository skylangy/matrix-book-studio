namespace AudioBookStudio.Common.Abstracts;
public interface IFileTransfer
{
    Task<bool> Transfer(string soruce, string destination);
}
