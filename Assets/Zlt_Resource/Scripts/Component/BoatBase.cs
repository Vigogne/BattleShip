using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatBase : MonoBehaviour
{
    /// <summary>
    /// 物品序号
    /// </summary>
    public int id;

    public virtual void Awake()
    {
        id = gameObject.GetInstanceID();
    }


}
