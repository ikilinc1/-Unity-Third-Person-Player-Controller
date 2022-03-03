using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    void OnCollisionEnter(Collision col)
    {
        Destroy(this.gameObject, 2);
    }

}
