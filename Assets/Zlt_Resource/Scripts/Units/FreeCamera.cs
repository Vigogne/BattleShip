using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;
/*
 2021.0629更新:添加相机类型选项,分为自由相机,旋转相机,自动旋转相机
 2021.0714更新:增加Y轴中键限位,滚轮中键限位
    修正目标旋转中心为空时报错
    增加键盘移动    
2021.0715更新:俯仰角可控制
    修正在旋转中心为空时,初始化相机旋转角错误
2021.0727更新:
    鼠标松开后不是自动旋转,而是加个延时
    增加相机退出状态,退出后就停止updata,不再能控制
2021.0729更新:
    优化了高度限位,将限位放在相机上,而不是中心点上
    添加了可设定设定旋转相机鼠标左右键可切换
2021.0802更新:
    添加了双击碰撞体,相机移动过去,同时distance减半
2021.0823更新
    修复了x轴负角度出现自动复位的bug
2021.0901更新
    可以选择是否开启射线移动
    增加LockTargetMove()方法,可以移动到目标点
2021.0907更新
    新增方法,外部强制修改相机位置ForceSetPos()
    将初始化分为private和Init() Init(),并增加bool变量CanInit控制是否可以外部初始化
2021.0915更新
    新增CameraPosLimit结构体和字段posLimit,设定两个vector3点限制位移,以及滚轮滚动
 */
//todo:自由相机
public class FreeCamera : MonoBehaviour
{
    public Transform testCubue;
    [Tooltip("相机模式")]
    public FreeCameraType freeCameraType;
    [Tooltip("决定鼠标左右键旋转")]
    public MouseButton mouseRotateButton;
    public ButtonState buttonState;
    [Tooltip("是否可以双击移动相机")]
    public bool canDoubleClickRayMove = false;
    [Tooltip("是否可以外部初始化")]
    /// <summary>
    ///是否可以外部初始化
    /// </summary>
    public bool canInit = true;
    [Tooltip("这里是虚拟相机")]
    public CinemachineVirtualCamera m_camera;
    [Tooltip("目标旋转中心")]
    public Transform lookAroundTrans;
    [Tooltip("相机旋转轴偏移")]
    public Vector3 offestRotate;
    [Tooltip("旋转灵敏度")]
    public float rotateSenstivity = 1;
    [Tooltip("自动旋转速度,正值是顺时针,负值是逆时针")]
    public float autoRotateSpeed = 10;
    public float autoRotateInternal = 30;
    /// <summary>
    /// 相机到目标的距离
    /// </summary>
    public float distance = 5;
    [Tooltip("滚轮速度")]
    public float zoomSpeed =5;
    [Tooltip("最小距离")]
    public float minDistance = 0;
    [Tooltip("最大距离")]
    public float maxDistance = 10;
    //[Tooltip("Y方向最小值")]
    //public float MinY = 0;
    [Tooltip("键盘移动速度")]
    public float keyBoardMoveSpeed = 10;
    /// <summary>
    /// 俯角
    /// </summary>
    [Tooltip("俯角")]
    public float depression = 5;
    /// <summary>
    /// 仰角
    /// </summary>
    [Tooltip("仰角")]
    public float eveation = 80;
    [Tooltip("相机位置限制")]
    public CameraPosLimit posLimit = new CameraPosLimit(new Vector3(-999,0,-999),new Vector3(999,999,999));





    /// <summary>
    /// 是否自动旋转
    /// </summary>
    private bool isAutoRotate;
    /// <summary>
    /// 旋转中心的位置
    /// </summary>
    private Vector3 lookAroundPos;
    private float angle_x;
    private float angle_y;
    private float angle_z;
    /// <summary>
    /// 目标距离
    /// </summary>
    private float targetDistance;
    /// <summary>
    /// 初始角度
    /// </summary>
    private Vector3 rootAngle;
    /// <summary>
    /// 初始默认距离
    /// </summary>
    private float rootDistance;
    /// <summary>
    /// 初始位置
    /// </summary>
    private Vector3 rootpos;
    /// <summary>
    /// 初始角度
    /// </summary>
    private Quaternion rootQuaternion;
    /// <summary>
    /// 上一个鼠标位置
    /// </summary>
    private Vector3 lastMousePos;
    /// <summary>
    /// 自动旋转的协程
    /// </summary>
    private Coroutine autoRotateCor;
    /// <summary>
    /// 自动移动的协程
    /// </summary>
    private Coroutine moveCor;
    /// <summary>
    /// 可以控制
    /// </summary>
    private bool canControl;
    /// <summary>
    /// 是否在自动移动
    /// </summary>
    private bool isCameraMove;
    /// <summary>
    /// 第一次点击
    /// </summary>
    private DateTime firstClick;
    /// <summary>
    /// 第二次点击
    /// </summary>
    private DateTime secondClick;
    /// <summary>
    /// 射线点击处
    /// </summary>
   private Vector3 targetPoint;
    

