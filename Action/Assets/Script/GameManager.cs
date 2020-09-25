using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SKLibrary.SaveAndLoad;
using System;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    UserData userData;

    private void Start()
    {
        //userData = new UserData(1,"a","s");
        //SaveUserData();
        LoadUserData();
        Debug.Log(userData.userId);
    }

    public void LoadUserData()
    {
        if (SaveLoadSystem.Load("UserData") == null)
        {
            Debug.Log("呼ばれた先にフォルダないよ");
            return;
        }
        //Userデータを渡す
        userData = (UserData)SaveLoadSystem.Load("UserData");
    }

    public void SaveUserData()
    {
        SaveLoadSystem.Save(userData,"UserData");
    }
}
