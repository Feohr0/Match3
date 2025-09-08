using Obi;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class Timer : MonoBehaviour
{
    public SoruPopUp pop;
    
    public bool isCounting = true;
    [SerializeField] public float startingtime;
    [SerializeField] public TextMeshProUGUI timertext;
    private float countdown;
    public bool isOver = false;
    private void Start()
    {
        countdown = startingtime;
    }
    public void Update()
    {
        if (!isCounting)
        {
            return;
            
        }

        timertext.enabled = true;

        if (timertext != null && countdown > 0)
        {
            
            countdown -= Time.deltaTime;
            if (countdown < (startingtime / 5))
            {
                timertext.color = Color.red;
            }
        }

        else
        {
            countdown = 0;
            isOver = true;
            isCounting = false;
        }

        int mins = Mathf.FloorToInt(countdown / 60);
        int secs = Mathf.FloorToInt(countdown % 60);
        timertext.text = string.Format("{0:00} : {1:00}", mins, secs);
    }
    
}
