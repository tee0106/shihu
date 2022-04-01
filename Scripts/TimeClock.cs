using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TimeClock : MonoBehaviour
{
    public GameObject timePanel;
    public Text mytext;
	public Text mytext2;
    private float timeSpend = 0.0f;
    private float time = 60.0f;
    private float time_MAX = 80.0f;
    private float opening_time = 21.5f;
    private int minute;
    private int second;
    HandsViewer hand;
    private int stone_counter;
    Shoal fish1;
    ShoalManager fish2;
	bool Is_twohand = false;
    bool hands_Detect = false;
    bool game_start = false;

    // sound effect
    public AudioSource ready;
    public AudioSource start;
    public AudioSource left_60s;
    public AudioSource left_30s;
    public AudioSource countdown_sound;
    public AudioSource times_up_sound;
    public AudioSource success_sound;

    // Use this for initialization
    void Start()
    {
        timePanel = GameObject.Find("TimePanel");
        mytext2.enabled = false;
        hand = GameObject.Find("Camera").GetComponent<HandsViewer>();
        fish1 = GameObject.Find("Fish").GetComponent<Shoal>();
        fish2 = GameObject.Find("Shoal Manager").GetComponent<ShoalManager> ();

        time = time + opening_time;
        time_MAX = time_MAX + opening_time;
        timePanel.SetActive(false);

        ready = GameObject.Find("Ready").GetComponent<AudioSource>();
        start = GameObject.Find("Start").GetComponent<AudioSource>();
        left_60s = GameObject.Find("Left60s").GetComponent<AudioSource>();
        left_30s = GameObject.Find("Left30s").GetComponent<AudioSource>();
        countdown_sound = GameObject.Find("Countdown").GetComponent<AudioSource>();
        times_up_sound = GameObject.Find("TimesUp").GetComponent<AudioSource>();
        success_sound = GameObject.Find("Success").GetComponent<AudioSource>();

        game_start = false;
    }

    // Update is called once per frame
    void Update()
    {
        hands_Detect = hand.Get_hand_detect();
        //Debug.Log("detect: " + hands_Detect);
        if (hands_Detect)
        {
            game_start = true;
        }
        else
        {
            timeSpend = 0.0f;
        }

        if (game_start)
        {
            timeSpend += Time.deltaTime;
            //Debug.Log(timeSpend);
            minute = (int)((time - timeSpend + 0.1) / 60);
            second = (int)(time - timeSpend - 60 * minute);
            stone_counter = hand.getStoneCount();
            Is_twohand = hand.Get_Twohand();

            if (stone_counter == 7)
            {
                time = hand.getCheck_Full_Stone_Time();
                time_MAX = time + 20.0f;
            }

            if (timeSpend < time)
            {
                if (timeSpend >= (opening_time - 8) && timeSpend < (opening_time - 7))
                {
                    ready.Play();
                }
                else if (timeSpend >= (opening_time - 7) && timeSpend < (opening_time - 5))
                {
                    timePanel.SetActive(true);
                    mytext.text = string.Format("預備 ~");
                }
                else if (timeSpend >= (opening_time - 5) && timeSpend < (opening_time - 4))
                {
                    start.Play();
                }
                else if (timeSpend >= (opening_time - 4) && timeSpend < (opening_time - 3))
                {
                    mytext.text = string.Format("預備 3");
                }
                else if (timeSpend >= (opening_time - 3) && timeSpend < (opening_time - 2))
                {
                    mytext.text = string.Format("預備 2");
                }
                else if (timeSpend >= (opening_time - 2) && timeSpend < (opening_time - 1))
                {
                    mytext.text = string.Format("預備 1");
                }
                else if (timeSpend >= (opening_time - 1) && timeSpend < opening_time)
                {
                    mytext.text = string.Format("開始！");
                }
                else if (timeSpend >= opening_time)
                {
                    // mytext2.enabled = true;
                    mytext.text = string.Format("時間 {0:D2}:{1:D2}\n疊了 {2:D1} 顆石頭", minute, second, stone_counter);
                    if (Is_twohand == true)
                        mytext2.enabled = false;
                }

                /*
                if (timeSpend >= (time - 62) && timeSpend < (time - 61))
                {
                    left_60s.Play();
                }
                */
                if (timeSpend >= (time - 32) && timeSpend < (time - 31))
                {
                    left_30s.Play();
                }
                else if (timeSpend >= (time - 5) && timeSpend < (time - 4))
                {
                    countdown_sound.Play();
                }
            }
            else if (timeSpend >= time && timeSpend < (time + 1))
            {
                if (stone_counter == 7)
                {
                    mytext.text = string.Format("成功！\n共花了 {0:D2} 秒", ((int)time - (int)opening_time));
                    success_sound.Play();
                }
                else
                {
                    mytext.text = string.Format("時間到！");
                    times_up_sound.Play();
                }

                mytext2.enabled = false;
            }

            if (timeSpend >= time_MAX + 1)
            {
                timeSpend = 0.0f;
            }
        }

    }

    public float getTime()
    {
        return timeSpend;
    }

    public int getTime_min()
    {
        return minute;
    }

    public int getTime_sec()
    {
        return second;
    }

    void Disfish(){
        fish1.Disfish();
        fish2.DisfishC();
    }
}

