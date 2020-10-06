using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SKLibrary.SaveAndLoad;
using System;

public class GameManager : MonoBehaviour
{
    FadeManager fadeManager;

    void Start()
    {

        Initialization();
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    StopWorld();
        //}
        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    StartWorld();
        //}
    }

    void Initialization()
    {
        fadeManager = FindObjectOfType<FadeManager>();
        if (fadeManager==null)
        {
            return;
        }
        fadeManager.StartFadeIn();

    }

    /// <summary>
    /// 時間を止める
    /// </summary>
    void StopWorld()
    {
        Time.timeScale = 0;
    }

    /// <summary>
    /// 時間を動かす
    /// </summary>
    void StartWorld()
    {
        Time.timeScale = 1;
    }
}
