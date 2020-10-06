using SKLibrary.SaveAndLoad;
using System;

[Serializable]
public class UserData
{
    public string userName;
    public float bestTime;
    public int id;
    /// <summary>
    /// ユーザーの情報
    /// </summary>
    /// <param name="_userId"></param>
    /// <param name="_userClass"></param>
    /// <param name="_userName"></param>
    public UserData(string _userName, int _id = 0,float _bestTime=999.999f)
    {
        userName = _userName;
        bestTime = _bestTime;
        id = _id;
    }
}
