using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whip : MonoBehaviour {

    //end of whip
    public GameObject end;
    //VR motion controller
    SteamVR_Controller.Device controller;

    // Use this for initialization
    void Start () {
        //Creates tracked controller
        SteamVR_TrackedObject trackedObj = GetComponent<SteamVR_TrackedObject>();
        controller = SteamVR_Controller.Input((int)trackedObj.index);
    }
	
	// Update is called once per frame
	void Update () {
        end.transform.position = transform.position;
	}
}
