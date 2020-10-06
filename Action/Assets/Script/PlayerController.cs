using SKLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// プレイヤーの遷移状態
    /// </summary>
    public enum PlayereState
    {
        Default = 0,
        WallRun = 1,
        Sliding = 2,
        Squat = 3
    }

    /// <summary>
    /// 壁走りが左右どちらでおこなっているか
    /// </summary>
    public enum WallRunState
    {
        Right = 1,
        Left = -1
    }

    [Header("プレイヤーのスーテト")]
    [SerializeField]
    public PlayereState playereState = PlayereState.Default;
    [SerializeField]
    public WallRunState wallRunState = WallRunState.Right;


    [Header("プレイヤー基礎関連")]
    [SerializeField]
    float walkSpeed = 10;                       //歩く速度
    [SerializeField]
    float airSpeed = 10;                        //空中の移動速度
    [SerializeField]
    float gravity = 9.8f;                       //重力
    [SerializeField]
    float jumpForce = 10;                       //ジャンプ力

    [Header("壁走り関連")]
    [SerializeField]
    float wallJumpForce = 10;

    [Header("制限")]
    [SerializeField]
    float breakAngle = 60;                      //ウォールラン中にカメラをどの程度動かせるかの制御
    [SerializeField]
    LayerMask layer;                            //Rayが適応されるレイヤー
    [SerializeField]
    LayerMask ceilingLayer;                     //頭上のRayのレイヤー制限
    [SerializeField]
    float maxSpeed = 10;                        //加速度の限界を設定
    [SerializeField]
    float friction = 10;                        //スライディング時の摩擦
    [SerializeField]
    float maxLine = 300;                        //集中線の多さ

    [Header("アタッチするオブジェクト")]
    [SerializeField]
    GameObject foot;                            //足元オブジェクト
    [SerializeField]
    GameObject head;                            //頭オブジェクト
    [SerializeField]
    CameraController cam;                       //カメラコントローラークラス
    [SerializeField]
    ParticleSystem effect_Line;                 //集中線エフェクト

    [Header("値をいじらないでください")]
    [SerializeField]
    bool ground = false;                        //地面に接しているか可視化
    [SerializeField]
    bool ctrlKey = false;
    [SerializeField]
    float addSpeed = 1;                         //歩く速度とは別の加速度
    [SerializeField]
    float wallCheckDistance = 1;                //壁のチェックするRayの長さ

    [Header("変動する値")]
    [SerializeField]
    bool airJumpFlag = true;
    [SerializeField]
    public float speed;
    [SerializeField]
    private bool leftKeyFlag = false;
    [SerializeField]
    private bool rightKeyFlag = false;
    //キャラクターコンローラー
    CharacterController controller;             //キャラクターコントローラー	
    float footDistance = 0;                     //足元までの距離	

    float characterVelocityY;                   //キャラクターのY方向の値
    Vector3 characterVelocityMomentum;          //キャラクターの感性方向
    float moveX;                                //X軸の移動値
    float moveZ;                                //Z軸の移動値
    public Vector3 characterVelocity;           //キャラクターの移動値
    Vector3 playerAngle;                        //キャラクターの角度
    bool wallRanIntervalFlag = true;
    float camAngle = 0;
    float wallAngle = 0;
    float timer;

    private void Start()
    {
        //コントローラーを取得
        controller = GetComponent<CharacterController>();
        //足元オブジェクトを取得
        footDistance = Vector3.Distance(foot.transform.position, transform.position);
        //カメラを取得
        cam = FindObjectOfType<CameraController>();

        //エフェクト開始
        effect_Line.Play();
    }

    private void Update()
    {
        //地面チェックを毎フレーム行う
        ground = controller.isGrounded;
		if (ground) airJumpFlag = true;
        leftKeyFlag = Input.GetKey(KeyCode.A);
        rightKeyFlag = Input.GetKey(KeyCode.D);
        Vector3 mag = characterVelocity;
        mag.y = 0;

        var em = effect_Line.emission;

        if (speed>=30)
        {
            em.rateOverTime = maxLine * (speed / maxSpeed);
        }
        else
        {
            em.rateOverTime = 0;
        }
       
        effect_Line.gameObject.transform.position = transform.position + (mag.normalized*4);
        speed = mag.magnitude;
        CeilingCheck();

        ctrlKey = Input.GetKey(KeyCode.LeftControl);
        if (ctrlKey)
        {
            transform.localScale = new Vector3(1, 0.5f, 1);
        }
        else if (!ctrlKey && !CeilingCheck())
        {
            transform.localScale = new Vector3(1,1,1);
        }

        //プレイヤーの遷移状態に応じて行う処理を変える
        switch (playereState)
        {
            default:
            case PlayereState.Default:
                Movemnet();
                timer = 0;
                break;
            case PlayereState.WallRun:
                WallRunMovemnet();
                timer = 0;
                break;
            case PlayereState.Sliding:
                SlidingMovemnet();
                break;
            case PlayereState.Squat:
                SquatMovemnet();
                timer = 0;
                break;
        }

    }

    public void Warp(Vector3 _pos)
    {
        gameObject.GetComponent<CharacterController>().enabled = false;
        transform.position = _pos;
        ResetData();
        gameObject.GetComponent<CharacterController>().enabled = true;
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
    /// JumpPadの処理
    /// </summary>
    /// <param name="_force"></param>
    public void JumpPad(float _force)
    {
        characterVelocityY = 0;
        characterVelocityY = _force;
        characterVelocity.y = characterVelocityY;
        controller.Move(characterVelocity*Time.deltaTime);
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
    private bool CeilingCheck()
    {
        bool topHit = false;
        //真上に伸びるRayを作成
        Ray ray = new Ray(head.transform.position, transform.up);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * 0.2f);

        //何かしらのオブジェクトにHitした場合
        if (Physics.Raycast(ray, out hit, 0.2f, ceilingLayer))
        {
            //上方向の力を下方向に変更
            characterVelocityY = -1;
            topHit = true;
            Debug.Log("頭当たったよ");
        }
        return topHit;
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
        playerAngle.z = 0;

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
            return;
        }
        //しゃがみ状態
        else if (ctrlKey && characterVelocity.x == 0 && characterVelocity.z == 0)
        {
            playereState = PlayereState.Squat;
            return;
        }

        //地面感知
        if (controller.isGrounded)
        {
            //カメラの向いている方向に歩く
            characterVelocity = transform.right * moveX  + transform.forward * moveZ;

            //重力を0に
            characterVelocityY = 0f;

            //正規化したほうにスピード分加速
            characterVelocity=characterVelocity.normalized * walkSpeed;

            //ジャンプ
            if (Input.GetKeyDown(KeyCode.Space))
            {
                characterVelocityY = 0;
                Jump(jumpForce);
                wallRanIntervalFlag = false;
                StartCoroutine(WallRunInterval());
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
			if (Input.GetKeyDown(KeyCode.Space) && airJumpFlag)
			{
				characterVelocityMomentum.y = 0;
				Vector3 mag = characterVelocity;
				mag.y = 0;
				characterVelocity = characterVelocityMomentum.normalized * mag.magnitude;
				Jump(jumpForce);
				airJumpFlag = false;
			}
        }

        //重力を作成
        float gravityDownForce = gravity * -1;

        //重力を加える
        characterVelocityY += gravityDownForce * Time.deltaTime;



        //キャラクターのうごきに慣性を追加させる
        characterVelocity += characterVelocityMomentum;
        if (maxSpeed <= (Mathf.Abs(characterVelocity.x) + Mathf.Abs(characterVelocity.z)))
        {
            characterVelocity.y = 0;
            characterVelocity = characterVelocity.normalized * maxSpeed;
        }

        Ray ray = new Ray(foot.transform.position, transform.up * -1);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * 0.2f);


        if (Physics.Raycast(ray, out hit, 1f, layer)&& wallRanIntervalFlag)
        {
            controller.Move(hit.point-transform.position);
            ground = true;
            Debug.Log("地面");
        }
        //重力を適応
        characterVelocity.y = characterVelocityY;
        //移動を適応
        controller.Move(characterVelocity * Time.deltaTime);

    }

    /// <summary>
    /// 壁走り
    /// </summary>
    private void WallRunMovemnet()
    {
        Vector3 test;
        test = characterVelocity;
        test.y = 0;
        Vector3 direction;
        int dir;

        int keyDir = 0;
        if (leftKeyFlag)
        {
            keyDir = 1;
        }
        else if (rightKeyFlag)
        {
            keyDir = 1;
        }
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
            Debug.DrawRay(ray.origin, ray.direction * wallCheckDistance* keyDir);
            
            Ray forwardRay = new Ray(transform.position,transform.forward);
            RaycastHit forwardRayhit;


            if (Physics.Raycast(forwardRay,out forwardRayhit, wallCheckDistance))
            {
                test.x = 0;
                test.z = 0;
            }

            //自身から左右に壁がないかチェック
            if (Physics.Raycast(ray, out hit, wallCheckDistance * keyDir, layer))
            {
				airJumpFlag = true;
				transform.right = hit.normal * dir;
                characterVelocityMomentum = Vector3.zero;
                characterVelocity = transform.forward * test.magnitude;
                characterVelocityY = 0;
                characterVelocity += hit.normal * -1;
                controller.Move(characterVelocity * Time.deltaTime);

                //壁ジャンプ
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Vector3 vec = characterVelocity;
                    vec.y = 0;
                    AddForce((vec.normalized * -1) * wallJumpForce);
                    AddForce(hit.normal * wallJumpForce);

                    Jump(jumpForce);
                    playereState = PlayereState.Default;
                    wallRanIntervalFlag = false;
                    StartCoroutine(WallRunInterval());
                }

                //カメラのアングルを取得
                var axis = Vector3.Cross(cam.transform.right, hit.normal);
                camAngle = Vector3.Angle(cam.transform.right, hit.normal) * (axis.x < 0 ? -1 : 1);

                //自身のアングルを取得
                var testaxis = Vector3.Cross(transform.right, hit.normal);
                wallAngle = Vector3.Angle(transform.right, hit.normal) * (testaxis.x < 0 ? -1 : 1);

                //カメラの角度が正面から一定角度を超えた場合
                if (Mathf.Abs(Mathf.Abs(camAngle) - Mathf.Abs(wallAngle)) >= breakAngle)
                {
                    //ウォールランモードを解除
                    playereState = PlayereState.Default;
                    //ウォールランフラグをオフ
                    wallRanIntervalFlag = false;
                    //一定時間ウォールランモードにしない
                    StartCoroutine(WallRunInterval());
                }
            }
            else
            {
                //フラグがオフならデフォルトモードに戻る
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
            if (rightKeyFlag)
            {
                Ray rightRay = new Ray(transform.position, transform.right);
                RaycastHit rightHit;

                Debug.DrawRay(rightRay.origin, rightRay.direction * wallCheckDistance);

                if (Physics.Raycast(rightRay, out rightHit, wallCheckDistance, layer))
                {
                    playereState = PlayereState.WallRun;
                    wallRunState = WallRunState.Right;
                }
            }

            if (leftKeyFlag)
            {
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

        if (!ctrlKey && !CeilingCheck())
        {
            playereState = PlayereState.Default;
            return;
        }


        //地面感知
        if (controller.isGrounded)
        {
            //加速度を1にする
            addSpeed = 1;
            //カメラの向いている方向に歩く
            characterVelocity = transform.right * moveX + transform.forward * moveZ;

            characterVelocity = characterVelocity.normalized * walkSpeed * 0.5f;

            //重力を0に
            characterVelocityY = 0f;

            //ジャンプ
            if (Input.GetKeyDown(KeyCode.Space))
            {
                gravity = 60;
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
			if (Input.GetKeyDown(KeyCode.Space) && airJumpFlag)
			{
				characterVelocityMomentum.y = 0;
				Vector3 mag = characterVelocity;
				mag.y = 0;
				characterVelocity = characterVelocityMomentum.normalized * mag.magnitude;
				Jump(jumpForce);
				airJumpFlag = false;
			}
		}
        //重力を作成
        float gravityDownForce = gravity * -1;

        //重力を加える
        characterVelocityY += gravityDownForce * Time.deltaTime;



        // キャラクターのうごきに慣性を追加させる
        characterVelocity += characterVelocityMomentum;

        if (maxSpeed <= (Mathf.Abs(characterVelocity.x) + Mathf.Abs(characterVelocity.z)))
        {
            characterVelocity.y = 0;
            characterVelocity = characterVelocity.normalized * maxSpeed;
        }
        // 重力を適応
        characterVelocity.y = characterVelocityY;

        // 移動を適応
        controller.Move(characterVelocity * Time.deltaTime);
    }


    /// <summary>
    /// スライディングの移動処理
    /// </summary>
    private void SlidingMovemnet()
    {
        //characterVelocityMomentum = Vector3.zero;

        if (!ctrlKey)
        {
            playereState = PlayereState.Default;
            return;
        }

        if (maxSpeed <= (Mathf.Abs(characterVelocity.x) + Mathf.Abs(characterVelocity.z)))
        {
            characterVelocity.y = 0;
            characterVelocity = characterVelocity.normalized * maxSpeed;
        }

        Ray ray = new Ray(foot.transform.position, transform.up * -1);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * 0.2f);
        if (ground)
        {
            if (GroundSlope() == 0)
            {
                Vector3 nor = characterVelocity;
                nor.y = 0;
                timer += Time.deltaTime*5;
                characterVelocityMomentum = (nor.normalized*(1-timer))*Time.deltaTime*friction;
                characterVelocityY = 0;
                if (nor.magnitude<=1)
                {
                    playereState = PlayereState.Squat;
                }

            }
            //何かしらのオブジェクトにHitした場合
            else if (Physics.Raycast(ray, out hit, 0.2f, layer))
            {

                characterVelocityMomentum = hit.normal*0.5f;
                //characterVelocityMomentum.y = 0;
            }


            if (Input.GetKeyDown(KeyCode.Space))
            {
                characterVelocityY = 0;
                playereState = PlayereState.Default;
                Jump(jumpForce);
                wallRanIntervalFlag = false;
                StartCoroutine(WallRunInterval());
            }

        }
        else
        {
            playereState = PlayereState.Default;
            //characterVelocityY = 0;
        }

        //重力を作成
        float gravityDownForce = gravity * -1;

        //重力を加える
        characterVelocityY += gravityDownForce * Time.deltaTime;

        if (Physics.Raycast(ray, out hit, 0.2f, layer) && wallRanIntervalFlag)
        {
            controller.Move(hit.point - transform.position);
            ground = true;
        }


        // キャラクターのうごきに慣性を追加させる
        characterVelocity += characterVelocityMomentum;

        // 重力を適応
        characterVelocity.y = characterVelocityY;

        // 移動を適応
        controller.Move(characterVelocity * Time.deltaTime);

    }

    /// <summary>
    /// 移動方向の距離などを初期化
    /// </summary>
    private void ResetData()
    {
        characterVelocity = Vector3.zero;
        characterVelocityY = 0;
    }
}
