using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// item controls the behavior of all manner of items
public class Item : MonoBehaviour {

    public Collider2D coldr;
    public enum Type { Weapon, Ammo, Food, Fuel };// MORE OF THESE
    public float weight = 1.0f; // assumingly in "lbs" 
    public Type type = Type.Weapon;
    public Weapon weapon; // is nothing if its NOT a weapon
    public int shots = 0; // only matters if its an ammo clip
    public int shotsPerClip = 1;
    GameObject target;
    float trigTime = 0;
    public static float TriggerWindow = 1f;


    // Use this for initialization
    void Start () {
        if (coldr == null)
        {
            coldr = GetComponent<Collider2D>();
        }
        if (type == Type.Weapon && weapon.shotspawn == null)
        {
            weapon.shotspawn = transform;
        }

    }
	
	// Update is called once per frame
	void Update () {
		if (target != null)
        {
            Character_BS character = target.GetComponent<Character_BS>();
            if (character != null)
            {
                if (character.State == Character_BS.AnimState.Loot)
                {
                    character.inventory.Add(this);
                    transform.SetParent(target.transform);
                    gameObject.SetActive(false); // dont need to see it anymore
                    print("You picked up " + gameObject.name);
                }
            }
        }
        else if (Time.time >= trigTime)
        {
            target = null;
        }
	}


    // pick me up when you pass over
    private void OnTriggerEnter2D(Collider2D collision)
    {
        target = collision.gameObject;
        
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        target = collision.gameObject;

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        trigTime = Time.time + TriggerWindow;
    }


}



// ----------------------------------------------------------------------------------------------------------------------------------------
// weapon stores functions and stats for a specific weapon (which is tied to an item)
[System.Serializable]
public class Weapon : System.Object
{
    public bool ignoreZombies = false;
    public enum Type { Melee, Missile, Projectile, Shield, Grenade };
    public Type type = Type.Missile;

    public bool isTwoHanded = false;
    public int damage = 0;
    public int noise = 0;

    public float range = 100; // range for projectile ray to go
    public float recoil = 0.0f; // number of degrees off center the shot can be (+- the number)

    public float attackSpeed = 0.0f; // How much time (in seconds) between attacks (or shots) when "full auto"
    public float accuracy = 1.0f; // When collision occurs, what is the chance that the hit is ignored

    public int armorPiercing = 0; // TODO implement armor

    public float nextAttack = 0.0f; //time of next available attack
    public float reloadTime = 1.0f; // how much time in seconds does it take to reload
    

    public bool isFullAuto = true; // whether its full auto or single fire

    public float critChance = 0.1f; // % chance for a critical hit, which multiplies damage by 10

    public bool moveAndAttack = true; // whether you can attack while walking, or if you have to be stationary
    
    // MUST SET THESE
    public Item item; // the greater ITEM object
    public GameObject Missile_Prefab; // what missile do we instantiate when attacking
    public Item ClipItem;
    public Item.Type AmmoType = Item.Type.Ammo;
    public GameObject Melee_Prefab; // use a tiny,hidden, temp object to act as the melee attack
    public Transform shotspawn;
    public AudioClip shotClip;




    // makes shooting noise when called
    public void makeSound()
    {
        
        if (shotClip != null)
        {
            AudioSource.PlayClipAtPoint(shotClip, shotspawn.position);
        }

    }

    // can we make an attack right now
    public bool canAttack( bool is_moving)
    {
        // are we moving?
        if (is_moving == true)
        {
            if (moveAndAttack == false)
            {
                return false; // if we are, and we arent allowed to attack, then return false
            }
        }

        // if we are a missile weapon, do we have enough ammo?
        if (type == Type.Missile && ClipItem.shots <= 0)
        {
            return false;
        }

        // has it been long enough
        if (Time.time >= nextAttack)
        {
            return true;
        }
        return false;
    }

    // can we reload?
    public bool canReload( bool is_moving)
    {
        // are we moving?
        if (is_moving == true)
        {
            if (moveAndAttack == false)
            {
                return false; // if we are, and we arent allowed to attack, then return false
            }
        }

        // has it been long enough
        if (Time.time >= nextAttack)
        {
            return true;
        }
        return false;
    }
    
    // update stats after an attack (like for example decrement ammo)
    public void attackUpdate()
    {
        if (type == Type.Missile || type == Type.Projectile)
        {
           ClipItem.shots -= 1;
        }

        nextAttack = Time.time + attackSpeed; // update time when next attack can happen
    }


    // reloads the weapon, returns the old clip
    public Item reload(Item newClip)
    {
        MonoBehaviour.print("Reloading " + item.name + " with " + newClip.name);
        if (ClipItem.type == AmmoType)
        {
            Item oldClip = ClipItem;
            ClipItem = newClip;
            nextAttack = Time.time + reloadTime;
            return oldClip;
        }
        return newClip;
        

    }
}