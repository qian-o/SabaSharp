using Saba.Helpers;

namespace Saba;

#region Classes
public class VmdNodeController : VmdAnimationController<VmdNodeAnimationKey, MMDNode>
{
    public VmdNodeController(MMDNode @object) : base(@object)
    {
    }

    public override void Evaluate(float t, float weight = 1.0f)
    {

    }
}

public class VmdMorphController : VmdAnimationController<VmdMorphAnimationKey, MMDMorph>
{
    public VmdMorphController(MMDMorph @object) : base(@object)
    {
    }

    public override void Evaluate(float t, float weight = 1.0f)
    {

    }
}

public class VmdIkController : VmdAnimationController<VmdIkAnimationKey, MMDIkSolver>
{
    public VmdIkController(MMDIkSolver @object) : base(@object)
    {
    }

    public override void Evaluate(float t, float weight = 1.0f)
    {

    }
}
#endregion

public class VmdAnimation : IDisposable
{
    private readonly List<VmdNodeController> _nodeControllers;
    private readonly List<VmdMorphController> _morphControllers;
    private readonly List<VmdIkController> _ikControllers;

    public MMDModel? Model { get; private set; }

    public int MaxKeyTime { get; private set; }

    public VmdAnimation()
    {
        _nodeControllers = new List<VmdNodeController>();
        _morphControllers = new List<VmdMorphController>();
        _ikControllers = new List<VmdIkController>();
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

        foreach (IGrouping<string, VmdMotion> group in vmd.Motions.GroupBy(item => item.BoneName))
        {
            MMDNode? node = Model.FindNode(item => item.Name == group.Key);

            if (node == null)
            {
                continue;
            }

            VmdNodeController controller = new(node);

            foreach (VmdMotion motion in group)
            {
                controller.AddKey(new VmdNodeAnimationKey(motion));
            }

            controller.SortKeys();

            _nodeControllers.Add(controller);
        }

        foreach (IGrouping<string, VmdMorph> group in vmd.Morphs.GroupBy(item => item.BlendShapeName))
        {
            MMDMorph? mmdMorph = Model.FindMorph(item => item.Name == group.Key);

            if (mmdMorph == null)
            {
                continue;
            }

            VmdMorphController controller = new(mmdMorph);

            foreach (VmdMorph morph in group)
            {
                controller.AddKey(new VmdMorphAnimationKey((int)morph.Frame, morph.Weight));
            }

            controller.SortKeys();

            _morphControllers.Add(controller);
        }

        Dictionary<string, VmdIkController> ikCtrlMap = new();
        foreach (VmdIk ik in vmd.Iks)
        {
            foreach (IGrouping<string, VmdIk.Info> ikInfo in ik.Infos.GroupBy(item => item.Name))
            {
                if (!ikCtrlMap.TryGetValue(ikInfo.Key, out VmdIkController? ikCtrl))
                {
                    MMDIkSolver? mmdIk = Model.FindIkSolver(item => item.IkNode!.Name == ikInfo.Key);

                    if (mmdIk == null)
                    {
                        continue;
                    }

                    ikCtrl = new VmdIkController(mmdIk);

                    ikCtrlMap.Add(ikInfo.Key, ikCtrl);
                }

                foreach (VmdIk.Info info in ikInfo)
                {
                    ikCtrl.AddKey(new VmdIkAnimationKey((int)ik.Frame, info.Enable));
                }

                ikCtrl.SortKeys();
            }
        }
        _ikControllers.AddRange(ikCtrlMap.Values);

        MaxKeyTime = CalculateMaxKeyTime();

        return true;
    }

    public void Evaluate(float t, float weight = 1.0f)
    {
        _nodeControllers.ForEach(controller => controller.Evaluate(t, weight));

        _morphControllers.ForEach(controller => controller.Evaluate(t, weight));

        _ikControllers.ForEach(controller => controller.Evaluate(t, weight));
    }

    public void Destroy()
    {
        _nodeControllers.Clear();
        _morphControllers.Clear();
        _ikControllers.Clear();

        Model = null;
    }

    public void Dispose()
    {
        Destroy();

        GC.SuppressFinalize(this);
    }

    private int CalculateMaxKeyTime()
    {
        int maxTime = 0;
        foreach (VmdNodeController controller in _nodeControllers)
        {
            if (controller.Keys.Any())
            {
                maxTime = MathHelper.Max(maxTime, controller.Keys.Last().Time);
            }
        }

        foreach (VmdMorphController controller in _morphControllers)
        {
            if (controller.Keys.Any())
            {
                maxTime = MathHelper.Max(maxTime, controller.Keys.Last().Time);
            }
        }

        foreach (VmdIkController controller in _ikControllers)
        {
            if (controller.Keys.Any())
            {
                maxTime = MathHelper.Max(maxTime, controller.Keys.Last().Time);
            }
        }

        return maxTime;
    }
}