    private void Awake()
    {
        //设定初始默认距离
        rootDistance = distance;
        rootpos = m_camera.transform.position;
        rootQuaternion = m_camera.transform.rotation;
    }
    private void Start()
    {
        PrivateInit();
    }

    private void FixedUpdate()
    {
        
        if (isCameraMove)
        {
            //差值移动放到fixedupdate里,不然会出现移动不到位的情况
            LockMove();
           
        }
    }
    private void Update()
    {
        if (testCubue != null)
        {
            testCubue.position = lookAroundPos;
        }
        //相机如果退出就跳过
        if (!canControl)
            return;

        //如果在进行射线移动,禁止其他操作
        if(isCameraMove)
        {
            return;
        }
        

        UpdateInput();
        LookAround();
        Zoom();
        if (freeCameraType == FreeCameraType.自动旋转相机)
        {
            AutoRotate();
        }
        else if (freeCameraType == FreeCameraType.自由相机)
        {
            Drag();
            KeyBoardMove();
        }else if(freeCameraType == FreeCameraType.旋转自由相机)
        {
            AutoRotate();
            Drag();
            KeyBoardMove();
        }
        //双击飞入,同时判定是否可双击级移动
        if(buttonState.leftButtonDown&&canDoubleClickRayMove)
        {
            //双击判定必须放在update里,放fixeupdate里会有问题
            OnDoubleClick();
        }


        //重置中心点位置
        ResetlookAroundPos();
        SetCameraPos();
    }


    private void UpdateInput()
    {
        buttonState.leftButtonDown = Input.GetMouseButtonDown(0);
        buttonState.leftButton = Input.GetMouseButton(0);
        buttonState.leftButtonUp = Input.GetMouseButtonUp(0);

        buttonState.rightButtonDown = Input.GetMouseButtonDown(1);
        buttonState.rightButton = Input.GetMouseButton(1);
        buttonState.rightButtonUp = Input.GetMouseButtonUp(1);

        buttonState.middlebuttonDown = Input.GetMouseButtonDown(2);
        buttonState.middlebutton = Input.GetMouseButton(2);
        buttonState.middleButtonUp = Input.GetMouseButtonUp(2);

        buttonState.horizontal = Input.GetAxis("Horizontal");
        buttonState.vertical = Input.GetAxis("Vertical");
    }


    /// <summary>
    /// 本地初始化
    /// </summary>
    private void PrivateInit()
    {
        canControl = true;
        m_camera.transform.position = rootpos;
        distance = rootDistance;
        targetDistance = distance;
        StartAutoRotateCountdown();
        ResetPos();
    }
    /// <summary>
    /// 外部初始化
    /// </summary>
    public void Init()
    {
        //不能外部初始化就return
        if(!canInit)
            return;
        
        canControl = true;
        m_camera.transform.position = rootpos;
        distance = rootDistance;
        targetDistance = distance;
        StartAutoRotateCountdown();
        ResetPos();
    }


    /// <summary>
    /// 相机退出,不再控制
    /// </summary>
    public void Exit()
    {
        //停止控制,停止旋转计时协程
        canControl = false;
        if (autoRotateCor != null)
        {
            StopCoroutine(autoRotateCor);
        }
    }



