using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField]
    float power;
    public GameObject player;

    private void Update()
    {
        transform.LookAt(player.transform.position,transform.up);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            Debug.Log("当たったよ");
            other.gameObject.GetComponent<PlayerController>().JumpPad(power);
        }
    }
}
