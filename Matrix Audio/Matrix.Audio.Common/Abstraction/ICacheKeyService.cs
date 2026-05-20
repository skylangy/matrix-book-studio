namespace Matrix.Audio.Common.Abstraction;
public interface ICacheKeyService
{
    string Create(string prefix, params object[] parts);
}

