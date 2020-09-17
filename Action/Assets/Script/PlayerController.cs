
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    enum PlayereState
    {
		Default=0,
		WallRun=1,
		Sliding=2
    }

    enum WallRunState
	{
		Right=0,
		Left=1
	}


	[SerializeField]
	PlayereState playereState = PlayereState.Default;
	[SerializeField]
	WallRunState wallRunState = WallRunState.Right; 

	//[SerializeField]


	[Header("プレイヤー基礎数値")]
	[SerializeField]
	float speed=10;
	[SerializeField]
	float airSpeed=10;
	[SerializeField]
	float gravity=9.8f;
	[SerializeField]
	float jumpForce = 10;

	[Header("制限")]
	[SerializeField]
	float slideAngle=0;
	[SerializeField]
	LayerMask layer;

	[Header("アタッチするオブジェクト")]
	[SerializeField]
	GameObject foot;
	[SerializeField]
	GameObject head;
	[SerializeField]
	CameraController cam;

	[Header("値をいじらないでください")]
	[SerializeField]
	bool ground=false;
	[SerializeField]
	float addSpeed=1;
	[SerializeField]
	float wallCheckDistance=1;

	//キャラクターコンローラー
	CharacterController controller;
	public float footDistance=0;
	Vector3 moveDirection;

	float characterVelocityY;
	Vector3 characterVelocityMomentum;
	float moveX;
	float moveZ;
	Vector3 characterVelocity;
	Vector3 playerAngle;
	
	private void Start()
    {
        controller = GetComponent<CharacterController>();
		footDistance = Vector3.Distance(foot.transform.position,transform.position);
		cam = FindObjectOfType<CameraController>();
	}

	private void Update()
    {
        ground = controller.isGrounded;

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
		Ray ray = new Ray(transform.position, transform.up * -1);
		RaycastHit hit;
		Debug.DrawRay(ray.origin, ray.direction * 3);
		float angle=0;
		if (Physics.Raycast(ray, out hit, 3,layer))
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
		Ray ray = new Ray(head.transform.position,transform.up);
		RaycastHit hit;
		Debug.DrawRay(ray.origin,ray.direction*0.2f);

        if (Physics.Raycast(ray,out hit,0.2f,layer))
        {
			Debug.Log("天井に頭をぶつけました");
			moveDirection.y = -1;
        }
    }

	/// <summary>
	/// 移動の処理
	/// </summary>
	private void Movemnet()
    {
		playerAngle = cam.transform.eulerAngles;
		playerAngle.x = 0;

		transform.eulerAngles = playerAngle;
		moveX = Input.GetAxisRaw("Horizontal");
		moveZ = Input.GetAxisRaw("Vertical");
		
		if (controller.isGrounded)
		{
			addSpeed = 1;
			characterVelocity = transform.right * moveX * speed + transform.forward * moveZ * speed;
			characterVelocityY = 0f;
			
			//ジャンプ
			if (Input.GetKeyDown(KeyCode.Space))
			{
				Jump(jumpForce);
			}
			characterVelocityMomentum = Vector3.zero;
		}
		
		if (!controller.isGrounded)
        {
			WallCheck();
			characterVelocityMomentum = ((transform.right * moveX  + transform.forward * moveZ)* airSpeed * Time.deltaTime)*addSpeed;
		}
		//重力を追加
		float gravityDownForce = gravity * -1;
		characterVelocityY += gravityDownForce * Time.deltaTime;


		// 重力を適応
		characterVelocity.y = characterVelocityY;

		// Apply momentum
		characterVelocity += characterVelocityMomentum;

		// Move Character Controller
		controller.Move(characterVelocity * Time.deltaTime);

		// Dampen momentum
		if (characterVelocityMomentum.magnitude > 0f)
		{
			float momentumDrag = 3f;
			characterVelocityMomentum -= characterVelocityMomentum * momentumDrag * Time.deltaTime;
			if (characterVelocityMomentum.magnitude < .0f)
			{
				characterVelocityMomentum = Vector3.zero;
			}
		}
	}

	bool test = true;
	/// <summary>
	/// 壁走り
	/// </summary>
	private void WallRunMovemnet()
    {

		Vector3 direction;
		int dir;
		cam.GetComponent<CameraController>().cameraMode = CameraController.CameraMode.Follow;
		switch (wallRunState)
        {
			default:
			case WallRunState.Right:
				direction = transform.right;
				dir = -1;

				break;
            case WallRunState.Left:
				direction = transform.right*-1;
				dir = 1;
				break;
        }
        if (test)
		{

			addSpeed = 1.2f;
			Ray ray = new Ray(transform.position, direction);
			RaycastHit hit;
			Debug.DrawRay(ray.origin, ray.direction * wallCheckDistance);
			if (Physics.Raycast(ray, out hit, wallCheckDistance))
			{
				transform.right = hit.normal* dir;
				characterVelocityMomentum = Vector3.zero;
				characterVelocity = transform.forward;
				characterVelocityY = 0;

				characterVelocity *= speed*addSpeed;
				controller.Move(characterVelocity * Time.deltaTime);

				if (Input.GetKeyDown(KeyCode.Space))
				{
					Debug.Log(hit.normal);
					AddForce(hit.normal * 10);
					Jump(jumpForce);
					test = false;
					playereState = PlayereState.Default;
					StartCoroutine(TTTT());
				}
			}
			else
			{
				playereState = PlayereState.Default;
			}
		}
	}

	private void WallCheck()
    {
        if (test)
        {

		Ray rightRay = new Ray(transform.position,transform.right);
		RaycastHit rightHit;

		Debug.DrawRay(rightRay.origin, rightRay.direction* wallCheckDistance);

        if (Physics.Raycast(rightRay, out rightHit, wallCheckDistance, layer))
        {
			playereState = PlayereState.WallRun;
			wallRunState = WallRunState.Right;
        }

		Ray leftRay = new Ray(transform.position, transform.right*-1);
		RaycastHit leftHit;

		Debug.DrawRay(leftRay.origin, leftRay.direction * wallCheckDistance);

		if (Physics.Raycast(leftRay, out leftHit, wallCheckDistance, layer))
		{
			playereState = PlayereState.WallRun;
			wallRunState = WallRunState.Left;
		}

		}
	}
	
	private IEnumerator TTTT()
    {
		yield return new WaitForSeconds(0.2f);
		test = true;
    }
}
