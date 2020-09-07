using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoke : MonoBehaviour
{
    public GameObject target;
    public float speed = 0.01f;

    // 매 프레임마다 연기 y 크기가 점점 커지게 하고 싶다.
    public float sizeSpeed = 0.005f;
    float ySize;

    void Start()
    {
        ResetSmokeSize();
    }

    void Update()
    {
        // Searcher 스크립트에서 Fire의 발생 위치 (myPos) 가져오고싶다. 
        GameObject.Find("Fire").GetComponent<Searcher>().ReSetPosition();

        // 만약 불이 방에 발생하면 해당 방의 RB를 내려오게 하고 싶다.
        //if ()
        {
            // 아래로 이동하게 하고 싶다. 
            transform.Translate(Vector3.down * speed * Time.deltaTime);

            // y축 크기를 키우고 싶다.
            ySize += transform.localScale.y * sizeSpeed * Time.deltaTime;

            transform.localScale = new Vector3(transform.localScale.x, ySize, transform.localScale.z);
        }

        // 만약 불이 복도에 발생하면 전체 RB를 내려오게 하고 싶다. 
        //if ()
        {

        }



    }

    void ResetSmokeSize()
    {
        transform.localScale = new Vector3(4.2f, 1.0f, 4.8f);
        ySize = transform.localScale.y;
    }



}


