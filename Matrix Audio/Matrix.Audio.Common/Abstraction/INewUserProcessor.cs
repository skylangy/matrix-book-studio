namespace Matrix.Audio.Common.Abstraction;
public interface INewUserProcessor
{
    Task Process(string userId);
}
