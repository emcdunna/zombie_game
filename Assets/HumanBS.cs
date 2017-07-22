using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// humans are NPC living characters (not zombies)
public class HumanBS : MonoBehaviour {
    public Character_BS character;

    public GameStateBS GAMESTATE;
    public GameObject hardTarget; // game object that the human is focused on killing, talking to, etc.
    public Vector2 softTarget; // location that human is focused on moving to, looking at, etc. 

    public bool holdFire = false;
    public float minSafeDist = 0.75f; // range within which character will run away from zombies
    public float targetIntensity = 0.0f;

    public float engagementRange = 3.0f; // range within which you will engage targets that make noise

    public enum BehaviorState { Looting, Fleeing, Following, Fighting, Resting, Talking };
    public BehaviorState BehState = BehaviorState.Resting;

    // Use this for initialization
    void Start() {
        // check character obj exists
        if (character == null)
        {
            character = gameObject.GetComponent<Character_BS>();

        }
        // check if gamestate reference exists
        if (GAMESTATE == null)
        {
            GAMESTATE = gameObject.GetComponentInParent<GameStateBS>();
        }
        // check for target
        // TODO do that...
    }


    // -----------------------------------------------------------------------------------------------------//
    // Decides what behavior the human should be in right now
   
        // If we see encounter an enemy, switch to fighting state.
        // If player requests to talk, and we are not busy, go to talking state.
        // If player asks us to follow, go to follow state, set target to near player (not bumping into them)
        // If player tells us to hide, go to hiding state. 
        // If player tells us to rest, go to resting state.
        // If player tells us to loot, go to looting state. 
        // If player tells us to flee, or if danger is high, go to fleeing state
        // 
         

    // Someone requests to talk
    void talkRequest(GameObject player)
    {
        // TODO: Dont always listen to request! Not now!
        BehState = BehaviorState.Talking;
        hardTarget = player;
        softTarget = player.transform.position;

    }

    // Ordered to loot surrounding areas
    void lootOrder(GameObject player)
    {
        // TODO: Conditional denial, "Not now!"
        BehState = BehaviorState.Looting;
        softTarget = player.transform.position; // set general location to be where player is standing
        hardTarget = GAMESTATE.findNearestLoot(gameObject); // TODO: Find hard target items later!
        
    }

    // Ordered to follow
    void followOrder(GameObject player)
    {
        // TODO: Conditional denial, "Not now!"
        BehState = BehaviorState.Following;
        hardTarget = player;
        softTarget = new Vector2(0, 0);
    }

    // Ordered to disengage (hold fire, stay quiet)
    void disengageOrder(GameObject player)
    {
        // TODO: Conditional denial, "Not now!" 
        holdFire = true;
        
    }

    // Ordered to rest or hold position
    void restOrder(GameObject player)
    {
        // TODO: Conditional denial, "Not now!"
        BehState = BehaviorState.Resting;
        softTarget = transform.position;
        hardTarget = null;
    }

    // Ordered to flee
    void fleeOrder(GameObject player)
    {
        // TODO: Conditional denial
        BehState = BehaviorState.Fleeing;
        hardTarget = null; // set this later
        softTarget = new Vector2(0, 0); // TODO: Find destination to flee towards
        // pick a target
    }

    // Order to engage enemies (stop holding fire)
    void engageOrder(GameObject player)
    {
        // TODO: Conditional denial!
        holdFire = false;


    }
    // -----------------------------------------------------------------------------------------------------//

