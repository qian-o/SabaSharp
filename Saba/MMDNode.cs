using Saba.Helpers;
using Silk.NET.Maths;

namespace Saba;

public abstract class MMDNode
{
    public int Index { get; set; }

    public string Name { get; set; }

    public bool EnableIK { get; set; }

    public MMDNode? Parent { get; set; }

    public List<MMDNode> Children { get; } = new();

    public Vector3D<float> Translate { get; set; }

    public Quaternion<float> Rotate { get; set; }

    public Vector3D<float> Scale { get; set; }

    public Vector3D<float> AnimTranslate { get; set; }

    public Quaternion<float> AnimRotate { get; set; }

    public Vector3D<float> BaseAnimTranslate { get; set; }

    public Quaternion<float> BaseAnimRotate { get; set; }

    public Quaternion<float> IkRotate { get; set; }

    public Matrix4X4<float> Local { get; set; }

    public Matrix4X4<float> Global { get; set; }

    public Matrix4X4<float> InverseInit { get; set; }

    public Vector3D<float> InitTranslate { get; set; }

    public Quaternion<float> InitRotate { get; set; }

    public Vector3D<float> InitScale { get; set; }

    protected MMDNode()
    {
        Name = string.Empty;
        Translate = Vector3D<float>.Zero;
        Rotate = Quaternion<float>.Identity;
        Scale = Vector3D<float>.One;
        AnimTranslate = Vector3D<float>.Zero;
        AnimRotate = Quaternion<float>.Identity;
        BaseAnimTranslate = Vector3D<float>.Zero;
        BaseAnimRotate = Quaternion<float>.Identity;
        IkRotate = Quaternion<float>.Identity;
        Local = Matrix4X4<float>.Identity;
        Global = Matrix4X4<float>.Identity;
        InverseInit = Matrix4X4<float>.Identity;
        InitTranslate = Vector3D<float>.Zero;
        InitRotate = Quaternion<float>.Identity;
        InitScale = Vector3D<float>.One;
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

    public void AddChild(MMDNode child)
    {
        Children.Add(child);

        child.Parent = this;
    }

    public void BeginUpdateTransform()
    {
        LoadInitialTRS();

        IkRotate = Quaternion<float>.Identity;

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
            child.UpdateChildTransform();
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
        Matrix4X4<float> t = Matrix4X4.CreateTranslation(AnimTranslate);
        Matrix4X4<float> r = Matrix4X4.CreateFromQuaternion(AnimRotate);
        Matrix4X4<float> s = Matrix4X4.CreateScale(Scale);

        if (EnableIK)
        {
            r *= Matrix4X4.CreateFromQuaternion(IkRotate);
        }

        Local = s * r * t;
    }
}
