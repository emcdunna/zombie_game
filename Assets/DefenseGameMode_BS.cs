using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseGameMode_BS : MonoBehaviour {

    public GameStateBS GAMESTATE;
    public int ZombiesLeft = 0;
    public static int smallHerd = 30;
    public static int largeHerd = 80;

    public float lastSpawn = 0;
    public float spawnFrequency = 2;
    public float spawnPointSize = 0.5f;

    public List<GameObject> Zombies = new List<GameObject>();
    public Vector2 spawnPoint = new Vector2(0, 0);

    // Use this for initialization
    void Start () {
        ZombiesLeft = Mathf.CeilToInt(Random.Range(smallHerd, largeHerd));
        GAMESTATE = GetComponent<GameStateBS>();

	}
	
	// Update is called once per frame
	void Update () {

        Vector2 currSpot;
        GameObject currZombie;
        float deltX;
        float deltY;
        if (ZombiesLeft > 0)
        {



            // time to spawn a new zombie
            if (Time.time > (lastSpawn + spawnFrequency))
            {
                deltX = Random.Range(-spawnPointSize, spawnPointSize);
                deltY = Random.Range(-spawnPointSize, spawnPointSize);
                currSpot = new Vector2(spawnPoint.x + deltX, spawnPoint.y + deltY);
                currZombie = GAMESTATE.SpawnZombie(currSpot);
                ZombiesLeft -= 1;
                Zombies.Add(currZombie);
                lastSpawn = Time.time;
            }
        }
        
	}

    
}
