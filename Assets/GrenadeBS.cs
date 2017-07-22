using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeBS : MonoBehaviour
{
    public float fuseTime = 5f; // seconds of fuse before explosion
    public Rigidbody2D rb;
    public float explosionRadius = 1f; // radius around grenade which damage is dealt
    public int damage = 100; // the damage for soft targets directly under the grenade when it explodes
    public bool armed = false; // whether grenade has been armed (pulled pin)
    public float damageDropoff = 0.25f; // percent less damage delt for every 1 unit away from the explosion
    public float maxForce = 2f; // controls how far it can be thrown
    public GameObject User;
    public Vector3 targetPos = Vector3.zero;
    public static float maxTravTime = 1f; // how close to target spot do we have to be to stop moving
    public float travTime = 0f;
    bool reached = false;
    public int explodeVolume = 100;
    public AudioClip shotClip;
    public Collider2D rb_collider;

    // called on load
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    // called every frame
    void Update()
    {

        if (armed)
        {
            fuseTime -= Time.deltaTime; // decrease fuse by how much time has passed since the last frame
            if (fuseTime <= 0)
            {
                explode();
                armed = false; // dont blow up several times!
            }

            if (reached == false && ( travTime >= maxTravTime))
            {
                rb.drag = rb.drag * 50; //  make it start rolling
                reached = true;
            }
            else
            {
                travTime += Time.deltaTime;
            }
        }
        
    }


    // chuck the grenade towards target, set fuse, boom...
    public void throwGrenade(Vector3 targetDir)
    {
        rb_collider.enabled = true;
        // TODO apply maximum throw distance here
        if (targetDir.magnitude > maxForce)
        {
            targetDir.Normalize();
            targetDir *= maxForce;
        }
        targetPos = transform.position + targetDir;
        rb.AddForce(targetDir);
        
        armed = true;

    }

    // explode the grenade, injure those nearby
    public void explode()
    {

        Character_BS userChar = User.GetComponent<Character_BS>();
        GameStateBS GAMESTATE = userChar.GAMESTATE;

        
        foreach(GameObject go in GAMESTATE.ALL_CHARACTERS)
        {
            
            float dist = Vector3.Distance(go.transform.position, transform.position);

            if (dist <= explosionRadius)
            {
                Character_BS targetChar = go.GetComponent<Character_BS>();
                int realDam = Mathf.FloorToInt(damage * (1 - dist * damageDropoff));

                // SET MIN DEPTH TO 1, so grenade wont be hit!
                RaycastHit2D[] hitInfo = Physics2D.RaycastAll(transform.position, go.transform.position - transform.position, dist, -1, 1);

                Collider2D targColl = go.GetComponent<Collider2D>();
                Collider2D grenColl = GetComponent<Collider2D>();
                bool isHit = true;
                foreach(RaycastHit2D hit in hitInfo)
                {
                    if (hit.collider == null || hit.collider == targColl)
                    {
                        // could be target, could be nothing
                    }
                    else if(hit.collider == grenColl)
                    {
                        // grenade hit itself
                    }
                    else
                    {
                        // grenade hit a wall, stops the hit from being processed
                        isHit = false;
                        //print("Missed: " + hit.collider.gameObject);
                    }
                }
                if(isHit == true)
                {
                    //print("Hit: " + targetChar.gameObject);
                    targetChar.hitByExplosion(this, realDam);
                }
                
                    
            }
        }
        if (shotClip != null)
        {
            AudioSource.PlayClipAtPoint(shotClip, transform.position);
        }
        GAMESTATE.MakeNoise(gameObject, transform.position, explodeVolume);
        GameObject.Destroy(gameObject, 0.05f);
    }



}