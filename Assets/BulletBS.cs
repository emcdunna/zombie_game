using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBS : MonoBehaviour {

    public GameObject shooter;
    private Rigidbody2D rb;
    public int velocity = 100; // dummy
    public WeaponBS weapon; // NEED TO SET THIS

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    // function to launch the bullet
    public void Shoot(Vector2 Direction)
    {
        float minR = -weapon.recoil;
        float maxR = weapon.recoil;
        // recoil (random direction)
        Character_BS sChar = shooter.GetComponent<Character_BS>();
        if (shooter != null && sChar != null)
        {
            // triple recoil if the shooter is moving
            if (sChar.State == Character_BS.AnimState.Walk || sChar.State == Character_BS.AnimState.Run)
            {
                minR *= 3;
                maxR *= 3;
            }

        }
        
        float recoil = Random.Range(minR, maxR);
        Direction.Normalize();
        Vector2 rotatedVector = Quaternion.AngleAxis(recoil, Vector3.forward) * Direction;

        // random velocity
        float randVel = Random.Range(0.9f, 1.15f);

        rb = GetComponent<Rigidbody2D>();
        rotatedVector.Normalize();

        rb.AddForce(rotatedVector * velocity * randVel);

        GameObject.Destroy(gameObject, 3);
    }

    // bullet hits something
    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool doNotDestroy = false;
        GameObject target = collision.gameObject;
        Character_BS targetChar = target.GetComponent<Character_BS>();

        // THROWS ERROR after player dies
        if (target.tag == "Bullet" || target == weapon.item.gameObject || target == shooter) 
        {
            // Dont destroy for colliding with the weapon or shooter or bullets
            doNotDestroy = true;
        }
        else if (targetChar != null)
        {

            targetChar.hitByProjectile(this, shooter.GetComponent<Character_BS>());
        }
        
        
        // get rid of the bullet if this was a valid collision
        if (doNotDestroy == false)
        {
            GameObject.Destroy(gameObject, 0.01f);
        }
    }
}
