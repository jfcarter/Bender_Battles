using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fire_Bending : MonoBehaviour
{
    //VR motion controller
    SteamVR_Controller.Device controller;
    //set fireball emitter (location of fireball
    public GameObject fireball_Emitter;
    //makes fireball appear when clicked
    public GameObject visual_Fireball;
    //controls speed of fireball
    float speed = 90.0f;
    //makes it so you cant spam fire
    float timer = 3.0f;
    //controls visibility of fireballs
    MeshRenderer visible;
    //checks to see if timer is up
    bool ReadyToFire = false;
    //holds original size of fireball
    Vector3 fireballStartSize;
    //Holds max size of fireball
    Vector3 fireSize;

    public bool client;
    public GameObject projectiles;

    int fireballIndex;

    // Use this for initialization
    void Start()
    {
        //Creates tracked controller
        SteamVR_TrackedObject trackedObj = GetComponentInParent<SteamVR_TrackedObject>();
        controller = SteamVR_Controller.Input((int)trackedObj.index);
        //Controls visibility of fireball
        visible = visual_Fireball.GetComponent<MeshRenderer>();
        //Holds original size of fireball
        fireballStartSize = visual_Fireball.transform.localScale;
        //Checks to see if max size of fireball is reached
        fireSize = new Vector3(0.256f, 0.256f, 0.256f);

        if (projectiles == null)
        {
            if (client)
            {
                projectiles = GameObject.Find("ProjectilesRed");
            }
            else
            {
                projectiles = GameObject.Find("ProjectilesBlue");
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        Vector3 size;

        //if button is pressed create fireball
        if (controller.GetPress(SteamVR_Controller.ButtonMask.Trigger))
        {
            if(timer < 3)
            {
                if (visual_Fireball.transform.localScale == fireSize)
                {
                    size = new Vector3(-.001f, -.001f, -.001f);
                    visual_Fireball.transform.localScale += size;
                }
                size = new Vector3(.001f, .001f, .001f);
                visual_Fireball.transform.localScale += size;

                ReadyToFire = true;
            }
        }

        if (ReadyToFire == true)
        {
            //Creates fireball
            MakeFireball();
            ShootFireball();
        }
    }

    void ShootFireball()
    {
        if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            //Get a fireball from the object pool.
            GameObject fireball = GetFireBall();
            fireball.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.player.ID); //Changes ownership when spawned
            fireball.transform.localScale = visual_Fireball.transform.localScale;
            fireball.transform.position = fireball_Emitter.transform.position;
            fireball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            //Kyle - Added PhotonNetwork so object can be tracked by the network
            //Temporary_Fireball_Handler = Instantiate(fireball, fireball_Emitter.transform.position, fireball_Emitter.transform.rotation) as GameObject;

            //Sometimes bullets may appear rotated incorrectly due to the way its pivot was set from the original modeling package.
            //This is EASILY corrected here, you might have to rotate it from a different axis and or angle based on your particular mesh.
            fireball.transform.Rotate(Vector3.left * 90);

            //Retrieve the Rigidbody component from the instantiated Bullet and control it.
            Rigidbody Temporary_RigidBody;
            Temporary_RigidBody = fireball.GetComponent<Rigidbody>();

            //Tell the bullet to be "pushed" forward by an amount set by Bullet_Forward_Force.
            Temporary_RigidBody.AddForce(-(transform.up * 1000));

            //Make fireball invisible
            StartCoroutine(DestroyFireball(3.0f));

            //set to false for timer
            ReadyToFire = false;
            timer = 4.0f;

            //Make fireball original size again
            visual_Fireball.transform.localScale = fireballStartSize;
        }
    }

    void MakeFireball ()
    {
        visible.enabled = true;
    }

    IEnumerator DestroyFireball(float timeToDestroy)
    {
        yield return new WaitForSeconds(timeToDestroy);
        //visible.enabled = false;
        //Temporarily disabled hiding fireballs cause doesn't work with object pooling.
    }

    private GameObject GetFireBall()
    {
        GameObject result = projectiles.transform.GetChild(fireballIndex).gameObject;
        fireballIndex = (fireballIndex + 1) % projectiles.transform.childCount;
        visible = result.GetComponent<MeshRenderer>();
        return result;
    }
}
