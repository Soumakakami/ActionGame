using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpPoint : MonoBehaviour
{
    [SerializeField]
    Transform targetPos;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            other.GetComponent<PlayerController>().Warp(targetPos);
        }
    }
}
