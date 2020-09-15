using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lock : MonoBehaviour
{
    public Vector3 direction;
    public Transform tar;
    public Vector3 tarPos;
    void Update()
    {
        //tarPos = tar.position;
        Ray ray = new Ray(transform.position, transform.right);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * 2);
        if (Physics.Raycast(ray, out hit, 2))
        {
            direction = hit.normal;
            transform.right = direction * -1;
            transform.position = hit.point;
            //transform.right = direction;
        }

        //if (Input.GetKey(KeyCode.W))
        //{
        //    transform.position += transform.forward * Time.deltaTime * 10;
        //}
        //if (Input.GetKey(KeyCode.S))
        //{
        //    transform.position -= transform.forward * Time.deltaTime * 10;
        //}
    }
}
