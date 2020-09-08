using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoke : MonoBehaviour
{
    //에피소드관리자
    GameManager gm;
    Searcher ply;

    public float downSpeed = 0.01f; // 연기하강 속력

    public float sizeSpeed = 0.005f; // 사이즈업 속력
    float ySize;

    public float detectTime = 10.0f; //  감지시간    
    float detectStartTime; // 감지시작시간


    void Start()
    {
        // Player의 Searcher 스크립트 호출
        ply = GameObject.Find("Player").GetComponent<Searcher>();

        ResetSmokePos();
        ResetSmokeSize();
    }

    void Update()
    {
        // 만약 불이 방에 발생하면 해당 방의 RB를 내려오게 하고 싶다.
        // 1. 모든 방을 검사해서
        // 2. firePos의 x와z 값이 x-scale.x/2 보다 크거나


        // 아래로 이동하게 하고 싶다. 
        transform.Translate(Vector3.down * downSpeed * Time.deltaTime);

        // y축 크기를 키우고 싶다.
        ySize += transform.localScale.y * sizeSpeed * Time.deltaTime;

        transform.localScale = new Vector3(transform.localScale.x, ySize, transform.localScale.z);

    }

    void ResetSmokePos()
    {
        transform.localPosition = new Vector3(0, 5, 0);
    }

    void ResetSmokeSize()
    {
        transform.localScale = new Vector3(4.2f, 1.0f, 4.8f);
        ySize = transform.localScale.y;
    }

    public void OnTriggerStay(Collider col)
    {

        // 연기에 Player가 부딪히면 벌점
        if(col.gameObject.tag.Contains("Player"))
        {
            detectStartTime += Time.deltaTime; // 감지시작

            ply.AddReward(-20.0f / gm.CustomMaxStep);
        }
        
        // 10초이상 감지가 지속되면 벌점
        if (detectStartTime >= detectTime)
        {
            ply.AddReward(-20.0f);
            gm.DeadCount++;
            ply.transform.gameObject.SetActive(false);
            detectStartTime = 0; // 초기화
        }
    }

}


