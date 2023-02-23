using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public Transform targetGO;
    public float angelPerSec;

    void Start()
    {
        if (targetGO == null)
            targetGO = GetComponent<Transform>();
    }

    
    void Update()
    {
        targetGO.Rotate(Vector3.up, angelPerSec * Time.deltaTime ,Space.World);
    }
}
