namespace Saba;

public class MMDMorph
{
    public string Name { get; set; }

    public float Weight { get; set; }

    public float SaveAnimWeight { get; set; }

    public MMDMorph()
    {
        Name = string.Empty;
    }

    public void SaveBaseAnimation()
    {
        SaveAnimWeight = Weight;
    }

    public void LoadBaseAnimation()
    {
        Weight = SaveAnimWeight;
    }

    public void ClearBaseAnimation()
    {
        SaveAnimWeight = 0.0f;
    }
}
