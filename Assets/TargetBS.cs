using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class TargetBS : MonoBehaviour {
    public float moveSpeed = 1000.0f;
    public bool FollowEnabled = true;
    public float Xsensitivity = 0.12f;
    public float Ysensitivity = 2f;
    public GameObject Player;
    public bool freezeTarget = false;

    public float maxDist = 15f;
    public float minDist = 0.5f;

    // distance from player, angle at which target is compared to world
    public float gameAngle = 0f;
    public float gameDist = 1f;

    // Use this for initialization
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        float deltaX = 0f;
        float deltaY = 0f;
        Vector3 NewXY = transform.position;
        float lookX = 0f;
        float lookY = 1f;
        PlayerBS player = Player.GetComponentInChildren<PlayerBS>();
        if (freezeTarget == false)
        {
            if (FollowEnabled)
            {


                deltaX = Input.GetAxis("Mouse X") * Xsensitivity;
                deltaY = Input.GetAxis("Mouse Y") * Ysensitivity;

                gameAngle -= deltaX;
                lookX = gameDist * Mathf.Cos(gameAngle);
                lookY = gameDist * Mathf.Sin(gameAngle);

                Vector3 rotation = new Vector3(transform.parent.position.x + lookX, transform.parent.position.y + lookY, 0);

                float newY = transform.localPosition.y + deltaY;
                newY = Mathf.Clamp(newY, minDist, maxDist);


                NewXY = new Vector3(transform.localPosition.x, newY, 0);
                transform.localPosition = NewXY; // sets the Y param, distance away
                player.character.SetFacing(rotation);

            }
        }

    }
}

