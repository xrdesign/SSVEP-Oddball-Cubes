using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class EscapeUtil {
    public static bool EpsilonEquals(float a, float b, float epsilon)
    {
        return Mathf.Abs(a - b) <= epsilon;
    }

    public static bool EpsilonEquals(Vector3 a, Vector3 b, float epsilon)
    {
        return EpsilonEquals(a.x, b.x, epsilon) && EpsilonEquals(a.y, b.y, epsilon) && EpsilonEquals(a.z, b.z, epsilon);
    }

    public static bool EpsilonEquals(Quaternion a, Quaternion b, float epsilon)
    {
        return EpsilonEquals(a.x, b.x, epsilon) && EpsilonEquals(a.y, b.y, epsilon) && EpsilonEquals(a.z, b.z, epsilon) && EpsilonEquals(a.w, b.w, epsilon);
    }

    private static float ClampAngle(float a)
    {
        a %= 360;
        if (a < 0)
            return 360 + a;
        return a;
    }

    public static Vector3 ClampAngle(Vector3 a)
    {
        return new Vector3(ClampAngle(a.x), ClampAngle(a.y), ClampAngle(a.z));
    }

    public static bool EulerAngleEpsilonEquals(Vector3 a, Vector3 b, float epsilon)
    {
        Quaternion left = Quaternion.Euler(a);
        Quaternion right = Quaternion.Euler(b);
        return Quaternion.Angle(left, right) <= epsilon;
    }

    public static bool LessThan(Vector3 a, Vector3 b)
    {
        return a.x < b.x && a.y < b.y && a.z < b.z;
    }
}
