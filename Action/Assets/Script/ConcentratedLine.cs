using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConcentratedLine : MonoBehaviour
{
    public GameObject player;

    private void Update()
    {
        transform.LookAt(player.transform.position, transform.up);
    }
}
