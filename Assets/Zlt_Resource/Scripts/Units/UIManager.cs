using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour//,IEventHandle
{
    public static Dictionary<string, PageBase> pageDic = new Dictionary<string, PageBase>();

    /// <summary>
    /// 注册
    /// </summary>
   public static void AddListener(PageBase _page)
    {
        Debug.Log("注册");
        pageDic.Add(_page.page, _page);
    }

    public static void SwitchUI(string _page)
    {
        Debug.Log("switchUI");
        CloseAllUI();
        AddUI(_page);
    }

    public static void CloseUI(string _page)
    {
        pageDic[_page].Exit();
    }

    public static void AddUI(string _page)
    {
        pageDic[_page].Enter();
    }

    /// <summary>
    /// 关闭所有ui
    /// </summary>
    public static void CloseAllUI()
    {
        foreach(var i in pageDic)
        {
            i.Value.Exit();
        }
    }


}
