using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using SKLibrary;
using UnityEngine.UI;
using System;

public class CSVTest : MonoBehaviour
{
    CSVController cSVController = new CSVController();
    string[][] data;
    private void Start()
    {
        int userID = (cSVController.GetLength("UserData"))-1;
        string[] a = { userID.ToString(), "GT4A", "各務" };
        data = cSVController.AllLoad("UserData");
        //data[0][0] = "2";
        StartCoroutine(cSVController.OverwriteSave("UserData", data));
        //StartCoroutine(cSVController.AddSave("UserData",a));
        //cSVController.Save("Test",a);
    }
}
