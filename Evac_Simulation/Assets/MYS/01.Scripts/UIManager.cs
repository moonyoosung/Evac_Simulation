using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    //Text
    [Header("TextMesh")]
    public TextMeshProUGUI DeadText;
    public TextMeshProUGUI EscapeText;
    public TextMeshProUGUI Min_TimeText;
    public TextMeshProUGUI Sec_TimeText;
    public TextMeshProUGUI PlayersCountText;

    [Header("DataObject")]
    public GameManager gameManager;

    void Start()
    {
        
    }

    void Update()
    {
        DeadText.text = gameManager.DeadCount.ToString();
        EscapeText.text = gameManager.EscapeCount.ToString();
        Min_TimeText.text = gameManager.m_Time.ToString();
        Sec_TimeText.text = ((int)gameManager.s_Time).ToString();
        PlayersCountText.text = (gameManager.players.Count - gameManager.DeadCount - gameManager.EscapeCount).ToString();
    }
}
