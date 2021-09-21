using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PageBase : MonoBehaviour, IEventHandle
{

    

    [Tooltip("当前页面")]
    public string page;

    public GameObject content;

   

    public virtual void Awake()
    {
        EventManager.onHandle.AddListener(gameObject, MyProcess);
        UIManager.AddListener(this);
    }
    public virtual void Start()
    {

    }
    public virtual void OnDestroy()
    {
        EventManager.onHandle.RemoveListener(gameObject);
    }

    /// <summary>
    /// 进入页面
    /// </summary>
    /// <param name="上一个页面"></param>
    public virtual void Enter()
    {
        content.SetActive(true);
    }



    /// <summary>
    /// 退出页面
    /// </summary>
    /// <param name="下一个页面"></param>
    public virtual void Exit()
    {
        if(isActive)
        {
            content.SetActive(false);
        }
    }

   

    public virtual void MyProcess(MyEventType type, object val = null)
    {
        switch (type)
        {
           
        }
    }

    /// <summary>
    /// 开始延时任务
    /// </summary>
    /// <param name="time"></param>
    /// <param name="action"></param>
    public void StartTask(float time, Action action)
    {
        StartCoroutine(IE_TaskTimer(time, action));
    }

    IEnumerator IE_TaskTimer(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();
    }

    /// <summary>
    /// 设置alpha值
    /// </summary>
    /// <param name="pre"></param>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public virtual Color SetAlpha(Color pre, float alpha)
    {
        pre.a = alpha;
        return pre;
    }
    /// <summary>
    /// content是否打开
    /// </summary>
    public bool isActive => content.activeSelf;



}
