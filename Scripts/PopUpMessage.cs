using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PopUpMessage : MonoBehaviour
{
    public GameObject panel;
    public GameObject image;
    public GameObject image2;
    public GameObject pane5finger;
    public GameObject panehint;
    public Text hinttext;
    public Text text;
    public Text finger5text;
    TimeClock timeClock;
    private float timeSec;
    HandsViewer hand;
    private int stone_count;
    private float end_time = 60.0f;
    private float opening_time = 21.5f;
    private bool finger5 = true ;
    private bool handup = false;
    public RectTransform rt;
    public RectTransform rt2;
    bool hands_Detect = false;

    // Start is called before the first frame update
    void Start()
    {
        panel = GameObject.Find("Panel");
        rt = panel.GetComponent<RectTransform>();
        image = GameObject.Find("Image");
        rt2 = image.GetComponent<RectTransform>();
        image2 = GameObject.Find("Image2");
        text = GameObject.Find("Text").GetComponent<Text>();
        timeClock = GameObject.Find("TimeText").GetComponent<TimeClock>();
        hand = GameObject.Find("Camera").GetComponent<HandsViewer>();
        finger5text = GameObject.Find("Text5finger").GetComponent<Text>();

        PopUp_Initial();

        end_time = end_time + opening_time;

        text.fontSize = 30;
        rt.sizeDelta = new Vector2(450, 250);
        rt.localPosition = new Vector3(-650, -280, 0);
        rt2.sizeDelta = new Vector2(152, 214);
        rt2.localPosition = new Vector3(-850, -170, 0);
    }

    // Update is called once per frame
    void Update()
    {
        timeSec = timeClock.getTime();
        stone_count = hand.getStoneCount();
        finger5 = hand.Get_5finger();
        handup = hand.Get_handup();
        hands_Detect = hand.Get_hand_detect();
        panehint.SetActive(false);
        pane5finger.SetActive(false);
        finger5text.text = string.Format("五指張開，手掌心面對鏡頭");
        
        if (!hands_Detect)
        {
            pane5finger.SetActive(true);
            finger5text.text = string.Format("將雙手對著鏡頭，進入遊戲畫面");
        }
        if (stone_count == 7)
        {
            end_time = hand.getCheck_Full_Stone_Time();
        }
        if (timeSec >= (opening_time - 7)  && timeSec < opening_time )
        {
            text.text = string.Format("請將手掌心對著鏡頭\n五指收起，撿起石頭\n五指完全張開，放開石頭\n請在60秒內疊完7顆石頭");
            //panel.SetActive(true);
            //image.SetActive(true);
            //pane5finger.SetActive(finger5);
            pane5finger.SetActive(false);
            panehint.SetActive(true);
        }
        if(timeSec > opening_time && timeSec < end_time)
        {
            panehint.SetActive(handup);
            hinttext.text = string.Format("請把手抬高點");
            //pane5finger.SetActive(finger5);
        }
        if (timeSec >= end_time)
        {
            PopUp_Initial();
            //image2.SetActive(true);
        }
        if (timeSec >= (end_time + 8))
        {
            text.fontSize = 36;
            rt.sizeDelta = new Vector2(600, 300);
            rt.localPosition = new Vector3(40, 200, 0);
            rt2.sizeDelta = new Vector2(228, 307);
            rt2.localPosition = new Vector3(-260, 280, 0);
            //PopUp_Initial();
            if (stone_count > 0)
            {
                text.text = string.Format("成功疊了 {0:D} 顆石頭！\n \n恭喜你捕抓到 {1:D} 條魚！", stone_count, stone_count * 10);
            }
            else
            {
                text.text = string.Format("沒疊到任何石頭\n \n 再接再厲！加油！");
            }
            panel.SetActive(true);
            image.SetActive(true);
        }
        if (timeSec >= (end_time + 18))
        {
            PopUp_Initial();
        }
        
    }

    void PopUp_Initial()
    {
        panel.SetActive(false);
        image.SetActive(false);
        image2.SetActive(false);
        pane5finger.SetActive(false);
        panehint.SetActive(false);
    }
}
