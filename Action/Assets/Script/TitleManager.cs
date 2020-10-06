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

    public GameObject cam;

    UserData userData;
    bool flag = true;
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

    public void CamMove(float _angle)
    {
        if (!flag)
        {
            return;
        }
        flag = false;
        StartCoroutine(CameraMove(_angle));
    }

    IEnumerator CameraMove(float _angle)
    {
        
        while (true)
        {
            cam.transform.eulerAngles = new Vector3(0,Mathf.Lerp(cam.transform.eulerAngles.y,_angle,0.1f),0);
            if (Mathf.Abs(_angle-cam.transform.eulerAngles.y) <= 1)
            {
                break;
            }
            yield return null;
        }

        Debug.Log("終了");
        flag = true;
        transform.eulerAngles = new Vector3(0,_angle,0);
        yield break;
    }
}
