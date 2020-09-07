using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System;
using Random = UnityEngine.Random;
using Unity.Barracuda;
using UnityEngine.AI;

public class Searcher : Agent
{
    //에피소드관리자
    GameManager gm;
    //레이퍼셉션
    public RayPerceptionSensorComponent3D raySensor;
    RayPerceptionInput r2;
    RayPerceptionOutput r3;
    //네비게이션
    //NavMeshAgent nav;
    // 회전속도, 각도
    public float rotSpeed = 5.0f;
    float rotY;
    // 이동속도
    public float moveSpeed = 5.0f;
    // 회전 : 왼쪽, 오른쪽, 가만히
    // 이동 : 간다, 안간다
    // 불
    Fire fire;
    public GameObject[] exits;
    //public List<GameObject> doors;
    //전 프레임 위치 좌표
    Vector3 previousPos;
    //가까운 출구
    Vector3 target;
    //불을 봤을 때 행동하는 bool값
    bool decisionExit;
    bool detectEixt;


    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        fire = GameObject.FindGameObjectWithTag("Fire").GetComponent<Fire>();
        previousPos = transform.position;
        r2 = raySensor.GetRayPerceptionInput();
        exits = GameObject.FindGameObjectsWithTag("Exit");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReSetPosition();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            // 레이퍼셉션의 각 태그번호를 가져오는 코드
            r3 = RayPerceptionSensor.Perceive(r2);
            for (int i = 0; i < r3.RayOutputs.Length; i++)
            {
                print(r3.RayOutputs[i].HitTagIndex + ", " + i + "번째");
            }

        }

    }
    public override void Initialize()
    {
        //ReSetPosition();
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // 만일 vectorAction의 0번 값이 0이면 왼쪽으로 회전하고, 1이면 회전하지 않고, 2면 오른쪽으로 회전하게 하고 싶다.
        float rotvalue = vectorAction[0] - 1;
        rotY += rotvalue * rotSpeed;

        transform.localEulerAngles = new Vector3(0, rotY, 0);
        // 만일 vectorAction의 1번 값이 0이면 가만히 있고, 1이면 앞으로 움직인다.
        Vector3 dir = transform.forward * vectorAction[1];
        transform.position += dir * moveSpeed * Time.deltaTime;



        //가만히 있을 때 받는 벌점
        if (transform.position == previousPos)
        {
            AddReward(-1.0f / gm.CustomMaxStep);
        }
        // 탈출 시간에 따른 벌점
        AddReward(-1.0f / gm.CustomMaxStep);

        // 불이랑 가까워 졌을 때 받는 벌점
        float nowFireDisFromPlayer = Vector3.Distance(transform.position, transform.position);
        float preFiredisFromPlayer = Vector3.Distance(transform.position, previousPos);
        if (preFiredisFromPlayer >= nowFireDisFromPlayer)
        {
            AddReward(-10.0f / gm.CustomMaxStep);
        }

        if (detectEixt == false)
        {
            // 레이퍼셉션의 각 태그번호를 가져오는 코드
            r3 = RayPerceptionSensor.Perceive(r2);
            for (int i = 0; i < r3.RayOutputs.Length; i++)
            {
                //print(r3.RayOutputs[i].HitTagIndex + ", " + i + "번째");
                // 만약 레이에서 출구가 감지되면 상점
                if (r3.RayOutputs[i].HitTagIndex == 1)
                {
                    AddReward(1.0f);
                    detectEixt = true;
                    break;
                }
            }
        }
        else
        {
            // 레이퍼셉션의 각 태그번호를 가져오는 코드
            r3 = RayPerceptionSensor.Perceive(r2);
            for (int i = 0; i < r3.RayOutputs.Length; i++)
            {
                //print(r3.RayOutputs[i].HitTagIndex + ", " + i + "번째");
                // 만약 레이에서 출구가 감지되지 않는다면 벌점
                if (r3.RayOutputs[i].HitTagIndex == 1)
                {
                    break;
                }
                if (i == r3.RayOutputs.Length)
                {
                    AddReward(-10.0f);
                }
            }
        }

        //전프레임 위치 저장
        previousPos = transform.position;
        // 불을 감지했을 때 결정할 함수(네비게이션)
        //OnDetectFireDecision();


    }

    //private void OnDetectFireDecision()
    //{
    //    // 레이퍼셉션의 각 태그번호를 가져오는 코드
    //    r3 = RayPerceptionSensor.Perceive(r2);
    //    foreach (RayPerceptionOutput.RayOutput ro in r3.RayOutputs)
    //    {
    //        changeDir = true;
    //        // 만약 불을 감지했다면 반대쪽 출구로 가라
    //        if (ro.HitTagIndex == 2)
    //        {
    //            /* 
    //             * 1. 네비게이션 이동을 끄고
    //             * 2. 자신의 행동에 따라 이동하며
    //             * 3. 만약 불이 감지가 안됐다면
    //             * 4. 자신의 현재 위치에서 가까운 출구를 검색하여 다시 네비게이션을 작동시켜라
    //             */
    //            nav.enabled = false;
    //            changeDir = false;
    //            decisionExit = true;
    //            break;
    //        }
    //    }
    //    if (changeDir)
    //    {
    //        //nav.enabled = true;
    //        if (decisionExit)
    //        {
    //            DecisionClosetExit();
    //            decisionExit = false;
    //        }
    //        if (nav.enabled)
    //        {
    //            nav.destination = target;

    //        }
    //    }

    //}

    public override void Heuristic(float[] actionsOut)
    {
        // 만일 A키를 누르면 actionOut[0]에 0을 넘기고, D키를 누르면 2를 넘기고 아무것도 안누르면 1을 넘기겠다.
        actionsOut[0] = 1;
        if (Input.GetKey(KeyCode.A))
        {
            actionsOut[0] = 0;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            actionsOut[0] = 2;
        }
        // 만일 W키를 누르면 actionOut[1]에 1을 넘기고, 안 누르면 0을 넘기겠다.
        actionsOut[1] = 0;
        if (Input.GetKey(KeyCode.W))
        {
            actionsOut[1] = 1.0f;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(exits[0].transform.position);
        sensor.AddObservation(exits[1].transform.position);
        sensor.AddObservation(fire.transform.position);
        float exits0Dis = Vector3.Distance(transform.position, exits[0].transform.position);
        float exits1Dis = Vector3.Distance(transform.position, exits[1].transform.position);
        float fireDis = Vector3.Distance(transform.position, fire.transform.position);
        sensor.AddObservation(exits0Dis);
        sensor.AddObservation(exits1Dis);
        sensor.AddObservation(fireDis);



    }
    public override void OnEpisodeBegin()
    {
        ReSetPosition();
    }



    public void ReSetPosition()
    {
        // 위치를 가운데로 옮긴다.
        transform.localPosition = Vector3.zero;
        // 불의 크기를 초기화한다
        if (fire)
        {
            fire.ResetFireSize();
        }
        // 만약 불의 위치가 정해지지 않았다면
        if (fire.firePosState == false)
        {
            fire.FirePosSetting();
            fire.firePosState = true;
        }
        if (transform.gameObject.activeSelf == false)
        {
            transform.gameObject.SetActive(true);
        }

        // 플레이어 랜덤한 위치에 배치한다.
        for (int i = 0; i < 1; i++)
        {
            float xPos = Random.Range(-25, 25);
            float zPos = Random.Range(-25, 25);
            Vector3 myPos = new Vector3(xPos, 1, zPos);
            // 단 아이템과 장애물들은 서로 겹치지 않아야 한다.
            // 레이어 마스크
            int ground = 1 << LayerMask.NameToLayer("Ground");
            int room = 1 << LayerMask.NameToLayer("RoomBox");
            int checkLayer = ground | room;
            Collider[] cols = Physics.OverlapBox(myPos, transform.localScale * 2, Quaternion.identity, ~checkLayer);
            if (cols.Length > 0)
            {
                i--;
            }
            else
            {
                transform.localPosition = myPos;
            }
        }

        //가까운 출구를 검색한다.
        //print("0번과 거리 : "+Vector3.Distance(transform.position, exits[0].transform.position));
        //print("1번과 거리 : " + Vector3.Distance(transform.position, exits[1].transform.position));
        DecisionClosetExit();

        // 제어변수 초기화
        detectEixt = false;
        gm.timerState = true;
    }

    private void DecisionClosetExit()
    {
        if (Vector3.Distance(transform.position, exits[0].transform.position) > Vector3.Distance(transform.position, exits[1].transform.position))
        {
            target = exits[1].transform.position;
            // print("1번 출구");
        }
        else
        {
            target = exits[0].transform.position;
            //print("0번 출구");
        }
    }

    // 문을 통과해서 나간다면
    private void OnTriggerExit(Collider col)
    {

    }
    private void OnTriggerStay(Collider col)
    {
        // 방안에 계속 있는 상태라면 벌점
        switch (col.gameObject.tag)
        {
            case "Room":
                AddReward(-10.0f / gm.CustomMaxStep);
                break;
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        switch (col.gameObject.tag)
        {
            //출구에 부딪히면 상점 부여 
            case "Exit":
                AddReward(10.0f);
                //EndEpisode();
                gm.EscapeCount++;
                transform.gameObject.SetActive(false);
                break;

            //불에 부딪히면 벌점 부여 상점에 2배 수치 부여
            case "Fire":
                AddReward(-20f);
                //EndEpisode();
                gm.DeadCount++;
                transform.gameObject.SetActive(false);
                break;

            // 부딪힌 오브젝트의 태그가 없다면
            default:
                break;

        }

    }


    private void OnCollisionStay(Collision coll)
    {

    }
    private void OnCollisionExit(Collision coll)
    {

    }
}
