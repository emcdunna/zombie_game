using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBS : MonoBehaviour {
 
    public bool ignoreZombies = false;
    public enum Type { Melee, Missile, Projectile, Shield, Grenade };
    public Type type = Type.Missile;

    public bool isTwoHanded = false;
    public int damage = 0;
    public int noise = 0;

    public float range = 100; // range for projectile ray to go
    public float recoil = 0.0f; // number of degrees off center the shot can be (+- the number)

    public float rateOfFire = 60f; // ROF in RPM 
    float attackSpeed = 0.0f; // How much time (in seconds) between attacks (or shots) when "full auto"
    public float accuracy = 1.0f; // When collision occurs, what is the chance that the hit is ignored

    public int armorPiercing = 0; // TODO implement armor
    public float nextAttack = 0.0f; //time of next available attack
    public float reloadTime = 1.0f; // how much time in seconds does it take to reload
    string ammoString = "Default";
    public bool isFullAuto = true; // whether its full auto or single fire
    public float critChance = 0.1f; // % chance for a critical hit, which multiplies damage by 10
    public bool moveAndAttack = true; // whether you can attack while walking, or if you have to be stationary

    // MUST SET THESE
    public ItemBS item; // the greater ITEM object
    public GameObject Missile_Prefab; // what missile do we instantiate when attacking
    public AmmoBS ClipItem = null;
    public AmmoBS.AmmoType ammoType = AmmoBS.AmmoType.M4A1;
    public GameObject Melee_Prefab; // use a tiny,hidden, temp object to act as the melee attack
    public Transform shotspawn;
    public AudioClip shotClip;
    public Character_BS character;
    // Use this for initialization
    void Start()
    {
        shotspawn = transform;
        attackSpeed = 60f / rateOfFire;
        character = GetComponentInParent<Character_BS>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public string getAmmoString()
    {

        switch (type){
            
            case WeaponBS.Type.Melee:
                ammoString = "Ready";
                break;
            
            default:
                
                int rnds = -1;
                if(character != null)
                {
                    rnds = character.getTotalRounds() - ClipItem.rounds;
                }
                ammoString = ClipItem.rounds + " / " + rnds; // TODO replace ? with # of clips left
                break;
        }
        return ammoString;
    }

    // makes shooting noise when called
    public void makeSound()
    {
        shotspawn = transform;
        item = GetComponent<ItemBS>();
        if (shotClip != null)
        {
            AudioSource.PlayClipAtPoint(shotClip, shotspawn.position);
        }

    }

    // can we make an attack right now
    public bool canAttack(bool is_moving)
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
        if (type == Type.Missile && ClipItem.rounds <= 0)
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
    public bool canReload(bool is_moving)
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
            ClipItem.rounds -= 1;
        }

        nextAttack = Time.time + attackSpeed; // update time when next attack can happen
    }


    // reloads the weapon, returns the old clip
    public void reload(AmmoBS newClip)
    {
        //print("Reloading " + item.name + " with " + newClip.name);
        if (newClip.isCompatible(this))
        {
            AmmoBS oldClip = ClipItem;
            ClipItem = newClip;
            nextAttack = Time.time + reloadTime;
            if(oldClip.rounds <= 0)
            {
                character.dropItem(oldClip.item);
            }
        }
        
    }
}
