using SKLibrary.SaveAndLoad;
using System;

[Serializable]
public class UserData
{
    public int userId;
    public string userClass;
    public string userName;

    /// <summary>
    /// ユーザーの情報
    /// </summary>
    /// <param name="_userId"></param>
    /// <param name="_userClass"></param>
    /// <param name="_userName"></param>
    public UserData(int _userId, string _userClass , string _userName)
    {
        userId =_userId;
        userClass = _userClass;
        userName = _userName;
    }
}
