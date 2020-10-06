using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeManager : SingletonMonoBehaviour<FadeManager>
{
    bool fadeOutFlag=false;
    bool fadeInFlag=false;

    [SerializeField]
    Image fadeImage;

    [SerializeField]
    Color imageColor;

    string sceneName;
    public void StartFadeOut(string _sceneName)
    {
        if (fadeOutFlag==true)
        {
            return;
        }
        fadeImage.gameObject.SetActive(true);
        sceneName = _sceneName;
        fadeOutFlag = true;
        StartCoroutine(FadeOut(SceneChange));
    }

    IEnumerator FadeOut(Action _end=null)
    {
        imageColor.a = 0;
        fadeImage.color = imageColor;
        while (true)
        {
            fadeImage.color = imageColor;
            imageColor.a += Time.deltaTime;
            if (fadeImage.color.a>=1f)
            {
                break;
            }
            yield return null;
        }


        if (_end!=null)
        {
            _end();
        }

        fadeOutFlag = false;
        yield break;
    }

    public void StartFadeIn()
    {
        if (fadeInFlag == true)
        {
            return;
        }
        fadeInFlag = true;
        StartCoroutine(FadeIn(NotActive));
    }
    IEnumerator FadeIn(Action _end=null)
    {
        imageColor.a = 1;
        fadeImage.color = imageColor;
        while (true)
        {
            fadeImage.color = imageColor;
            imageColor.a -= Time.deltaTime;
            if (fadeImage.color.a <= 0f)
            {
                break;
            }
            yield return null;
        }


        if (_end != null)
        {
            _end();
        }

        fadeInFlag = false;
        yield break;
    }

    void NotActive()
    {
        fadeImage.gameObject.SetActive(false);
    }
    void SceneChange()
    {
        SceneManager.LoadScene(sceneName);
    }
}
