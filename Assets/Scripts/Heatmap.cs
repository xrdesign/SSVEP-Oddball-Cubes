using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class Heatmap : MonoBehaviour
{
    public Material mat;

    public Vector3 startingPoint;
    public Vector3 step;
    public Vector3Int size;

    public float[] data;

    public Texture3D heatData;

    public float affectingRadius;
    public float maxValue;
    public float minValue;
    public float smoothness = 15;

    private Camera _camera;

    public bool save = false;
    public float rate = 0.001f;
    public RenderTexture rt;

    public bool enableHeatmap = false;
    public bool lastState = false;

    public bool forceUpdate = false;

    void Start()
    {
        _camera = GetComponent<Camera>();
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;

        // Create data holder on CPU
        data = new float[size.x * size.y * size.z];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = 0;
        }

        // Create texture3d on GPU
        heatData = new Texture3D(size.x, size.y, size.z, GraphicsFormat.R32_SFloat, TextureCreationFlags.None);
        heatData.wrapMode = TextureWrapMode.Clamp;
        heatData.filterMode = FilterMode.Point;

        mat.SetTexture("_HeatData", heatData);
        mat.SetFloat("startX", startingPoint.x);
        mat.SetFloat("startY", startingPoint.y);
        mat.SetFloat("startZ", startingPoint.z);

        mat.SetFloat("stepX", step.x);
        mat.SetFloat("stepY", step.y);
        mat.SetFloat("stepZ", step.z);

        mat.SetInt("sizeX", size.x);
        mat.SetInt("sizeY", size.y);
        mat.SetInt("sizeZ", size.z);

        mat.SetFloat("enableHeatmap", -1);
    }

    public void SetHeatmap(bool enable)
    {
        if (enable)
        {
            mat.SetFloat("enableHeatmap", 1);
            
        }
        else
        {
            mat.SetFloat("enableHeatmap", -1);
        }
        enableHeatmap = enable;
    }

    //void LateUpdate()
    //{

    //    // To investigate: do we need to use non-jittered version for antialiasing effects?
    //    var p = _camera.projectionMatrix;
    //    // Undo some of the weird projection-y things so it's more intuitive to work with.
    //    p[2, 3] = p[3, 2] = 0.0f;
    //    p[3, 3] = 1.0f;

    //    // I'll confess I don't understand entirely why this is right,
    //    // I just kept fiddling with numbers until it worked.
    //    p = Matrix4x4.Inverse(p * _camera.worldToCameraMatrix)
    //        * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);

    //    _material.SetMatrix("_ClipToWorld", p);
    //}

    //[ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //mat.SetMatrix("_ViewProjectInverse", (_camera.projectionMatrix * _camera.worldToCameraMatrix).inverse);
        var p = GL.GetGPUProjectionMatrix(_camera.projectionMatrix, false);// Unity flips its 'Y' vector depending on if its in VR, Editor view or game view etc... (facepalm)
        p[2, 3] = p[3, 2] = 0.0f;
        p[3, 3] = 1.0f;
        var clipToWorld = Matrix4x4.Inverse(p * _camera.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);
        mat.SetMatrix("clipToWorld", clipToWorld);
        mat.SetTexture("_MainTex", source);

        mat.SetFloat("affectingRadius", affectingRadius);
        mat.SetFloat("smoothness", 15);
        mat.SetFloat("_MinVal", minValue);
        mat.SetFloat("_MaxVal", maxValue);

        int rangeX = Mathf.CeilToInt(affectingRadius / step.x);
        int rangeY = Mathf.CeilToInt(affectingRadius / step.y);
        int rangeZ = Mathf.CeilToInt(affectingRadius / step.z);

        mat.SetInt("rangeX", rangeX);
        mat.SetInt("rangeY", rangeY);
        mat.SetInt("rangeZ", rangeZ);

        if (enableHeatmap)
        {
            Graphics.Blit(source, destination, mat);
            if (rt)
            {
                Graphics.Blit(source, rt, mat);
            }
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
    public void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
    {
        byte[] _bytes = _texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullPath, _bytes);
        Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullPath);
    }

    void Update()
    {
        if (save)
        {
            //SaveTextureAsPNG(heatData, "D://test.png");
            save = false;
        }
    }

    public float GazeAt(Vector3 gip, float timePassed)
    {
        // range affected 
        int rangeX = Mathf.CeilToInt(affectingRadius / step.x);
        int rangeY = Mathf.CeilToInt(affectingRadius / step.y);
        int rangeZ = Mathf.CeilToInt(affectingRadius / step.z);

        // starting xyz in index
        int X0 = Mathf.FloorToInt((gip.x - startingPoint.x) / step.x);
        int Y0 = Mathf.FloorToInt((gip.y - startingPoint.y) / step.y);
        int Z0 = Mathf.FloorToInt((gip.z - startingPoint.z) / step.z);

        // Loops over all the points
        float h = 0;
        float scale = maxValue - minValue;
        float oldValue = 0.0f;

        for (int i = X0 - rangeX; i < X0 + rangeX; i++)
        {
            for (int j = Y0 - rangeY; j < Y0 + rangeY; j++)
            {
                for (int k = Z0 - rangeZ; k < Z0 + rangeZ; k++)
                {
                    // Sanity check
                    if (i < 0 || i >= size.x || j < 0 || j >= size.y || k < 0 || k >= size.z)
                    {
                        continue;
                    }

                    // look up in the texture
                    Vector3 gridPos = new Vector3(i * step.x + startingPoint.x,
                        j * step.y + startingPoint.y,
                        k * step.z + startingPoint.z);

                    float distance = (gip - gridPos).magnitude;
                    float ratio = 1.0f - distance * distance * distance / affectingRadius;
                    if (ratio < 0)
                    {
                        ratio = 0;
                    }
                    float value = (ratio * rate * timePassed);

                    int index = k * size.y * size.x + j * size.x + i;
                    //if (index<0 || index>= )

                    oldValue += ratio * data[index];

                    data[index] += value;
                }
            }
        }

        // Calculates the contribution of each point
        //half di = distance(worldspace, );

        //half hi = 1 - saturate(di / affectingRadius);

        //h += hi * (_Properties[i].w - _MinVal) / scale;
        //h += hi * (_Properties[i].w) / smoothness;
        if (lastState != enableHeatmap)
        {
            heatData.SetPixelData(data, 0);
            heatData.Apply();
        }

        if (forceUpdate)
        {
            heatData.SetPixelData(data, 0);
            heatData.Apply();
        }

        lastState = enableHeatmap;
        return oldValue;
    }
}
