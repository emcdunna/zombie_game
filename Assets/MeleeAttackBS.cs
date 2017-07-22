using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackBS : MonoBehaviour {

    public WeaponBS weapon;
    public GameObject attacker;
    bool isMoving = false;
    public Vector2 moveTo;

    // Use this for initialization
    void Start () {
		if (weapon == null)
        {
            weapon = gameObject.GetComponentInParent<WeaponBS>(); // the item is the parent obj, then weapon is a field
        }
        GameObject.Destroy(gameObject, 0.1f); // delete after 1 second (DEFAULT, REPLACE LATER)
    }
	
	// Update is called once per frame
	void Update () {
		if (isMoving == true)
        {
            // move towards the moveTo spot

        }
	}

    // weapon hits something
    private void OnTriggerEnter2D(Collider2D collision)
    {

        GameObject target = collision.gameObject;
        GameObject item = weapon.item.gameObject;
        GameObject wielder = item.GetComponentInParent<Character_BS>().gameObject;
        if (target == wielder || (target.tag == "Zombie" && weapon.ignoreZombies == true))
        {
            // do nothing, i hit myself
             
        }
        else
        {
            Character_BS targetChar = target.GetComponent<Character_BS>();

            if (targetChar != null)
            {
                targetChar.hitByMelee(weapon, attacker.GetComponent<Character_BS>());
            }

        }
        
    }
}
