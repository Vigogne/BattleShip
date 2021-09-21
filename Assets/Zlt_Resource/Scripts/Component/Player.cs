using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : BoatBase
{
    /// <summary>
    /// 是否是玩家
    /// </summary>
    public bool isPlayer;
    public BulletItem bulletPrefab;
    /// <summary>
    /// 船的碰撞体
    /// </summary>
    public Collider boatCollider;
    
    /// <summary>
    /// 装备列表
    /// </summary>
    public List<Equip> equipList = new List<Equip>();
    /// <summary>
    /// 目标船只列表
    /// </summary>
    public Dictionary<float, List<BoatBase>> targetBoatDic = new Dictionary<float, List<BoatBase>>();
    [SerializeField]
    /// <summary>
    /// 装备范围最大值,也就是碰撞体大小
    /// </summary>
    private float equipRangeMax;
    [SerializeField]
    private List<Coroutine> launcherCor = new List<Coroutine>();

 

    private void Start()
    {
       
    }

    private void FixedUpdate()
    {
        GetRangeEnemy();
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 dir = new Vector3(horizontal, 0, vertical);
        if(dir!=Vector3.zero)
        transform.rotation = Quaternion.LookRotation(dir);
        transform.Translate(dir * 0.2F, Space.World);



        if (Input.GetKeyDown(KeyCode.Space))
        {
           RefreshGoods();
        }
    }


    /// <summary>
    /// 得到范围内的敌人
    /// </summary>
    private void GetRangeEnemy()
    {
        
        //只获取船这一层
        var cols = (Physics.OverlapSphere(transform.position, equipRangeMax, 1 << 8).ToList());
        //排序
       // cols.Sort((x, y) => ((x.transform.position - transform.position).magnitude >= (y.transform.position - transform.position).magnitude)?-1:1);
        var _boatList = new List<BoatBase>();

        foreach(var i in cols)
        {
            _boatList.Add(i.GetComponent<BoatBase>());
        }
        //更新范围内的敌人列表
        foreach(var i in targetBoatDic)
        {
            i.Value.Clear();
        }

        foreach(var i in targetBoatDic)
        {
            foreach(var j in _boatList)
            {
                var _magnitude = (j.transform.position - transform.position).sqrMagnitude;
               
                if (_magnitude<i.Key*i.Key)
                {
                    Debug.Log("_key" + i.Key + "_magnitude" + _magnitude);
                    //在范围内就加进列表
                    targetBoatDic[i.Key].Add(j);
                   
                }
            }
        }



      
    }





    /// <summary>
    /// 刷新物品栏
    /// </summary>
    public void RefreshGoods()
    {


        //获得范围最大值
       equipRangeMax = 0.0f;
        targetBoatDic.Clear();

        foreach(var i in equipList)
        {
            if (i.range > equipRangeMax)
            {
                equipRangeMax = i.range;
            }
            if (!targetBoatDic.ContainsKey(i.range))
            {
                //创建范围列表
                targetBoatDic.Add(i.range, new List<BoatBase>());
            }
           
        }

        Debug.Log(targetBoatDic);

        StartLauncher();

    }


    /// <summary>
    /// 开始发射
    /// </summary>
    private void StartLauncher()
    {

        foreach(var i in launcherCor)
        {
            StopCoroutine(i);
        }
        launcherCor.Clear();

        foreach(var i in equipList)
        {
            if(i.goodType== GoodType.武器)
            {
                var _cor = StartCoroutine(IE_Launcher(i));
             launcherCor.Add(_cor);
            }
        }
      
    }

    IEnumerator IE_Launcher(Equip _equip)
    {
        while(true)
        {
            //获得范围内的船只列表
            var _targetList = targetBoatDic[_equip.range];
            //列表内没有就跳过到下一帧
            if (_targetList.Count == 0)
            {
                Debug.Log("跳过");
                yield return (0);
                continue;
            }
               
            //在列表内随机一个目标
           var _randomIndex = Random.Range(0, _targetList.Count);

            Launch(_equip, _targetList[_randomIndex]);
            //cd上加一个轻微的扰动,防止开火太整齐
            yield return new WaitForSeconds(_equip.cd+Random.Range(0,0.05f));
           
        }
    }

    /// <summary>
    /// 发射子弹
    /// </summary>
    /// <param name="_boat"></param>
    private void Launch(Equip _equip, BoatBase _boat)
    {
        if (_boat!=null)
        {
            var bullet = Instantiate(bulletPrefab,transform.position,Quaternion.identity);
            bullet.Init( _boat, _equip);
        }

    }

}
