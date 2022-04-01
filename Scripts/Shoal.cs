﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoal : MonoBehaviour {

	public float speed;
	public ShoalManager manager;
	public int rotSpeed = 1;
	private int water;
	private GameObject fish;

	// Use this for initialization
	void Start () {
		water = manager.waterLevel;
		speed = Random.Range(manager.minSpeed,manager.maxSpeed);
		fish = GameObject.Find("Fish");
		fish.SetActiveRecursively(true);
	}

	// Update is called once per frame
	void Update() {
		//print(Terrain.activeTerrain.SampleHeight(transform.position));

		GameObject[] gos;
		gos = manager.allFish;
		Vector3 vavoid = Vector3.zero;
		float nDistance;



		foreach (GameObject go in gos)
		{
			if (go != this.gameObject)
			{
				nDistance = Vector3.Distance(go.transform.position, this.transform.position);

				if (nDistance < 1.0f)
				{
					vavoid = vavoid + (this.transform.position - go.transform.position);
				}
			}
		}

		Vector3 direction = manager.goalActual + vavoid - this.transform.position;
		if (transform.position.y < Terrain.activeTerrain.SampleHeight(transform.position) + ShoalManager.terrainOffset + 2)
		{

			direction.y = 1;
		}
		if (transform.position.y > water - 2)
		{

			direction.y = -1;
		}

		//speed = Random.Range(5.0f, 10.0f);
		
		if (Random.Range(0,1000)<10)
		{
			speed = Random.Range(manager.minSpeed,manager.maxSpeed);
		}
		

		transform.Translate(0,0,Time.deltaTime*speed);
		transform.rotation = Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(direction),rotSpeed * Time.deltaTime);
	}
	public void Disfish()
    {
		fish.SetActiveRecursively(false);
	}
	
}
