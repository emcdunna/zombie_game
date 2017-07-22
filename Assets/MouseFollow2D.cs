using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFollow2D : MonoBehaviour {

    private Vector3 mousePosition;
    public float moveSpeed = 500.0f;
    public bool FollowEnabled = true;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (FollowEnabled)
        {
            mousePosition = Input.mousePosition;
            mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            transform.position = Vector2.Lerp(transform.position, mousePosition, moveSpeed);
        }

    }
}

