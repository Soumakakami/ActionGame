using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    //割り当て
    public Transform playerCam;
    public Transform orientation;

    //その他
    private Rigidbody rb;

    //回転と外観
    private float xRotation;
    private float sensitivity = 50f;
    private float sensMultiplier = 1f;

    //移動
    public float moveSpeed = 4500;
    public float maxSpeed = 20;
    public bool grounded;

    //地面のレイヤーを判別
    public LayerMask whatIsGround;
    
    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    //クラウチ＆スライド
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;

    //ジャンピング
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 550f;
    
    //入力
    float x, y;
    bool jumping, sprinting, crouching;
    
    //スライディング
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;

    void Awake() 
    {
        //Rigidbodyを取得
        rb = GetComponent<Rigidbody>();
    }
    
    void Start() 
    {
        //プレイヤーの初期スケールを保存
        playerScale =  transform.localScale;
        //カーソルをロックする
        Cursor.lockState = CursorLockMode.Locked;
        //マウスポインタを非表示
        Cursor.visible = false;
    }

    
    private void FixedUpdate()
    {
        //プレイヤーの動き
        Movement();
    }

    private void Update() 
    {
        //ユーザーのインプットを管理する
        MyInput();
        Look();
    }

    /// <summary>
    /// ユーザー入力を検索します。
    /// </summary>
    private void MyInput() 
    {
        //WASAD&上下左右の入力を取得
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");

        //地面に触れている間はジャンプできる
        if (grounded)
        {
            jumping = Input.GetButton("Jump");
        }

        //左コントロールキーを取得(スライディング用)
        crouching = Input.GetKey(KeyCode.LeftControl);
      
        //スライディング開始
        if (Input.GetKeyDown(KeyCode.LeftControl))
            StartCrouch();
        //スライディング終了
        if (Input.GetKeyUp(KeyCode.LeftControl))
            StopCrouch();
    }

    //スライディング開始
    private void StartCrouch() 
    {
        //プレイヤーをスレイディング時のスケールまで小さくする
        transform.localScale = crouchScale;

        //強制的に小さくなったスケール分下に移動
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);

        if (rb.velocity.magnitude > 0.5f) {
            if (grounded) {
                rb.AddForce(orientation.transform.forward * slideForce);
            }
        }
    }

    private void StopCrouch()
    {
        transform.localScale = playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }

    private void Movement() 
    {
        //余分な重力
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        //プレーヤーが見ている場所を基準にして実際の速度を見つける
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //スライドとずさんな動きを打ち消す
        CounterMovement(x, y, mag);

        //ジャンプを保持 && ジャンプの準備ができている場合は、ジャンプ
        if (readyToJump && jumping) Jump();

        //最高速度を設定
        float maxSpeed = this.maxSpeed;

        //スロープを滑り降りる場合は、フォースダウンを追加して、プレイヤーが接地したままで速度も上げる
        if (crouching && grounded && readyToJump) {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }
        
        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;
        
        // Movement in air
        if (!grounded) {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }
        
        // Movement while sliding
        if (grounded && crouching) multiplierV = 0f;

        //Apply forces to move player
        rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * multiplier);
    }

    private void Jump() 
    {
        if (grounded && readyToJump)
        {

            readyToJump = false;

            //Add jump forces
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);
            
            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0) 
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
            
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    
    private void ResetJump() 
    {
        readyToJump = true;
    }
    
    private float desiredX;
    private void Look() 
    {
        //マウスのX,Y軸方向の動きを取得
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        //現在の外観の回転を見つける
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        //回転させて上下方向の動きに制限をかける
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //計算した結果をプレイヤーに反映させる
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    //カウンタームーブ
    private void CounterMovement(float x, float y, Vector2 mag)
    {
        //地面から離れていたりジャンプ中は処理しない
        if (!grounded || jumping) return;

        //Slow down sliding
        if (crouching) {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0)) {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0)) {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }
        
        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed) {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    public Vector2 FindVelRelativeToLook() 
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);
        
        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v) 
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;
    
    /// <summary>
    /// Handle ground detection
    /// </summary>
    private void OnCollisionStay(Collision other) 
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++) {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal)) 
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded() 
    {
        grounded = false;
    }
    
}
