using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// player is the human that the player is currently controlling
public class PlayerBS : MonoBehaviour {
    public Character_BS character;
    public GameObject target;
    public GameStateBS GAMESTATE;
    public enum MoveControls { MouseRotate, Fixed8Ways };
    public MoveControls moveControls = MoveControls.Fixed8Ways;

    
	// Use this for initialization
	void Start () {
        
        // check character obj exists
        if( character == null)
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

    // Update is called once per frame
    void Update() {

        // always face target
        //

        // enable user to play/pause game with escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GAMESTATE.togglePaused();
        }

        if ((character.freezeChar == false) && (character.busyState <= Time.time))
        {
            Vector2 newPos = transform.position;
            // ---- MOVING ---- //

            // DIRECTION // 
            bool move_forward = Input.GetKey(KeyCode.W);
            bool move_right = Input.GetKey(KeyCode.D);
            bool move_left = Input.GetKey(KeyCode.A);
            bool move_back = Input.GetKey(KeyCode.S);


            float movespeed = 0.0f;
            bool enableAttack = true;
            bool is_moving = false;
            bool RightAttack = false;
            bool LeftAttack = false;
            
            
            // reload weapons
            if (Input.GetKeyDown(KeyCode.R))
            {
                character.reload();
                character.State = Character_BS.AnimState.Reload;

            }
            // pick up items
            else if (Input.GetKey(KeyCode.E))
            {
                character.State = Character_BS.AnimState.Loot;
                character.busyState = Time.time + character.lootTime; // TODO: technically this should be done in character class
            }
            else if (move_forward || move_left || move_right || move_back)
            {
                is_moving = true;
                if (character.canSprint == true && Input.GetKey(KeyCode.LeftShift))
                {
                    character.State = Character_BS.AnimState.Run;
                    movespeed = character.runSpeed;
                    enableAttack = false;
                }
                else
                {
                    movespeed = character.walkSpeed;
                    character.State = Character_BS.AnimState.Walk;
                    
                }
            }
            else
            {
                
                // be idle
                character.State = Character_BS.AnimState.Idle;
                movespeed = 0.0f;
                
            }

            // attack with melee weapon
            if (character.meleeWeapon != null)
            {

                if (character.meleeWeapon.isFullAuto == true)
                {
                    if (enableAttack && Input.GetKey(KeyCode.Mouse1))
                    {
                        LeftAttack = true;
                    }
                }
                else
                {
                    if (enableAttack && Input.GetKeyDown(KeyCode.Mouse1))
                    {
                        LeftAttack = true;
                    }
                }

                if (LeftAttack == true)
                {
                    if (character.meleeWeapon.canAttack(is_moving) == true)
                    {
                        Vector2 direction = target.transform.position - character.meleeWeapon.shotspawn.position;
                        GAMESTATE.MakeAttack(gameObject, direction, character.meleeWeapon);
                    }

                }

            }


            // attack with missile weapon
            if (character.missileWeapon != null)
            {
                if (character.missileWeapon.isFullAuto == true)
                {
                    if (enableAttack && Input.GetKey(KeyCode.Mouse0))
                    {
                        RightAttack = true;
                    }
                }
                else
                {
                    if (enableAttack && Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        RightAttack = true;
                    }
                }

                if (RightAttack == true)
                {
                    if (character.missileWeapon.canAttack(is_moving) == true)
                    {
                        Vector2 direction = target.transform.position - character.missileWeapon.shotspawn.position;
                        GAMESTATE.MakeAttack(gameObject, direction, character.missileWeapon);
                    }

                }
            }

            //TODO: interrupt other weapon firing!
            if (Input.GetKeyDown(KeyCode.G))
            {
                if(character.tertiaryWeapon != null)
                {
                    if(is_moving == false)
                    {
                        Vector2 direction = target.transform.position - transform.position;
                        GAMESTATE.MakeTertiaryAttack(gameObject, direction, character.tertiaryWeapon);
                    }
                }

            }
            

            // ACTUALLY MOVE AND SCALE VALUE
            Vector3 forwardDirection = target.transform.position - transform.position;
            forwardDirection.Normalize();
            Vector3 finalDirection = forwardDirection;

            switch (moveControls)
            {
                case MoveControls.MouseRotate:
                    if (move_forward && move_right)
                    {
                        finalDirection = Quaternion.Euler(0, 0, -45) * forwardDirection;
                    }
                    else if (move_forward && move_left)
                    {
                        finalDirection = Quaternion.Euler(0, 0, 45) * forwardDirection;
                    }
                    else if (move_back && move_right)
                    {
                        movespeed = character.walkSpeed;
                        movespeed = movespeed * character.strafeRate;
                        finalDirection = Quaternion.Euler(0, 0, -135) * forwardDirection;
                    }
                    else if (move_back && move_left)
                    {
                        movespeed = character.walkSpeed;
                        movespeed = movespeed * character.strafeRate;
                        finalDirection = Quaternion.Euler(0, 0, 135) * forwardDirection;
                    }
                    else if (move_back)
                    {
                        movespeed = character.walkSpeed;
                        finalDirection = Quaternion.Euler(0, 0, 180) * forwardDirection;
                        movespeed = movespeed * character.strafeRate;
                    }
                    else if (move_left)
                    {
                        movespeed = character.walkSpeed;
                        finalDirection = Quaternion.Euler(0, 0, 90) * forwardDirection;
                        movespeed = movespeed * character.strafeRate;

                    }
                    else if (move_right)
                    {
                        movespeed = character.walkSpeed;
                        finalDirection = Quaternion.Euler(0, 0, -90) * forwardDirection;
                        movespeed = movespeed * character.strafeRate;
                    }
                    break;
                case MoveControls.Fixed8Ways:
                    character.SetFacing(target);
                    
                    if (move_forward && move_right)
                    {
                        finalDirection = new Vector3(0.707f, 0.707f, 0);
                    }
                    else if (move_forward && move_left)
                    {
                        finalDirection = new Vector3(-0.707f, 0.707f, 0);
                    }
                    else if (move_back && move_right)
                    {
                        finalDirection = new Vector3(0.707f, -0.707f, 0);
                    }
                    else if (move_back && move_left)
                    {
                        finalDirection = new Vector3(-0.707f, -0.707f, 0);
                    }
                    else if (move_back)
                    {
                        finalDirection = new Vector3(0, -1, 0);
                    }
                    else if (move_left)
                    {
                        finalDirection = new Vector3(-1, 0, 0);
                    }
                    else if (move_right)
                    {
                        finalDirection = new Vector3(1, 0, 0);
                    }
                    else
                    {
                        finalDirection = new Vector3(0, 1, 0);
                    }
                    
                    break;
                default:
                    break;
            }
            

            if (is_moving)
            {
                //print("Speed: " + Vector3.Magnitude(finalDirection * movespeed * Time.deltaTime));
                Vector3 scaledDir = (finalDirection * movespeed * Time.deltaTime);
                newPos = transform.position + scaledDir;
                character.rigidbod.AddForce(scaledDir * 5000);
            }
            // ---- END MOVING ---- //
        }
    }
}
