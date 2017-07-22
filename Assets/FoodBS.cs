using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodBS : MonoBehaviour {

    public float sustainence = 0f;
    public float happiness = 0f;
    public bool edible = true;
    public int daysTillExpiration = 30;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // END DAY ?
    void EndDay()
    {
        if(daysTillExpiration > 0)
        {
            daysTillExpiration -= 1;
        }
        else
        {
            edible = false;
        }
        
    }
}
