using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateGizmo : MonoBehaviour
{
#if CBT_MODE
    public Color myColor = Color.red;
    public float myRadius = 0.05f;

    private void OnDrawGizmos()
    {
        Gizmos.color = myColor;
        Gizmos.DrawSphere(transform.position, myRadius);
    }
#endif
}
