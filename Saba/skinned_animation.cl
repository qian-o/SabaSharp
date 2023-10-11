__constant float SlerpEpsilon = 1e-6f;

typedef enum { Weight1, Weight2, Weight4, SDEF, DualQuaternion } SkinningType;

typedef struct {
  float X;
  float Y;
} Vector2;

typedef struct {
  float X;
  float Y;
  float Z;
} Vector3;

typedef struct {
  float X;
  float Y;
  float Z;
  float W;
} Vector4;

typedef struct {
  float X;
  float Y;
  float Z;
  float W;
} Quaternion;

typedef struct {
  float M11;
  float M12;
  float M13;
  float M14;

  float M21;
  float M22;
  float M23;
  float M24;

  float M31;
  float M32;
  float M33;
  float M34;

  float M41;
  float M42;
  float M43;
  float M44;
} Matrix4x4;

typedef struct {
  int BoneIndices[2];
  float BoneWeight;
  Vector3 C;
  Vector3 R0;
  Vector3 R1;
} BoneSDEF;

typedef struct {
  SkinningType SkinningType;
  int BoneIndices[4];
  float BoneWeights[4];
  BoneSDEF BoneSDEF;
} VertexBoneInfo;

Vector2 CreateVector2(float x, float y) {
  Vector2 v;

  v.X = x;
  v.Y = y;

  return v;
}

Vector3 CreateVector3(float x, float y, float z) {
  Vector3 v;

  v.X = x;
  v.Y = y;
  v.Z = z;

  return v;
}

Vector4 CreateVector4(float x, float y, float z, float w) {
  Vector4 v;

  v.X = x;
  v.Y = y;
  v.Z = z;
  v.W = w;

  return v;
}

Quaternion CreateQuaternion(float x, float y, float z, float w) {
  Quaternion q;

  q.X = x;
  q.Y = y;
  q.Z = z;
  q.W = w;

  return q;
}

Matrix4x4 CreateMatrix4x4(float m11, float m12, float m13, float m14, float m21,
                          float m22, float m23, float m24, float m31, float m32,
                          float m33, float m34, float m41, float m42, float m43,
                          float m44) {
  Matrix4x4 m;

  m.M11 = m11;
  m.M12 = m12;
  m.M13 = m13;
  m.M14 = m14;

  m.M21 = m21;
  m.M22 = m22;
  m.M23 = m23;
  m.M24 = m24;

  m.M31 = m31;
  m.M32 = m32;
  m.M33 = m33;
  m.M34 = m34;

  m.M41 = m41;
  m.M42 = m42;
  m.M43 = m43;
  m.M44 = m44;

  return m;
}

Vector2 Vector2_Add_Vector2(Vector2 left, Vector2 right) {
  return CreateVector2(left.X + right.X, left.Y + right.Y);
}

float Vector3_Length(Vector3 value) {
  return sqrt((value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z));
}

Vector3 Vector3_Add_Vector3(Vector3 left, Vector3 right) {
  return CreateVector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
}

Vector3 Vector3_Subtract_Vector3(Vector3 left, Vector3 right) {
  return CreateVector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
}

Vector3 Vector3_Multiply_Vector3(Vector3 left, Vector3 right) {
  return CreateVector3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
}

Vector3 Vector3_Multiply_Float(Vector3 left, float right) {
  return Vector3_Multiply_Vector3(left, CreateVector3(right, right, right));
}

Vector3 Vector3_Divide_Vector3(Vector3 left, Vector3 right) {
  return CreateVector3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
}

Vector3 Vector3_Divide_Float(Vector3 value1, float value2) {
  return Vector3_Divide_Vector3(value1, CreateVector3(value2, value2, value2));
}

Vector3 Vector3_Normalize(Vector3 value) {
  return Vector3_Divide_Float(value, Vector3_Length(value));
}

Vector3 Vector3_Transform_Matrix4x4(Vector3 position, Matrix4x4 matrix) {
  return CreateVector3((position.X * matrix.M11) + (position.Y * matrix.M21) +
                           (position.Z * matrix.M31) + matrix.M41,
                       (position.X * matrix.M12) + (position.Y * matrix.M22) +
                           (position.Z * matrix.M32) + matrix.M42,
                       (position.X * matrix.M13) + (position.Y * matrix.M23) +
                           (position.Z * matrix.M33) + matrix.M43);
}

Quaternion Quaternion_Slerp(Quaternion quaternion1, Quaternion quaternion2,
                            float amount) {

  float t = amount;

  float cosOmega =
      quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y +
      quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;

  bool flip = false;

  if (cosOmega < 0.0f) {
    flip = true;
    cosOmega = -cosOmega;
  }

  float s1, s2;

  if (cosOmega > (1.0f - SlerpEpsilon)) {
    // Too close, do straight linear interpolation.
    s1 = 1.0f - t;
    s2 = (flip) ? -t : t;
  } else {
    float omega = acos(cosOmega);
    float invSinOmega = 1 / sin(omega);

    s1 = sin((1.0f - t) * omega) * invSinOmega;
    s2 = (flip) ? -sin(t * omega) * invSinOmega : sin(t * omega) * invSinOmega;
  }

  Quaternion ans;

  ans.X = s1 * quaternion1.X + s2 * quaternion2.X;
  ans.Y = s1 * quaternion1.Y + s2 * quaternion2.Y;
  ans.Z = s1 * quaternion1.Z + s2 * quaternion2.Z;
  ans.W = s1 * quaternion1.W + s2 * quaternion2.W;

  return ans;
}