    // Update is called once per frame
    void Update() {

        /* 
         * STATE, holdFire, HardTarget, and SoftTarget are all already set for us
         * 
         * This means just decide what to do based on the state and targets given
         * 
         * */

        // SET UP DEFAULT VALUES FOR STATE-BASED VARIABLES
        Vector2 moveDirection = new Vector2(0,0); // what direction to move in (normalized later)
        bool shouldMove = false; // whether or not we should move
        bool shouldAttack = false; // whether or not we should attack
        bool shouldSprint = false; // whether or not to sprint
        bool shouldReload = false; // whether or not we should reload (if possible)
        bool shouldLoot = false; // whether or not to pick up items (if possible)

        float targetDistance = 0f;
        if (hardTarget != null)
        { 
            targetDistance = Vector2.Distance(hardTarget.transform.position, transform.position); // value of distance to hard target
        }


        // determine what to do based on what state we are in
        // STATES DO NOT CHANGE HERE!
        switch (BehState) { 
            case BehaviorState.Fighting:
                if (holdFire == false)
                {
                    shouldAttack = true;
                }
                
                // hardTarget = enemies?? TODO: will this be somewhere else?
                if( targetDistance < minSafeDist)
                {
                    shouldMove = true;
                    moveDirection = GAMESTATE.findFleeDirection(gameObject);
                }
                
                shouldReload = true;
                break;
            case BehaviorState.Fleeing:
                shouldMove = true;
                shouldSprint = true;
                moveDirection = GAMESTATE.findFleeDirection(gameObject); // TODO set move target
                
                break;
            case BehaviorState.Looting:
                shouldMove = true;
                shouldReload = true;
                shouldLoot = true;
                // hard target will hold the closest item to go get, so move towards it...
                // TODO: hard pathfinding!
                moveDirection = hardTarget.transform.position - transform.position;

                break;
            case BehaviorState.Following:
                shouldReload = true;
                // TODO: dont move perfectly towards the player, Pathfinding

                if (targetDistance > minSafeDist)
                {
                    shouldMove = true;
                    moveDirection = hardTarget.transform.position - transform.position;
                }             
                break;
            
            default:
                // just dont change anything, leave values the preset above
                break;

        }
        
        // PROCESS ACTIONS BASED ON ABOVE VARIABLES


        // if the character was busy doing something, can't perform the below actions
        if (character.busyState <= Time.time)
        {

            // the following get set below
            Vector2 newPos = transform.position;
            float movespeed = 0.0f;
            bool enableAttack = true;

            // reload weapons
            if (shouldReload && character.hasLowAmmo()) // TODO what if no ammo?
            {
                character.reload();
                character.State = Character_BS.AnimState.Reload;
            }
            // pick up items
            else if (shouldLoot && (targetDistance < GAMESTATE.minLootDist)) // TODO: should we pickup items?
            {
                character.State = Character_BS.AnimState.Loot;
                character.busyState = Time.time + character.lootTime; // technically this should be done in character class

                // TODO: Now we need to go try for another item
            }
            // should we be moving?
            else if (shouldMove)
            {
                
                // Sprint or walk
                if (character.canSprint == true && (shouldSprint)) // are we going to sprint?
                {
                    character.State = Character_BS.AnimState.Run;
                    movespeed = character.runSpeed;
                    character.SetFacing(moveDirection);
                    enableAttack = false;

                }
                else
                {
                    character.State = Character_BS.AnimState.Walk;
                    movespeed = character.walkSpeed;
                    character.SetFacing(moveDirection); // look in the direction we are moving unless interrupted by shooting later

                }
                // actually set new position
                Vector3 movDir = moveDirection.normalized;
                newPos = transform.position + (movDir * movespeed * Time.deltaTime);

                // actually move to the new location
                character.rigidbod.MovePosition(newPos);
            }
            // be idle
            else
            {
                character.State = Character_BS.AnimState.Idle;
                movespeed = 0.0f;
                if(hardTarget != null)
                {
                    character.SetFacing(hardTarget);
                }

            }

            // HANDLE SHOOTING/ATTACKING
            if (enableAttack && shouldAttack && (hardTarget != null) ) { 
                
                bool is_moving = false;
                if(movespeed > 0.0f)
                {
                    is_moving = true;
                }

                // TODO: Handle when hard target dies, or is behind a barrier (and a new target should be found)

                // aim at your target
                character.SetFacing(hardTarget);
                
                // attack with LEFT weapon
                if (character.meleeWeapon != null)
                {
                    if (character.meleeWeapon.canAttack(is_moving) == true)
                    {
                        Vector2 direction = hardTarget.transform.position - character.meleeWeapon.shotspawn.position;
                        GAMESTATE.MakeAttack(gameObject, direction, character.meleeWeapon);
                        
                    }

                }
                // attack with RIGHT weapon
                if (character.missileWeapon != null)
                {
                    if (character.missileWeapon.canAttack(is_moving) == true)
                    {
                        Vector2 direction = hardTarget.transform.position - character.missileWeapon.shotspawn.position;
                        GAMESTATE.MakeAttack(gameObject, direction, character.missileWeapon);
                        
                    }
                }

            }

            // ---- END MOVING ---- //
        }
        // if busy, like mid reload, we just face the target
        else
        {
            if(hardTarget != null)
            {
                character.SetFacing(hardTarget);
            }
           
        }
    }



    // process noises, TODO: trigger zombie engagement!
    void HearNoise(Noise noise)
    {
        if (noise.gameObject != null) {
            if (noise.gameObject != gameObject && noise.gameObject.tag == "Zombie") // If a zombie made the noise
            {
                Vector2 loc = noise.location;
                int vol = noise.volume;

                // scale response based upon distance away
                float dist = Vector2.Distance(loc, transform.position);
                float intensity = 3 * vol * (1 / (3 + dist)); // grows with lower distance, and with higher volume


                if ((dist < engagementRange) && (intensity > targetIntensity))
                {
                    // TODO: Trigger fighting mode with a specific hard target
                    if(noise.gameObject != hardTarget)
                    {
                        print("new HT: " + noise.gameObject + " at "  + Time.fixedTime + " seconds.");
                    }
                    hardTarget = noise.gameObject; // we have a hard target
                    BehState = BehaviorState.Fighting;
                    
                    targetIntensity = intensity;
                }

                else
                {
                    // noise doesnt matter to us
                }
            }
        }
    }

    
}

    // 





