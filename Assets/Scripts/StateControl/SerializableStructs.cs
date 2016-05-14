using UnityEngine;

//==================================================
//  Custom serializable structs
//==================================================

namespace StateControl
{

    [System.Serializable]
    public struct SVector3
    {
        public SVector3(Vector3 vec) { x = vec.x; y = vec.y; z = vec.z; }
        public SVector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }

        public static implicit operator SVector3(Vector3 vec) { return new SVector3(vec); }
        public static implicit operator Vector3(SVector3 vec) { return new Vector3(vec.x, vec.y, vec.z); }

        public float x, y, z;
    }
    [System.Serializable]
    public struct SVector2
    {
        public SVector2(Vector2 vec) { x = vec.x; y = vec.y; }
        public SVector2(float x, float y) { this.x = x; this.y = y; }

        public static implicit operator SVector2(Vector2 vec) { return new SVector2(vec); }
        public static implicit operator Vector2(SVector2 vec) { return new Vector2(vec.x, vec.y); }

        public float x, y;
    }
    [System.Serializable]
    public struct SQuaternion
    {
        public SQuaternion(Quaternion q) { x = q.x; y = q.y; z = q.z; w = q.w; }
        public SQuaternion(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }

        public static implicit operator SQuaternion(Quaternion q) { return new SQuaternion(q); }
        public static implicit operator Quaternion(SQuaternion q) { return new Quaternion(q.x, q.y, q.z, q.w); }

        public float x, y, z, w;
    }
    [System.Serializable]
    public struct SColor
    {
        public SColor(Color c) { r = c.r; g = c.g; b = c.b; a = c.a; }
        public SColor(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }

        public static implicit operator SColor(Color c) { return new SColor(c); }
        public static implicit operator Color(SColor c) { return new Color(c.r, c.g, c.b, c.a); }

        public float r, g, b, a;
    }
}

