
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    enum PlayerState
    {
		Ground=0,
		Air=1,
		Sliding
	}
	[Header("プレイヤー基礎数値")]
	[SerializeField]
	float speed=0;
	[SerializeField]
	float gravity=9.8f;

	CharacterController controller;

	Vector3 moveDirection;

	private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float h = Input.GetAxis("Horizontal");    //左右矢印キーの値(-1.0~1.0)
        float v = Input.GetAxis("Vertical");      //上下矢印キーの値(-1.0~1.0)
		if (controller.isGrounded)
		{
			moveDirection = new Vector3(h, 0, v);
			moveDirection = transform.TransformDirection(moveDirection);
			moveDirection *= 10;
		}
		moveDirection.y -= gravity;
        if (Input.GetKeyDown(KeyCode.Space))
        {
			moveDirection.y = 10;
        }
		controller.Move((moveDirection) * Time.deltaTime);
	}
}
