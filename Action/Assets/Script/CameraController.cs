using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;

    public float sensitivity = 3;
    [HideInInspector]
    public float mouseX, mouseY;
    public float maxUpAngle = 80;
    public float maxDownAngle = -80;
    public Transform player;
    public Transform CameraPosition;

    private void Awake()
    {
        cam = this.GetComponent<Camera>();

        //カーソルロック
        Cursor.lockState = CursorLockMode.Locked;

        //カーソル不可視化
        Cursor.visible = false;
    }

    private float rotX = 0.0f, rotY = 0.0f;

    [HideInInspector]
    public float rotZ = 0.0f;

    private void Update()
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
        //プレイヤーの動きにも適応
        player.Rotate(Vector3.up * mouseX);
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
}
