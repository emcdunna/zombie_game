using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNodeBS : MonoBehaviour {

    public float footSpeed = 1f;// affect on speed for units on foot moving through square
    public List<MapNodeBS> adjacentNodes = new List<MapNodeBS>();

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public MapNodeBS(Vector3 pos)
    {
        transform.position = pos;
        
    }

    public void addAdjacentNode(MapNodeBS node)
    {
        adjacentNodes.Add(node);
    }





}
