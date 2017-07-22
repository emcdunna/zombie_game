using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Beh script to abstract the looting of small buildings
public class BuidlingBS : MonoBehaviour {

    public Collider2D coldr;
    public List<ItemBS> Items = new List<ItemBS>();
    List<GameObject> targets = new List<GameObject>();
    public string Name = "House";
    public float DangerChance = 0f; // Chance to be hurt during a search action
    public float SearchTime = 5f; // How many minutes it would take to search the entire building
    public float LootChance = 0.35f; // The chance to find loot items given default search time. 


    // Use this for initialization
    void Start () {
        if (coldr == null)
        {
            coldr = GetComponent<Collider2D>();
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    
    // TODO Test!! What if more than 1 object is in the trigger?
    private void OnTriggerEnter2D(Collider2D collision)
    {
        targets.Add(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        targets.Remove(collision.gameObject);
    }
}
