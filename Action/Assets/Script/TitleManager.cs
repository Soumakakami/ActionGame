using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SKLibrary.SaveAndLoad;

public class TitleManager : MonoBehaviour
{
    public Text userName;
    public Text userID;
    public Text userBestTime;

    public GameObject userDataInput;

    UserData userData;
    void Start()
    {
        Initialization();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialization()
    {
        FindObjectOfType<FadeManager>().StartFadeIn();
        userData = (UserData)SaveLoadSystem.Load("UserData");
        if (userData==null)
        {
            Instantiate(userDataInput);
            return;
        }

        SetUserDataUI();
    }

    /// <summary>
    /// UserのデータをUIに表示させます
    /// </summary>
    public void SetUserDataUI()
    {
        if (userData==null)
        {
            userName.text = "Null";
            userID.text = "0";
            userBestTime.text = "999.999";
        }
        userName.text       =   userData.userName;
        userID.text         =   userData.id.ToString();
        userBestTime.text   =   userData.bestTime.ToString();
    }
}
