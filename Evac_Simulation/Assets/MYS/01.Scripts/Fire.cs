using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    // 매프레임마다 크기가 점점 커진다.
    public float sizeSpeed = 0.05f;
    public float fireSize = 5.0f;
    public bool firePosState = false;
    float xSize;
    float zSize;
    void Start()
    {
        ResetFireSize();
    }

    void Update()
    {
        if (xSize <= fireSize)
        {
            xSize += transform.localScale.x * sizeSpeed * Time.deltaTime;
            zSize += transform.localScale.z * sizeSpeed * Time.deltaTime;

            transform.localScale = new Vector3(xSize, transform.localScale.y, zSize);
        }
    }
    public void ResetFireSize()
    {
        transform.localScale = new Vector3(2.0f, 1.0f, 2.0f);
        xSize = transform.localScale.x;
        zSize = transform.localScale.z;
    }
    public void FirePosSetting()
    {
        // 불을 랜덤한 위치에 배치한다.
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
    }
}
