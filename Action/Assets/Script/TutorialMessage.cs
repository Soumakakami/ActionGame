using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMessage : MonoBehaviour
{
    public string str;
    public Text tex;
    void Start()
    {
        tex = GameObject.Find("Question").GetComponent<Text>();
        Invoke("Test",Time.deltaTime);
    }

    private void Test()
    {
        tex.transform.parent.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        tex.transform.parent.gameObject.SetActive(true);
        tex.text=str;
    }

    private void OnTriggerExit(Collider other)
    {
        tex.transform.parent.gameObject.SetActive(false);
    }

}
