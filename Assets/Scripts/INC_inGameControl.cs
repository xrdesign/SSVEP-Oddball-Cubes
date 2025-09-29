using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class INC_inGameControl : MonoBehaviour {

    public InputField surveyTimerSetInput;
    public Text surveyCountdownText;
    public Text onoffStatusText;
    public Text surveyResultText;
    public SurveyController SurveyDataProvider;

    public Text gazeResultText;
    public LightDir GazeDataProvider;

    public GameObject head;

    public int lapsTime;
    public bool isSurveyEnable;

    float sinceLastAction;
    string SurveyRecordList = "";
    string currSurveyResult = "";

    string GazeRecordList = "";
    string currGazeResult = "";

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        
        
        if (SurveyDataProvider)
        {
            isSurveyEnable = SurveyDataProvider.getStatus();

            if (lapsTime != 0 && !isSurveyEnable)
            {
                sinceLastAction += Time.deltaTime;

                if (sinceLastAction > lapsTime)
                {

                    //SurveyDataProvider.GetComponent<SurveyController>().toggle(true, 0);
                    toggleHeadSurvey(true);
                    isSurveyEnable = SurveyDataProvider.GetComponent<SurveyController>().getStatus();
                    sinceLastAction -= lapsTime;
                }
            }

            string newSurveyResult = SurveyDataProvider.getSurveyResult();
            if (newSurveyResult != currSurveyResult)
            {
                currSurveyResult = newSurveyResult;
                if (SurveyRecordList == "Waiting for data")
                    SurveyRecordList = "";
                SurveyRecordList = currSurveyResult + SurveyRecordList;
            }
            SurveyRecordList = (currSurveyResult == "" ? "Waiting for data" : SurveyRecordList);
        }
        else
        {
            SurveyRecordList = "Survey has been disable for this scene";
        }
        
        if (GazeDataProvider)
        {
            string newGazeResult = GazeDataProvider.getCurrGazeObjName();
            if (newGazeResult != currGazeResult)
            {
                currGazeResult = newGazeResult;
                if (GazeRecordList == "Waiting for data")
                    GazeRecordList = "";
                GazeRecordList = currGazeResult + "\n" + GazeRecordList;
            }
            GazeRecordList = (currGazeResult == "" ? "Waiting for data" : GazeRecordList);
            if (GazeRecordList.Length > 200)
            {
                GazeRecordList = GazeRecordList.Substring(0, 200);
            }
        }
        else
        {
            GazeRecordList = "Gaze has been disable for this scene";
        }


    }

    private void LateUpdate()
    {
        surveyCountdownText.text = ((int)sinceLastAction).ToString();
        surveyResultText.text = SurveyRecordList;

        if(gazeResultText)
            gazeResultText.text = GazeRecordList;
        onoffStatusText.text = (isSurveyEnable ? "On" : "Off");
        onoffStatusText.color = (isSurveyEnable ? Color.green : Color.red);
    }

    public void toggleHeadSurvey(bool b)
    {

        if (!b)
        {
            SurveyDataProvider.whichToEnable = 0;
        }
        SurveyDataProvider.toggle(b);

        if (SurveyDataProvider.transformFollowController)
        {
            SurveyDataProvider.gameObject.transform.localEulerAngles =
                new Vector3(0, SurveyDataProvider.transformFollowController.gameObjectToFollow.transform.localEulerAngles.y, 0);
        }

        //if (b)
        //{
        //    SurveyDataProvider = Instantiate(SurveyDataProviderPrefab);
        //    SurveyDataProvider.whichToEnable = 0;
        //    SurveyDataProvider.toggle(b);
        //    SurveyDataProvider.GetComponent<VRTK.VRTK_TransformFollow>().gameObjectToFollow = head;
        //    SurveyDataProvider.gameObject.transform.localEulerAngles =
        //        new Vector3(0, head.transform.localEulerAngles.y, 0);
        //}
        //else
        //{
        //    Destroy(SurveyDataProvider);
        //    SurveyDataProvider = null;
        //}
    }

    public void setTimerforSurveyByStr(string s)
    {
        int timeInt = 0;
        if (int.TryParse(s, out timeInt))
        {
            lapsTime = timeInt;
        }
        else
        {
            Debug.Log("String - " + s + " - is not int");
        }
    }

    public void ShowExplorer(/*string itemPath*/)
    {
        string itemPath = "C:/Program Files";
        itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like front slashes
        System.Diagnostics.Process.Start("explorer.exe", "/select," + itemPath);
    }

}
