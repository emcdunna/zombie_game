using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPlayerTarget_BS : MonoBehaviour {
    public GameObject PlayerObject;
    public GameObject PlayerTarget;
    Camera cam;
    PlayerBS playerbs;
    public float minSize = 1.5f;
    public float maxSize = 2.5f;
    public float Zoom_speed = 6.0f;
    public float FOLLOWRATE = 0.1f; // smaller is faster
    public float ROTATIONRATE = 0.5f;
    public bool pauseCamRotation = true;

    // for linear interpolation
    float t;
    Vector3 startPosition;
    Vector3 target; 
    
    float timeToReachTarget;
    public GameObject playerCamTarget;

    public enum CameraMode { FollowPlayer, FollowNoRotate, Fixed };
    public CameraMode cameramode = CameraMode.FollowPlayer;

    // Use this for initialization
    void Start () {

        startPosition = target = transform.position;
        //startRotation = targetRotation = transform.rotation;

        cam = GetComponent<Camera>();
        playerbs = PlayerObject.GetComponent<PlayerBS>();
        PlayerTarget = playerbs.target;

        if(playerCamTarget == null)
        {
            //playerCamTarget
        }
    }

    // sets new destination for the camera
    public void SetDestination(Vector3 destination, float time)
    {
        t = 0;
        startPosition = transform.position;
        timeToReachTarget = time;
        target = destination;
    }


    // Update is called once per frame
    void Update () {
        
        t += Time.deltaTime / timeToReachTarget;
        transform.position = Vector3.Lerp(startPosition, target, t);        

        if (PlayerObject != null)
        {
            Vector3 player_tf = new Vector3(PlayerObject.transform.position.x, PlayerObject.transform.position.y, PlayerObject.transform.position.z);

            if (playerCamTarget != null)
            {
                player_tf = new Vector3(playerCamTarget.transform.position.x, playerCamTarget.transform.position.y, -2);
            }

            // Follow Player Camera mode
            if (CameraMode.FollowPlayer == cameramode)
            {
                // MOVE TO LOCATION
                SetDestination(player_tf, FOLLOWRATE);

                // ROTATE TO MATCH TARGET
                if (pauseCamRotation == false)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, PlayerObject.transform.rotation, ROTATIONRATE);
                }
           
                // ZOOM SETTINGS
                float curr_size = cam.orthographicSize;

                bool zoom_in = false;
                bool zoom_out = false;
                float zoomNum = Input.GetAxis("Mouse ScrollWheel");

                if (zoomNum > 0 )
                {
                    zoom_in = true;
                }
                else if (zoomNum < 0)
                {
                    zoom_out = true;
                }
                if ((zoom_in == true) && (curr_size > minSize))
                {
                    cam.orthographicSize -= (curr_size * Time.deltaTime * Zoom_speed);
                }
                if ((zoom_out == true) && (curr_size < maxSize))
                {
                    cam.orthographicSize += (curr_size * Time.deltaTime * Zoom_speed);
                }
            }
       
        }

    }
}