    /// <summary>
    /// 回到初始位置
    /// </summary>
    private void ResetPos()
    {
        
        if(lookAroundTrans!=null)
        {
            lookAroundPos = lookAroundTrans.position;
            var forward = Vector3.Normalize(lookAroundPos - m_camera.transform.position);
            var q = Quaternion.LookRotation(forward);
            angle_x = q.eulerAngles.x;
            angle_y = q.eulerAngles.y;
            if (angle_x > 180)
            {
                angle_x = angle_x - 360;
            }

            angle_x = Mathf.Clamp(angle_x, depression, eveation);
            angle_y = Mathf.Clamp(angle_y, -365, 365);
            SetCameraPos();
        }
        else
        {
            //如果不存在跟踪目标,就对准相机前方
            ResetlookAroundPos();

            angle_x = rootQuaternion.eulerAngles.x;
            angle_y = rootQuaternion.eulerAngles.y;
            if (angle_x > 180)
            {
                angle_x = angle_x - 360;
            }
            angle_x = Mathf.Clamp(angle_x, depression, eveation);
            angle_y = Mathf.Clamp(angle_y, -365, 365);

            m_camera.transform.rotation = Quaternion.Euler(angle_x, angle_y, angle_z);
        }
       
       
      
    }

    /// <summary>
    /// 重置中心点位置
    /// </summary>
    private void ResetlookAroundPos()
    {
        
       
        var tmpPos = m_camera.transform.position + m_camera.transform.rotation * (Vector3.forward * distance);
        lookAroundPos = tmpPos;
        
    }

    /// <summary>
    /// 相机位置变化应用
    /// </summary>
    public void SetCameraPos()
    {
        //距离差值
        distance = Mathf.Lerp(distance, targetDistance, Time.deltaTime * 10);

        m_camera.transform.rotation = Quaternion.Euler(angle_x, angle_y, angle_z);
        var tmpPos = lookAroundPos + m_camera.transform.rotation * (-distance * Vector3.forward);
        //if (tmpPos.y <= MinY+1)
        //{
        //    tmpPos.y = MinY+1;
        //}

        tmpPos = posLimit.Clamp(tmpPos);

        m_camera.transform.position = tmpPos;
        m_camera.transform.rotation = Quaternion.Euler(angle_x, angle_y + offestRotate.y, angle_z);
    }


    /// <summary>
    /// 强制设定旋转角度
    /// </summary>
    /// <param name="_angel"></param>
    public void ForceSetPos(Vector3 _pos, Vector3 _angel)
    {
        angle_x = _angel.x;
        angle_y = _angel.y;
        angle_z = _angel.z;

        if (angle_x > 180)
        {
            angle_x = angle_x - 360;
        }
        angle_x = Mathf.Clamp(angle_x, depression, eveation);
        angle_y = Mathf.Clamp(angle_y, -365, 365);
        m_camera.transform.rotation = Quaternion.Euler(angle_x, angle_y, angle_z);
        m_camera.transform.position = _pos;
    }
    /// <summary>
    /// 旋转查看
    /// </summary>
    private void LookAround()
    {
        var down = false;
        var on = false;
        var up = false;
        switch(mouseRotateButton)
        {
            case MouseButton.左键:
                {
                    down = buttonState.leftButtonDown;
                    on = buttonState.leftButton;
                    up = buttonState.leftButtonUp;
                }
                break;
            case MouseButton.右键:
                {
                    down = buttonState.rightButtonDown;
                    on = buttonState.rightButton;
                    up = buttonState.rightButtonUp;
                }
                break;
        }

        if (down)
        {
            isAutoRotate = false;
            angle_x = m_camera.transform.eulerAngles.x;
            angle_y = m_camera.transform.eulerAngles.y - offestRotate.y;

             angle_z = m_camera.transform.eulerAngles.z;
            if(angle_x>180)
            {
                angle_x = angle_x - 360;
            }
        }

        if (on)
        {
            float deltaY = Input.GetAxis("Mouse X");
            float deltaX = Input.GetAxis("Mouse Y");

            angle_x -= deltaX * rotateSenstivity;
            angle_y += deltaY * rotateSenstivity;
          
            //控制相机旋转的俯仰角
            angle_x = Mathf.Clamp(angle_x, depression, eveation);
            //不要给y轴限位
            //Debug.LogWarning("后" + angle_x);
        }

        if (up)
        {
            StartAutoRotateCountdown();
        }
    }
    /// <summary>
    /// 滚轮滚动
    /// </summary>
    private void Zoom()
    {
        float scrollerValue = Input.GetAxis("Mouse ScrollWheel");
        if (!IsEqual(scrollerValue, 0))
        {
            //这一次滚轮推下前的targetdistance
            var rootDistance = targetDistance;
            targetDistance -= scrollerValue * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

            //todo:滚轮滚动预计到达的位置,现在只限制了y的最小值
            var estimatedPos = lookAroundPos + m_camera.transform.rotation * (-targetDistance * Vector3.forward);
            if (estimatedPos.y <= posLimit.minValue.y)
            {
                targetDistance = rootDistance;
            }
        }
    }
    /// <summary>
    /// 自动旋转
    /// </summary>

