using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectGizmo : MonoBehaviour
{
#if CBT_MODE
    public Color myColor = Color.red;
    public float explosionMyRadius = 7f;

    private void OnDrawGizmosSelected()
    {
        Vector3 pos = transform.position;
        Gizmos.color = myColor;
        Gizmos.DrawSphere(pos, explosionMyRadius);
    }
#endif
}
