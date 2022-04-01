using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneCollision : MonoBehaviour
{
    HandsViewer hands;

    private void OnCollisionEnter(Collision collision)
    {

        //hands.set_stone_collision_detect(true);
        //Debug.Log(hands.get_stone_collision_detect());
        Debug.Log("collision");
        //Destroy
    }
}
