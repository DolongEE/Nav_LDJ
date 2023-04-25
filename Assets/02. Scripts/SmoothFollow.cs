using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SmoothFollow : MonoBehaviour
{

    public Transform target;
    public float distance = 10.0f;
    public float height = 5.0f;
    public float heightDamping = 2.0f;
    public float rotationDamping = 3.0f;

    void LateUpdate()
    {
        float wantedRotationgAngle = target.eulerAngles.y;
        float wantedHeight = target.position.y + height;
        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationgAngle, rotationDamping * Time.deltaTime);
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        Vector3 tempDis = target.position;

        tempDis -= currentRotation * Vector3.forward * distance;

        tempDis.y = currentHeight;
        transform.position = tempDis;
        transform.LookAt(target);
    }
}