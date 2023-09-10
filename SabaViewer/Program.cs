using Saba;
using Saba.Contracts;

internal class Program
{
    private static void Main(string[] args)
    {
        _ = args;

        IModel model = new PmxModel();
        model.Load("Resources/大喜/模型/登门喜鹊泠鸢yousa-ver2.0/泠鸢yousa登门喜鹊153cm-Apose2.1完整版(2).pmx", "Resources/MMD/");
    }
}