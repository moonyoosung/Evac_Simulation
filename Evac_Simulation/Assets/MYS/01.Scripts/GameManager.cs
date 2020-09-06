using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //불
    public Fire fire;
    //플레이어들
    public List<Searcher> players;
    GameObject[] player;


    // 에피소드에 대한 리셋을 제어하는 변수
    public bool isReSetting;
    // 탈출 개체 수
    public int EscapeCount;
    // 불에 닿은 개체 수
    public int DeadCount;
    // 지정스텝수
    public int CustomMaxStep = 2000;
    void Start()
    {
        player = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < player.Length; i++)
        {
            players.Add(player[i].GetComponent<Searcher>());
        }
    }

    private void FixedUpdate()
    {
        // 만약 maxstep에 도달한 경우
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].gameObject.activeSelf)
            {
                if (players[i].StepCount >= CustomMaxStep)
                {
                    //죽은 오브젝트로 변경한다.
                    DeadCount++;
                    players[i].gameObject.SetActive(false);
                }
            }
        }

        // 만약 개체수가 플레이어와 같다면
        if (EscapeCount + DeadCount == players.Count)
        {
            //print("리셋");
            // 만약 리셋중이 아니라면 에피소드를 다시 실행시킨다.
            for (int i = 0; i < players.Count; i++)
            {
                // 각각의 플레이어들 자리 리셋
                players[i].EndEpisode();
            }
            // 불 위치 변경
            fire.firePosState = false;
            // 개체수 초기화
            EscapeCount = 0;
            DeadCount = 0;
        }
    }
}
