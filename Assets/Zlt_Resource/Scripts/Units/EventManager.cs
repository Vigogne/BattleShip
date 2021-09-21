using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Linq;



public class EventManager 
{
    public static MyEvent<MyEventType, object> onHandle =new MyEventHandleClass();
    
    /// <summary>
    /// 执行事件
    /// </summary>
    public static void InvokeHandle(MyEventType type,object val = null)
    {
        onHandle?.Invoke(type, val);
    }

    /// <summary>
    /// 移除所有注册的action
    /// </summary>
    public static void RemoveAll()
    {

    }
}

public interface IEventHandle
{
    void MyProcess(MyEventType type, object val = null);

}
public class MyEventHandleClass:MyEvent<MyEventType,object>
{

}

/// <summary>
/// 等级
/// </summary>
public enum Level
{
    None,



}
public enum MyEventType
{
    /// <summary>
    /// 空事件，什么都不做
    /// </summary>
    None,
    /// <summary>
    /// 加载完成
    /// </summary>
  LoadDown,

}
public class MyEvent<T0, T1>
{
    private Dictionary<object, Action<T0,T1>> eventDic;

    public MyEvent()
        {
        eventDic = new Dictionary<object, Action<T0, T1>>();
        }
    /// <summary>
    /// 注册
    /// </summary>
    public void AddListener(object listener, Action<T0,T1> call)
    {
        eventDic.Add(listener, call);
    }

    public void Invoke(T0 t0,T1 t1)
    {
     
        for(int i=0;i<eventDic.Count;i++)
        {
            eventDic.ElementAt(i).Value.Invoke(t0, t1);
            //try
            //{
            //}
            //catch
            //{
            //    Debug.LogError(string.Format("{0}执行 {1}时报错 参数{2}", eventDic.ElementAt(i).Key, t0, t1));
            //}
        }
        
    }

    /// <summary>
    /// 移除观察者
    /// </summary>
    public void RemoveListener(object _key)
    {
        try
        {
            eventDic.Remove(_key);
        }
        catch
        {
#if UNITY_EDITOR
            Debug.LogError(string.Format("不存在 {0}", _key));
#endif
        }
        
    }


    /// <summary>
    /// 移除所有
    /// </summary>
    public void RemoveAllListener()
    {
        eventDic.Clear();
    }

}
