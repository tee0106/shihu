using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class HandsViewer : MonoBehaviour
{
    public GameObject camera;
    public GameObject JointPrefab; //Prefab for Joints
    public GameObject TipPrefab; //Prefab for Finger Tips
    public GameObject BonePrefab; //Prafab for Bones
    public GameObject PalmCenterPrefab;//Prefab for Palm Center
    public GameObject StonePrefab;
    public Material StoneMaterial;
    public GameObject HandPrefab_L;
    public GameObject HandPrefab_R;

    public Text myTextLeft;//GuiText for Left hand
    public Text myTextRight;//Pointer for Right hand

    private GameObject[][] myJoints; //Array of Joint GameObjects
    private GameObject[][] myBones; //Array of Bone GameObjects

    private GameObject[] stones;
    private int stones_max = 7;
    private int hold_fingers_pose = 0;
    private int hold_fingers = 0;
    private int[] respawn_time_counter;
    private Boolean[] stones_locked;
    private Boolean[] places_locked;
    private Double[][] place;
    private Double[] jointDistance;
    private Double jointDistance5, jointDistance9, jointDistance13, jointDistance17, jointDistance21, objDistance;
    private Double dx5, dy5, dz5;
    private Double dx9, dy9, dz9;
    private Double dx13, dy13, dz13;
    private Double dx17, dy17, dz17;
    private Double dx21, dy21, dz21;
    private Double dxObj, dyObj, dzObj;
    private Double stone_x, stone_y, stone_z;
    private Animator[] animator;
    private bool[] fingers_hold;
    private bool pose = false;
    private int stone_p = 0;
    private bool[] hold_stone_trigger;
    private bool hand_detect = false;

    private PXCMHandData.JointData[][] jointData; //non-smooth joint values
    private PXCMSmoother smoother = null; //Smoothing module instance
    private PXCMSmoother.Smoother3D[][] smoother3D = null; //smooth joint values
    private int weightsNum = 4; //smoothing factor

    private PXCMSenseManager sm = null; //SenseManager Instance
    private pxcmStatus sts; //StatusType Instance
    private PXCMHandModule handAnalyzer; //Hand Module Instance
    private int MaxHands = 2; //Max Hands
    private int MaxJoints = PXCMHandData.NUMBER_OF_JOINTS; //Max Joints

    private Hashtable handList;//keep track of bodyside and hands for GUItext

    // hand rotation x,y
    private Double a, b, res;
    private float res2, res21, res22, res23;
    private float[] past_x, past_y, past_z;
    private Vector3 v1, v2, v3, v4,v5;
    private bool hand_right = true;
    private bool hold_stone = false;

    // pop text 5 finger
    private bool pop_5finger = true;

    //  hand rotation smooth flag
    private int[] flag_x, flag_y, flag_z;
    private int flag_rotation_num = 12;

    // count z rotation
    private Vector3 z_orig, z_now;

    // catch vector
    private Vector3 Origin1, Origin2, resA, resB, ChangeVector;
    private Quaternion rotation1, rotation2;
    private bool cancatch = true;
	
	// new two hand operation
    bool Is_twohand = false; //two hands or not
    private bool changehand = true; //change hand or not
    private int past_ihand = 0, now_ihand = 0; // the number of hands of past and now
    private bool lefthandflag = true; //flip hand one time flag
    int flamecount = 0; //count flame for one hand only

    // Hint stone place
    public GameObject UP;
    public GameObject DOWN;

    // Hint handup
    private bool handup = false;

    TimeClock timeClock;
    private float timeSec;
    private float end_time = 60.0f;
    private float restart_time = 80.0f;
    private float opening_time = 21.5f;
    private float check_Full_Stone_Time = 9999.0f;
    private float timeSpend = 0.0f;

    // sound effect
    public AudioSource bgm;
    public AudioSource stone_sound;
    public AudioSource pick_up_sound;

    // Use this for initialization
    void Start()
    {
        hand_detect = false;
        bgm = GameObject.Find("BGM").GetComponent<AudioSource>();
        stone_sound = GameObject.Find("StoneSound").GetComponent<AudioSource>();
        pick_up_sound = GameObject.Find("Pickup").GetComponent<AudioSource>();

        timeClock = GameObject.Find("TimeText").GetComponent<TimeClock>();
        timeSec = timeClock.getTime();

        end_time = end_time + opening_time;
        restart_time = restart_time + opening_time;

        handList = new Hashtable();

        /* Initialize a PXCMSenseManager instance */
        sm = PXCMSenseManager.CreateInstance();
        if (sm == null)
            Debug.LogError("SenseManager Initialization Failed");

        /* Enable hand tracking and retrieve an hand module instance to configure */
        sts = sm.EnableHand();
        handAnalyzer = sm.QueryHand();
        if (sts != pxcmStatus.PXCM_STATUS_NO_ERROR)
            Debug.LogError("PXCSenseManager.EnableHand: " + sts);

        /* Initialize the execution pipeline */
        sts = sm.Init();
        if (sts != pxcmStatus.PXCM_STATUS_NO_ERROR)
            Debug.LogError("PXCSenseManager.Init: " + sts);

        /* Retrieve the the DataSmoothing instance */
        sm.QuerySession().CreateImpl<PXCMSmoother>(out smoother);

        /* Create a 3D Weighted algorithm */
        smoother3D = new PXCMSmoother.Smoother3D[MaxHands][];

        /* Configure a hand - Enable Gestures and Alerts */
        PXCMHandConfiguration hcfg = handAnalyzer.CreateActiveConfiguration();
        if (hcfg != null)
        {
            hcfg.EnableAllGestures();
            hcfg.EnableAlert(PXCMHandData.AlertType.ALERT_HAND_NOT_DETECTED);
            hcfg.ApplyChanges();
            hcfg.Dispose();
        }

        InitializeGameobjects();
        
    }

    // Update is called once per frame
    void Update()
    {
        timeSec = timeClock.getTime();

        /* Make sure SenseManager Instance is valid */
        if (sm == null)
            return;

        /* Wait until any frame data is available */
        if (sm.AcquireFrame(false) != pxcmStatus.PXCM_STATUS_NO_ERROR)
            return;

        /* Retrieve hand tracking Module Instance */
        handAnalyzer = sm.QueryHand();

        if (handAnalyzer != null)
        {
            /* Retrieve hand tracking Data */
            PXCMHandData _handData = handAnalyzer.CreateOutput();
            if (_handData != null)
            {
                _handData.Update();

                /* Retrieve Gesture Data to manipulate GUIText */
                PXCMHandData.GestureData gestureData;
                for (int i = 0; i < _handData.QueryFiredGesturesNumber(); i++)
                    if (_handData.QueryFiredGestureData(i, out gestureData) == pxcmStatus.PXCM_STATUS_NO_ERROR)
                    {
                        try
                        {
                            DisplayGestures(gestureData);
                        }
                        catch (NullReferenceException ex)
                        {
                            //Debug.Log("null gesture");
                        }
                    }

                /* Retrieve Alert Data to manipulate GUIText */
                PXCMHandData.AlertData alertData;
                for (int i = 0; i < _handData.QueryFiredAlertsNumber(); i++)
                    if (_handData.QueryFiredAlertData(i, out alertData) == pxcmStatus.PXCM_STATUS_NO_ERROR)
                    {
                        try
                        {
                            ProcessAlerts(alertData);
                        }
                        catch (NullReferenceException ex)
                        {
                            //Debug.Log("process alerts");
                        }
                    }

                /* Retrieve all joint Data */
                for (int i = 0; i < _handData.QueryNumberOfHands(); i++)
                {
                    PXCMHandData.IHand _iHand;
                    if (_handData.QueryHandData(PXCMHandData.AccessOrderType.ACCESS_ORDER_FIXED, i, out _iHand) == pxcmStatus.PXCM_STATUS_NO_ERROR)
                    {
                        for (int j = 0; j < MaxJoints; j++)
                        {
                            if (_iHand.QueryTrackedJoint((PXCMHandData.JointType)j, out jointData[i][j]) != pxcmStatus.PXCM_STATUS_NO_ERROR)
                                jointData[i][j] = null;
                        }
                        if (!handList.ContainsKey(_iHand.QueryUniqueId()))
                            handList.Add(_iHand.QueryUniqueId(), _iHand.QueryBodySide());
                    }
                }

                /* Smoothen and Display the Data - Joints and Bones*/
                DisplayJoints();

                for (int h = 0; h < MaxHands; h++)
                {
                    int h2 = h + 2;
                    if (hold_stone_trigger[h] == true && hold_stone_trigger[h2] == false)
                    {
                        pick_up_sound.Play();
                    }
                    hold_stone_trigger[h2] = hold_stone_trigger[h];
                    hold_stone_trigger[h] = false;
                }

                for (int k = 0; k < stones_max; k++) //stones respawn when stones fly out the boundary
                {
                    stone_x = stones[k].transform.position.x;
                    stone_y = stones[k].transform.position.y;
                    stone_z = stones[k].transform.position.z;

                    if ( (stone_x < -25 || stone_x > 30) || (stone_z < 15 || stone_z > 55) || stone_y < -15)
                    {
                        respawn_time_counter[k]++;
                    }

                    //stones respawn every 3s
                    //1s about per 70 counters
                    if (respawn_time_counter[k] == 210)
                    {
                        Rigidbody rb = stones[k].GetComponent<Rigidbody>();
                        rb.isKinematic = true;
                        rb.isKinematic = false;
                        stones[k].transform.position = new Vector3(UnityEngine.Random.Range(-10.0f, 20.0f), 10.0f, 30.0f);
                        stones[k].transform.rotation = Quaternion.Euler(0, 0, 0);
                        respawn_time_counter[k] = 0;
                    }
                }
            }

            handAnalyzer.Dispose();
        }

        sm.ReleaseFrame();

        RotateCam();

        if (now_ihand != 0 && hand_detect == false)
        {
            hand_detect = true;
            bgm.Play();
        }

        //Debug.Log("detect: " + hand_detect);

        if ( getStoneCount() == 7 && check_Full_Stone_Time == 9999.0f)
        {
            check_Full_Stone_Time = Mathf.Min(check_Full_Stone_Time, timeSec);
            //Debug.Log("check_Full_Stone_Time: " + check_Full_Stone_Time);
            end_time = check_Full_Stone_Time;
            restart_time = check_Full_Stone_Time + 20.0f;
        }

        if (timeSec >= (end_time + 1) && timeSec < (end_time + 3))
        {
            transform.RotateAround(new Vector3(1, 1f, 90f), Vector3.right, 10 * Time.deltaTime);
        }
        if (timeSec >= (end_time + 3) && timeSec < (end_time + 5))
        {
            transform.Translate(Vector3.forward * 20 * Time.deltaTime);
        }
        if (timeSec >= restart_time)
        {
            SceneManager.LoadScene("Beach");
        }
    }

    //Close any ongoing Session
    void OnDisable()
    {
        if (smoother3D != null)
        {
            for (int i = 0; i < MaxHands; i++)
            {
                if (smoother3D[i] != null)
                {
                    for (int j = 0; j < MaxJoints; j++)
                    {
                        smoother3D[i][j].Dispose();
                        smoother3D[i][j] = null;
                    }
                }
            }
            smoother3D = null;
        }

        if (smoother != null)
        {
            smoother.Dispose();
            smoother = null;
        }

        if (sm != null)
        {
            sm.Close();
            sm.Dispose();
            sm = null;
        }
    }

    //Smoothen and Display the Joint Data
    void DisplayJoints()
    {
        hold_fingers = 0;
        hold_fingers_pose = 0;
        pop_5finger = true;
        handup = false;

        //detect the number of hands 
        now_ihand = 0;
		Is_twohand = false;
        if ( (jointData[0][1] != null && jointData[0][1].confidence == 100) || (jointData[0][10] != null && jointData[0][10].confidence == 100) || (jointData[0][3] != null && jointData[0][3].confidence == 100))
        {
            now_ihand = 1;
            
            if ((jointData[1][1] != null && jointData[1][1].confidence == 100) || (jointData[1][10] != null && jointData[1][10].confidence == 100) || (jointData[1][3] != null && jointData[1][3].confidence == 100))
            {
                now_ihand = 2;
                Is_twohand = true;
                PXCMPoint3DF32 smoothedPoint = smoother3D[0][1].SmoothValue(jointData[0][1].positionWorld);
                PXCMPoint3DF32 smoothedPoint2 = smoother3D[1][1].SmoothValue(jointData[1][1].positionWorld);
                if ( (-1 * smoothedPoint.x + 0.05f) < (-1 * smoothedPoint2.x + 0.05f) )
                    hand_right = false;
                else
                    hand_right = true;
            }
                
        }

        //detect change hand or not 
        if (now_ihand != past_ihand)
        {
            changehand = true;
            lefthandflag = true;
        }
        past_ihand = now_ihand;

        for (int i = 0; i < MaxHands; i++)
        {
            hold_stone = false;
            if (i == 0)
                ChangeVector = resA;
            else if (i == 1)
                ChangeVector = resB;

            for (int j = 0; j < MaxJoints; j++)
            {
                if (jointData[i][j] != null && jointData[i][j].confidence == 100)
                {
                    PXCMPoint3DF32 smoothedPoint = smoother3D[i][j].SmoothValue(jointData[i][j].positionWorld);
                    myJoints[i][j].SetActive(true);

                    //hand joints position
                    //myJoints[i][j].transform.position = new Vector3(-1 * smoothedPoint.x, smoothedPoint.y, smoothedPoint.z) * 100f;
                    myJoints[i][j].transform.position = new Vector3(-1 * smoothedPoint.x + 0.05f, smoothedPoint.y, -1 * smoothedPoint.z + 0.9f) * 100f;

                    /*if (j == 1)
                    {
                        Debug.Log(i + ", " + j + ", " + myJoints[i][j].transform.position);
                    }*/
                    if (j == 10)
                    {
                        v1 = new Vector3(-1 * smoothedPoint.x + 0.05f, smoothedPoint.y, -1 * smoothedPoint.z + 0.9f) * 100f;
                    }
                    else if (j == 0)
                    {
                        v2 = new Vector3(-1 * smoothedPoint.x + 0.05f, smoothedPoint.y, -1 * smoothedPoint.z + 0.9f) * 100f;
                    }
                    else if (j == 3)
                    {
                        v3 = new Vector3(-1 * smoothedPoint.x + 0.05f, smoothedPoint.y, -1 * smoothedPoint.z + 0.9f) * 100f;
                    }
                    else if (j == 1)
                    {
                        v4 = new Vector3(-1 * smoothedPoint.x + 0.05f, smoothedPoint.y, -1 * smoothedPoint.z + 0.9f) * 100f;
                    }
                    else if (j == 11)
                    {
                        v5 = new Vector3(-1 * smoothedPoint.x + 0.05f, smoothedPoint.y, -1 * smoothedPoint.z + 0.9f) * 100f;
                    }

                    if (myJoints[i][1].transform.position.y < -16)
                    {
                        jointData[i][j] = null;
                        continue;
                    }
                    
                    int hand_id = i;

                    dx5 = Math.Pow(myJoints[hand_id][1].transform.position.x - myJoints[hand_id][5].transform.position.x, 2);
                    dy5 = Math.Pow(myJoints[hand_id][1].transform.position.y - myJoints[hand_id][5].transform.position.y, 2);
                    dz5 = Math.Pow(myJoints[hand_id][1].transform.position.z - myJoints[hand_id][5].transform.position.z, 2);
                    jointDistance5 = Math.Sqrt(dx5 + dy5 + dz5);
                    jointDistance[0] = jointDistance5;

                    dx9 = Math.Pow(myJoints[hand_id][1].transform.position.x - myJoints[hand_id][9].transform.position.x, 2);
                    dy9 = Math.Pow(myJoints[hand_id][1].transform.position.y - myJoints[hand_id][9].transform.position.y, 2);
                    dz9 = Math.Pow(myJoints[hand_id][1].transform.position.z - myJoints[hand_id][9].transform.position.z, 2);
                    jointDistance9 = Math.Sqrt(dx9 + dy9 + dz9);
                    jointDistance[1] = jointDistance9;

                    dx13 = Math.Pow(myJoints[hand_id][1].transform.position.x - myJoints[hand_id][13].transform.position.x, 2);
                    dy13 = Math.Pow(myJoints[hand_id][1].transform.position.y - myJoints[hand_id][13].transform.position.y, 2);
                    dz13 = Math.Pow(myJoints[hand_id][1].transform.position.z - myJoints[hand_id][13].transform.position.z, 2);
                    jointDistance13 = Math.Sqrt(dx13 + dy13 + dz13);
                    jointDistance[2] = jointDistance13;

                    dx17 = Math.Pow(myJoints[hand_id][1].transform.position.x - myJoints[hand_id][17].transform.position.x, 2);
                    dy17 = Math.Pow(myJoints[hand_id][1].transform.position.y - myJoints[hand_id][17].transform.position.y, 2);
                    dz17 = Math.Pow(myJoints[hand_id][1].transform.position.z - myJoints[hand_id][17].transform.position.z, 2);
                    jointDistance17 = Math.Sqrt(dx17 + dy17 + dz17);
                    jointDistance[3] = jointDistance17;

                    dx21 = Math.Pow(myJoints[hand_id][1].transform.position.x - myJoints[hand_id][21].transform.position.x, 2);
                    dy21 = Math.Pow(myJoints[hand_id][1].transform.position.y - myJoints[hand_id][21].transform.position.y, 2);
                    dz21 = Math.Pow(myJoints[hand_id][1].transform.position.z - myJoints[hand_id][21].transform.position.z, 2);
                    jointDistance21 = Math.Sqrt(dx21 + dy21 + dz21);
                    jointDistance[4] = jointDistance21;

                    
                    for (int d = 0; d < 5; d++)
                    {
                        if (d == 0 && jointDistance[d] < 7)
                        {
                            fingers_hold[d] = true;
                        }
                        else if (d > 0 && d < 4 && jointDistance[d] < 7)
                        {
                            fingers_hold[d] = true;
                        }
                        else if (d == 4 && jointDistance[d] < 6)
                        {
                            fingers_hold[d] = true;
                        }

                        if (d == 0 && jointDistance[d] < 8)
                        {
                            hold_fingers_pose++;
                        }
                        else if (d > 0 && d < 4 && jointDistance[d] < 10)
                        {
                            hold_fingers_pose++;
                        }
                        else if (d == 4 && jointDistance[d] < 9)
                        {
                            hold_fingers_pose++;
                        }
                    }

                    animator[i].SetBool("OpenBool", false);
                    animator[i].SetBool("HoldBool", false);
                    animator[i].SetBool("ThumbBool", false);
                    animator[i].SetBool("ForefingerBool", false);
                    animator[i].SetBool("TwoBool", false);
                    animator[i].SetBool("ThreeBool", false);
                    animator[i].SetBool("FourBool", false);
                    animator[i].SetBool("ShootBool", false);
                    animator[i].SetBool("StoneBool", false);
                    pose = false;

                    if (!hold_stone)
                    {
                        int fingers_check = 0;
                        for (int d = 0; d < 5; d++)
                        {
                            if (fingers_hold[d])
                            {
                                fingers_check++;
                            }
                        }

                        if (fingers_check == 5 && !pose)
                        {
                            animator[i].SetBool("StoneBool", true);
                            pose = true;
                        }
                        else if (fingers_check == 4 && !pose)
                        {
                            if (!fingers_hold[0])
                            {
                                animator[i].SetBool("ThumbBool", true);
                            }
                            else if (!fingers_hold[1])
                            {
                                animator[i].SetBool("ForefingerBool", true);
                            }
                            pose = true;
                        }
                        else if (fingers_check == 3 && !pose)
                        {
                            if (!fingers_hold[1] && !fingers_hold[2])
                            {
                                animator[i].SetBool("TwoBool", true);
                            }
                            else if (!fingers_hold[0] && !fingers_hold[1])
                            {
                                animator[i].SetBool("ShootBool", true);
                            }
                            pose = true;
                        }
                        else if (fingers_check == 2 && !pose)
                        {
                            if (!fingers_hold[1] && !fingers_hold[2] && !fingers_hold[3])
                            {
                                animator[i].SetBool("ThreeBool", true);
                            }
                            pose = true;
                        }
                        else if (fingers_check == 1 && !pose)
                        {
                            if (fingers_hold[0])
                            {
                                animator[i].SetBool("FourBool", true);
                            }
                            pose = true;
                        }
                    }

                    if (!pose)
                    {
                        if (hold_fingers_pose > 2)
                        {
                            animator[i].SetBool("HoldBool", true);
                        }
                        else
                        {
                            animator[i].SetBool("OpenBool", true);
                        }
                    }

                    if (timeSec >= opening_time && timeSec <= end_time && cancatch == true)
                    {
                        for (int n = 0; n < stones_max; n++)
                        {
                            if (!hold_stone)
                            {
                                //hand point and stone distance
                                dxObj = Math.Pow(myJoints[hand_id][1].transform.position.x + (ChangeVector.x * 0.8) - stones[n].transform.position.x, 2);
                                dyObj = Math.Pow(myJoints[hand_id][1].transform.position.y + (ChangeVector.y * 0.8) - stones[n].transform.position.y, 2);
                                dzObj = Math.Pow(myJoints[hand_id][1].transform.position.z + (ChangeVector.z * 0.8) - stones[n].transform.position.z, 2);
                                objDistance = Math.Sqrt(dxObj + dyObj + dzObj);

                                for (int d = 0; d < 5; d++)
                                {
                                    if (objDistance < 9)
                                    {
                                        if (d == 0 && jointDistance[d] < 8)
                                        {
                                            hold_fingers++;
                                        }
                                        else if (d > 0 && d < 4 && jointDistance[d] < 10)
                                        {
                                            hold_fingers++;
                                        }
                                        else if (d == 4 && jointDistance[d] < 9)
                                        {
                                            hold_fingers++;
                                        }
                                    }
                                }

                                Collider stone_collider;
                                Vector3 check_Point;
                                bool point_in_stone = false;

                                //stones position
                                if (!stones_locked[n] && hold_fingers > 2 && objDistance < 9 && i == hand_id && j == 1)
                                {
                                    check_Point = new Vector3(-1 * smoothedPoint.x + 0.05f, smoothedPoint.y, -1 * smoothedPoint.z + 0.9f) * 100f + ChangeVector;
                                    for (int k = 0; k < stones_max; k++)
                                    {
                                        if (k != n)
                                        {
                                            stone_collider = stones[k].GetComponent<Collider>();
                                            if (stone_collider.bounds.Contains(check_Point))
                                            {
                                                point_in_stone = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (!point_in_stone)
                                    {
                                        stones[n].transform.position = check_Point;
                                        hold_stone = true;
                                        pop_5finger = false;
                                        hold_stone_trigger[i] = true;
                                    }
                                    break;
                                }
                            }

                            int s = n;
                            stone_x = stones[s].transform.position.x;
                            stone_y = stones[s].transform.position.y;
                            stone_z = stones[s].transform.position.z;

                            /*
                            //stone place detection pattern 1
                            for (int p = 0; p < stones_max; p++)
                            {
                                if (p == 6 && (!places_locked[0] || !places_locked[2]))
                                {
                                    continue;
                                }
                                else if (p == 5 && (!places_locked[0] || !places_locked[1] || !places_locked[2] || !places_locked[3]))
                                {
                                    continue;
                                }
                                else if (p == 4 && (!places_locked[1] || !places_locked[3]))
                                {
                                    continue;
                                }

                                if (!places_locked[p] && (stone_x > place[p][0] && stone_x < place[p][1]) && (stone_y > place[p][2] && stone_y < place[p][3]) && (stone_z > place[p][4] && stone_z < place[p][5]))
                                {
                                    stones[s].transform.position = new Vector3((float)place[p][6], (float)place[p][7], (float)place[p][8]);
                                    stones[s].transform.rotation = Quaternion.Euler(0, 0, 0);
                                    Destroy(stones[s].GetComponent<Rigidbody>());
                                    stones_locked[s] = true;
                                    places_locked[p] = true;
                                    break;
                                }
                            }
                            */

                            //stone place detection pattern 2
                            if (stone_p < 4 && !places_locked[stone_p] && (stone_x > -5.5 && stone_x < 18.0) && (stone_y > -5.5 && stone_y < 1.5) && (stone_z > 42.0 && stone_z < 55.0))
                            {
                                stones[s].transform.position = new Vector3((float)place[stone_p][6], (float)place[stone_p][7], (float)place[stone_p][8]);
                                stones[s].transform.rotation = Quaternion.Euler(0, 0, 0);
                                Destroy(stones[s].GetComponent<Rigidbody>());
                                stones_locked[s] = true;
                                places_locked[stone_p] = true;
                                stone_p = stone_p + 1;
                                stone_sound.Play();
                            }
                            else if (stone_p >= 4)
                            {
                                for (int p = stone_p; p < stones_max; p++)
                                {
                                    if (p == 4 && (!places_locked[1] || !places_locked[3]))
                                    {
                                        continue;
                                    }
                                    if (p == 6 && (!places_locked[0] || !places_locked[2]))
                                    {
                                        continue;
                                    }
                                    if (p == 5 && (!places_locked[0] || !places_locked[1] || !places_locked[2] || !places_locked[3]))
                                    {
                                        continue;
                                    }

                                    if (!places_locked[p] && (stone_x > place[p][0] && stone_x < place[p][1]) && (stone_y > place[p][2] && stone_y < place[p][3]) && (stone_z > place[p][4] && stone_z < place[p][5]))
                                    {
                                        stones[s].transform.position = new Vector3((float)place[p][6], (float)place[p][7], (float)place[p][8]);
                                        stones[s].transform.rotation = Quaternion.Euler(0, 0, 0);
                                        Destroy(stones[s].GetComponent<Rigidbody>());
                                        stones_locked[s] = true;
                                        places_locked[p] = true;
                                        stone_sound.Play();
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    for (int d = 0; d < 5; d++)
                    {
                        fingers_hold[d] = false;
                    }

                    hold_fingers = 0;
                    hold_fingers_pose = 0;
                    jointData[i][j] = null;
                }
                else
                {
                    myJoints[i][j].SetActive(false);
                }
            }


            if (Is_twohand == false && i == 0)
            {
                //change hand initial
                if (changehand == true)
                {
                    init_hand_rotate();
                    init_rightleft();

                    changehand = false;
                    hand_right = true;
                    flamecount = 0;
                }
                flamecount = flamecount + 1;
                //detect left hand, then flip the right hand prefab into left hand
                if (myJoints[i][3].transform.position.x > myJoints[i][1].transform.position.x && lefthandflag == true && flamecount == 10)
                {
                    myJoints[i][1].transform.localScale = new Vector3(-1 * myJoints[i][1].transform.localScale.x, myJoints[i][1].transform.localScale.y, myJoints[i][1].transform.localScale.z);
                    lefthandflag = false;
                    hand_right = false;
                }
                if(flamecount >5)
                    cancatch = true;
                if (hold_stone == false)
                {
                    Update_hand_roatation_XYZ(2, past_x[2], past_y[2], past_z[2], hand_right);
                }
            }
            else if (Is_twohand == true)
            {
                //change hand initial
                if (changehand == true)
                {
                    init_hand_rotate();
                    init_rightleft();
                    changehand = false;
                    flamecount = 0;
                }
                flamecount = flamecount + 1;
                //detect left hand, then flip the right hand prefab into left hand
                if (hand_right == false && lefthandflag == true && flamecount > 10)
                {
                    myJoints[i][1].transform.localScale = new Vector3(-1 * myJoints[i][1].transform.localScale.x, myJoints[i][1].transform.localScale.y, myJoints[i][1].transform.localScale.z);
                    lefthandflag = false;
                
                }
                if (flamecount > 5)
                    cancatch = true;
                if (hold_stone == false)
                {
                    Update_hand_roatation_XYZ(i, past_x[i], past_y[i], past_z[i], hand_right);
                }
                hand_right = !hand_right;
            }
            // Hint handup
            if (v4.y <= -14)
            {
                handup = true;
            }
            if (now_ihand == 0)
            {
                handup = false;
            }

        }
        //Debug.Log("Object to Hand: " + objDistance);
        //Debug.Log("Fingers to Center: " + jointDistance5 + ", " + jointDistance9 + ", " + jointDistance13 + ", " + jointDistance17 + ", " + jointDistance21);
        //Debug.Log(hold_stone);
        //Debug.Log(pop_5finger);
        //Debug.Log(v4);
        //Debug.Log(handup);
        for (int i = 0; i < MaxHands; i++)
            for (int j = 0; j < MaxJoints; j++)
            {
                if (j != 21 && j != 0 && j != 1 && j != 5 && j != 9 && j != 13 && j != 17)
                    UpdateBoneTransform(myBones[i][j], myJoints[i][j], myJoints[i][j + 1]);

                UpdateBoneTransform(myBones[i][21], myJoints[i][0], myJoints[i][2]);
                UpdateBoneTransform(myBones[i][17], myJoints[i][0], myJoints[i][18]);

                UpdateBoneTransform(myBones[i][5], myJoints[i][14], myJoints[i][18]);
                UpdateBoneTransform(myBones[i][9], myJoints[i][10], myJoints[i][14]);
                UpdateBoneTransform(myBones[i][13], myJoints[i][6], myJoints[i][10]);
                UpdateBoneTransform(myBones[i][0], myJoints[i][2], myJoints[i][6]);
            }
        if (places_locked[0]==true && places_locked[1] == true && places_locked[2] == true&& places_locked[3] == true)
        {
            UP.SetActive(true);
        }
    }

    //Update Bones
    void UpdateBoneTransform(GameObject _bone, GameObject _prevJoint, GameObject _nextJoint)
    {

        if (_prevJoint.activeSelf == false || _nextJoint.activeSelf == false)
            _bone.SetActive(false);
        else
        {
            _bone.SetActive(true);

            // Update Position
            _bone.transform.position = ((_nextJoint.transform.position - _prevJoint.transform.position) / 2f) + _prevJoint.transform.position;

            // Update Scale
            _bone.transform.localScale = new Vector3(0.8f, (_nextJoint.transform.position - _prevJoint.transform.position).magnitude - (_prevJoint.transform.position - _nextJoint.transform.position).magnitude / 2f, 0.8f);

            // Update Rotation
            _bone.transform.rotation = Quaternion.FromToRotation(Vector3.up, _nextJoint.transform.position - _prevJoint.transform.position);
        }

        _bone.GetComponent<Renderer>().enabled = false;

    }

    //Key inputs to rotate camera and restart
    void RotateCam()
    {
        Vector3 _RotateAround = camera.transform.position;

        if (_RotateAround != Vector3.zero)
        {
            if (Input.GetKey(KeyCode.RightArrow))
                transform.RotateAround(_RotateAround, Vector3.up, 20 * Time.deltaTime);

            if (Input.GetKey(KeyCode.LeftArrow))
                transform.RotateAround(_RotateAround, Vector3.up, -20 * Time.deltaTime);

            if (Input.GetKey(KeyCode.UpArrow))
                transform.RotateAround(_RotateAround, Vector3.right, 20 * Time.deltaTime);

            if (Input.GetKey(KeyCode.DownArrow))
                transform.RotateAround(_RotateAround, Vector3.right, -20 * Time.deltaTime);
            
            if (Input.GetKey(KeyCode.E))
                transform.Translate(Vector3.up * 20 * Time.deltaTime);

            if (Input.GetKey(KeyCode.C))
                transform.Translate(Vector3.up * -20 * Time.deltaTime);

            if (Input.GetKey(KeyCode.W))
                transform.Translate(Vector3.forward * 20 * Time.deltaTime);

            if (Input.GetKey(KeyCode.S))
                transform.Translate(Vector3.forward * -20 * Time.deltaTime);

            if (Input.GetKey(KeyCode.A))
                transform.Translate(Vector3.right * -20 * Time.deltaTime);

            if (Input.GetKey(KeyCode.D))
                transform.Translate(Vector3.right * 20 * Time.deltaTime);
        }

        /* Restart the Level/Refresh Scene */
        if (Input.GetKeyDown(KeyCode.R))
        {
            //Application.LoadLevel("Beach");
            SceneManager.LoadScene("Beach");
        }

        /* Quit the Application */
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Application.Quit();
        }
    }

    //Display Gestures
    void DisplayGestures(PXCMHandData.GestureData gestureData)
    {
        if (handList.ContainsKey(gestureData.handId))
        {
            switch ((PXCMHandData.BodySideType)handList[gestureData.handId])
            {
                case PXCMHandData.BodySideType.BODY_SIDE_LEFT:
                    myTextLeft.text = gestureData.name.ToString();
                    break;
                case PXCMHandData.BodySideType.BODY_SIDE_RIGHT:
                    myTextRight.text = gestureData.name.ToString();
                    break;
                default:
                    break;
            }
        }
    }

    //Process Alerts to keep track of hands for Gesture Display
    void ProcessAlerts(PXCMHandData.AlertData alertData)
    {

        if (handList.ContainsKey(alertData.handId))
        {
            switch ((PXCMHandData.BodySideType)handList[alertData.handId])
            {
                case PXCMHandData.BodySideType.BODY_SIDE_LEFT:
                    myTextLeft.text = "";
                    break;
                case PXCMHandData.BodySideType.BODY_SIDE_RIGHT:
                    myTextRight.text = "";
                    break;
                default:
                    break;
            }
        }

    }

    //Populate bones and joints gameobjects
    void InitializeGameobjects()
    {
        stones = new GameObject[stones_max];
        stone_p = 0;
        for (int n = 0; n < stones_max; n++)
        {
            //stones[n] = (GameObject)Instantiate(StonePrefab, Vector3.zero, Quaternion.identity);
            stones[n] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(stones[n].GetComponent<SphereCollider>());
            stones[n].AddComponent(typeof(CapsuleCollider));
            CapsuleCollider cc = stones[n].GetComponent<CapsuleCollider>();
            cc.radius = 0.45f;
            cc.direction = 0;
            stones[n].AddComponent(typeof(Rigidbody));
            Rigidbody rb = stones[n].GetComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.mass = 1000;
            MeshRenderer mr = stones[n].GetComponent<MeshRenderer>();
            mr.material = StoneMaterial;
            if (n % 2 == 1)
            {
                stones[n].transform.position = new Vector3( n * 7.0f - 15.0f, 7.0f, 25.0f);
            }
            else
            {
                stones[n].transform.position = new Vector3( n * 7.0f - 20.0f, 7.0f, 33.0f);
            }
            stones[n].transform.localScale = new Vector3(9, 6, 7);
        }
        hold_stone_trigger = new Boolean[4] { false, false, false, false };
        handup = false;
        past_x = new float[3] { 0, 0 ,0};
        past_y = new float[3] { 0, 0, 0};
        past_z = new float[3] { 0, 0, 0};
        resA = new Vector3(0, -1, 0);
        resB = new Vector3(0, -1, 0);
        flag_x = new int[2] { 0, 0 };
        flag_y = new int[2] { 0, 0 };
        flag_z = new int[2] { 0, 0 };
        respawn_time_counter = new int[stones_max];
        stones_locked = new Boolean[stones_max];
        places_locked = new Boolean[stones_max];
        place = new Double[stones_max][];
        place[0] = new Double[9] { 0, 0, 0, 0, 0, 0, 0, -5.5, 52 };
        place[1] = new Double[9] { 0, 0, 0, 0, 0, 0, 9, -5.5, 52.5 };
        place[2] = new Double[9] { 0, 0, 0, 0, 0, 0, 3, -5.5, 45 };
        place[3] = new Double[9] { 0, 0, 0, 0, 0, 0, 12.2, -5.5, 44.5 };
        place[4] = new Double[9] { 0, 0, 0, 0, 0, 0, 15.5, -1.5, 48.6 };
        place[5] = new Double[9] { 0, 0, 0, 0, 0, 0, 6.5, -1.5, 48.5 };
        place[6] = new Double[9] { 0, 0, 0, 0, 0, 0, -2.5, -1.5, 49 };
        Double place_bound = 4;
        DOWN.SetActive(true);
        UP.SetActive(false);
        for (int s = 0; s < stones_max; s++)
        {
            place[s][0] = place[s][6] - place_bound;
            place[s][1] = place[s][6] + place_bound;
            place[s][2] = place[s][7] - place_bound;
            place[s][3] = place[s][7] + place_bound;
            place[s][4] = place[s][8] - place_bound;
            place[s][5] = place[s][8] + place_bound;
        }
        jointDistance = new Double[5];

        myJoints = new GameObject[MaxHands][];
        myBones = new GameObject[MaxHands][];
        jointData = new PXCMHandData.JointData[MaxHands][];
        animator = new Animator[2];
        fingers_hold = new bool[5];
        for (int i = 0; i < MaxHands; i++)
        {
            myJoints[i] = new GameObject[MaxJoints];
            myBones[i] = new GameObject[MaxJoints];
            smoother3D[i] = new PXCMSmoother.Smoother3D[MaxJoints];
            jointData[i] = new PXCMHandData.JointData[MaxJoints];
        }

        for (int i = 0; i < MaxHands; i++)
        {
            for (int j = 0; j < MaxJoints; j++)
            {
                smoother3D[i][j] = smoother.Create3DWeighted(weightsNum);
                jointData[i][j] = new PXCMHandData.JointData();

                if (j == 1)
                {
                    //myJoints[i][j] = (GameObject)Instantiate(PalmCenterPrefab, Vector3.zero, Quaternion.identity);
                    myJoints[i][j] = (GameObject)Instantiate(HandPrefab_R, Vector3.zero, Quaternion.identity);
                    animator[i] = myJoints[i][j].GetComponent<Animator>();
                }
                else if (j == 21 || j == 17 || j == 13 || j == 9 || j == 5)
                {
                    myJoints[i][j] = (GameObject)Instantiate(TipPrefab, Vector3.zero, Quaternion.identity);
                    myJoints[i][j].GetComponent<ParticleSystem>().enableEmission = false;
                    //myJoints[i][j].transform.localScale = new Vector3(0, 0, 0);
                }
                else
                {
                    myJoints[i][j] = (GameObject)Instantiate(JointPrefab, Vector3.zero, Quaternion.identity);
                }

                if (j != 1)
                {
                    myBones[i][j] = (GameObject)Instantiate(BonePrefab, Vector3.zero, Quaternion.identity);
                    myJoints[i][j].GetComponent<Renderer>().enabled = false;
                    //myJoints[i][j].transform.localScale = new Vector3(0, 0, 0);
                }

            }
        }

    }
    public int getStoneCount()
    {
        int a = 0;
        for (int p = 0; p < stones_max; p++)
        {
            if (places_locked[p] == true)
                a = a + 1;
        }
        return a;
    }
    public float getCheck_Full_Stone_Time()
    {
        return check_Full_Stone_Time;
    }
    float Update_hand_roatation_x(int i, float past)
    {
        // J10 = (x1,y1,z1)   J0 = (x2,y2,x2)  J11 = (x5,y5,z5)
        a = Math.Sqrt(Math.Pow(v1.x - v2.x, 2) + Math.Pow(v1.z - v2.z, 2));
        b = v2.y - v1.y;

        res = (Math.Atan(b / a) / Math.PI) * 180;
        res2 = Convert.ToSingle(res);
        //myJoints[i][1].transform.eulerAngles = new Vector3(res2, 0f, 0f);

        // Vector3.forward   (0,0,1)
        if (  Vector3.Dot(Vector3.forward, (v1 - v2)) < 0  )
        {
            if (res2 >= 0)
            {
                //res2 = 180 - res2 ;
                res2 = 90 ;
            }
            else if (res2 < 0)
            {
                //res2 = -180 - res2 ;
                res2 = -90;
            }
        }
        if( Math.Abs(v1.y - v2.y) < 1.5)
        {
            res2 = 0;
        }
        else if (Math.Abs(v1.y - v2.y) <= 2.2)
        {
            if ((v1.y - v2.y) > 0)
                res2 = -5;
            else
                res2 = 5;
        }
        else if (Math.Abs(v1.y - v2.y) <= 3)
        {
            if ((v1.y - v2.y) > 0)
                res2 = -10;
            else
                res2 = 10;
        }
        else if (Math.Abs(v1.y - v2.y) <= 3.5)
        {
            if ((v1.y - v2.y) > 0)
                res2 = -15;
            else
                res2 = 15;
        }


        if (Math.Abs(past - res2) < 30.0f)
        {
            flag_x[i] = 0;
            return res2;
        }
        else if (flag_x[i] < flag_rotation_num)
        {
            flag_x[i] = flag_x[i] + 1;
            return past;
        }
        else if (flag_x[i] >= flag_rotation_num)
        {
            flag_x[i] = 0;
            return res2;
        }
        return res2;
    }
    float Update_hand_roatation_y(int i, float past)
    {
        // J10 = (x1,y1,z1)   J0 = (x2,y2,x2)  
        a = v1.z - v2.z;
        b = v1.x - v2.x;

        res = (Math.Atan(b / a) / Math.PI) * 180;
        res2 = Convert.ToSingle(res);
        //myJoints[i][1].transform.eulerAngles = new Vector3(0f, res2, 0f);

        if (res2 >= 45)
        {
            res2 = 45;
        }
        else if (res2 <= -45)
        {
            res2 = -45;
        }





        if (Math.Abs(past - res2) < 35.0f)
        {
            flag_y[i] = 0;
            return res2;
        }
        else if (flag_y[i] < flag_rotation_num)
        {
            flag_y[i] = flag_y[i] + 1;
            return past;
        }
        else if (flag_y[i] >= flag_rotation_num)
        {
            flag_y[i] = 0;
            return res2;
        }
        return res2;
    }
    float Update_hand_roatation_z(int i, float past, bool right)
    {
        float zdisa, zdisb, zdisc, zans;
        zdisa = (v3 - v4).magnitude;
        z_orig = new Vector3(1, 0, 0);
        if (i == 0)
        {
            z_now = rotation1 * z_orig;
            if (Vector3.Dot(z_now, (v3 - v4)) <= 0)
            {
                z_now = rotation1 * new Vector3(-1, 0, 0);
            }
        }
        else if (i == 1)
        {
            z_now = rotation2 * z_orig;
            if (Vector3.Dot(z_now, (v3 - v4)) <= 0)
            {
                z_now = rotation2 * new Vector3(-1, 0, 0);
            }
        }
        zdisb = z_now.magnitude;
        zdisc = ((v3 - v4) - z_now).magnitude;
        zans = ((zdisa * zdisa) + (zdisb * zdisb) - (zdisc * zdisc)) / (2 * (zdisa * zdisb));
        res = (Math.Acos(zans) / Math.PI) * 180;
        res2 = Convert.ToSingle(res);
        if (Vector3.Dot(resA, (v3 - v4)) > 0 && i==0)
            res2 = -1 * res2;
        if (Vector3.Dot(resB, (v3 - v4)) > 0 && i == 1)
            res2 = -1 * res2;
        if (right == true)
            res2 = -1 * res2;
        //myJoints[i][1].transform.eulerAngles = new Vector3(past_x[i], past_y[i], res2);


        if (res2 >= 20)
        {
            res2 = 20;
        }
        else if (res2 <= -20)
        {
            res2 = -20;
        }


        if (Math.Abs(past - res2) < 15.0f)
        {
            flag_z[i] = 0;
            return res2;
        }
        else if (flag_z[i] < flag_rotation_num)
        {
            flag_z[i] = flag_z[i] + 1;
            return past;
        }
        else if (flag_z[i] >= flag_rotation_num)
        {
            flag_z[i] = 0;
            return res2;
        }
        return res2;
    }
    void Update_hand_roatation_XYZ(int i, float x, float y, float z, bool right)
    {
        if (i == 2)
        {
            i = 0;
            past_x[2] = Update_hand_roatation_x(i, x);
            past_y[2] = Update_hand_roatation_y(i, y);
            CatchVector(2, 1);
            past_z[2] = Update_hand_roatation_z(i, z, right);
            if (past_x[2] <= -85)
            {
                past_y[2] = 0;
                past_z[2] = 0;
                flag_y[0] = flag_rotation_num - 5;
                flag_z[0] = flag_rotation_num - 5;
            }
            myJoints[i][1].transform.eulerAngles = new Vector3(past_x[2], past_y[2], past_z[2]);
            //myJoints[i][1].transform.eulerAngles = new Vector3(past_x[2], past_y[2], 0);
            CatchVector(2, 2);
        }
        else
        {
            past_x[i] = Update_hand_roatation_x(i, x);
            past_y[i] = Update_hand_roatation_y(i, y);
            //myJoints[i][1].transform.eulerAngles = new Vector3(past_x[i], past_y[i], 0);
            CatchVector(i, 1);
            //Debug.Log(resA);
            past_z[i] = Update_hand_roatation_z(i, z, right);
            if (past_x[i] <= -85)
            {
                past_y[i] = 0;
                past_z[i] = 0;
                flag_y[i] = flag_rotation_num - 5;
                flag_z[i] = flag_rotation_num - 5;
            }
            myJoints[i][1].transform.eulerAngles = new Vector3(past_x[i], past_y[i], past_z[i]);
            //myJoints[i][1].transform.eulerAngles = new Vector3(past_x[i], past_y[i], 0);
            CatchVector(i, 2);
            //Debug.Log(resA);
        }
    }
    void CatchVector(int i, int time)
    {
        if (time == 1)
        {
            if (i == 0)
            {
                //Origin1 = new Vector3(0, -3, (float)8.5);
                Origin1 = new Vector3(0, -1, 0);
                rotation1 = Quaternion.Euler(past_x[0], past_y[0], 0);
                resA = rotation1 * Origin1;
            }
            else if (i == 1)
            {
                Origin2 = new Vector3(0, -1, 0);
                rotation2 = Quaternion.Euler(past_x[1], past_y[1], 0);
                resB = rotation2 * Origin2;
            }
            else if (i == 2)
            {
                Origin2 = new Vector3(0, -1, 0);
                rotation2 = Quaternion.Euler(past_x[2], past_y[2], 0);
                resA = rotation2 * Origin2;
            }
        }
        else if (time == 2)
        {
            if (i == 0)
            {
                Origin1 = new Vector3(0, -3, (float)8.5);
                rotation1 = Quaternion.Euler(past_x[0], past_y[0], past_z[0]);
                resA = rotation1 * Origin1;
            }
            else if (i == 1)
            {
                Origin2 = new Vector3(0, -3, (float)8.5);
                rotation2 = Quaternion.Euler(past_x[1], past_y[1], past_z[1]);
                resB = rotation2 * Origin2;
            }
            else if (i == 2)
            {
                Origin2 = new Vector3(0, -3, (float)8.5);
                rotation2 = Quaternion.Euler(past_x[2], past_y[2], past_z[2]);
                resA = rotation2 * Origin2;
            }
        }
    }
    void init_hand_rotate()
    {
        past_x = new float[3] { 0, 0, 0 };
        past_y = new float[3] { 0, 0, 0 };
        past_z = new float[3] { 0, 0, 0 };
        flag_x = new int[2] { 0, 0 };
        flag_y = new int[2] { 0, 0 };
        flag_z = new int[2] { 0, 0 };
        //resA = new Vector3(0, -3, (float)8.5);
        //resB = new Vector3(0, -3, (float)8.5);
        resB = resA;
        cancatch = false;
    }
    void init_rightleft()
    {
        myJoints[0][1].SetActive(false);
        myJoints[1][1].SetActive(false);
        myJoints[0][1] = (GameObject)Instantiate(HandPrefab_R, -50 * Vector3.up, Quaternion.identity);
        animator[0] = myJoints[0][1].GetComponent<Animator>();
        myJoints[1][1] = (GameObject)Instantiate(HandPrefab_R, -50 * Vector3.up, Quaternion.identity);
        animator[1] = myJoints[1][1].GetComponent<Animator>();
        myJoints[0][1].SetActive(true);
        myJoints[1][1].SetActive(true);
    }
	public bool Get_Twohand()
    {
        return Is_twohand;
    }
    public bool Get_5finger()
    {
        if (now_ihand == 0)
        {
            return false;
        }
        return pop_5finger;
    }
    public bool Get_handup()
    {
        return handup;
    }
    public bool Get_hand_detect()
    {
        return hand_detect;
    }
}
