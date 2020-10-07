using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SKLibrary.SaveAndLoad;

public class InputUserData : MonoBehaviour
{
    CSVController controller=new CSVController();
    public Text name;
    UserData userData;
    public GameObject button;
    public GameObject note;
    public void UserDataCreate()
    {
        if (!controller.CheckFile("UserData"))
        {
            LocalSave();
            return;
        }

        button.SetActive(false);
        note.SetActive(true);

        userData =new UserData(name.text, controller.GetLength("UserData"));



        string[] userDataStr=new string[3];

        userDataStr[0] = userData.userName;
        userDataStr[1] = userData.id.ToString();
        userDataStr[2] = userData.bestTime.ToString();

        StartCoroutine(controller.AddSave("UserData", userDataStr, MyDestroy));

        TitleManager titleManager = FindObjectOfType<TitleManager>();
        SaveLoadSystem.Save((object)userData, "UserData");
        if (titleManager!=null)
        {
            titleManager.Initialization();
        }
        
    }

    private void LocalSave()
    {
        button.SetActive(false);
        note.SetActive(true);
        userData = new UserData(name.text, 0);

        SaveLoadSystem.Save((object)userData, "UserData");

        TitleManager titleManager = FindObjectOfType<TitleManager>();

        if (titleManager != null)
        {
            titleManager.Initialization();
        }
        Destroy(gameObject);
    }
    void MyDestroy(bool _flag)
    {
        if (_flag)
        {
            Destroy(gameObject);
        }
        else
        {

        }
    }
}
