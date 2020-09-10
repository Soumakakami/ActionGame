using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    //���蓖��
    public Transform playerCam;
    public Transform orientation;

    //���̑�
    private Rigidbody rb;

    //��]�ƊO��
    private float xRotation;
    private float sensitivity = 50f;
    private float sensMultiplier = 1f;

    //�ړ�
    public float moveSpeed = 4500;
    public float maxSpeed = 20;
    public bool grounded;

    //�n�ʂ̃��C���[�𔻕�
    public LayerMask whatIsGround;
    
    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    //�N���E�`���X���C�h
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;

    //�W�����s���O
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 550f;
    
    //����
    float x, y;
    bool jumping, sprinting, crouching;
    
    //�X���C�f�B���O
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;

    void Awake() 
    {
        //Rigidbody���擾
        rb = GetComponent<Rigidbody>();
    }
    
    void Start() 
    {
        //�v���C���[�̏����X�P�[����ۑ�
        playerScale =  transform.localScale;
        //�J�[�\�������b�N����
        Cursor.lockState = CursorLockMode.Locked;
        //�}�E�X�|�C���^���\��
        Cursor.visible = false;
    }

    
    private void FixedUpdate()
    {
        //�v���C���[�̓���
        Movement();
    }

    private void Update() 
    {
        //���[�U�[�̃C���v�b�g���Ǘ�����
        MyInput();
        Look();
    }

    /// <summary>
    /// ���[�U�[���͂��������܂��B
    /// </summary>
    private void MyInput() 
    {
        //WASAD&�㉺���E�̓��͂��擾
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");

        //�n�ʂɐG��Ă���Ԃ̓W�����v�ł���
        if (grounded)
        {
            jumping = Input.GetButton("Jump");
        }

        //���R���g���[���L�[���擾(�X���C�f�B���O�p)
        crouching = Input.GetKey(KeyCode.LeftControl);
      
        //�X���C�f�B���O�J�n
        if (Input.GetKeyDown(KeyCode.LeftControl))
            StartCrouch();
        //�X���C�f�B���O�I��
        if (Input.GetKeyUp(KeyCode.LeftControl))
            StopCrouch();
    }

    //�X���C�f�B���O�J�n
    private void StartCrouch() 
    {
        //�v���C���[���X���C�f�B���O���̃X�P�[���܂ŏ���������
        transform.localScale = crouchScale;

        //�����I�ɏ������Ȃ����X�P�[�������Ɉړ�
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
        //�]���ȏd��
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        //�v���[���[�����Ă���ꏊ����ɂ��Ď��ۂ̑��x��������
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //�X���C�h�Ƃ�����ȓ�����ł�����
        CounterMovement(x, y, mag);

        //�W�����v��ێ� && �W�����v�̏������ł��Ă���ꍇ�́A�W�����v
        if (readyToJump && jumping) Jump();

        //�ō����x��ݒ�
        float maxSpeed = this.maxSpeed;

        //�X���[�v������~���ꍇ�́A�t�H�[�X�_�E����ǉ����āA�v���C���[���ڒn�����܂܂ő��x���グ��
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
        //�}�E�X��X,Y�������̓������擾
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        //���݂̊O�ς̉�]��������
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        //��]�����ď㉺�����̓����ɐ�����������
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //�v�Z�������ʂ��v���C���[�ɔ��f������
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    //�J�E���^�[���[�u
    private void CounterMovement(float x, float y, Vector2 mag)
    {
        //�n�ʂ��痣��Ă�����W�����v���͏������Ȃ�
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
