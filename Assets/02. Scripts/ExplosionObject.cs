using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionObject : MonoBehaviour
{
    private Transform myTr;
    private Rigidbody rigid;
    public GameObject exploEffect;
    private int hitCount = 0;

    private void Awake()
    {
        myTr = GetComponent<Transform>();
        rigid = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Bullet")
        {
            if (++hitCount >= 5)
            {
                FireObject();
            }
        }
    }

    private void OnCollision(object[] _params) 
    {
        Vector3 hitPos = (Vector3)_params[0];
        Vector3 firePos = (Vector3)_params[1];
        Vector3 collVector = (hitPos - firePos).normalized;
        rigid.AddForceAtPosition(collVector * 50f, hitPos);

        if (++hitCount >= 5)
        {
            FireObject();
        }        
    }
    void FireObject()
    {
        StartCoroutine(this.ExpObject());
    }
    IEnumerator ExpObject()
    {
        GameObject.Find("Player").GetComponent<ctrlPlayer>().barrelFire = false;
        Instantiate(exploEffect, myTr.position, Quaternion.identity);
        Collider[] colls = Physics.OverlapSphere(myTr.position, 5.0f);

        foreach (Collider coll in colls)
        {
            Rigidbody collrigid = coll.GetComponent<Rigidbody>();
            if (collrigid != null) 
            {
                if (collrigid.gameObject.tag == "Barrel")
                {
                    collrigid.mass = 1.0f;
                    collrigid.AddExplosionForce(1000.0f, myTr.position, 7.0f, 100.0f);
                }
                else if (collrigid.gameObject.tag == "Enemy")
                {
                    collrigid.gameObject.SendMessage("OnCollisionBarrel", myTr.position, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        Destroy(gameObject, 5.5f);
        yield return null;
    }
}
