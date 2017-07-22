using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// zombies are the enemy, mindless beasts
public class ZombieBS : MonoBehaviour
{
    public Character_BS character;
    public Vector2 softTarget; // basic weak target, go towards it with vague interest

    public float targetIntensity = 0.0f; // how interested in the soft target are we?
    public float maxIntensity = 100.0f; // never goes above this
    public float delayTime = 0.5f;
    public float nextSyncTime = 0;
    public GameStateBS GAMESTATE; // refernece to the gamestate instance
    public GameObject hardTarget; // we saw someone, go after them!

    // specifies which mode of targetting we are using
    public enum TargetMode { NoTarget, SoftTarget, HardTarget, FollowPath };
    public TargetMode targetmode = TargetMode.NoTarget;
    public List<MapNodeBS> path = null;
    public float minPathNodeDist = 0.1f; // minimum distance to mapnode before taking next instruction
    public MapNodeBS pathTarget;
    public Vector2 pathDestinationTarget;

    // how close do we have to be to get the hard target
    public float hardDetectRange = 1.5f;
    public float meleeDist = 0.3f;
    // how intense does it need to be
    public float minIntensity = 0.25f;

    // how quickly does it go down
    float forgetNoiseRate = 20.0f; // this time in seconds is about how long it takes to always forget a target
    float minForgetDistance = 0.05f; // completely forget a soft target if we get within this distance

    // Use this for initialization
    void Start()
    {
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

        
        softTarget = transform.position; // set target to where we already are
        

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 newPos = transform.position; // default
        bool is_moving = false;
        bool canSeeTarg = false;

        // NOW: lose % of intensity every X seconds
        float TimeMult = 1f;

        // The ratio of delta time into X seconds
        if(Time.deltaTime < forgetNoiseRate)
        {
            TimeMult = (forgetNoiseRate - Time.deltaTime) / forgetNoiseRate;
        }

        // drop intensity by a certain % each frame, drops quickly, stays nonzero for a long time
        if (targetIntensity > 0f )
        {
            targetIntensity = targetIntensity * TimeMult;
            
        }
        // lose intensity if we reach destination
        if (Vector2.Distance(transform.position, softTarget) < minForgetDistance)
        {
            targetIntensity = 0;
        }

        // only do logic every delayTime seconds
        if (Time.time > nextSyncTime)
        {
            nextSyncTime = Time.time + delayTime;
            
            // control which target mode we are in
            if (hardTarget != null)
            {
                targetmode = TargetMode.HardTarget;
                canSeeTarg = GAMESTATE.canSee(gameObject, hardTarget);

                // if the hard target gets too far away, switch to soft target
                float dist = Vector2.Distance(hardTarget.transform.position, transform.position);
                
                // if we cant see the hard target, set soft target and create a path
                if (canSeeTarg == false)
                {
                    targetmode = TargetMode.FollowPath;
                    softTarget = hardTarget.transform.position;
                    pathDestinationTarget = softTarget;
                    hardTarget = null;
                    if (path == null) {
                        //print("Find path1 at " + Time.time);
                        path = GAMESTATE.findPath(transform.position, pathDestinationTarget);
                        if (null == path)
                        {
                            targetmode = TargetMode.SoftTarget;
                        }
                        else
                        {
                            pathTarget = path[0];
                            pathDestinationTarget = softTarget;
                        }
                    }
                    
                }
                // if hard target moves out of range, go back to soft 
                else if (dist > hardDetectRange)
                {
                    targetmode = TargetMode.SoftTarget;
                    softTarget = hardTarget.transform.position; // set the soft target to the old pos
                    targetIntensity = maxIntensity;
                    hardTarget = null;
                    path = null;
                }
            }
            // is the intensity great enough to follow? if so, set soft target
            else if (targetIntensity > minIntensity)
            {
                // there already is a soft target
                canSeeTarg = GAMESTATE.canSeeIgnoreCharacters(transform.position, softTarget);

                if (canSeeTarg == false)
                {
                    // follow path if we cant see where the sound came from
                    // if we dont yet have a path, get one, or if this is a new soft target, generate a new path
                    targetmode = TargetMode.FollowPath;
                    
                    if (path == null || (pathDestinationTarget != softTarget))
                    {
                        path = GAMESTATE.findPath(transform.position, pathDestinationTarget);
                        // there is no path
                        if(null == path)
                        {
                            targetmode = TargetMode.SoftTarget;
                        }
                        else
                        {
                            pathTarget = path[0];
                            pathDestinationTarget = softTarget;
                        }
                        
                    }
                }
                else
                {
                    // otherwise just go towards it
                    targetmode = TargetMode.SoftTarget;
                    path = null;
                }

            }
            else
            {
                targetmode = TargetMode.NoTarget;
                softTarget = transform.position;
                targetIntensity = 0;
                path = null;
            }
            //print(Vector3.Magnitude(character.rigidbod.velocity) + " " + Time.time);
        }

       
        // handle behavior based off of the target mode
        switch (targetmode)
        {
            case (TargetMode.NoTarget):
                {
                    character.State = Character_BS.AnimState.Idle;
                    break;
                }
            case (TargetMode.SoftTarget):
                {

                    is_moving = true;
                    character.SetFacing(softTarget); // face the soft target VECTOR
                    character.State = Character_BS.AnimState.Walk;
                    Vector2 dir = new Vector2(softTarget.x, softTarget.y);
                    dir -= new Vector2(transform.position.x,transform.position.y);
                    dir.Normalize();
                    character.rigidbod.velocity = dir * character.walkSpeed;
                    

                    break;
                }
            case (TargetMode.HardTarget):
                {
                    is_moving = true;
                    character.SetFacing(hardTarget); // face the target game object
                    character.State = Character_BS.AnimState.Run;
                    Vector2 dir = hardTarget.transform.position - transform.position;
                    float dist = dir.magnitude;
                    dir.Normalize();
                    character.rigidbod.velocity = dir * character.runSpeed;

                    if (dist < meleeDist)
                    { 
                        // makes melee attack
                        if (character.meleeWeapon.canAttack(is_moving) == true)
                        {
                            GAMESTATE.MakeAttack(gameObject, dir, character.meleeWeapon);
                        }
                    }
                    
                    break;
                }
            case (TargetMode.FollowPath):
                {
                    // If we can see the next pathTarget, also just choose it (instead of walking too far the wrong way)
                    float dist = Vector2.Distance(transform.position, pathTarget.transform.position);
                    int i = path.IndexOf(pathTarget);
                    is_moving = true;
                    if (i + 1 < path.Count)
                    {
                        bool skipNode = GAMESTATE.canSeeIgnoreCharacters(transform.position, path[i + 1].transform.position);
                        if (dist <= minPathNodeDist || skipNode == true)
                        {
                            // get the next path target
                            pathTarget = path[i + 1];
                            //print("going to next node " + Time.time);

                        }
                    }
                    // if too close to last path
                    else if(dist <= minPathNodeDist)
                    {
                        targetIntensity = 0;
                        is_moving = false;
                    }

                    if(is_moving == true)
                    {
                        Vector2 pathvector = pathTarget.transform.position;
                        character.SetFacing(pathvector); // face the soft target VECTOR
                        character.State = Character_BS.AnimState.Walk;
                        Vector2 dir = pathvector;
                        dir -= new Vector2(transform.position.x, transform.position.y);
                        dir.Normalize();
                        character.rigidbod.velocity = Vector2.Lerp(character.rigidbod.velocity, dir * character.walkSpeed, 0.5f);
                    }

                    break;
                }
            default:
                {
                    break;
                }


        }

    }
    
