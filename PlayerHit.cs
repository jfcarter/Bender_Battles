using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHit : MonoBehaviour {

    Rigidbody rb;
    int health = 4;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (health <= 0)
        {
            Lose();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject go = collision.gameObject;
        Debug.Log(go.name + " hit with tag " + go.tag);
        if (go.transform.parent.name == "EarthDiscsRed" && tag == "BluePlayer")
        {
            LaunchBack(go);
        }
        else if (go.transform.parent.name == "EarthDiscsBlue" && tag == "RedPlayer")
        {
            LaunchBack(go);
        }
        else if (go.transform.parent.name == "ProjectilesBlue" && tag == "RedPlayer")
        {
            LaunchBack(go);
        }
        else if (go.transform.parent.name == "ProjectilesRed" && tag == "BluePlayer")
        {
            LaunchBack(go);
        }
    }

    private void LaunchBack(GameObject projectile)
    {
        Debug.Log("launching back");
        Vector3 vel = projectile.GetComponent<Rigidbody>().velocity;
        vel.y = 1;
        rb.velocity = vel;
        Destroy(projectile); //shouldn't be destroying. limited number of fireballs in object pool. limited number of discs in holes.
        health--;
    }

    private void Lose()
    {
        Debug.Log("you lost");
    }
}
