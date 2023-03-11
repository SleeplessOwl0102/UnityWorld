using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class LookSceneViewCamera : MonoBehaviour
{
    void Update()
    {
        transform.LookAt(SceneView.lastActiveSceneView.camera.transform);
    }
}
