using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructionRay : MonoBehaviour
{
    public GameObject fireEffect;

    Ray ray;
    RaycastHit hitInfo;


    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
#if CBT_MODE
        Debug.DrawRay(ray.origin, ray.direction * 150.0f, Color.green);
#elif RELEASE_MODE
        Debug.DrawRay(ray.origin, ray.direction * 100.0f, Color.red);
#endif

        if(Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hitInfo, 150.0f))
            {
                if (hitInfo.collider.tag == "DestroyObject")
                {
                    Instantiate(fireEffect, hitInfo.point, Quaternion.identity);
                    Debug.Log(123);
                    Destroy(hitInfo.collider.gameObject);
                }
                else if (hitInfo.collider.tag == "Barrel")
                {
                    GameObject.Find("Player").GetComponent<ctrlPlayer>().BarrelFire(hitInfo.collider.gameObject.transform);
                }
            }
        }

#if UNITY_ANDROID
        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
            if (Physics.Raycast(ray, out hitInfo, 150.0f))
            {
                if (hitInfo.collider.tag == "DestroyObject")
                {
                    //Instantiate(fireEffect, hitInfo.point, Quaternion.identity);
                    Destroy(hitInfo.collider.gameObject);
                }
            }
        }
#endif
    }
}
