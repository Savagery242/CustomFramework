using System.Runtime.Serialization;
using UnityEngine;
using UnityEditor;

namespace StateControl
{
    sealed class QuaternionSerializationSurrogate : ISerializationSurrogate
    {
        // Method called to serialize a Vector3 object
        public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
        {
            
            Quaternion q = (Quaternion)obj;
            info.AddValue("x", q.x);
            info.AddValue("y", q.y);
            info.AddValue("z", q.z);
            info.AddValue("w", q.w);
        }

        // Method called to deserialize a Vector3 object
        public System.Object SetObjectData(System.Object obj,
                                           SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {

            Quaternion q = (Quaternion)obj;
            q.x = info.GetSingle("x");
            q.y = info.GetSingle("y");
            q.z = info.GetSingle("z");
            q.w = info.GetSingle("w");
            obj = q;
            return obj;
        }
    }

    sealed class Vector3SerializationSurrogate : ISerializationSurrogate
    {

        // Method called to serialize a Vector3 object
        public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
        {

            Vector3 v3 = (Vector3)obj;
            info.AddValue("x", v3.x);
            info.AddValue("y", v3.y);
            info.AddValue("z", v3.z);
        }

        // Method called to deserialize a Vector3 object
        public System.Object SetObjectData(System.Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {

            Vector3 v3 = (Vector3)obj;
            v3.x = info.GetSingle("x");
            v3.y = info.GetSingle("y");
            v3.z = info.GetSingle("z");
            obj = v3;
            return obj;
        }
    }

    sealed class Vector2SerializationSurrogate : ISerializationSurrogate
    {

        // Method called to serialize a Vector3 object
        public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
        {

            Vector2 v2 = (Vector2)obj;
            info.AddValue("x", v2.x);
            info.AddValue("y", v2.y);
        }

        // Method called to deserialize a Vector3 object
        public System.Object SetObjectData(System.Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {

            Vector2 v2 = (Vector2)obj;
            v2.x = info.GetSingle("x");
            v2.y = info.GetSingle("y");
            obj = v2;
            return obj;
        }
    }

    sealed class ColorSerializationSurrogate : ISerializationSurrogate
    {

        // Method called to serialize a Vector3 object
        public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
        {

            Color c = (Color)obj;
            info.AddValue("r", c.r);
            info.AddValue("g", c.g);
            info.AddValue("b", c.b);
            info.AddValue("a", c.a);
        }

        // Method called to deserialize a Vector3 object
        public System.Object SetObjectData(System.Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {

            Color c = (Color)obj;
            c.r = info.GetSingle("r");
            c.g = info.GetSingle("g");
            c.b = info.GetSingle("b");
            c.a = info.GetSingle("a");
            obj = c;
            return obj;
        }
    }

    sealed class MeshSerializationSurrogate : ISerializationSurrogate
    {

        // Method called to serialize a Vector3 object
        public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
        {

            Mesh m = (Mesh)obj;
            info.AddValue("meshPath", AssetDatabase.GetAssetPath(m));
            info.AddValue("meshName", m.name);

        }

        // Method called to deserialize a Vector3 object
        public System.Object SetObjectData(System.Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {

            Mesh m = (Mesh)obj;
            string fullPath = info.GetString("meshPath") + "/" + info.GetString("meshName");
            obj = AssetDatabase.LoadAssetAtPath<Mesh>(fullPath);
            Debug.Log(fullPath);
            return obj;
        }
    }

    sealed class GameObjectSerializationSurrogate : ISerializationSurrogate
    {

        // Method called to serialize a Vector3 object
        public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
        {

            GameObject go = (GameObject)obj;
            SavableObject so = go.GetComponent<SavableObject>();
            if (so == null)
            {
                Debug.LogError("Attempting to serialize GameObject with no SavableObject component attached. You must attach a SavableObject component to the desired GameObject in order to serialize it. ", go);
                return;
            }
            info.AddValue("guid", so.guid);

        }

        // Method called to deserialize a Vector3 object
        public System.Object SetObjectData(System.Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {

            GameObject m = (GameObject)obj;
            string guid = info.GetString("guid");
            SavableObject[] sos = StateController.GetAllSavableObjects();
            foreach (var so in sos)
            {
                if (so.guid == guid)
                {
                    m = so.gameObject;
                }
            }
            return m;
        }
    }

}
