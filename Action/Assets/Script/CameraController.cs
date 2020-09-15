using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float camHeight = 1;
    [SerializeField]
    private float fog;
    [SerializeField, Header("プレイヤー")]
    private GameObject player;


    private Camera cam;
    void Start()
    {
        //プレイヤーがアタッチされていなければプレイヤーをさがして適応
        if (!player) { player = GameObject.FindObjectOfType<PlayerController>().gameObject; }
        cam = GetComponent<Camera>();
        fog = cam.fieldOfView;
    }


    void Update()
    {

    }

    private void TargetFollow()
    {

    }
}
