using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeCameraFOV : MonoBehaviour {

    public Camera cam;
    public GameObject VisualSpectrumObject;

    // Use this for initialization
    void Start()
    {
        if (!cam)
            cam = gameObject.GetComponent<Camera>(); //finds camera on this object
        if (!VisualSpectrumObject)
        {
            VisualSpectrumObject = GameObject.CreatePrimitive(PrimitiveType.Cube); //makes a cube
        }
 
        Destroy(VisualSpectrumObject.GetComponent<BoxCollider>()); //destroy the box collider on the cube because it's not needed
        MeshFilter meshFilter = VisualSpectrumObject.GetComponent<MeshFilter>(); //get the meshfilter on cube
                                                              //make a new mesh


        Mesh mesh = new Mesh();
        Vector3[] points = new Vector3[5];
        points[0] = cam.transform.position;
        points[1] = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.farClipPlane));
        points[2] = cam.ViewportToWorldPoint(new Vector3(0, 1, cam.farClipPlane));
        points[3] = cam.ViewportToWorldPoint(new Vector3(1, 0, cam.farClipPlane));
        points[4] = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.farClipPlane));
        mesh.vertices = new Vector3[] {
        points[0], points[1], points[2],
        points[0], points[3], points[1],
        points[0], points[4], points[2],
        points[0], points[3], points[4],
        points[1], points[2], points[4],
        points[1], points[4], points[3]
        };

        mesh.triangles = new int[] {
        0, 1, 2,
        3, 4, 5,
        8, 7, 6,
        11, 10, 9,
        14, 13, 12,
        17, 16, 15
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.MarkDynamic();
        //set the new mesh to cube's mesh
        meshFilter.mesh = mesh;

        Renderer ren = VisualSpectrumObject.GetComponent<MeshRenderer>();
        if (ren)
        {
            ren.sharedMaterial = (Material)Resources.Load("AreaVisibleLow", typeof(Material));
            ren.receiveShadows = false;
            ren.shadowCastingMode = 0;
        }
        VisualSpectrumObject.gameObject.layer = LayerMask.NameToLayer("SpectatorViewOnly");
        //set the camera as the cube's parent
        VisualSpectrumObject.transform.SetParent(cam.transform);
    }

    // Update is called once per frame
    void Update () {
		
	}

    private void OnDisable()
    {
        VisualSpectrumObject.SetActive(false);
    }

    private void OnEnable()
    {
        VisualSpectrumObject.SetActive(true);
    }
}