    // registers that a noise was heard
    void HearNoise(Noise noise)
    {
        
        if (noise.gameObject != gameObject && noise.gameObject.tag != "Zombie") // IF a zombie made the noise
        {
            Vector2 loc = noise.location;
            int vol = noise.volume;

            // Scale response based upon distance away
            float dist = Vector2.Distance(loc, transform.position);
            float intensity = vol / (1 + dist); // grows with lower distance, and with higher volume
            bool cansee = false;

            if(noise.gameObject != null)
            {
                
                try
                {
                    cansee = GAMESTATE.canSee(gameObject, noise.gameObject);
                }
                // TODO: WHY DOES THIS ERROR WITH LARGE # OF ZOMBIES?
                catch (Exception e)
                {
                    print("ERR: " + gameObject + " - " + noise.gameObject);
                    print(e);
                }
                
            }
            
            
            //print(noise.gameObject + " " + cansee + " " + noise.location);

            // TODO: Higher intensity if the noise location can be seen from the zombie
            if ( cansee )
            {
                intensity *= 3;
            }

            // 
            if (intensity > targetIntensity)
            {
                // switch to Hard target
                if ( cansee && (dist < hardDetectRange) && (noise.gameObject != null))
                {
                    if( noise.gameObject != hardTarget )
                    {
                        //print("new HT: " + noise.gameObject + " at " + Time.fixedTime + " seconds.");
                    }
                    hardTarget = noise.gameObject; // we have a hard target
                
                }
                
                if (intensity > maxIntensity)
                {
                    targetIntensity = maxIntensity;
                }
                else
                {
                    targetIntensity = intensity;
                }
                softTarget = loc;
                //print("new ST: " + loc + " at " + Time.fixedTime + " seconds.");

            }
            else
            {
                // noise doesnt matter to us
            }
        }

    }

    
}
