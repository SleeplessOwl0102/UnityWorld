using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Smear : MonoBehaviour
{
    //创建坐标队列
    Queue<Vector3> _recentPositions = new Queue<Vector3>();
    //用于推迟之前坐标的出列
    [SerializeField]
    int _frameLag = 0;

    public Material smearMat = null;

    void LateUpdate()
    {
        if (smearMat == null)
            return;

        //使得当前坐标的入列与之前坐标的出列相差_frameLag个元素，即shader中的worldOffset为_frameLag个帧的位移，速度拖尾会因此变长
        if (_recentPositions.Count > _frameLag)
        {
            for (int i = _recentPositions.Count - _frameLag; i > 0; i--)
            {
                smearMat.SetVector("_PrevPosition", _recentPositions.Dequeue());
            }
        }
        smearMat.SetVector("_Position", transform.position);
        _recentPositions.Enqueue(transform.position);
    }
}