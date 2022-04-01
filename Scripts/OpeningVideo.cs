using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System;

public class OpeningVideo : MonoBehaviour
{
    public GameObject panelvideo;
    public VideoPlayer videoPlayer;
    public GameObject loadingvideo;
    public VideoPlayer loadingvideoPlayer;
    TimeClock timeClock;
    private float timeSec;
    private float start_time = 0.0f;
    private float video_time = 14.5f;
    HandsViewer hand;
    bool hands_Detect = false;

    // Start is called before the first frame update
    void Start()
    {
        panelvideo = GameObject.Find("PanelVideo");
        videoPlayer = panelvideo.GetComponent<VideoPlayer>();
        loadingvideo = GameObject.Find("LoadingVideo");
        loadingvideoPlayer = loadingvideo.GetComponent<VideoPlayer>();
        timeClock = GameObject.Find("TimeText").GetComponent<TimeClock>();
        hand = GameObject.Find("Camera").GetComponent<HandsViewer>();

        panelvideo.SetActive(false);
        loadingvideo.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        hands_Detect = hand.Get_hand_detect();

        if (hands_Detect)
        {
            timeSec = timeClock.getTime();
            VideoPlayerEndReached(loadingvideoPlayer);

            if (timeSec >= start_time && timeSec < (start_time + video_time))
            {
                panelvideo.SetActive(true);
            }
            if (timeSec >= (start_time + video_time))
            {
                VideoPlayerEndReached(videoPlayer);
            }
        }
        else
        {
            loadingvideo.SetActive(true);
        }
        
        
    }

    void VideoPlayerEndReached(VideoPlayer vp1)
    {
        StartCoroutine(RemoveAfterDelay(vp1));
        vp1 = null;
    }

    IEnumerator RemoveAfterDelay(VideoPlayer vp)
    {
        yield return null;
        if (vp != null)
        {
            Destroy(vp.gameObject);
            vp = null;
        }
    }
}
