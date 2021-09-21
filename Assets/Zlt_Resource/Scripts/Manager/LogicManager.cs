using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LogicManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

/// <summary>
/// 物品
/// </summary>
[Serializable]
public class GoodsBase
{
    /// <summary>
    /// 装备名
    /// </summary>
    public string name;
    /// <summary>
    /// 物品类型
    /// </summary>
    public GoodType goodType;
    /// <summary>
    /// 图标名
    /// </summary>
    public string iconName;
}

/// <summary>
/// 装备
/// </summary>

[Serializable]
public class Equip:GoodsBase
{
    
    /// <summary>
    /// 冷却时间
    /// </summary>
    public float cd;
    /// <summary>
    /// 攻击范围
    /// </summary>
    public float range;

    /// <summary>
    /// 攻击力
    /// </summary>
    public float damage;
    /// <summary>
    /// 防御力,百分比
    /// </summary>
    public float defense;
    /// <summary>
    /// 子弹速度
    /// </summary>
    public float bulletSpeed;
    /// <summary>
    /// 船速
    /// </summary>
    public float BoatSpeed;
    
}

public enum GoodType
{
    None,
    武器,
    装备,
    消耗品
}