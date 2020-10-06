using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class RankingManager : MonoBehaviour
{
    const int MAX_RANKING_USER_COUNT=5;
    const string FILE_NAME = "Ranking";
    CSVController controller = new CSVController();
    public List<UserData> userDatas;
    string[][] strs;

    public Text[] rankings;
    public Text[] bestTimes;
    private void Start()
    {
        strs = controller.AllLoad(FILE_NAME);

        UserDataConversion(strs);

        //Sort(userDatas);

        //StartCoroutine(controller.OverwriteSave(FILE_NAME, UserDataJug())); 

        for (int i = 0; i < userDatas.Count; i++)
        {
            if (userDatas[i].userName.Length <= 3)
            {
                rankings[i].text = (i + 1).ToString() + "     " + userDatas[i].id.ToString("D3") + "  " + userDatas[i].userName+ "   " ;
            }
            else
            {
                rankings[i].text = (i + 1).ToString() + "     " + userDatas[i].id.ToString("D3") + "  " + userDatas[i].userName.Substring(0, 4) + "   ";
            }
            bestTimes[i].text = userDatas[i].bestTime.ToString();
        }
    }

    void UserDataConversion(string[][] _userDatas)
    {
        for (int i = 0; i < MAX_RANKING_USER_COUNT; i++)
        {
            userDatas.Add(new UserData(_userDatas[i][0],int.Parse(_userDatas[i][1]),float.Parse(_userDatas[i][2])));
        }
    }
    string[][] UserDataJug()
    {
        string[][]users = new string[userDatas.Count][];
        for (int i = 0; i < userDatas.Count; i++)
        {
            string[] u=new string[3];
            u[0] = userDatas[i].userName;
            u[1] = userDatas[i].id.ToString();
            u[2] = userDatas[i].bestTime.ToString();
            users[i] = u;
        }
        return users;
    }
    void  Sort(List<UserData> _userDatas)
    {
        for (int i = 0; i < _userDatas.Count; i++)
        {
            for (int j = i+1; j < _userDatas.Count; j++)
            {
                if (_userDatas[i].bestTime>_userDatas[j].bestTime)
                {
                    var user = userDatas[i];
                    userDatas[i] = userDatas[j];
                    userDatas[j] = user;
                }
            }
        }
    }
}