Quaternion CreateFromRotationMatrix(Matrix4x4 matrix) {
  float trace = matrix.M11 + matrix.M22 + matrix.M33;

  Quaternion q = CreateQuaternion(0.0f, 0.0f, 0.0f, 0.0f);

  if (trace > 0.0f) {
    float s = sqrt(trace + 1.0f);
    q.W = s * 0.5f;
    s = 0.5f / s;
    q.X = (matrix.M23 - matrix.M32) * s;
    q.Y = (matrix.M31 - matrix.M13) * s;
    q.Z = (matrix.M12 - matrix.M21) * s;
  } else {
    if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33) {
      float s = sqrt(1.0f + matrix.M11 - matrix.M22 - matrix.M33);
      float invS = 0.5f / s;
      q.X = 0.5f * s;
      q.Y = (matrix.M12 + matrix.M21) * invS;
      q.Z = (matrix.M13 + matrix.M31) * invS;
      q.W = (matrix.M23 - matrix.M32) * invS;
    } else if (matrix.M22 > matrix.M33) {
      float s = sqrt(1.0f + matrix.M22 - matrix.M11 - matrix.M33);
      float invS = 0.5f / s;
      q.X = (matrix.M21 + matrix.M12) * invS;
      q.Y = 0.5f * s;
      q.Z = (matrix.M32 + matrix.M23) * invS;
      q.W = (matrix.M31 - matrix.M13) * invS;
    } else {
      float s = sqrt(1.0f + matrix.M33 - matrix.M11 - matrix.M22);
      float invS = 0.5f / s;
      q.X = (matrix.M31 + matrix.M13) * invS;
      q.Y = (matrix.M32 + matrix.M23) * invS;
      q.Z = 0.5f * s;
      q.W = (matrix.M12 - matrix.M21) * invS;
    }
  }

  return q;
}

Matrix4x4 Matrix4x4_Add_Matrix4x4(Matrix4x4 value1, Matrix4x4 value2) {
  Matrix4x4 m;

  m.M11 = value1.M11 + value2.M11;
  m.M12 = value1.M12 + value2.M12;
  m.M13 = value1.M13 + value2.M13;
  m.M14 = value1.M14 + value2.M14;
  m.M21 = value1.M21 + value2.M21;
  m.M22 = value1.M22 + value2.M22;
  m.M23 = value1.M23 + value2.M23;
  m.M24 = value1.M24 + value2.M24;
  m.M31 = value1.M31 + value2.M31;
  m.M32 = value1.M32 + value2.M32;
  m.M33 = value1.M33 + value2.M33;
  m.M34 = value1.M34 + value2.M34;
  m.M41 = value1.M41 + value2.M41;
  m.M42 = value1.M42 + value2.M42;
  m.M43 = value1.M43 + value2.M43;
  m.M44 = value1.M44 + value2.M44;

  return m;
}

Matrix4x4 Matrix4x4_Multiply_Float(Matrix4x4 value1, float value2) {

  Matrix4x4 m;

  m.M11 = value1.M11 * value2;
  m.M12 = value1.M12 * value2;
  m.M13 = value1.M13 * value2;
  m.M14 = value1.M14 * value2;
  m.M21 = value1.M21 * value2;
  m.M22 = value1.M22 * value2;
  m.M23 = value1.M23 * value2;
  m.M24 = value1.M24 * value2;
  m.M31 = value1.M31 * value2;
  m.M32 = value1.M32 * value2;
  m.M33 = value1.M33 * value2;
  m.M34 = value1.M34 * value2;
  m.M41 = value1.M41 * value2;
  m.M42 = value1.M42 * value2;
  m.M43 = value1.M43 * value2;
  m.M44 = value1.M44 * value2;

  return m;
}

Matrix4x4 CreateFromQuaternion(Quaternion quaternion) {

  Matrix4x4 result =
      CreateMatrix4x4(1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                      0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f);

  float xx = quaternion.X * quaternion.X;
  float yy = quaternion.Y * quaternion.Y;
  float zz = quaternion.Z * quaternion.Z;

  float xy = quaternion.X * quaternion.Y;
  float wz = quaternion.Z * quaternion.W;
  float xz = quaternion.Z * quaternion.X;
  float wy = quaternion.Y * quaternion.W;
  float yz = quaternion.Y * quaternion.Z;
  float wx = quaternion.X * quaternion.W;

  result.M11 = 1.0f - 2.0f * (yy + zz);
  result.M12 = 2.0f * (xy + wz);
  result.M13 = 2.0f * (xz - wy);

  result.M21 = 2.0f * (xy - wz);
  result.M22 = 1.0f - 2.0f * (zz + xx);
  result.M23 = 2.0f * (yz + wx);

  result.M31 = 2.0f * (xz + wy);
  result.M32 = 2.0f * (yz - wx);
  result.M33 = 1.0f - 2.0f * (yy + xx);

  return result;
}

