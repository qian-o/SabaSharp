using Saba.Helpers;
using System.Numerics;

namespace Saba;

public abstract class MMDNode
{
    private readonly FixedArray<Matrix4x4> _globals;
    private readonly FixedArray<Matrix4x4> _inverseInits;

    public int Index { get; set; }

    public string Name { get; set; }

    public bool EnableIK { get; set; }

    public MMDNode? Parent { get; set; }

    public List<MMDNode> Children { get; } = new();

    public Vector3 Translate { get; set; }

    public Quaternion Rotate { get; set; }

    public Vector3 Scale { get; set; }

    public Vector3 AnimTranslate { get; set; }

    public Vector3 AnimateTranslate => AnimTranslate + Translate;

    public Quaternion AnimRotate { get; set; }

    public Quaternion AnimateRotate => AnimRotate * Rotate;

    public Vector3 BaseAnimTranslate { get; set; }

    public Quaternion BaseAnimRotate { get; set; }

    public Quaternion IkRotate { get; set; }

    public Matrix4x4 Local { get; set; }

    public ref Matrix4x4 Global => ref _globals[Index];

    public ref Matrix4x4 InverseInit => ref _inverseInits[Index];

    public Vector3 InitTranslate { get; set; }

    public Quaternion InitRotate { get; set; }

    public Vector3 InitScale { get; set; }

    protected MMDNode(FixedArray<Matrix4x4> globals, FixedArray<Matrix4x4> inverseInits)
    {
        _globals = globals;
        _inverseInits = inverseInits;

        Name = string.Empty;
        Translate = Vector3.Zero;
        Rotate = Quaternion.Identity;
        Scale = Vector3.One;
        AnimTranslate = Vector3.Zero;
        AnimRotate = Quaternion.Identity;
        BaseAnimTranslate = Vector3.Zero;
        BaseAnimRotate = Quaternion.Identity;
        IkRotate = Quaternion.Identity;
        Local = Matrix4x4.Identity;
        Global = Matrix4x4.Identity;
        InverseInit = Matrix4x4.Identity;
        InitTranslate = Vector3.Zero;
        InitRotate = Quaternion.Identity;
        InitScale = Vector3.One;
    }

    public void SaveInitialTRS()
    {
        InitTranslate = Translate;
        InitRotate = Rotate;
        InitScale = Scale;
    }

    public void LoadInitialTRS()
    {
        Translate = InitTranslate;
        Rotate = InitRotate;
        Scale = InitScale;
    }

    public void SaveBaseAnimation()
    {
        BaseAnimTranslate = AnimTranslate;
        BaseAnimRotate = AnimRotate;
    }

    public void LoadBaseAnimation()
    {
        AnimTranslate = BaseAnimTranslate;
        AnimRotate = BaseAnimRotate;
    }

    public void ClearBaseAnimation()
    {
        AnimTranslate = Vector3.Zero;
        AnimRotate = Quaternion.Identity;
    }

    public void AddChild(MMDNode child)
    {
        Children.Add(child);

        child.Parent = this;
    }

    public void BeginUpdateTransform()
    {
        LoadInitialTRS();

        IkRotate = Quaternion.Identity;

        OnBeginUpdateTransform();
    }

    public void EndUpdateTransform()
    {
        OnEndUpdateTransform();
    }

    public void UpdateLocalTransform()
    {
        OnUpdateLocalTransform();
    }

    public void UpdateGlobalTransform()
    {
        if (Parent == null)
        {
            Global = Local;
        }
        else
        {
            Global = Local * Parent.Global;
        }

        foreach (MMDNode child in Children)
        {
            child.UpdateGlobalTransform();
        }
    }

    public void UpdateChildTransform()
    {
        foreach (MMDNode child in Children)
        {
            child.UpdateGlobalTransform();
        }
    }

    public void CalculateInverseInitTransform()
    {
        InverseInit = Global.Invert();
    }

    protected virtual void OnBeginUpdateTransform()
    {

    }

    protected virtual void OnEndUpdateTransform()
    {

    }

    protected virtual void OnUpdateLocalTransform()
    {
        Matrix4x4 t = Matrix4x4.CreateTranslation(AnimateTranslate);
        Matrix4x4 r = Matrix4x4.CreateFromQuaternion(AnimateRotate);
        Matrix4x4 s = Matrix4x4.CreateScale(Scale);

        if (EnableIK)
        {
            r *= Matrix4x4.CreateFromQuaternion(IkRotate);
        }

        Local = s * r * t;
    }
}
