using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankingManager : MonoBehaviour
{
    const int RANKING_COUNT=3;
    CSVController controller;
    [SerializeField]
    List<float> rankings = new List<float>();
    string[][] rankingData=new string[RANKING_COUNT][];
    [SerializeField]
    string stageName;
    float time = 24;
    string[] userData;
    private void Start()
    {
        controller = new CSVController();
        rankingData=controller.AllLoad(stageName);
        Debug.Log(rankingData.Length);
        for (int i = 0; i < controller.GetLength(stageName)-1; i++)
        {
            rankings.Add(float.Parse(rankingData[i][2]));
        }
        rankings.Add(time);
        for (int i = 0; i < rankings.Count; i++)
        {
            for (int j = i+1; j < rankings.Count; j++)
            {
                if (rankings[i]>rankings[j])
                {

                }
            }
        }
    }
}
