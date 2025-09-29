using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using LSL;

public class LightDir : MonoBehaviour {

    public GameObject lighter;
    public GameObject vrViewer;

    private RaycastHit hit;
    private StreamWriter _writer;
    private string currObjLookAtStr = "";

    private string monitorData = "";

    private float secondsCount = 0.0f;

    public Material customMaterial;

    protected Dictionary<string, Material[]> originalSharedRendererMaterials = new Dictionary<string, Material[]>();
    protected Dictionary<string, Material[]> originalRendererMaterials = new Dictionary<string, Material[]>();

    private liblsl.StreamOutlet markerStream;

    // Use this for initialization
    void Start () {
        string filename = String.Format("{1}_{0:MMddyyyy-HHmmss}{2}", DateTime.Now, "GazeRecord", ".txt");
        Directory.CreateDirectory(@"C:\EscapeRoomData");
        string path = Path.Combine(@"C:\EscapeRoomData", filename);
        _writer = File.CreateText(path);
        _writer.Write("\n\n=============== Game started ================\n\n");

        originalSharedRendererMaterials = new Dictionary<string, Material[]>();
        originalRendererMaterials = new Dictionary<string, Material[]>();

        liblsl.StreamInfo inf = new liblsl.StreamInfo("ProEyeMarker", "Markers", 1, 0, liblsl.channel_format_t.cf_string, "giu4569");
        markerStream = new liblsl.StreamOutlet(inf);
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        if ( Physics.Raycast(lighter.transform.position, lighter.transform.forward, out hit) )
        {
            if (currObjLookAtStr != hit.collider.gameObject.name)
            {
                //StoreOriginalMaterials(hit.collider.gameObject);
                //ChangeToHighlightColor(hit.collider.gameObject);
                if (currObjLookAtStr != "")
                {
                    _writer.WriteLine("\t\t\t" + secondsCount);
                    secondsCount = 0.0f;
                    
                }

                if (hit.collider.gameObject.GetComponent<UniqueID>())
                    currObjLookAtStr = hit.collider.gameObject.GetComponent<UniqueID>().guid;
                else 
                    currObjLookAtStr = hit.collider.gameObject.name;
                //print(currObjLookAtStr + " : " + secondsCount);
                monitorData = String.Format(/*String.Format("{0:HH:mm:ss.fff}", DateTime.Now) 
                                            + "\t" + */Time.time.ToString() + "\t\t" + currObjLookAtStr);
                _writer.Write(monitorData);
                //Material[] tempMaterials = { hit.collider.GetComponent<MeshRenderer>().materials[0], hit.collider.GetComponent<MeshRenderer>().materials[1], customMaterial };
                //hit.collider.GetComponent<MeshRenderer>().materials = tempMaterials;
                string[] tempSample = { currObjLookAtStr };
                markerStream.push_sample(tempSample);

                
            }
            secondsCount += Time.fixedDeltaTime;
            //print(currObjLookAtStr + " : " + secondsCount);
        }
    }

    void OnDestroy()
    {
        _writer.WriteLine("\t\t\t" + secondsCount);
        _writer.Close();
    }

    public string getCurrGazeObjName()
    {
        return monitorData;
    }

    protected virtual void StoreOriginalMaterials(GameObject obj)
    {
        originalSharedRendererMaterials.Clear();
        originalRendererMaterials.Clear();
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            string objectReference = renderer.gameObject.GetInstanceID().ToString();
            originalSharedRendererMaterials[objectReference] = renderer.sharedMaterials;
            originalRendererMaterials[objectReference] = renderer.materials;
            renderer.sharedMaterials = originalSharedRendererMaterials[objectReference];
        }
    }

    protected virtual void ChangeToHighlightColor(GameObject obj)
    {
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        Material[] swapCustomMaterials = new Material[renderer.materials.Length + 1];

        for (int i = 0; i < renderer.materials.Length; i++)
        {
            Material material = renderer.materials[i];
            swapCustomMaterials[i] = material;
        }

        swapCustomMaterials[renderer.materials.Length] = customMaterial;

        if (customMaterial != null)
        {
            renderer.materials = swapCustomMaterials;
        }
        //Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        //for (int j = 0; j < renderers.Length; j++)
        //{
        //    Renderer renderer = renderers[j];
        //    Material[] swapCustomMaterials = new Material[renderer.materials.Length+1];

        //    for (int i = 0; i < renderer.materials.Length; i++)
        //    {
        //        Material material = renderer.materials[i];
        //        swapCustomMaterials[i] = material;
        //    }

        //    swapCustomMaterials[renderer.materials.Length] = customMaterial;

        //    if (customMaterial != null)
        //    {
        //        renderer.materials = swapCustomMaterials;
        //    }
        //}
    }


}
