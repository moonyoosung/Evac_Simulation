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
    public GameObject[] exits;
    public List<GameObject> doors;
    //전 프레임 위치 좌표
    Vector3 previousPos;
    //가까운 출구
    Vector3 target;
    void Start()
    {
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
        //// 만일 vectorAction의 0번 값이 0이면 왼쪽으로 회전하고, 1이면 회전하지 않고, 2면 오른쪽으로 회전하게 하고 싶다.
        //float rotvalue = vectorAction[0] - 1;
        ////print(rotvalue);
        ////transform.Rotate(transform.up, rotvalue * rotSpeed);
        //rotY += rotvalue * rotSpeed;

        //transform.localEulerAngles = new Vector3(0, rotY, 0);
        //// 만일 vectorAction의 1번 값이 0이면 가만히 있고, 1이면 앞으로 움직인다.
        //Vector3 dir = transform.forward * vectorAction[1];
        //transform.position += dir * moveSpeed * Time.deltaTime;


        //시간에 경과에 따른 벌점 부여
        AddReward(-10.0f / (float)MaxStep);
        ////현재 거리와 출구 1의 거리
        //float nowDis = Vector3.Distance(transform.position, target);
        ////예전 거리와 출구 1의 거리
        //float preDis = Vector3.Distance(previousPos, target);

        //if (preDis <= nowDis)
        //{
        //    //MaxStep이 0 보다 크다면 1 / maxStep만큼 점수를 감점
        //    if (MaxStep > 0)
        //    {

        //    }
        //}
        //else
        //{
        //    //AddReward(0.1f);
        //}


        ////전프레임 위치 저장
        //previousPos = transform.position;
        nav.destination = target;

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
            foreach (RayPerceptionOutput.RayOutput ro in r3.RayOutputs)
            {
                print(ro.HitFraction);
            }
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
        for (int j = 0; j < objects.Length; j++)
        {
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
        switch (col.gameObject.tag)
        {
            //문에 부딪히면 상점 부여
            case "Door":
                CheckDoor(col);
                break;

            // 부딪힌 오브젝트의 태그가 없다면
            default:
                break;
        }
    }
    private void OnCollisionEnter(Collision col)
    {
        switch (col.gameObject.tag)
        {
            //벽에 부딪히면 벌점 부여
            //case "Wall":
            //    AddReward(-0.3f);
            //    break;
            //출구에 부딪히면 상점 부여 
            case "Exit":
                AddReward(10.0f);
                EndEpisode();
                break;
            //불에 부딪히면 벌점 부여 상점에 2배 수치 부여
            case "Fire":
                AddReward(-20f);

                EndEpisode();
                break;
            ////    문에 부딪히면 상점 부여
            //case "Door":
            //    CheckDoor(col);
            //    break;

            // 부딪힌 오브젝트의 태그가 없다면
            default:
                break;
        }
    }

    private void CheckDoor(Collider coll)
    {
        // print("문ㅇ체크");
        //지나간 문이 doors 리스트에 있다면 상점을 부여하고 리스트에서 삭제한다.
        for (int i = 0; i < doors.Count; i++)
        {
            if (coll.gameObject == doors[i])
            {
                AddReward(2.0f);
                // print(doors[i].transform.name);
                doors.RemoveAt(i);
                break;
            }
        }
    }

    private void OnCollisionStay(Collision coll)
    {

    }
    private void OnCollisionExit(Collision coll)
    {

    }
}
