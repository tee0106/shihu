using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoalManager3 : MonoBehaviour
{
	public Terrain terrain;
	public GameObject fishPrefab;
	public Vector3 goal1;
	public Vector3 goal2;
	public Vector3 goalActual;
	public int waterLevel;
	public Vector3 swimLimits = new Vector3(5, 5, 5);
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
	//private float time_Min_Up = 180.0f; //time minimum fish swim up when game failed
	//private float time_Max_Up = 185.0f; //time maximum fish swim up when game failed
	//private float up_value = 0.0f;
	//private bool up = false;
	TimeClock timeClock;
	HandsViewer hand;
	private int stone_count;

	// Start is called before the first frame update
	void Start()
	{
		terrainOffset = terrain.transform.position.y;
		allFish = new GameObject[numFish];
		for (int i = 0; i < numFish; i++)
		{
			Vector3 pos = this.transform.position + new Vector3(Random.Range(-swimLimits.x, swimLimits.x),
																Random.Range(-swimLimits.y, swimLimits.y),
																Random.Range(-swimLimits.z, swimLimits.z));
			if (pos.y < (Terrain.activeTerrain.SampleHeight(pos) + terrainOffset))
			{
				pos.y = Terrain.activeTerrain.SampleHeight(pos) + terrainOffset;
			}
			if (pos.y > waterLevel)
			{
				pos.y = waterLevel;
			}

			allFish[i] = (GameObject)Instantiate(fishPrefab, pos, Quaternion.identity);
			allFish[i].GetComponent<ShoalThree>().manager = this;
		}
		timeClock = GameObject.Find("TimeText").GetComponent<TimeClock>();
		hand = GameObject.Find("Camera").GetComponent<HandsViewer>();
	}

	// Update is called once per frame
	void Update()
	{
		timer -= Time.deltaTime;

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

	void NewGoal()
	{
		goal1 = this.transform.position + new Vector3(Random.Range(-swimLimits.x, swimLimits.x), Random.Range(terrainOffset, waterLevel), Random.Range(-swimLimits.z, swimLimits.z));
		goal2 = this.transform.position + new Vector3(Random.Range(-swimLimits.x, swimLimits.x), Random.Range(terrainOffset, waterLevel), Random.Range(-swimLimits.z, swimLimits.z));
	}

	public void DisfishC()
	{
		for (int i = 0; i < numFish; i++)
		{
			allFish[i].SetActiveRecursively(false);
		}
	}
}
