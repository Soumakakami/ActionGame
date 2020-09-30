using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gool : MonoBehaviour
{
    private GameObject cam;
    private GameManager gameManager;
    void Start()
    {
        cam = GameObject.Find("Main Camera");
        gameManager = FindObjectOfType<GameManager>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            gameManager.Gool();
        }
    }
}
