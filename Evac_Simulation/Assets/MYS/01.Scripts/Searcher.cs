using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System;
using Random = UnityEngine.Random;
using Unity.Barracuda;

public class Searcher : Agent
{
    // 회전속도
    public float rotSpeed = 5.0f;
    // 이동속도
    public float moveSpeed = 5.0f;
    // 회전 : 왼쪽, 오른쪽, 가만히
    // 이동 : 간다, 안간다
    // 오브젝트
    public GameObject[] objects;
    public GameObject[] exits;
    //전 프레임 위치 좌표
    Vector3 previousPos;
    //가까운 출구
    Vector3 target;
    void Start()
    {
        previousPos = transform.position;
    }
    public override void Initialize()
    {
        ReSetPosition();

    }
    public override void OnActionReceived(float[] vectorAction)
    {
        // 만일 vectorAction의 0번 값이 0이면 왼쪽으로 회전하고, 1이면 회전하지 않고, 2면 오른쪽으로 회전하게 하고 싶다.
        float rotvalue = vectorAction[0] - 1;
        transform.Rotate(transform.up, rotvalue * rotSpeed);
        // 만일 vectorAction의 1번 값이 0이면 가만히 있고, 1이면 앞으로 움직인다.
        Vector3 dir = new Vector3(0, 0, vectorAction[1]);
        dir = transform.TransformDirection(dir);
        transform.position += dir * moveSpeed * Time.deltaTime;
        //MaxStep이 0 이상이라면 1 / maxStep만큼 점수를 감점
        if (MaxStep > 0)
        {
            //시간에 따른 벌점 부여
            AddReward(-1.0f / (float)MaxStep);
        }


        //현재 거리와 출구 1의 거리
        float nowDis = Vector3.Distance(transform.position, target);
        //예전 거리와 출구 1의 거리
        float preDis = Vector3.Distance(previousPos, target);

        if (preDis < nowDis)
        {
            AddReward(-0.1f);
        }


        //전프레임 위치 저장
        previousPos = transform.position;
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
    }
    public void ReSetPosition()
    {
        for (int j = 0; j < objects.Length; j++)
        {
            objects[j].transform.localPosition = Vector3.zero;
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

        if (Vector3.Distance(transform.position, exits[0].transform.position) > Vector3.Distance(transform.position, exits[1].transform.position))
        {
            target = exits[1].transform.position;
        }
        else
        {
            target = exits[0].transform.position;
        }
    }
    private void OnCollisionEnter(Collision col)
    {
        switch (col.gameObject.tag)
        {
            //벽에 부딪히면 벌점 부여
            case "Wall":
                AddReward(-1.0f);
                break;
            //출구에 부딪히면 상점 부여 
            case "Exit":
                AddReward(10.0f);
                EndEpisode();
                break;
            //불에 부딪히면 벌점 부여
            case "Fire":
                AddReward(-10.0f);
                EndEpisode();
                break;
            //문에 부딪히면 상점 부여
            case "Door":
                AddReward(1.0f);
                break;
        }
    }
}
