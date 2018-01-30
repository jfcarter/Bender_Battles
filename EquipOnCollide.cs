using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipOnCollide : MonoBehaviour {

    public GameObject curWeapon;


    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("SelectableWeapon"))
        {
            GameObject weaponPrefab = col.gameObject.GetComponent<EquippableWeapon>().weaponPrefab;
            Destroy(curWeapon);
            curWeapon = Instantiate(weaponPrefab, transform.parent);
        }
    }
}