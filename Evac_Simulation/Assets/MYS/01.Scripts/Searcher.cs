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
    NavMeshAgent nav;
    // 회전속도, 각도
    public float rotSpeed = 5.0f;
    float rotY;
    // 이동속도
    public float moveSpeed = 5.0f;
    // 회전 : 왼쪽, 오른쪽, 가만히
    // 이동 : 간다, 안간다
    // 오브젝트
    public GameObject[] objects;
    public GameObject[] players;
    public GameObject[] exits;
    //public List<GameObject> doors;
    //전 프레임 위치 좌표
    Vector3 previousPos;
    //가까운 출구
    Vector3 target;
    //불을 봤을 때 행동하는 bool값
    bool changeDir;
    bool decisionExit;
    bool detectEixt;


    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        //players = GameObject.FindGameObjectsWithTag("Player");
        //objects = new GameObject[players.Length + 1];

        //// 불은 0번에 넣는다.
        //objects[0] = GameObject.FindGameObjectWithTag("Fire");
        //// 플레이어는 1번부터 obejcts에 넣고
        //for (int i = 0; i < players.Length; i++)
        //{
        //    objects[1 + i] = players[i];
        //}

        previousPos = transform.position;
        nav = GetComponent<NavMeshAgent>();
        r2 = raySensor.GetRayPerceptionInput();
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
            AddReward(-1.0f / (float)MaxStep);
        }
        // 탈출 시간에 따른 벌점
        AddReward(-1.0f / (float)MaxStep);

        // 불이랑 가까워 졌을 때 받는 벌점
        float nowFireDisFromPlayer = Vector3.Distance(objects[1].transform.position, transform.position);
        float preFiredisFromPlayer = Vector3.Distance(objects[1].transform.position, previousPos);
        if (preFiredisFromPlayer >= nowFireDisFromPlayer)
        {
            AddReward(-10.0f / (float)MaxStep);
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

    private void OnDetectFireDecision()
    {
        // 레이퍼셉션의 각 태그번호를 가져오는 코드
        r3 = RayPerceptionSensor.Perceive(r2);
        foreach (RayPerceptionOutput.RayOutput ro in r3.RayOutputs)
        {
            changeDir = true;
            // 만약 불을 감지했다면 반대쪽 출구로 가라
            if (ro.HitTagIndex == 2)
            {
                /* 
                 * 1. 네비게이션 이동을 끄고
                 * 2. 자신의 행동에 따라 이동하며
                 * 3. 만약 불이 감지가 안됐다면
                 * 4. 자신의 현재 위치에서 가까운 출구를 검색하여 다시 네비게이션을 작동시켜라
                 */
                nav.enabled = false;
                changeDir = false;
                decisionExit = true;
                break;
            }
        }
        if (changeDir)
        {
            //nav.enabled = true;
            if (decisionExit)
            {
                DecisionClosetExit();
                decisionExit = false;
            }
            if (nav.enabled)
            {
                nav.destination = target;

            }
        }

    }

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
        sensor.AddObservation(objects[1].transform.position);
        float exits0Dis = Vector3.Distance(transform.position, exits[0].transform.position);
        float exits1Dis = Vector3.Distance(transform.position, exits[1].transform.position);
        float fireDis = Vector3.Distance(transform.position, objects[1].transform.position);
        sensor.AddObservation(exits0Dis);
        sensor.AddObservation(exits1Dis);
        sensor.AddObservation(fireDis);



    }
    public override void OnEpisodeBegin()
    {
        ReSetPosition();
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
                print(r3.RayOutputs[i].HitTagIndex+", "+i+"번째");
            }

            //    // 레이퍼셉션의 각 태그번호를 가져오는 코드
            //    r3 = RayPerceptionSensor.Perceive(r2);
            //foreach (RayPerceptionOutput.RayOutput ro in r3.RayOutputs)
            //{
            //    print(ro.HitTagIndex);
            //}
        }
        //Ray ray = new Ray(transform.position, transform.forward);
        //RaycastHit hit = new RaycastHit();
        //if (Physics.Raycast(ray, out hit))
        //{
        //    print(hit.transform.tag);
        //}
    }
    public void ReSetPosition()
    {
        //print("리셋됨 : " +gm.isReSetting);
        if (gm.isReSetting)
        {
            return;
        }
        gm.isReSetting = true;

        for (int j = 0; j < objects.Length; j++)
        {
            //objects[j].SetActive(true);
            objects[j].transform.localPosition = Vector3.zero;
            // 크기를 초기화한다
            if (objects[j].GetComponent<Fire>())
            {
                objects[j].GetComponent<Fire>().ResetFireSize();
            }
        }
        // 플레이어와 불은 랜덤한 위치에 배치한다.
        for (int i = 0; i < objects.Length; i++)
        {
            float xPos = Random.Range(-25, 25);
            float zPos = Random.Range(-25, 25);
            Vector3 myPos = new Vector3(xPos, 1, zPos);
            // 월드 위치를 로컬 위치로 변경
            //myPos = transform.parent.TransformPoint(myPos);
            // 단 아이템과 장애물들은 서로 겹치지 않아야 한다.
            // 레이어 마스크
            int ground = 1 << LayerMask.NameToLayer("Ground");
            Collider[] cols = Physics.OverlapBox(myPos, transform.localScale * 2, Quaternion.identity, ~ground);
            if (cols.Length > 0)
            {
                i--;
            }
            else
            {
                objects[i].transform.localPosition = myPos;
            }
        }
        //가까운 출구를 검색한다.
        //print("0번과 거리 : "+Vector3.Distance(transform.position, exits[0].transform.position));
        //print("1번과 거리 : " + Vector3.Distance(transform.position, exits[1].transform.position));
        DecisionClosetExit();



        // 제어변수 초기화
        changeDir = false;
        gm.isReSetting = false;
        detectEixt = false;
        // 개체수 초기화
        gm.DeadCount = 0;
        gm.EscapeCount = 0;
        //네비게이션 켜주기
        nav.enabled = true;
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
                AddReward(-10.0f / (float)MaxStep);
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
                //gm.EscapeCount++;
                //transform.gameObject.SetActive(false);


                EndEpisode();
                break;

            //불에 부딪히면 벌점 부여 상점에 2배 수치 부여
            case "Fire":
                AddReward(-20f);
                //gm.DeadCount++;
                //transform.gameObject.SetActive(false);


                EndEpisode();
                break;

            // 부딪힌 오브젝트의 태그가 없다면
            default:
                break;

        }
        // 만약 개체수가 플레이어와 같다면

        //if (gm.EscapeCount + gm.DeadCount == players.Length)
        //{
        //    // 만약 리셋중이 아니라면 에피소드를 다시 실행시킨다.
        //    if (gm.isReSetting == false)
        //    {
        //        print(gm.EscapeCount + gm.DeadCount);

        //        EndEpisode();

        //    }
        //}
    }


    private void OnCollisionStay(Collision coll)
    {

    }
    private void OnCollisionExit(Collision coll)
    {

    }
}
