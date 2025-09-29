using UnityEngine;
using System.Runtime.Serialization;
using System.Collections;

public class Vector3Surrogate : ISerializationSurrogate
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
    public System.Object SetObjectData(System.Object obj, SerializationInfo info,
        StreamingContext context, ISurrogateSelector selector)
    {

        Vector3 v3 = (Vector3)obj;
        v3.x = (float)info.GetValue("x", typeof(float));
        v3.y = (float)info.GetValue("y", typeof(float));
        v3.z = (float)info.GetValue("z", typeof(float));
        obj = v3;
        return obj;
    }
}

public class QuaternionSurrogate : ISerializationSurrogate
{

    // Method called to serialize a Quaternion object
    public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
    {

        Quaternion q4 = (Quaternion)obj;
        info.AddValue("w", q4.w);
        info.AddValue("x", q4.x);
        info.AddValue("y", q4.y);
        info.AddValue("z", q4.z);
    }

    // Method called to deserialize a Quaternion object
    public System.Object SetObjectData(System.Object obj, SerializationInfo info,
        StreamingContext context, ISurrogateSelector selector)
    {

        Quaternion q4 = (Quaternion)obj;
        q4.w = (float)info.GetValue("w", typeof(float));
        q4.x = (float)info.GetValue("x", typeof(float));
        q4.y = (float)info.GetValue("y", typeof(float));
        q4.z = (float)info.GetValue("z", typeof(float));
        obj = q4;
        return obj;
    }
}