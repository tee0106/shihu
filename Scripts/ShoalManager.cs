using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoalManager : MonoBehaviour {

	public Terrain terrain;
	public GameObject fishPrefab;
	public Vector3 goal1;
	public Vector3 goal2;
	public Vector3 goalActual;
	public int waterLevel;
	public Vector3 swimLimits = new Vector3(5,5,5);
	private float timer = 5;
	public int numFish = 1;
	public GameObject[] allFish;
	[Header("Fish Settings")]
	[Range(0.0f, 5.0f)]
	public float minSpeed; //[Range(0.0f,5.0f)]
	[Range(0.0f, 5.0f)]
	public float maxSpeed; //[Range(0.0f,5.0f)]
	public static float terrainOffset;
	private float timeSec;
	private float time_Min_Up = 60.0f; //time minimum fish swim up when game failed
	private float time_Max_Up = 65.0f; //time maximum fish swim up when game failed
	private float opening_time = 21.5f;
	private float up_value = 0.0f;
	//private bool up = false;
	TimeClock timeClock;
	HandsViewer hand;
	private int stone_count;

	// Use this for initialization
	void Start () {
		terrainOffset = terrain.transform.position.y;
		allFish = new GameObject[numFish];
		for(int i =0; i < numFish; i++)
		{
			Vector3 pos = this.transform.position + new Vector3(Random.Range(-swimLimits.x,swimLimits.x),
																Random.Range(-swimLimits.y,swimLimits.y),
																Random.Range(-swimLimits.z,swimLimits.z));
			if (pos.y < (Terrain.activeTerrain.SampleHeight(pos)+terrainOffset))
			{
				pos.y = Terrain.activeTerrain.SampleHeight(pos)+terrainOffset;
			}
			if (pos.y > waterLevel)
			{
				pos.y = waterLevel;
			}

			allFish[i] = (GameObject) Instantiate (fishPrefab,pos,Quaternion.identity);
			allFish[i].GetComponent<Shoal>().manager = this;
		}
		timeClock = GameObject.Find("TimeText").GetComponent<TimeClock>();
		hand = GameObject.Find("Camera").GetComponent<HandsViewer>();

		time_Min_Up = time_Min_Up + opening_time;
		time_Max_Up = time_Max_Up + opening_time;
	}
	
	// Update is called once per frame
	void Update () {
		timer -= Time.deltaTime;
		timeSec = timeClock.getTime();
		stone_count = hand.getStoneCount();
		//Debug.Log(timeSec);

		//fish swim to goalActual
		if (stone_count < 7 && timeSec >= (time_Min_Up - 10.0f) && timeSec < time_Min_Up)
        {
			goalActual = this.transform.position + new Vector3(15.0f, Random.Range(terrainOffset, waterLevel), -15.0f);
		}
		else if (stone_count < 7 && timeSec >= time_Min_Up && timeSec < time_Max_Up)
		{
			//fish swim up
			up_value = 7.0f / (time_Max_Up - time_Min_Up) / 35.0f;
			for (int i = 0; i < numFish; i++)
			{
				allFish[i].transform.position += new Vector3(0.0f, up_value, 0.0f);
			}

			goalActual = this.transform.position + new Vector3(15.0f, Random.Range(terrainOffset, waterLevel), -70.0f);
		}
		else if (stone_count < 7 && timeSec >= time_Max_Up)
		{
			goalActual = this.transform.position + new Vector3(Random.Range(-swimLimits.x, swimLimits.x), Random.Range(terrainOffset, waterLevel), -70.0f);
		}
		else
        {
			if (timer <= 0)
			{
				NewGoal();

				timer = (Random.Range(4, 7));

				if (Random.Range(0, 3) == 0)
				{
					goalActual = goal1;
				}
				else
				{
					goalActual = goal2;
				}
			}
		}
	}

	void NewGoal()
	{
		goal1 = this.transform.position + new Vector3(Random.Range(-swimLimits.x,swimLimits.x),Random.Range(terrainOffset,waterLevel), Random.Range(-swimLimits.z,swimLimits.z));															
		goal2 = this.transform.position + new Vector3(Random.Range(-swimLimits.x,swimLimits.x),Random.Range(terrainOffset,waterLevel), Random.Range(-swimLimits.z,swimLimits.z));	
	}

	public void DisfishC()
    {
		for (int i = 0; i < numFish; i++)
        {
			allFish[i].SetActiveRecursively(false);
		}
	}

}
