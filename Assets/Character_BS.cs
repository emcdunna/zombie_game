using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_BS : MonoBehaviour
{
    // weapon related stuff
    public WeaponBS meleeWeapon;
    public WeaponBS missileWeapon;
    public GrenadeBS tertiaryWeapon;
    public string name = "No name";
    private float lowAmmoThreshold = 0.0f; // at what percent ammo should we reload?
    public Rigidbody2D rigidbod;
    public int kills = 0;
    // INVENTORY OF EVERYTHING CARRIED ON-PERSON
    public List<ItemBS> inventory = new List<ItemBS>();
    public float weightCapacity = 100f;

    int noiseFrame = 10; // how many frames between each noise (so you dont make one every frame)
    public float BaseWalkSpeed = 1.0f; // multiplied down by weight
    public float BaseRunSpeed = 2.0f; // multiplied down by weight
    public float walkSpeed = 1.0f;
    public float runSpeed = 2.0f;
    public float rotSpeed = 1f;
    public int health = 100;
    public int maxHealth = 100;
    public int idleNoise = 1;
    public int walkNoise = 3;
    public int runNoise = 5;
    public float strafeRate = 0.5f; // rate at which you can move side to side, or backwards
    public bool freezeChar = false; // TODO: make it so that characters dont move, shoot, etc. while frozen (for pausing game)
    public int stamina = 10; // TODO make it so that this affects how quickly you can sprint

    // SKILLS 
    public float missileSkill = 1.0f; // better with missile weaponry (accuracy, crit chance)
    public float meleeSkill = 1.0f; // makes attacks better or worse
    public float craftingSkill = 1.0f; // better at crafting (combining items)
    // metalworking?
    // construction

    public float huntingSkill = 1.0f; // forage for supplies (wilds)
    public float lootingSkill = 1.0f; // forage for supplies (urban)

    public bool canSprint = true;
    float nextSprintTime = 0;

    // TODO: Implement armor?
    public int Armor = 0; // how much armor the person has on
    public enum AnimState { Idle, Run, Walk, Reload, Loot }; // only things you do for a period of time
    public AnimState State = AnimState.Idle;
    public float busyState = 0; // when the next state change is allowed
    public float lootTime = 0.2f;
    public float healTime = 0;
    public float healRate = 1; // how often 1 HP is added
    public GameStateBS GAMESTATE;

    // Use this for initialization
    void Start()
    {
        rigidbod = GetComponent<Rigidbody2D>();

        // check if gamestate reference exists
        if (GAMESTATE == null)
        {
            GAMESTATE = gameObject.GetComponentInParent<GameStateBS>();
        }

        List<ItemBS> subitems = new List<ItemBS>();
        subitems.AddRange(gameObject.GetComponentsInChildren<ItemBS>());
        // TODO: This only detects active objects!
        foreach(ItemBS i in subitems)
        {
            
            if (false == inventory.Contains(i))
            {
                inventory.Add(i);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        // only make a noise once every 10 frames
        if(noiseFrame <= 0)
        {
            noiseFrame = 10;
            switch (State)
            {
                case AnimState.Walk:
                    GAMESTATE.MakeNoise(gameObject, transform.position, walkNoise);
                    break;
                case AnimState.Run:
                    GAMESTATE.MakeNoise(gameObject, transform.position, runNoise);
                    break;
                default:
                    GAMESTATE.MakeNoise(gameObject, transform.position, idleNoise);
                    break;
            }
        }
        else
        {
            noiseFrame -= 1;
        }



        if (health <= 0)
        {
            // I am dead!
            kill();
        }

        // handle NOT being able to sprint sometimes
        if(canSprint == false && Time.time >= nextSprintTime)
        {
            canSprint = true;
        }

        // you are automatically slower when you carry more things
        float weight = getWeight();
        float wc = 1.25f * weightCapacity;
        walkSpeed = BaseWalkSpeed * (wc - weight) / (wc);
        runSpeed = BaseRunSpeed * (wc - weight) / (wc);

        // slowly regain health over time
        if (health < maxHealth && Time.time > healTime)
        {
            health++;
            healTime = Time.time + healRate;
        }
    }

    // return how many rounds left there are for the missile weapon
    public int getTotalRounds()
    {
        int total = 0;
        if(missileWeapon != null)
        {
            foreach(ItemBS i in inventory)
            {
                if (i != null)
                {
                    AmmoBS a = i.GetComponent<AmmoBS>();
                    if (a != null && a.ammoType == missileWeapon.ammoType)
                    {
                        total += a.rounds;
                    }
                }
                
            }
        }

        return total;
    }
    
    // TODO: can the character carry this item?
    public bool canCarry(ItemBS item)
    {
        if(item.size == ItemBS.Size.Small)
        {
            float w = getWeight();

            // You cant carry more than weight capacity
            if( (w + item.weight) <= (weightCapacity))
            {
                return true;
            }
            
        }
        return false;
    }

    // puts in inventory if possible
    public bool pickUp(ItemBS item)
    {
        if (canCarry(item))
        {
            if(false == inventory.Contains(item))
            {
                inventory.Add(item);

                
                return true;
            }
            
        }
        return false;
    }

    // get total weight of inventory
    public float getWeight()
    {
        float totalWeight = 0;
        foreach (ItemBS i in inventory)
        {
            if(i != null)
            {
                totalWeight += i.weight;
            }
            
        }
        return totalWeight;
        
    }

    // TODO equip an item (as a weapon)
    public void equipWeapon(WeaponBS weapon)
    {
        SpriteRenderer sr;
        if (weapon.type == WeaponBS.Type.Projectile || weapon.type == WeaponBS.Type.Missile)
        {
            if (missileWeapon != null)
            {
                sr = missileWeapon.gameObject.GetComponent<SpriteRenderer>();
                sr.enabled = false;
                
            }
            missileWeapon = weapon;
        }
        else
        {
            if (meleeWeapon != null)
            {
                
                sr = meleeWeapon.gameObject.GetComponent<SpriteRenderer>();
                sr.enabled = false;
                
            }
            meleeWeapon = weapon;
        }
        sr = weapon.gameObject.GetComponent<SpriteRenderer>();
        sr.enabled = true;
        
    }

    // TODO TEST
    public void dropItem(ItemBS item)
    {
        if (inventory.Contains(item))
        {
            inventory.Remove(item);
        }
        GameObject go = item.gameObject;
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        sr.enabled = true; // make it visible
        Collider2D c = item.coldr;
        c.enabled = true;
        Vector3 newPos = transform.position;
        go.transform.SetParent(GAMESTATE.transform);
        go.transform.position = transform.position;
    }

    // set facing towards an object
    public void SetFacing(GameObject target)
    {
        // handles when trying to look at nothing, vector length 0
        if (target.transform.position != transform.position)
        {
            Quaternion rotation = Quaternion.LookRotation(target.transform.position - transform.position, transform.TransformDirection(-Vector3.forward));
            rotation = new Quaternion(0, 0, rotation.z, rotation.w);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotSpeed);
        }
    }

    // set facing to a specific position
    public void SetFacing(Vector3 position)
    {
        // handles when trying to look at nothing, vector length 0
        if (position != transform.position)
        {
            Quaternion rotation = Quaternion.LookRotation(position - transform.position, transform.TransformDirection(-Vector3.forward));
            rotation = new Quaternion(0, 0, rotation.z, rotation.w);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotSpeed);
        }
    }

    

    // projectile hit me!
    public void hitByProjectile(BulletBS bullet, Character_BS shooter)
    {
        WeaponBS weapon = bullet.weapon;
        float critRoll = UnityEngine.Random.Range(0.0f, 1.0f);
        float accRoll = UnityEngine.Random.Range(0.0f, 1.0f);
        int damage = weapon.damage;
        if (critRoll <= weapon.critChance * shooter.missileSkill) // less than or equal to crit chance
        {
            // crit!
            damage *= 10; // mult by 10
        }
        
        if (accRoll >= weapon.accuracy * shooter.missileSkill)
        {
            damage = 0;
            
        }
        health -= damage;
        
    }

    // missile hit me!
    public void hitByMissile(WeaponBS weapon, Character_BS shooter)
    {
        float critRoll = UnityEngine.Random.Range(0.0f, 1.0f);
        float accRoll = UnityEngine.Random.Range(0.0f, 1.0f);
        int damage = weapon.damage;
        if (critRoll <= weapon.critChance * shooter.missileSkill) // less than or equal to crit chance
        {
            // crit!
            damage *= 10; // mult by 10
        }

        if (accRoll >= weapon.accuracy * shooter.missileSkill)
        {
            damage = 0;

        }
        health -= damage;

    }


    // melee attack hit  me
    public void hitByMelee(WeaponBS weapon, Character_BS attacker)
    {
        float critRoll = UnityEngine.Random.Range(0.0f, 1.0f);
        float accRoll = UnityEngine.Random.Range(0.0f, 1.0f);
        int damage = weapon.damage;
        if (critRoll <= weapon.critChance * attacker.meleeSkill) // less than or equal to crit chance
        {
            // crit!
            damage *= 10; // mult by 10
        }
        if (accRoll >= weapon.accuracy * attacker.meleeSkill )
        {
            damage = 0;

        }
        health -= damage;
        //print("Melee attack from: " + weapon.item.gameObject.name + " dealt " + damage.ToString() + " damage.");
    }

    // when an explosion hits the character
    public void hitByExplosion(GrenadeBS grenade, int realDam)
    {
        if (realDam > 0)
        {
            health -= realDam;
        }
    }

    // kill self!
    public void kill()
    {
        PlayerBS player = gameObject.GetComponent<PlayerBS>();
        if (player == null)
        {
            GameObject.Destroy(gameObject, 0.01f);
            //gameObject.SetActive(false);
            GAMESTATE.ALL_CHARACTERS.Remove(gameObject);
        }
        else
        {
            print("GAME OVER!");
            gameObject.SetActive(false);

        }
        
        //
    }

    // TODO TEST THIS
    public bool hasLowAmmo()
    {
        bool lowAmmo = false;
        int right_shots = 0;
        float rightRatio = 1.0f;
        
        if (missileWeapon != null)
        {
            if (missileWeapon.ClipItem != null)
            {
                right_shots = missileWeapon.ClipItem.rounds;
                rightRatio = right_shots / missileWeapon.ClipItem.maxRounds;
            }
        }
        if ( (rightRatio <= lowAmmoThreshold))
        {
            lowAmmo = true;
        }
       
        return lowAmmo;

    }

    // Reload missile weapon
    public void reload()
    {

        bool foundAmmo = false;
        float reloadTime = Time.time;
        if (missileWeapon != null)
        {
            foreach (ItemBS i in inventory)
            {
                //print(i.name);
                AmmoBS a = i.GetComponent<AmmoBS>();
                // if a is Ammo, and a is compatible with the right weapon
                if ((a != null) && (a.isCompatible(missileWeapon)) && (a != missileWeapon.ClipItem) )
                {
                    print("Reloading " + missileWeapon.name);
                    missileWeapon.reload(a);
                    foundAmmo = true;
                    break;
                }
            }
            if(foundAmmo == true)
            {
                reloadTime += missileWeapon.reloadTime;
            }

        }

        // makes it so you cant move and reload
        busyState = reloadTime;
        canSprint = false;
        
    }
}
