using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ctrlBullet : MonoBehaviour
{
    public float speed = 50f;
    public float range = 400f;

    public int power = 10;

    public GameObject ExploPtcl;
    private Rigidbody rigid;

    private float dist;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }
    void Update()
    {
        //rigid.AddForce(transform.forward * speed);            // 로컬 좌표 z축으로 움직임
        //rigid.AddRelativeForce(transform.forward * speed);    // 이상하게 움직임
        rigid.AddRelativeForce(Vector3.forward * speed);        // 로컬 좌표 z축으로 움직임

        // addforce 사용해도 되지만 매우 가벼움 but 정확한 값을 모르면 통과 가능성있음
        // transform.Translate(Vector3.forward * Time.deltaTime * speed); 


        dist += Time.deltaTime * speed;

        if (dist >= range)
        {
            Instantiate(ExploPtcl, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(ExploPtcl, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
