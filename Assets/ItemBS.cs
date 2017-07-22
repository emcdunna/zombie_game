using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// A class to assign to game objects which can be picked up, collected, or looted
// This class SOLELY controls how that object acts in terms of size, weight, ability to be carried, etc.
public class ItemBS : MonoBehaviour {

    public Collider2D coldr;

    // USED IN INVENTORY MATH
    public enum Size { Small, Medium, Large };
    public Size size = Size.Small;
    public float weight = 1.0f; // assumingly in "lbs" 

    // USED FOR BEING PICKED UP
    GameObject target;
    

    // Use this for initialization
    void Start()
    {
        if (coldr == null)
        {
            coldr = GetComponent<Collider2D>();
        }
        

    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            Character_BS character = target.GetComponent<Character_BS>();
            if (character != null)
            {
                if (character.State == Character_BS.AnimState.Loot)
                {
                    if (character.pickUp(this))
                    {
                        transform.SetParent(target.transform);
                        transform.position = target.transform.position;
                        SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
                        coldr.enabled = false;
                        sr.enabled = false;
                        
                    }
                    else
                    {
                        // couldnt carry it, dont pick it up
                    }
                }
            }
        }
    }


    // pick me up when you pass over

    private void OnTriggerStay2D(Collider2D collision)
    {
        target = collision.gameObject;

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        target = null;
    }

}
