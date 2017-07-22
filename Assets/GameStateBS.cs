using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateBS : MonoBehaviour {

    public GameObject prefabBullet;
    public GameObject prefabZombie;
    public GameObject prefabHuman;
    public GameObject Player1;
    public List<GameObject> ALL_CHARACTERS = new List<GameObject>();
    public List<MapNodeBS> mapNodes = new List<MapNodeBS>();
    public GameModeBS gameMode;
    public MapBS map;
    public float minLootDist = 0.05f;
    public float MaxMapNodeDist = 5f; // only used for drawing X's in path finding
    public bool isPaused = false;
    public int maxIter = 10000;
    public int maxPaths = 15;
    public List<BarricadeBS> ALL_BARRICADES = new List<BarricadeBS>();

    // Use this for initialization
    void Start () {
        map = GetComponentInChildren<MapBS>();
        mapNodes.AddRange(map.GetComponentsInChildren<MapNodeBS>());
        gameMode = GetComponentInChildren<GameModeBS>();
        ALL_BARRICADES.AddRange(map.GetComponentsInChildren<BarricadeBS>());
        makeMap();
    }

    // Update is called once per frame
    void Update()
    {
        float val = 1.0f / Time.deltaTime;
        if (val < 20)
        {
            print("Slowdown framerate at " + Time.fixedTime + " seconds.");
        }

        
        
        
    }

    // returns the vector in gamespace that corresponds to this map node
    public Vector3 mapNodeToVector(MapNodeBS node)
    {
        Vector3 result = new Vector3(node.transform.position.x, node.transform.position.y, 10);
        return result;
    }

    // returns the closest map node to the loc that is visible (not through a wall or person)
    public MapNodeBS VectorToMapNode(Vector3 loc)
    {
        MapNodeBS closestNode = null;
        float closestDist = Mathf.Infinity;
        foreach(MapNodeBS node in mapNodes)
        {
            // can we see the node?
            if (canSeeIgnoreCharacters(loc, node.transform.position))
            {
                // is it the closest?
                float dist = Vector3.Distance(loc, node.transform.position);
                if (dist <= closestDist)
                {
                    closestDist = dist;
                    closestNode = node;
                }
            }
        }
        
        return closestNode;
    }

    // can you move from one map node to another
    bool canMove(MapNodeBS start, MapNodeBS destination)
    {
        bool result = false;
        Color color = Color.red;
        Vector3 startPos = mapNodeToVector(start);
        Vector3 destPos = mapNodeToVector(destination);
        float dist = Vector3.Distance(startPos, destPos);
        if (MaxMapNodeDist > dist)
        {
            RaycastHit2D hitInfo = Physics2D.Raycast(startPos, destPos - startPos, dist, -1, 5f);

            // is there a collider in the way with hieght at least 5 (walls)
            if (hitInfo.collider == null)
            {
                //color = Color.green;
                //Debug.DrawLine(mapNodeToVector(start), mapNodeToVector(destination), color, 2000f);
                result = true;
            }
            else
            {
                //Debug.DrawLine(mapNodeToVector(start), mapNodeToVector(destination), color, 2000f);
            }

        }

        return result;
    }

    // can you see from one spot to another
    public bool canSee(Vector3 start, Vector3 destination)
    {
        bool result = false;
        RaycastHit2D hitInfo = Physics2D.Raycast(start, destination - start, Vector3.Distance(start,destination));

        // is there a collider in the way?
        if (hitInfo.collider == null)
        {
            result = true;
        }

        return result;
    }

    // can see, but ignore a list of game objects
    public bool canSee(Vector3 start, Vector3 destination, List<GameObject> ignoreList)
    {
        RaycastHit2D[] allHits = Physics2D.RaycastAll(start, destination - start, Vector3.Distance(start, destination));
        foreach (RaycastHit2D hit in allHits)
        {
            GameObject go = hit.collider.gameObject;
            if (go != null && ignoreList.Contains(go))
            {
                //print("Sight not blocked by " + go.name);
            }
            else
            {
                return false;
            }

        }
        return true;
    }
    // can one game object see another
    public bool canSee(GameObject observer, GameObject target)
    {
        RaycastHit2D[] allHits = Physics2D.RaycastAll(observer.transform.position, target.transform.position - observer.transform.position);
        Collider2D[] targetGos = target.GetComponentsInChildren<Collider2D>();
        List<Collider2D> tgs = new List<Collider2D>();
        tgs.AddRange(targetGos);
        foreach( RaycastHit2D hit in allHits )
        {
            if(hit.collider.gameObject == observer)
            {
                // nothing
            }
            else if( hit.collider.gameObject == target || tgs.Contains(hit.collider))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    // ignores character objects when doing a "can see"
    public bool canSeeIgnoreCharacters(Vector3 start, Vector3 destination)
    {
        RaycastHit2D[] allHits = Physics2D.RaycastAll(start, destination - start, Vector3.Distance(start, destination));
        foreach (RaycastHit2D hit in allHits)
        {
            GameObject go = hit.collider.gameObject;
            Character_BS ch = null;
            if (go!= null)
            {
                ch = go.GetComponent<Character_BS>();
            }
            
            if (ch != null)
            {
                //print("Sight not blocked by " + go.name);
            }
            else
            {
                return false;
            }
            
        }
        return true;
    }


    // finds path between two locations, in squares
    public List<MapNodeBS> findPath(Vector3 start, Vector3 end)
    {
        MapNodeBS startNode = VectorToMapNode(start);
        MapNodeBS endNode = VectorToMapNode(end);
        
        Queue<List<MapNodeBS>> pathsQueue = new Queue<List<MapNodeBS>>(); // queue of paths to process
        List<MapNodeBS> path = new List<MapNodeBS>();
        path.Add(startNode);
        pathsQueue.Enqueue(path);

        List<List<MapNodeBS>> successPaths = new List<List<MapNodeBS>>();
        int numPaths = 0;
        int i = 0;

        // iteratively go through all paths
        while ((i < maxIter) && (numPaths < maxPaths))
        {
            i++;
            if (pathsQueue.Count > 0)
            {
                path = pathsQueue.Dequeue();
            }
            else
            {
                // just break out of while and let it continue processing what we have
                break;
            }
            MapNodeBS nd = path[path.Count - 1];

            // does the path contain the destination?
            if (nd == endNode)
            {
                successPaths.Add(path);
                numPaths += 1;   
            }
            if (nd != null && nd.adjacentNodes != null)
            {
                foreach (MapNodeBS adj in nd.adjacentNodes)
                {

                    // is this node already in our path? if so, dont use it
                    if (path.Contains(adj) == false)
                    {
                        List<MapNodeBS> newPath = new List<MapNodeBS>();
                        foreach (MapNodeBS mp in path)
                        {
                            newPath.Add(mp);
                        }
                        newPath.Add(adj);
                        pathsQueue.Enqueue(newPath); // enqueue new path
                    }
                }
            }
            
            
        }

        // how many good paths did we get? look through and return one
        if(numPaths > 0)
        {
            float totalLength = 0;
            float minLength = Mathf.Infinity;
            List<MapNodeBS> bestPath = null;
            MapNodeBS prevP = null;
            //Dictionary<List<MapNodeBS>, float> pathLengths = new Dictionary<List<MapNodeBS>, float>(); 
            foreach (List<MapNodeBS> l in successPaths)
            {
                totalLength = 0;
                prevP = null;
                foreach (MapNodeBS p in l)
                {
                    if(prevP != null)
                    {
                        //Debug.DrawLine(mapNodeToVector(p), mapNodeToVector(prevP), Color.blue, 2000f);
                        totalLength += Vector3.Distance(mapNodeToVector(p), mapNodeToVector(prevP));
                    }
                    prevP = p;
                    
                }
                if(totalLength < minLength)
                {
                    bestPath = l;
                    minLength = totalLength;
                }
                //print(totalLength);
                //pathLengths[l] = totalLength;
                
            }

            // TODO: Instead select randomly from a few good paths
            // DRAW BEST PATH
            prevP = null;
            foreach (MapNodeBS p in bestPath)
            {
                if (prevP != null)
                {
                    Debug.DrawLine(mapNodeToVector(p), mapNodeToVector(prevP), Color.yellow, 10f);
                    totalLength += Vector3.Distance(mapNodeToVector(p), mapNodeToVector(prevP));
                }
                prevP = p;

            }

            return bestPath; 
        }
        else
        {
            return null;
        }
        

    }

    // connects all existing map nodes
    void makeMap()
    {
        foreach(MapNodeBS start in mapNodes)
        {
            foreach (MapNodeBS dest in mapNodes)
            {
                if(start != dest)
                {
                    if (canMove(start, dest))
                    {
                        start.addAdjacentNode(dest);
                    }
                }
            }
        }
    }
    

    // TODO: Implement finding of nearest loot item
    public GameObject findNearestLoot(GameObject looter)
    {
        return null;
    }
	


    

    // pauses game
    public void togglePaused()
    {
        if(isPaused == false)
        {
            Time.timeScale = 0;
            isPaused = true;
            foreach(GameObject go in ALL_CHARACTERS)
            {
                Character_BS chr = go.GetComponent<Character_BS>();
                if(chr != null)
                {
                    chr.freezeChar = true;
                }
            }
            TargetBS target = Player1.GetComponentInChildren<TargetBS>();
            if(target != null)
            {
                target.freezeTarget = true;
            }
        }
        else
        {
            Time.timeScale = 1;
            isPaused = false;
            foreach (GameObject go in ALL_CHARACTERS)
            {
                Character_BS chr = go.GetComponent<Character_BS>();
                if (chr != null)
                {
                    chr.freezeChar = false;
                }
            }
            TargetBS target = Player1.GetComponentInChildren<TargetBS>();
            if (target != null)
            {
                target.freezeTarget = false;
            }
        }
        
    }

    // handles the GUI state
    private void OnGUI()
    {
        PlayerBS pl1 = Player1.GetComponent<PlayerBS>();
        float w = Screen.width;
        float h = Screen.height;
        UI_menu pause_menu = new UI_menu(Screen.width * 0.15f, Screen.height * 0.15f, Screen.width * 0.7f, Screen.height * 0.7f, 4, 1, true);
        UI_menu player_stats = new UI_menu(w * 0f, h * 0.8f, w * 0.25f, h * 0.2f, 4, 2, true);
        UI_menu weapons = new UI_menu(w * 0.6f, h * 0.8f, w * 0.4f, h * 0.2f, 3, 3, true);


        if (isPaused == true)
        {
            GUI.Box(pause_menu.mainrect, "Pause menu");
            if (GUI.Button(pause_menu.sub_rects[0],"Continue game"))
            {
                togglePaused();
            }
            if (GUI.Button(pause_menu.sub_rects[1], "Inventory"))
            {
               
            }
            if (GUI.Button(pause_menu.sub_rects[2], "Settings"))
            {
               
            }
            if (GUI.Button(pause_menu.sub_rects[3], "Quit Game"))
            {
                
            }
            
        } else
        {
            GUI.Box(player_stats.mainrect, pl1.character.name);
            
            GUI.Box(player_stats.sub_rects[0], "Health");
            GUI.Box(player_stats.sub_rects[1], "" + (pl1.character.health));

            GUI.Box(weapons.mainrect, "Weapons");

            string rw = "None";
            string mw = "None";
            string tw = "None";
            string rw2 = "None";
            string mw2 = "None";
            string tw2 = "None";

            if (pl1.character.missileWeapon != null)
            {
                rw = pl1.character.missileWeapon.name;
                rw2 = pl1.character.missileWeapon.getAmmoString();
            }
            if(pl1.character.meleeWeapon != null)
            {
                mw = pl1.character.meleeWeapon.name;
                mw2 = pl1.character.meleeWeapon.getAmmoString();
            }
            if(pl1.character.tertiaryWeapon != null)
            {
                tw = pl1.character.tertiaryWeapon.name;
                tw2 = "???"; // TODO : replace with # of grenades
            }
            GUI.Box(weapons.sub_rects[0], "Ranged");
            GUI.Box(weapons.sub_rects[1], rw);
            GUI.Box(weapons.sub_rects[2], rw2);
            GUI.Box(weapons.sub_rects[3], "Melee");
            GUI.Box(weapons.sub_rects[4], mw);
            GUI.Box(weapons.sub_rects[5], mw2);
            GUI.Box(weapons.sub_rects[6], "Tertiary");
            GUI.Box(weapons.sub_rects[7], tw);
            GUI.Box(weapons.sub_rects[8], tw2);

        }



    }

    // throw a grenade!
    public void MakeTertiaryAttack(GameObject attacker, Vector2 direction, GrenadeBS grenade)
    {
        GameObject grenObj = grenade.gameObject;
        SpriteRenderer sr = grenObj.GetComponent<SpriteRenderer>();
        sr.enabled = true;
        
        //grenade.GetComponent<ItemBS>().coldr.enabled = true; THIS IS DONE IN THE GRENADE CLASS
        grenObj.transform.position = attacker.transform.position;
        grenObj.transform.rotation = attacker.transform.rotation;
        grenObj.transform.parent = gameObject.transform; // set gamestate to parent, not shooter
        grenade.User = attacker;
        grenade.throwGrenade(direction);

        Character_BS att_char = attacker.GetComponent<Character_BS>();
        att_char.tertiaryWeapon = null; // TODO: set this to the next tertiary weapon

    }

    public void MakeAttack(GameObject attacker, Vector2 direction, WeaponBS weapon)
    {
        // checks for validity in the character class
        GameObject prefab;
        GameObject newMissile;
        GameObject newMelee;
        bool didAttack = true;
        Transform shotspawn = weapon.shotspawn; // shortcut

        switch (weapon.type)
        {
            // creates an object to act like projectile
            case WeaponBS.Type.Projectile:
                if(weapon.ClipItem.rounds > 0)
                {
                    prefab = weapon.Missile_Prefab;
                    newMissile = Instantiate(prefab, shotspawn.position, shotspawn.rotation) as GameObject;
                    BulletBS newBBS = newMissile.GetComponent<BulletBS>(); // mono behave, act like bullet (maybe need to add more variety)
                    newBBS.weapon = weapon;
                    newBBS.shooter = attacker;
                    newBBS.Shoot(direction);
                }
                else
                {
                    didAttack = false;
                }

                break;
            // detects hits with raycast
            case WeaponBS.Type.Missile:
                if (weapon.ClipItem.rounds > 0)
                {
                    List<GameObject> ignoreList = new List<GameObject>();
                    ignoreList.Add(attacker);
                    
                    RaycastHit2D[] allHits = Physics2D.RaycastAll(weapon.shotspawn.position, direction, weapon.range);
                    foreach (RaycastHit2D hit in allHits)
                    {
                        GameObject go = hit.collider.gameObject;
                        BarricadeBS br = go.GetComponent<BarricadeBS>();

                        if (ignoreList.Contains(go) || br != null )
                        {
                            //print("SHOT not blocked by " + go.name);
                        }
                        else
                        {
                            Character_BS ch = go.GetComponent<Character_BS>();
                            if(ch != null)
                            {
                                ch.hitByMissile(weapon, attacker.GetComponent<Character_BS>());
                            }
                            break;
                        }

                    }
                    
                }
                else
                {
                    didAttack = false;
                }
                break;
            // creates a little nonmoving object to detect hits
            case WeaponBS.Type.Melee:
                prefab = weapon.Melee_Prefab;
                newMelee = Instantiate(prefab, shotspawn.position, shotspawn.rotation) as GameObject;

                MeleeAttackBS meleeattack = newMelee.GetComponent<MeleeAttackBS>();
                meleeattack.weapon = weapon;
                newMelee.transform.parent = attacker.transform;
                meleeattack.attacker = attacker;
                // still make noise when we attack

                break;
           
            default:
                break;
            
        }
        if (didAttack)
        {
            MakeNoise(attacker, shotspawn.position, weapon.noise);
            weapon.makeSound();
            weapon.attackUpdate(); // update the weapon
        }
    }

    // makes a sound at the location with certain volume
    public void MakeNoise(GameObject originator, Vector2 location, int volume)
    {
        Noise noise = new Noise(location, volume, originator);
        gameObject.BroadcastMessage("HearNoise", noise, SendMessageOptions.DontRequireReceiver);
    }

    // spawns a new zombie (and returns)
    public GameObject SpawnZombie(Vector2 spawnPoint)
    {
        GameObject zombie = Instantiate(prefabZombie, this.transform) as GameObject;
        ALL_CHARACTERS.Add(zombie);
        zombie.transform.position = spawnPoint;
        Character_BS zChar = zombie.GetComponent<Character_BS>();
        zChar.health = (int)UnityEngine.Random.Range(60, 100);
        zChar.maxHealth = zChar.health;
        zChar.BaseWalkSpeed = UnityEngine.Random.Range(0.25f, 0.35f);
        zChar.BaseRunSpeed = zChar.BaseWalkSpeed * 1.5f;
        
        //zChar.items; does the zombie have some items

        ZombieBS zBS = zombie.GetComponent<ZombieBS>();
        zBS.delayTime = UnityEngine.Random.Range(0.3f, 0.9f); // how long after going idle till they stop
        zBS.minIntensity = UnityEngine.Random.Range(0.5f, 1f); // how likely to respond to minor sounds
        zBS.hardDetectRange = UnityEngine.Random.Range(1.5f, 2f); // how close before hard target

        return zombie;
        
    }

    // Finds a vector which takes a human player away from nearby enemies
    internal Vector2 findFleeDirection(GameObject human)
    {
        // TODO: Make this work
        Vector2 destination = human.transform.position;
        Vector2 direction = new Vector2(0, 0);
        HumanBS humanBS = GetComponent<HumanBS>();
        if(humanBS != null)
        {

            if(humanBS.hardTarget != null)
            {
                direction = humanBS.hardTarget.transform.position - humanBS.transform.position;
            }

        }
        direction.Normalize();
        destination = destination + 3*direction;

        return destination;
    }
}

// the noise class, an instance of this specifies where a noise occurs, how loud it was, and who made it
public class Noise
{
    public Vector3 location;
    public int volume;
    public GameObject gameObject;

    public Noise(Vector3 loc, int vol, GameObject go)
    {
        location = loc;
        volume = vol;
        gameObject = go;
    }
}


// a square block of UI where there is a set number of columns and rows
public class UI_menu{
    public float offW = 0f;
    public float offH = 0f;
    public float W = 0f;
    public float H = 0f;
    public int rows = 0;
    public int columns = 0;
    public Rect mainrect;
    public List<Rect> sub_rects = new List<Rect>();
    

    public UI_menu(float o_W, float o_H, float Wi,float He, int rs, int cs, bool title)
    {
        offW = o_W;
        offH = o_H;
        W = Wi;
        H = He;
        rows = rs;
        columns = cs;
        mainrect = new Rect(offW, offH, W, H);

        float dW = W / (columns);
        float dH = H / (rows);

        if (title) {
            dH = H / (rows + 1);
            for (int i = 1; i < rows+1; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    sub_rects.Add(new Rect(offW + j * dW, offH + i * dH, dW, dH));
                }
            }
        }
        else
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    sub_rects.Add(new Rect(offW + j * dW, offH + i * dH, dW, dH));
                }
            }
        }

        
        
    }
}