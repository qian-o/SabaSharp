namespace Saba.Contracts;

public interface IModel : IDisposable
{
    bool Load(string path, string mmdDataDir);

    void Destroy();
}