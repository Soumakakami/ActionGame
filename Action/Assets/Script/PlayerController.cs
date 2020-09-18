﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// プレイヤーの遷移状態
    /// </summary>
    enum PlayereState
    {
        Default = 0,
        WallRun = 1,
        Sliding = 2,
        Squat = 3
    }

    /// <summary>
    /// 壁走りが左右どちらでおこなっているか
    /// </summary>
    enum WallRunState
    {
        Right = 0,
        Left = 1
    }


    [SerializeField]
    PlayereState playereState = PlayereState.Default;
    [SerializeField]
    WallRunState wallRunState = WallRunState.Right;


    [Header("プレイヤー基礎関連")]
    [SerializeField]
    float walkSpeed = 10;                       //歩く速度
    [SerializeField]
    float airSpeed = 10;                        //空中の移動速度
    [SerializeField]
    float gravity = 9.8f;                       //重力
    [SerializeField]
    float jumpForce = 10;                   //ジャンプ力

    [Header("壁走り関連")]
    [SerializeField]
    float wallRunSpeed = 10;
    [SerializeField]
    float wallJumpForce = 10;

    [Header("制限")]
    [SerializeField]
    float slideAngle = 0;                   //スライディング中の地面の角度
    [SerializeField]
    float breakAngle = 60;                  //ウォールラン中にカメラをどの程度動かせるかの制御
    [SerializeField]
    LayerMask layer;                        //Rayが適応されるレイヤー
    [SerializeField]
    float maxAddSpeed = 10;                 //加速度の限界を設定

    [Header("アタッチするオブジェクト")]
    [SerializeField]
    GameObject foot;                        //足元オブジェクト
    [SerializeField]
    GameObject head;                        //頭オブジェクト
    [SerializeField]
    CameraController cam;                   //カメラコントローラークラス

    [Header("値をいじらないでください")]
    [SerializeField]
    bool ground = false;                      //地面に接しているか可視化
    [SerializeField]
    bool ctrlKey = false;
    [SerializeField]
    float addSpeed = 1;                     //歩く速度とは別の加速度
    [SerializeField]
    float wallCheckDistance = 1;                //壁のチェックするRayの長さ


    //キャラクターコンローラー
    CharacterController controller;         //キャラクターコントローラー	
    float footDistance = 0;                 //足元までの距離	

    float characterVelocityY;               //キャラクターのY方向の値
    Vector3 characterVelocityMomentum;      //キャラクターの感性方向
    float moveX;                            //X軸の移動値
    float moveZ;                            //Z軸の移動値
    public Vector3 characterVelocity;              //キャラクターの移動値
    Vector3 playerAngle;                    //キャラクターの角度
    bool wallRanIntervalFlag = true;
    float camAngle = 0;
    float wallAngle = 0;

    private void Start()
    {
        //コントローラーを取得
        controller = GetComponent<CharacterController>();
        //足元オブジェクトを取得
        footDistance = Vector3.Distance(foot.transform.position, transform.position);
        //カメラを取得
        cam = FindObjectOfType<CameraController>();
    }

    private void Update()
    {
        //地面チェックを毎フレーム行う
        ground = controller.isGrounded;

        //加速度が限界を超えた場合限界で止める
        if (addSpeed >= maxAddSpeed) addSpeed = maxAddSpeed;
        ctrlKey = Input.GetKey(KeyCode.LeftControl);
        transform.localScale = (ctrlKey ? new Vector3(1,0.5f,1): new Vector3(1, 1, 1));

        //プレイヤーの遷移状態に応じて行う処理を変える
        switch (playereState)
        {
            default:
            case PlayereState.Default:
                Movemnet();
                break;
            case PlayereState.WallRun:
                WallRunMovemnet();
                break;
            case PlayereState.Sliding:
                SlidingMovemnet();
                break;
            case PlayereState.Squat:
                SquatMovemnet();
                break;
        }

    }

    /// <summary>
    /// 上方向に力を加える
    /// </summary>
    /// <param name="_force">力の量</param>
    public void Jump(float _force)
    {
        characterVelocityY = _force;

    }

    /// <summary>
    /// 力を加える方向
    /// </summary>
    /// <param name="_force">力の方向</param>
    public void AddForce(Vector3 _force)
    {
        characterVelocity += _force;
    }

    /// <summary>
    /// 力の向きを代入する
    /// </summary>
    /// <param name="_force">力の方向</param>
    public void SetForce(Vector3 _force)
    {
        characterVelocity = _force;
    }

    /// <summary>
    /// 地面の角度を求める
    /// </summary>
    /// <returns></returns>
    private float GroundSlope()
    {
        //Rayを地面に向けて作成
        Ray ray = new Ray(transform.position, transform.up * -1);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * 3);
        //地面の角度
        float angle = 0;

        //Rayがヒットしたところの角度を計算
        if (Physics.Raycast(ray, out hit, 3, layer))
        {
            var axis = Vector3.Cross(transform.up, hit.normal);
            angle = Vector3.Angle(transform.up, hit.normal) * (axis.x < 0 ? -1 : 1);
        }
        return angle;
    }

    /// <summary>
    /// 頭上に何もないか検知する
    /// </summary>
    private void CeilingCheck()
    {
        //真上に伸びるRayを作成
        Ray ray = new Ray(head.transform.position, transform.up);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * 0.2f);

        //何かしらのオブジェクトにHitした場合
        if (Physics.Raycast(ray, out hit, 0.2f, layer))
        {
            Debug.Log("天井に頭をぶつけました");
            //上方向の力を下方向に変更
            characterVelocityY = -1;
        }
    }

    /// <summary>
    /// 移動の処理
    /// </summary>
    private void Movemnet()
    {
        //カメラの向いている方向にプレイヤーを向ける
        playerAngle = cam.transform.eulerAngles;
        //プレイヤーが上下に回転しないようにxを固定
        playerAngle.x = 0;

        //自身に適応
        transform.eulerAngles = playerAngle;

        moveX = Input.GetAxisRaw("Horizontal");
        moveZ = Input.GetAxisRaw("Vertical");

        Vector3 slidingCheck = characterVelocity;
        slidingCheck.y = 0;
        //スライディング状態
        if (ctrlKey && ground && slidingCheck.magnitude > 0)
        {
            playereState = PlayereState.Sliding;
            addSpeed += 0.2f;
        }
        //しゃがみ状態
        else if (ctrlKey && characterVelocity.x == 0 && characterVelocity.z == 0)
        {
            playereState = PlayereState.Squat;
        }

        //地面感知
        if (controller.isGrounded)
        {
            //加速度を1にする
            addSpeed = 1;
            //カメラの向いている方向に歩く
            characterVelocity = transform.right * moveX * walkSpeed + transform.forward * moveZ * walkSpeed;
            //重力を0に
            characterVelocityY = 0f;

            //ジャンプ
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump(jumpForce);
            }
            //地面にいる間は慣性を受けない
            characterVelocityMomentum = Vector3.zero;
        }

        //地面から離れたら
        if (!controller.isGrounded)
        {
            //左右に壁があるかチェック
            WallCheck();
            //空中にいる間に動ける方向を作成
            characterVelocityMomentum = ((transform.right * moveX + transform.forward * moveZ) * airSpeed * Time.deltaTime) * addSpeed;
        }

        //重力を作成
        float gravityDownForce = gravity * -1;

        //重力を加える
        characterVelocityY += gravityDownForce * Time.deltaTime;

        //重力を適応
        characterVelocity.y = characterVelocityY;

        //キャラクターのうごきに慣性を追加させる
        characterVelocity += characterVelocityMomentum;

        //移動を適応
        controller.Move(characterVelocity.normalized * walkSpeed * addSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 壁走り
    /// </summary>
    private void WallRunMovemnet()
    {

        Vector3 direction;
        int dir;
        switch (wallRunState)
        {
            default:
            case WallRunState.Right:
                direction = transform.right;
                dir = -1;

                break;
            case WallRunState.Left:
                direction = transform.right * -1;
                dir = 1;
                break;
        }
        if (wallRanIntervalFlag)
        {
            Ray ray = new Ray(transform.position, direction);
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction * wallCheckDistance);
            if (Physics.Raycast(ray, out hit, wallCheckDistance))
            {
                transform.right = hit.normal * dir;
                characterVelocityMomentum = Vector3.zero;
                characterVelocity = transform.forward;
                characterVelocityY = 0;

                characterVelocity *= wallRunSpeed * addSpeed;
                controller.Move(characterVelocity * Time.deltaTime);

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    AddForce(hit.normal * wallJumpForce);
                    Jump(jumpForce);
                    addSpeed += 0.1f;
                    playereState = PlayereState.Default;
                    wallRanIntervalFlag = false;
                    StartCoroutine(WallRunInterval());
                }

                var axis = Vector3.Cross(cam.transform.right, hit.normal);
                camAngle = Vector3.Angle(cam.transform.right, hit.normal) * (axis.x < 0 ? -1 : 1);

                var testaxis = Vector3.Cross(transform.right, hit.normal);
                wallAngle = Vector3.Angle(transform.right, hit.normal) * (testaxis.x < 0 ? -1 : 1);

                if (Mathf.Abs(Mathf.Abs(camAngle) - Mathf.Abs(wallAngle)) >= breakAngle)
                {
                    playereState = PlayereState.Default;
                    wallRanIntervalFlag = false;
                    StartCoroutine(WallRunInterval());
                }
            }
            else
            {
                playereState = PlayereState.Default;
            }
        }
    }

    /// <summary>
    /// プレイヤーから左右に壁がないか探す
    /// </summary>
    private void WallCheck()
    {
        if (wallRanIntervalFlag)
        {

            Ray rightRay = new Ray(transform.position, transform.right);
            RaycastHit rightHit;

            Debug.DrawRay(rightRay.origin, rightRay.direction * wallCheckDistance);

            if (Physics.Raycast(rightRay, out rightHit, wallCheckDistance, layer))
            {
                playereState = PlayereState.WallRun;
                wallRunState = WallRunState.Right;
            }

            Ray leftRay = new Ray(transform.position, transform.right * -1);
            RaycastHit leftHit;

            Debug.DrawRay(leftRay.origin, leftRay.direction * wallCheckDistance);

            if (Physics.Raycast(leftRay, out leftHit, wallCheckDistance, layer))
            {
                playereState = PlayereState.WallRun;
                wallRunState = WallRunState.Left;
            }

        }
    }

    /// <summary>
    /// 次にWallRunを行えるインターバル行う
    /// </summary>
    /// <returns></returns>
    private IEnumerator WallRunInterval()
    {
        yield return new WaitForSeconds(0.2f);
        wallRanIntervalFlag = true;
    }

    /// <summary>
    /// しゃがみの移動処理
    /// </summary>
    private void SquatMovemnet()
    {
        //カメラの向いている方向にプレイヤーを向ける
        playerAngle = cam.transform.eulerAngles;
        //プレイヤーが上下に回転しないようにxを固定
        playerAngle.x = 0;

        //自身に適応
        transform.eulerAngles = playerAngle;

        moveX = Input.GetAxisRaw("Horizontal");
        moveZ = Input.GetAxisRaw("Vertical");

        if (!ctrlKey)
        {
            playereState = PlayereState.Default;
        }


        //地面感知
        if (controller.isGrounded)
        {
            //加速度を1にする
            addSpeed = 1;
            //カメラの向いている方向に歩く
            characterVelocity = transform.right * moveX * walkSpeed * 0.5f + transform.forward * moveZ * walkSpeed * 0.5f;
            //重力を0に
            characterVelocityY = 0f;

            //ジャンプ
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump(jumpForce);
            }
            //地面にいる間は慣性を受けない
            characterVelocityMomentum = Vector3.zero;
        }

        //地面から離れたら
        if (!controller.isGrounded)
        {
            //左右に壁があるかチェック
            WallCheck();
            //空中にいる間に動ける方向を作成
            characterVelocityMomentum = ((transform.right * moveX + transform.forward * moveZ) * (airSpeed * 0.5f) * Time.deltaTime) * addSpeed;
        }
        //重力を作成
        float gravityDownForce = gravity * -1;

        //重力を加える
        characterVelocityY += gravityDownForce * Time.deltaTime;

        // 重力を適応
        characterVelocity.y = characterVelocityY;

        // キャラクターのうごきに慣性を追加させる
        characterVelocity += characterVelocityMomentum;

        // 移動を適応
        controller.Move(characterVelocity * Time.deltaTime);
    }

    /// <summary>
    /// スライディングの移動処理
    /// </summary>
    private void SlidingMovemnet()
    {
        characterVelocityY = 0;
        if (wallRanIntervalFlag)
        {

        if (!ctrlKey)
        {
            playereState=PlayereState.Default;
        }

        if (ground)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playereState = PlayereState.Default;
                Jump(jumpForce);
                    wallRanIntervalFlag = false;
                    StartCoroutine(WallRunInterval());
            }
        }
        
        //重力を作成
        float gravityDownForce = gravity * -1;

        //重力を加える
        characterVelocityY += gravityDownForce * Time.deltaTime;

        // 重力を適応
        characterVelocity.y = characterVelocityY;

        // キャラクターのうごきに慣性を追加させる
        characterVelocity += characterVelocityMomentum;

        characterVelocity.Normalize();

        // 移動を適応
        controller.Move(characterVelocity * walkSpeed * addSpeed * Time.deltaTime);

        Ray ray = new Ray(transform.position, transform.up * -1);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * 3);

        ////Rayがヒットしたところの角度を計算
        //if (Physics.Raycast(ray, out hit, 3, layer))
        //{
        //    Vector3 nor=hit.normal;
        //    nor.y = 0;
        //    characterVelocity += nor*10 * Time.deltaTime;
        //    transform.position = new Vector3(transform.position.x, hit.point.y + 0.5f, transform.position.z);
        //}

        }
        // 移動を適応
        controller.Move(characterVelocity * walkSpeed * addSpeed * Time.deltaTime);
    }
}
