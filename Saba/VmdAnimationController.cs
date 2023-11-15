using Saba.Helpers;
using System.Numerics;

namespace Saba;

#region Classes
public abstract class VmdAnimationKey
{
    public int Time { get; }

    protected VmdAnimationKey(int time)
    {
        Time = time;
    }
}

public class VmdBezier
{
    public Vector2 Cp1 { get; set; }

    public Vector2 Cp2 { get; set; }

    public VmdBezier(byte[] cp)
    {
        int x0 = cp[0];
        int y0 = cp[4];
        int x1 = cp[8];
        int y1 = cp[12];

        Cp1 = new Vector2(x0 / 127.0f, y0 / 127.0f);
        Cp2 = new Vector2(x1 / 127.0f, y1 / 127.0f);
    }

    public float EvalX(float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        float it = 1.0f - t;
        float it2 = it * it;
        float it3 = it2 * it;
        float[] x = [0, Cp1.X, Cp2.X, 1];

        return t3 * x[3] + 3 * t2 * it * x[2] + 3 * t * it2 * x[1] + it3 * x[0];
    }

    public float EvalY(float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        float it = 1.0f - t;
        float it2 = it * it;
        float it3 = it2 * it;
        float[] y = [0, Cp1.Y, Cp2.Y, 1];

        return t3 * y[3] + 3 * t2 * it * y[2] + 3 * t * it2 * y[1] + it3 * y[0];
    }

    public Vector2 Eval(float t)
    {
        return new Vector2(EvalX(t), EvalY(t));
    }

    public float FindBezierX(float time)
    {
        const float e = 0.00001f;
        float start = 0.0f;
        float stop = 1.0f;
        float t = 0.5f;
        float x = EvalX(t);
        while (MathHelper.Abs(time - x) > e)
        {
            if (time < x)
            {
                stop = t;
            }
            else
            {
                start = t;
            }
            t = (stop + start) * 0.5f;
            x = EvalX(t);
        }

        return t;
    }
}

public class VmdNodeAnimationKey : VmdAnimationKey
{
    public Vector3 Translate { get; }

    public Quaternion Rotate { get; }

    public VmdBezier TxBezier { get; }

    public VmdBezier TyBezier { get; }

    public VmdBezier TzBezier { get; }

    public VmdBezier RotBezier { get; }

    public VmdNodeAnimationKey(VmdMotion motion) : base((int)motion.Frame)
    {
        Translate = motion.Translate * new Vector3(1.0f, 1.0f, -1.0f);

        Matrix4x4 rot0 = Matrix4x4.CreateFromQuaternion(motion.Quaternion);
        Matrix4x4 rot1 = rot0.InvZ();
        Rotate = Quaternion.CreateFromRotationMatrix(rot1);

        TxBezier = new VmdBezier(motion.Interpolation[0..]);
        TyBezier = new VmdBezier(motion.Interpolation[1..]);
        TzBezier = new VmdBezier(motion.Interpolation[2..]);
        RotBezier = new VmdBezier(motion.Interpolation[3..]);
    }
}

public class VmdMorphAnimationKey(int time, float weight) : VmdAnimationKey(time)
{
    public float Weight { get; } = weight;
}

public class VmdIkAnimationKey(int time, bool enable) : VmdAnimationKey(time)
{
    public bool Enable { get; } = enable;
}
#endregion

public abstract class VmdAnimationController<TKey, TObject> where TKey : VmdAnimationKey
{
    protected readonly List<TKey> _keys;

    public TObject Object { get; }

    public TKey[] Keys => _keys.ToArray();

    public int StartKeyIndex { get; protected set; }

    protected VmdAnimationController(TObject @object)
    {
        _keys = [];

        Object = @object;
    }

    public void AddKey(TKey key)
    {
        _keys.Add(key);
    }

    public void SortKeys()
    {
        TKey[] keys = [.. _keys.OrderBy(key => key.Time)];

        _keys.Clear();
        _keys.AddRange(keys);
    }

    public abstract void Evaluate(float t, float weight = 1.0f);
}
