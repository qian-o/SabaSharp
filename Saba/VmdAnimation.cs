namespace Saba;

public class VmdAnimation : IDisposable
{
    public MMDModel? Model { get; private set; }

    public VmdAnimation()
    {
    }

    public bool Load(string path, MMDModel model)
    {
        Destroy();

        Model = model;

        VmdParsing? vmd = VmdParsing.ParsingByFile(path);
        if (vmd == null)
        {
            return false;
        }

        return true;
    }

    public void Destroy()
    {
        Model = null;
    }

    public void Dispose()
    {
        Destroy();

        GC.SuppressFinalize(this);
    }
}
