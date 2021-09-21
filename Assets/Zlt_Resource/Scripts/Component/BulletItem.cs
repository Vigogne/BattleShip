using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletItem : MonoBehaviour
{

    public MeshRenderer meshRenderer;
    public BoatBase targetBoat;
    public Material red;
    public Material blue;
    public float speed;

    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, targetBoat.transform.position,0.1f*speed);
        if(!targetBoat.gameObject.activeSelf)
        {
            Destory();
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
   public void Init(BoatBase _target,Equip _equip)
    {
        targetBoat = _target;
        speed = _equip.bulletSpeed;
        name = "Bullet_" + _equip.name;
        switch(_equip.name)
        {
            case "red":
                {
                    meshRenderer.material = red;
                }
                break;
            case "blue":
                {
                    meshRenderer.material = blue;
                }
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetInstanceID() == targetBoat.id)
        {
            Destory();
        }

    }

    /// <summary>
    /// 销毁
    /// </summary>
    public void Destory()
    {
        GameObject.Destroy(gameObject);
        
    }
}