    private void AutoRotate()
    {

        if (!isAutoRotate)
            return;
        angle_y += Time.deltaTime * autoRotateSpeed;
    }

    /// <summary>
    /// 开始自动旋转的倒计时
    /// </summary>
    private void StartAutoRotateCountdown()
    {

        if (autoRotateCor != null)
        {
            StopCoroutine(autoRotateCor);
        }
        autoRotateCor = StartCoroutine(IE_AutoRotate());
    }

    /// <summary>
    /// 自动旋转协程
    /// </summary>
    /// <returns></returns>
    private IEnumerator IE_AutoRotate()
    {
        for(int i=0;i<autoRotateInternal;i++)
        {
            yield return new WaitForSeconds(1);
        }
        isAutoRotate = true;
    }

    /// <summary>
    /// 中键拖动相机移动
    /// </summary>
    private void Drag()
    {
        if (buttonState.middlebuttonDown)
        {
            lastMousePos = Input.mousePosition;
        }
        else if (buttonState.middlebutton)
        {
            Vector3 newMousePos = Input.mousePosition;
            //获得一帧的相机移动
            var delta = newMousePos - lastMousePos;
          var pos  = m_camera.transform.position - CalcuateDragNormal(m_camera, delta, distance);
            //if(pos.y<=MinY)
            //{
            //    pos.y = MinY;
            //}

            pos = posLimit.Clamp(pos);
            m_camera.transform.position = pos;
           lastMousePos = newMousePos;
            isAutoRotate = false;

        }
        else if(buttonState.middleButtonUp)
        {
            //松手开始倒计时
            StartAutoRotateCountdown();
           
        }

    }

    /// <summary>
    /// 键盘移动
    /// </summary>
    private void KeyBoardMove()
    {
        float deltaX = buttonState.horizontal;
        float deltaZ = buttonState.vertical;

        var dir = new Vector3(deltaX, 0, deltaZ);
       
         var pos= m_camera.transform.position+ m_camera.transform.rotation * dir*Time.deltaTime*keyBoardMoveSpeed;
        //if (pos.y <= MinY)
        //{
        //    pos.y = MinY;
        //}
        pos = posLimit.Clamp(pos);

        m_camera.transform.position = pos;
    }

    /// <summary>
    /// 计算拖动的位移
    /// </summary>
    /// <returns></returns>
    private Vector3 CalcuateDragNormal(CinemachineVirtualCamera _camera, Vector3 _delta, float _distance)
    {
        //通过fov计算旋转中心在视锥截面上的高
        var rect_height = 2 * _distance * Mathf.Tan(_camera.m_Lens.FieldOfView / 2 * Mathf.Deg2Rad);
        //计算屏幕高与中心点截面高的比例,这样能计算出鼠标移动的距离对应在旋转点截面上移动的距离
        var screenRate = rect_height / Screen.height;

        var move_normal = screenRate * _delta.x * _camera.transform.right + screenRate * _delta.y * _camera.transform.up;



        return move_normal;
    }


    /// <summary>
    /// 是否约等于
    /// </summary>
    /// <returns></returns>
    private bool IsEqual(float a, float b)
    {
        return Mathf.Abs(Mathf.Abs(a) - Mathf.Abs(b)) < 0.0001;
    }


