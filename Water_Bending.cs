using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water_Bending : MonoBehaviour
{

    //This holds the whip of water
    public GameObject Water_Whip;
    //Used to see if you should move the whip or not
    bool moveWhip = false;
    //location for the end of the line where the whip will go
    Vector3 whipEnd;
    //VR motion controller
    SteamVR_Controller.Device controller;
    //holds where ray goes. Beginning and end of line.
    private Vector3[] positions = new Vector3[2];
    //used for check to see if collider on kyles are hit
    RaycastHit hit;
    LineRenderer lRender;
    //Move dot
    Vector3 direction = new Vector3(0.0f, 0.0f, 0.0f);


    // Use this for initialization
    void Start()
    {
        //Creates tracked controller
        SteamVR_TrackedObject trackedObj = GetComponentInParent<SteamVR_TrackedObject>();
        controller = SteamVR_Controller.Input((int)trackedObj.index);
        //Used to create line
        lRender = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //creates a ray for kyles as well as the locker
        //Raycast();

        if(controller.GetPress(SteamVR_Controller.ButtonMask.Trigger))
        {
            direction = transform.position - Water_Whip.transform.position;
            Water_Whip.transform.position += direction * Time.deltaTime * 4.0f;
        }
        else 
        {
            if ((direction.magnitude <= 3.0))
            {
                direction = Water_Whip.transform.position - transform.position;
                Water_Whip.transform.position += direction * Time.deltaTime * 4.0f;
            }
        }
    }

    void Raycast()
    {
        //creates ray
        Ray r = new Ray(transform.position, -(transform.up));
        Debug.DrawRay(transform.position, -(transform.up));

        //if ray hits a kyle you make it display
        if (Physics.Raycast(r, out hit, 4.0f))
        {
            DisplayLine(true, hit.point);
        }
        else
        {
            DisplayLine(false, hit.point);
        }
    }

    void DisplayLine(bool check, Vector3 endpoint)
    {
        //turn on or off line if kyle is in contact with line
        lRender.enabled = check;
        //end and beginning of line
        positions[0] = transform.position;
        positions[1] = endpoint;
        lRender.SetPositions(positions);
    }
}
