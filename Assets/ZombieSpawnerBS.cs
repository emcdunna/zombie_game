using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawnerBS : MonoBehaviour {

    public float height = 1;
    public float width = 1;
    public float spawnFrequency = 5f; // how often to spawn new zombies
    public int spawnNumber = 2; // how many zombies to spawn each time
    public int spawnCap = 10; // how many zombies to spawn maximum
    public int totalSpawned = 0;
    public bool ACTIVE_SPAWNER = true;
    public float delt_time = 0;
    public GameStateBS GAMESTATE;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        
		if(ACTIVE_SPAWNER == true)
        {
            if(delt_time > spawnFrequency )
            {
                delt_time = 0;
                for(int i = 0; i < spawnNumber; i++)
                {
                    if(totalSpawned <= spawnCap)
                    {
                        GAMESTATE.SpawnZombie(selectRandomPosition());
                        totalSpawned += 1;
                    }
                    else
                    {
                        ACTIVE_SPAWNER = false;
                    }
                }
                
            }
        }
        delt_time += Time.deltaTime;
	}

    public Vector2 selectRandomPosition()
    {
        float x = Random.Range(transform.position.x - width/2f, transform.position.x + width / 2f);
        float y = Random.Range(transform.position.y - height / 2f, transform.position.y + height / 2f);

        Vector2 pos = new Vector2(x,y);
        return pos;
    }
}