    /// <summary>
    /// 进行双击检测
    /// </summary>
    private void OnDoubleClick()
    {
        secondClick = DateTime.Now;
        if(( secondClick-firstClick).TotalSeconds<0.2)
        {
            Debug.Log("双击");
            DrawRay();
        }
        else
        {
            firstClick = secondClick;
        }
    }


   
    Ray tmpray;
    /// <summary>
    /// 绘制射线
    /// </summary>
    private void DrawRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        tmpray = ray;
        RaycastHit hit;
        bool res = Physics.Raycast(ray, out hit);
        if(res)
        {
            targetPoint = hit.point;
            Debug.Log(hit.point);
            StartRayMove();
            
        }
    }

    /// <summary>
    /// 相机锁定目标移动
    /// </summary>
    private void LockMove()
    {
        var tmpPos = Vector3.Lerp(transform.position, transform.rotation * Vector3.back * distance + targetPoint, 0.05f);

        //if (tmpPos.y < MinY + 1)
        //{
        //    tmpPos.y = MinY + 1;
        //}
        tmpPos = posLimit.Clamp(tmpPos);

        transform.position = tmpPos;

    }
    /// <summary>
    /// 开始射线移动
    /// </summary>
    private void StartRayMove()
    {
        if(moveCor!=null)
        {
            StopCoroutine(moveCor);
        }
        moveCor = StartCoroutine(IE_RayMove());
    }

    /// <summary>
    /// 开始锁定目标移动协程
    /// </summary>
    public void LockTargetMove(Vector3 _pos,float _dis = 10)
    {
        targetPoint = _pos;
        if(moveCor!=null)
        {
            StopCoroutine(moveCor);
        }
        moveCor = StartCoroutine(IE_LockTargetMove(_dis));
    }
    /// <summary>
    /// 射线移动协程
    /// </summary>
    /// <returns></returns>
    private IEnumerator IE_RayMove()
    {
        isCameraMove = true;

        //大于一半就到一半的位置上
        //小于一半就直接飞上去
        float tmpDistance = 0;
       if( distance>(minDistance+maxDistance)/2)
        {
            tmpDistance = (distance + minDistance) / 2;
        }
       else
        {
            tmpDistance = minDistance;
        }
        //distance减半
        targetDistance = tmpDistance;
        distance = targetDistance;

        yield return new WaitForSeconds(1.1f);
        
        isCameraMove = false;
       
    }

    /// <summary>
    /// 锁定目标移动
    /// </summary>
    /// <returns></returns>
    private IEnumerator IE_LockTargetMove(float _dis)
    {
        isCameraMove = true;
        targetDistance = _dis;
        distance = targetDistance;
        yield return new WaitForSeconds(1.1f);
        isCameraMove = false;

       
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, targetPoint);
    }

    [Serializable]
    public struct ButtonState
    {
        public bool leftButtonDown;
        public bool leftButton;
        public bool leftButtonUp;

        public bool rightButtonDown;
        public bool rightButton;
        public bool rightButtonUp;

        public bool middlebuttonDown;
        public bool middlebutton;
        public bool middleButtonUp;

        public float horizontal;
        public float vertical;

    }

    /// <summary>
    /// 相机位置限制
    /// </summary>
    [Serializable]
    public struct CameraPosLimit
    {
        public Vector3 minValue;
        public Vector3 maxValue;

        public CameraPosLimit(Vector3 _min, Vector3 _max)
        {
            minValue = _min;
            maxValue = _max;
            
        }

        /// <summary>
        /// 获得pos在最大最小值之间
        /// </summary>
        /// <param name="_pos"></param>
        /// <returns></returns>
        public Vector3 Clamp(Vector3 _pos)
        {
            var _x = Mathf.Clamp(_pos.x, minValue.x, maxValue.x);
            var _y = Mathf.Clamp(_pos.y, minValue.y, maxValue.y);
            var _z = Mathf.Clamp(_pos.z, minValue.z, maxValue.z);
            return new Vector3(_x, _y, _z);
        }
    }



    public enum FreeCameraType
    {
        自由相机,
        旋转相机,
        自动旋转相机,
        旋转自由相机
    }

    /// <summary>
    /// 鼠标按键
    /// </summary>
    public enum MouseButton
    {
        左键,
        右键
    }

}

