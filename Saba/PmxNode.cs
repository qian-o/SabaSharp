using System.Numerics;

namespace Saba;

public class PmxNode : MMDNode
{
    public int DeformDepth { get; set; }

    public bool IsDeformAfterPhysics { get; set; }

    public PmxNode? AppendNode { get; set; }

    public bool IsAppendRotate { get; set; }

    public bool IsAppendTranslate { get; set; }

    public bool IsAppendLocal { get; set; }

    public float AppendWeight { get; set; }

    public Vector3 AppendTranslate { get; set; }

    public Quaternion AppendRotate { get; set; }

    public MMDIkSolver? IkSolver { get; set; }

    public PmxNode()
    {
        DeformDepth = -1;
    }

    public void UpdateAppendTransform()
    {
        if (AppendNode == null)
        {
            return;
        }

        if (IsAppendRotate)
        {
            Quaternion appendRotate;
            if (IsAppendLocal)
            {
                appendRotate = AppendNode.AnimateRotate;
            }
            else
            {
                if (AppendNode.AppendNode != null)
                {
                    appendRotate = AppendNode.AppendRotate;
                }
                else
                {
                    appendRotate = AppendNode.AnimateRotate;
                }
            }

            if (AppendNode.EnableIK)
            {
                appendRotate = AppendNode.IkRotate * appendRotate;
            }

            AppendRotate = Quaternion.Slerp(Quaternion.Identity, appendRotate, AppendWeight);
        }

        if (IsAppendTranslate)
        {
            Vector3 appendTranslate;
            if (IsAppendLocal)
            {
                appendTranslate = AppendNode.Translate - AppendNode.InitTranslate;
            }
            else
            {
                if (AppendNode.AppendNode != null)
                {
                    appendTranslate = AppendNode.AppendTranslate;
                }
                else
                {
                    appendTranslate = AppendNode.Translate - AppendNode.InitTranslate;
                }
            }

            AppendTranslate = appendTranslate * AppendWeight;
        }

        UpdateLocalTransform();
    }

    protected override void OnBeginUpdateTransform()
    {
        AppendTranslate = Vector3.Zero;
        AppendRotate = Quaternion.Identity;
    }

    protected override void OnEndUpdateTransform()
    {

    }

    protected override void OnUpdateLocalTransform()
    {
        Vector3 t = AnimateTranslate;
        if (IsAppendTranslate)
        {
            t += AppendTranslate;
        }

        Quaternion r = AnimateRotate;
        if (EnableIK)
        {
            r = IkRotate * r;
        }
        if (IsAppendRotate)
        {
            r *= AppendRotate;
        }

        Vector3 s = Scale;

        Local = Matrix4x4.CreateScale(s) * Matrix4x4.CreateFromQuaternion(r) * Matrix4x4.CreateTranslation(t);
    }
}