kernel void Run(global const Vector3 *positions, global const Vector3 *normals,
                global const Vector2 *uvs, global const Vector3 *morphPositions,
                global const Vector4 *morphUVs,
                global const VertexBoneInfo *vertexBoneInfos,
                global const Matrix4x4 *transforms,
                global const Matrix4x4 *globalTransforms,
                global Vector3 *updatePositions, global Vector3 *updateNormals,
                global Vector2 *updateUVs) {

  int i = get_global_id(0);

  Vector3 position = positions[i];
  Vector3 normal = normals[i];
  Vector2 uv = uvs[i];
  Vector3 morphPos = morphPositions[i];
  Vector4 morphUV = morphUVs[i];
  VertexBoneInfo vtxInfo = vertexBoneInfos[i];

  Matrix4x4 m;

  if (vtxInfo.SkinningType == Weight1) {
    m = transforms[vtxInfo.BoneIndices[0]];
  } else if (vtxInfo.SkinningType == Weight2) {
    Matrix4x4 m1 = Matrix4x4_Multiply_Float(transforms[vtxInfo.BoneIndices[0]],
                                            vtxInfo.BoneWeights[0]);
    Matrix4x4 m2 = Matrix4x4_Multiply_Float(transforms[vtxInfo.BoneIndices[1]],
                                            vtxInfo.BoneWeights[1]);
    m = Matrix4x4_Add_Matrix4x4(m1, m2);
  } else if (vtxInfo.SkinningType == Weight4) {
    Matrix4x4 m1 = Matrix4x4_Multiply_Float(transforms[vtxInfo.BoneIndices[0]],
                                            vtxInfo.BoneWeights[0]);
    Matrix4x4 m2 = Matrix4x4_Multiply_Float(transforms[vtxInfo.BoneIndices[1]],
                                            vtxInfo.BoneWeights[1]);
    Matrix4x4 m3 = Matrix4x4_Multiply_Float(transforms[vtxInfo.BoneIndices[2]],
                                            vtxInfo.BoneWeights[2]);
    Matrix4x4 m4 = Matrix4x4_Multiply_Float(transforms[vtxInfo.BoneIndices[3]],
                                            vtxInfo.BoneWeights[3]);
    m = Matrix4x4_Add_Matrix4x4(
        Matrix4x4_Add_Matrix4x4(Matrix4x4_Add_Matrix4x4(m1, m2), m3), m4);
  } else if (vtxInfo.SkinningType == SDEF) {
    int i0 = vtxInfo.BoneSDEF.BoneIndices[0];
    int i1 = vtxInfo.BoneSDEF.BoneIndices[1];
    float w0 = vtxInfo.BoneSDEF.BoneWeight;
    float w1 = 1.0f - vtxInfo.BoneSDEF.BoneWeight;
    Vector3 center = vtxInfo.BoneSDEF.C;
    Vector3 cr0 = vtxInfo.BoneSDEF.R0;
    Vector3 cr1 = vtxInfo.BoneSDEF.R1;
    Quaternion q0 = CreateFromRotationMatrix(globalTransforms[i0]);
    Quaternion q1 = CreateFromRotationMatrix(globalTransforms[i1]);
    Matrix4x4 m0 = transforms[i0];
    Matrix4x4 m1 = transforms[i1];

    Vector3 pos = Vector3_Add_Vector3(position, morphPos);
    Matrix4x4 rot_mat = CreateFromQuaternion(Quaternion_Slerp(q0, q1, w1));

    Vector3 v1 = Vector3_Transform_Matrix4x4(
        Vector3_Subtract_Vector3(pos, center), rot_mat);
    Vector3 v2 =
        Vector3_Multiply_Float(Vector3_Transform_Matrix4x4(cr0, m0), w0);
    Vector3 v3 =
        Vector3_Multiply_Float(Vector3_Transform_Matrix4x4(cr1, m1), w1);

    updatePositions[i] = Vector3_Add_Vector3(Vector3_Add_Vector3(v1, v2), v3);
    updateNormals[i] = Vector3_Transform_Matrix4x4(normal, rot_mat);
  }

  if (vtxInfo.SkinningType != SDEF) {
    updatePositions[i] =
        Vector3_Transform_Matrix4x4(Vector3_Add_Vector3(position, morphPos), m);
    updateNormals[i] =
        Vector3_Normalize(Vector3_Transform_Matrix4x4(normal, m));
  }

  updateUVs[i] = Vector2_Add_Vector2(uv, CreateVector2(morphUV.X, morphUV.Y));
}