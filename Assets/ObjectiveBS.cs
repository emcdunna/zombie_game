using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// gamestate should be parent
public class ObjectiveBS : MonoBehaviour {
    /*
     * WinLevel    -   When the player enters this region, the level is won
     * QuitLevel   -   When the player enters this region, the level is quit (player still alive)
     * WinItem     -   An item needed to win the level
     * LoseLevel   -   When an enemy enters this region, the level is lost
     * 
     * */
    public enum ObjType { WinLevel, QuitLevel, LoseLevel };

    public bool finishedObjective = false;
    // TODO: list of required items or conditions

    public ObjType objType = ObjType.WinLevel;
    GameStateBS GAMESTATE;

    public List<ItemBS> ReqItems = new List<ItemBS>();

	// Use this for initialization
	void Start () {

        GAMESTATE = GetComponentInParent<GameStateBS>(); 
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // when someone enters the objective region
    private void OnTriggerEnter2D(Collider2D collision)
    {

        GameObject go = collision.gameObject;
        PlayerBS pl = null;
        ZombieBS zm = null;
        if (go != null)
        {
            switch (objType)
            {
                case ObjType.WinLevel:
                    pl = go.GetComponent<PlayerBS>();
                    if(pl != null)
                    {
                        bool hasAllItems = true;
                        if (ReqItems.Count > 0)
                        {
                            
                            foreach(ItemBS i in ReqItems)
                            {
                                if (false == pl.character.inventory.Contains(i))
                                {
                                    hasAllItems = false;
                                }
                            }
                        }
                        if (hasAllItems)
                        {
                            finishedObjective = true;
                        }
                        
                    }
                    break;
                case ObjType.QuitLevel:
                     pl = go.GetComponent<PlayerBS>();
                    if (pl != null)
                    {
                        finishedObjective = true;
                    }
                    break;
                case ObjType.LoseLevel:
                    zm = go.GetComponent<ZombieBS>();
                    if (zm != null)
                    {
                        finishedObjective = true;
                    }
                    break;
                default:
                    break;
            }
            
        }
    }
}
