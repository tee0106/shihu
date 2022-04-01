using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class WaterDown : MonoBehaviour
{
    private GameObject Water;
    private float timeSpend = 0.0f;
    private float down_TimeMin = 200.0f;
    private float down_TimeMax = 230.0f;
    private float game_time = 60.0f;
    private float opening_time = 21.5f;
    private float temp = 13.5f; //let the water rise up late
    TimeClock timeClock;

    void Start()
    {
        Water = GameObject.Find("Lake Polygon");
        timeClock = GameObject.Find("TimeText").GetComponent<TimeClock>();

        opening_time = opening_time + temp;
    }
    void Update()
    {
        timeSpend = timeClock.getTime();
        if (timeSpend >= (opening_time) && timeSpend < (game_time + opening_time))
        {
            Water.transform.position = new Vector3(-14.19f, -8 + (( 7.2f* (timeSpend - opening_time) ) / game_time), 37.76f);
            
        }
        else if (timeSpend > down_TimeMin && timeSpend <= down_TimeMax)
        {
            Water.transform.position = new Vector3(-14.19f, -0.8f - ((7.2f * (timeSpend - down_TimeMin)) / (down_TimeMax - down_TimeMin)), 37.76f);
        }
        //Debug.Log(timeSpend);
    }
}