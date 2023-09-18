using Saba.Helpers;
using System.Numerics;

namespace SabaViewer.Tools;

public class Camera
{
    private Vector3 front = -Vector3.UnitZ;
    private Vector3 up = Vector3.UnitY;
    private Vector3 right = Vector3.UnitX;
    private float pitch;
    private float yaw = -MathHelper.PiOver2;
    private float fov = MathHelper.PiOver2;

    public int Width { get; set; }

    public int Height { get; set; }

    public Vector3 Position { get; set; } = new(0.0f, 0.0f, 0.0f);

    public Vector3 Front => front;

    public Vector3 Up => up;

    public Vector3 Right => right;

    public float Pitch
    {
        get => MathHelper.RadiansToDegrees(pitch);
        set
        {
            pitch = MathHelper.DegreesToRadians(MathHelper.Clamp(value, -89f, 89f));

            UpdateVectors();
        }
    }

    public float Yaw
    {
        get => MathHelper.RadiansToDegrees(yaw);
        set
        {
            yaw = MathHelper.DegreesToRadians(value);

            UpdateVectors();
        }
    }

    public float Fov
    {
        get => MathHelper.RadiansToDegrees(fov);
        set
        {
            fov = MathHelper.DegreesToRadians(MathHelper.Clamp(value, 1f, 90f));
        }
    }

    public Matrix4x4 View => Matrix4x4.CreateLookAt(Position, Position + Front, Up);

    public Matrix4x4 Projection => Matrix4x4.CreatePerspectiveFieldOfView(fov, (float)Width / Height, 0.1f, 1000.0f);

    private void UpdateVectors()
    {
        front.X = MathF.Cos(pitch) * MathF.Cos(yaw);
        front.Y = MathF.Sin(pitch);
        front.Z = MathF.Cos(pitch) * MathF.Sin(yaw);

        front = Vector3.Normalize(front);

        right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
        up = Vector3.Normalize(Vector3.Cross(right, front));
    }
}
