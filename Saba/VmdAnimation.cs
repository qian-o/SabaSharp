using System.Numerics;
using Saba.Helpers;

namespace Saba;

#region Classes
public class VmdNodeController(MMDNode @object) : VmdAnimationController<VmdNodeAnimationKey, MMDNode>(@object)
{
    public override void Evaluate(float t, float weight = 1.0f)
    {
        if (Object == null)
        {
            return;
        }

        if (_keys.Count == 0)
        {
            Object.AnimTranslate = Vector3.Zero;
            Object.AnimRotate = Quaternion.Identity;

            return;
        }

        Vector3 vt;
        Quaternion q;
        int keyIndex = _keys.FindIndex(StartKeyIndex, item => item.Time > t);
        if (keyIndex == -1)
        {
            vt = _keys.Last().Translate;
            q = _keys.Last().Rotate;
        }
        else
        {
            vt = _keys[keyIndex].Translate;
            q = _keys[keyIndex].Rotate;
            if (keyIndex != 0)
            {
                VmdNodeAnimationKey key0 = _keys[keyIndex - 1];
                VmdNodeAnimationKey key1 = _keys[keyIndex];

                float timeRange = (float)key1.Time - key0.Time;
                float time = (t - key0.Time) / timeRange;
                float tx_x = key0.TxBezier.FindBezierX(time);
                float ty_x = key0.TyBezier.FindBezierX(time);
                float tz_x = key0.TzBezier.FindBezierX(time);
                float rot_x = key0.RotBezier.FindBezierX(time);
                float tx_y = key0.TxBezier.EvalY(tx_x);
                float ty_y = key0.TyBezier.EvalY(ty_x);
                float tz_y = key0.TzBezier.EvalY(tz_x);
                float rot_y = key0.RotBezier.EvalY(rot_x);

                vt = MathHelper.Lerp(key0.Translate, key1.Translate, new Vector3(tx_y, ty_y, tz_y));
                q = Quaternion.Slerp(key0.Rotate, key1.Rotate, rot_y);

                StartKeyIndex = keyIndex;
            }
        }

        if (weight == 1.0f)
        {
            Object.AnimTranslate = vt;
            Object.AnimRotate = q;
        }
        else
        {
            Vector3 baseT = Object.BaseAnimTranslate;
            Quaternion baseQ = Object.BaseAnimRotate;

            Object.AnimTranslate = Vector3.Lerp(baseT, vt, weight);
            Object.AnimRotate = Quaternion.Slerp(baseQ, q, weight);
        }
    }
}

public class VmdMorphController(MMDMorph @object) : VmdAnimationController<VmdMorphAnimationKey, MMDMorph>(@object)
{
    public override void Evaluate(float t, float weight = 1.0f)
    {
        if (Object == null)
        {
            return;
        }

        if (_keys.Count == 0)
        {
            return;
        }

        float w;
        int keyIndex = _keys.FindIndex(StartKeyIndex, item => item.Time > t);
        if (keyIndex == -1)
        {
            w = _keys.Last().Weight;
        }
        else
        {
            w = _keys[keyIndex].Weight;
            if (keyIndex != 0)
            {
                VmdMorphAnimationKey key0 = _keys[keyIndex - 1];
                VmdMorphAnimationKey key1 = _keys[keyIndex];

                float timeRange = (float)key1.Time - key0.Time;
                float time = (t - key0.Time) / timeRange;
                w = (key1.Weight - key0.Weight) * time + key0.Weight;

                StartKeyIndex = keyIndex;
            }
        }

        if (weight == 1.0f)
        {
            Object.Weight = w;
        }
        else
        {
            Object.Weight = MathHelper.Lerp(Object.SaveAnimWeight, w, weight);
        }
    }
}

public class VmdIkController(MMDIkSolver @object) : VmdAnimationController<VmdIkAnimationKey, MMDIkSolver>(@object)
{
    public override void Evaluate(float t, float weight = 1.0f)
    {
        if (Object == null)
        {
            return;
        }

        if (_keys.Count == 0)
        {
            Object.Enable = true;

            return;
        }

        bool enable;
        int keyIndex = _keys.FindIndex(StartKeyIndex, item => item.Time > t);
        if (keyIndex == -1)
        {
            enable = _keys.Last().Enable;
        }
        else
        {
            enable = _keys.First().Enable;
            if (keyIndex != 0)
            {
                VmdIkAnimationKey key = _keys[keyIndex - 1];
                enable = key.Enable;

                StartKeyIndex = keyIndex;
            }
        }

        if (weight == 1.0f)
        {
            Object.Enable = enable;
        }
        else
        {
            if (weight < 1.0f)
            {
                Object.Enable = Object.BaseAnimEnable;
            }
            else
            {
                Object.Enable = enable;
            }
        }
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
        _nodeControllers = [];
        _morphControllers = [];
        _ikControllers = [];
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

        Dictionary<string, VmdIkController> ikCtrlMap = [];
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

    public void SyncPhysics(float t, int frameCount = 30)
    {
        Model!.SaveBaseAnimation();

        for (int i = 0; i < frameCount; i++)
        {
            Model.BeginAnimation();

            Evaluate((float)t, (1.0f + i) / frameCount);

            Model.UpdateMorphAnimation();

            Model.UpdateNodeAnimation(false);

            Model.UpdatePhysicsAnimation(1.0f / 30.0f);

            Model.UpdateNodeAnimation(true);

            Model.EndAnimation();
        }
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
            if (controller.Keys.Length != 0)
            {
                maxTime = MathHelper.Max(maxTime, controller.Keys.Last().Time);
            }
        }

        foreach (VmdMorphController controller in _morphControllers)
        {
            if (controller.Keys.Length != 0)
            {
                maxTime = MathHelper.Max(maxTime, controller.Keys.Last().Time);
            }
        }

        foreach (VmdIkController controller in _ikControllers)
        {
            if (controller.Keys.Length != 0)
            {
                maxTime = MathHelper.Max(maxTime, controller.Keys.Last().Time);
            }
        }

        return maxTime;
    }
}
