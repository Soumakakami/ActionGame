﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum CameraMode
    {
        Frre=0,
        Follow=1,
        Target=2
    }

    public CameraMode cameraMode = CameraMode.Frre;

    private Camera cam;

    public float sensitivity = 3;
    //[HideInInspector]
    public float mouseX, mouseY;
    public float maxUpAngle = 80;
    public float maxDownAngle = -80;
    public Transform player;
    public Transform CameraPosition;

    public Transform target;

    private PlayerController playerController;

    [Header("Fov")]
    [SerializeField]
    float min_Fov;
    [SerializeField]
    float max_Fov;


    private void Start()
    {
        cam = this.GetComponent<Camera>();

        //カーソルロック
        Cursor.lockState = CursorLockMode.Locked;

        //カーソル不可視化
        Cursor.visible = false;
        playerController = FindObjectOfType<PlayerController>();

    }

    private float rotX = 0.0f, rotY = 0.0f;

    public float rotZ = 0.0f;

    private void Update()
    {
        if (playerController.speed>=30)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, min_Fov + (playerController.speed / 50) * max_Fov, 0.1f);
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, min_Fov + (playerController.speed / 50) * max_Fov, 0.01f);
        }
        
        switch (cameraMode)
        {
            default:
            case CameraMode.Frre:
                FreeCamera();
                break;
            case CameraMode.Target:
                TargetCamera(target);
                break;
        }

        DiagonalCamera();
    }

    private void FreeCamera()
    {
        //マウスの入力
        mouseX = Input.GetAxis("Mouse X") * sensitivity;
        mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        //計算
        rotX -= mouseY;
        //上下のカメラの動き制限
        rotX = Mathf.Clamp(rotX, maxDownAngle, maxUpAngle);
        //左右の移動量
        rotY += mouseX;

        //カメラに回転を適応
        transform.localRotation = Quaternion.Euler(rotX, rotY, rotZ);

        //カメラの位置に自身を移動
        transform.position = CameraPosition.position;
    }


    public void Shake(float magnitude, float duration)
    {
        StartCoroutine(IShake(magnitude, duration));
    }

    private IEnumerator IShake(float mag, float dur)
    {
        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();
        for (float t = 0.0f; t <= dur; t += Time.deltaTime)
        {
            rotZ = Random.Range(-mag, mag) * (t / dur - 1.0f);
            yield return wfeof;
        }
        rotZ = 0.0f;
    }

    public void StartTargetCamera(Transform _target)
    {
        target = _target;
        cameraMode = CameraMode.Follow;
    }

    private void TargetCamera(Transform _target)
    {
        float speed = 0.1f;

        // ターゲット方向のベクトルを取得
        Vector3 relativePos = _target.position - this.transform.position;
        // 方向を、回転情報に変換
        Quaternion rotation = Quaternion.LookRotation(relativePos);
        // 現在の回転情報と、ターゲット方向の回転情報を補完する
        transform.rotation = Quaternion.Slerp(this.transform.rotation, rotation, speed);
    }

    private void FollowCamera()
    {   
        //カメラの位置に自身を移動
        transform.position = CameraPosition.position;
        Vector3 pos = player.forward;
        Quaternion rotation = Quaternion.LookRotation(pos,player.up);
        // 現在の回転情報と、ターゲット方向の回転情報を補完する
        transform.rotation = Quaternion.Slerp(this.transform.rotation, rotation, 0.05f);
    }

    private void DiagonalCamera()
    {
        if (playerController.playereState!=PlayerController.PlayereState.WallRun)
        {
            rotZ = Mathf.Lerp(rotZ,0,0.1f);
            return;
        }
        else
        {
            switch (playerController.wallRunState)
            {
                case PlayerController.WallRunState.Right:
                    rotZ = Mathf.Lerp(rotZ, 20, 0.1f);
                    break;
                case PlayerController.WallRunState.Left:
                    rotZ = Mathf.Lerp(rotZ, -20, 0.1f);
                    break;
                default:
                    rotZ = Mathf.Lerp(rotZ, 20, 0.9f);
                    break;
            }
        }
    }
}
