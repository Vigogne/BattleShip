using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class CameraManager : MonoBehaviour
{
    [Header("把相机拖到列表里")]
    [Tooltip("相机列表")]
    public List<CinemachineVirtualCamera> cameraList = new List<CinemachineVirtualCamera>();
    public CinemachineBrain brain;
    public CinemachineVirtualCamera _cut;
    public CinemachineVirtualCamera _animation;



    private Dictionary<string, CinemachineVirtualCamera> cameraDic = new Dictionary<string, CinemachineVirtualCamera>();



    private void Awake()
    {
        foreach (var i in cameraList)
        {
            
                cameraDic.Add(i.name, i);
            
        }

       
    }

    /// <summary>
    /// 选择相机
    /// </summary>
    public void SelectCamera(string cameraName)
    {
        SelectCamera(cameraDic[cameraName]);
        
    }

    /// <summary>
    /// 选择相机
    /// </summary>
    public void SelectCamera(CinemachineVirtualCamera virtualCamera)
    {
        virtualCamera.MoveToTopOfPrioritySubqueue();
        var freeCamera = virtualCamera.GetComponent<FreeCamera>();

        if (freeCamera != null)
        {
            //自由相机初始化
            freeCamera.Init();
        }

    }
    /// <summary>
    /// 播放入场动画
    /// </summary>
    /// <param name="target"></param>
    /// <param name="distance"></param>
    public void PlayAnimationFrom(CinemachineVirtualCamera target, float distance)
    {
        Debug.LogWarning("PlayAnimationFrom:" + target?.name);
        var _target = target.transform;
        _cut.ForceCameraPosition(_target.position - _target.forward * distance, _target.rotation);
        _cut.MoveToTopOfPrioritySubqueue();
        brain.ManualUpdate();
        SelectCamera(target);
      
    }

    /// <summary>
    /// 播放出场动画
    /// </summary>
    /// <param name="target"></param>
    /// <param name="distance"></param>
    public void PlayAnimationTo(CinemachineVirtualCamera target, float distance)
    {
        Debug.LogWarning("PlayAnimationTo:" + target?.name);
        var _target = target.transform;
        _animation.ForceCameraPosition(_target.position + _target.forward * distance, _target.rotation);
        brain.ManualUpdate();
        _animation.MoveToTopOfPrioritySubqueue();
      
    }
   

}

public struct MyCameraData
{
    public MyCameraData(string name, float dis)
    {
        cameraName = name;
        distance = dis;
    }


    public string cameraName;
    public float distance;
}
