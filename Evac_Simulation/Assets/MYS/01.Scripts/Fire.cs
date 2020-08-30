using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    // 매프레임마다 크기가 점점 커진다.
    public float sizeSpeed = 0.05f;
    float xSize;
    float zSize;
    void Start()
    {
        ResetFireSize();
    }

    void Update()
    {
        xSize += transform.localScale.x * sizeSpeed * Time.deltaTime;
        zSize += transform.localScale.z * sizeSpeed * Time.deltaTime;

        transform.localScale = new Vector3(xSize, transform.localScale.y, zSize);

    }
    public void ResetFireSize()
    {
        transform.localScale = new Vector3(2.0f,1.0f,2.0f);
        xSize = transform.localScale.x;
        zSize = transform.localScale.z;
    }
}
