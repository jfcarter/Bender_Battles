using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Player Climb|Locomotion|20120
namespace VRTK
{
    using GrabAttachMechanics;
    using UnityEngine;

    public class WeaponSelect : MonoBehaviour
    {
        public List<GameObject> elements;
        public List<GameObject> elementModels;
        public float radius = 3;
        public float angleOffset = 2.74f;
        public float hideSelectorRange = 4;

        private Transform playArea;
        private uint controllerIndex;

        protected virtual void Awake()
        {
            playArea = VRTK_DeviceFinder.PlayAreaTransform();
        }

        protected virtual void OnEnable()
        {
            InitListeners(true);
        }

        protected virtual void OnDisable()
        {
            InitListeners(false);
        }

        private void InitListeners(bool state)
        {
            InitControllerListeners(VRTK_DeviceFinder.GetControllerLeftHand(), state);
            InitControllerListeners(VRTK_DeviceFinder.GetControllerRightHand(), state);
        }

        private GameObject InstatiateSelectableElement(int i, GameObject parent, GameObject controller)
        {
            GameObject weaponPrefab = elements[i];
            GameObject weaponModel = elementModels[i];

            //Get positions in a circle around controller
            float angle = (i + angleOffset) * Mathf.PI * 2 / elements.Count;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;

            GameObject displayModel = Instantiate(weaponModel, parent.transform);

            weaponModel.tag = "SelectableWeapon";
            weaponModel.name = "WeaponSelectionModel";

            weaponModel.transform.rotation = controller.transform.rotation;
            weaponModel.transform.localPosition = pos;
            return weaponModel;
        }

        private void OnMenuPressed(object sender, ControllerInteractionEventArgs e)
        {
            GameObject selector = new GameObject();
            var controller = ((VRTK_ControllerEvents)sender).gameObject;
            selector.transform.position = controller.transform.position;
            selector.transform.rotation = controller.transform.rotation * Quaternion.Euler(90, 0, 0);
            selector.name = "Selector" + e.controllerIndex;
            selector.transform.parent = playArea;

            for (int i = 0; i < elements.Count; i++)
            {
                GameObject newWeaponModel = InstatiateSelectableElement(i, selector, controller);
            }
        }

        private void OnMenuReleased(object sender, ControllerInteractionEventArgs e)
        {
            GameObject selector = playArea.transform.Find("Selector" + e.controllerIndex).gameObject;
            Destroy(selector);
        }



        private void InitControllerListeners(GameObject controller, bool state)
        {
            if (controller)
            {
                var eventScript = controller.GetComponent<VRTK_ControllerEvents>();
                if (eventScript)
                {
                    if (state)
                    {
                        eventScript.ButtonTwoPressed += new ControllerInteractionEventHandler(OnMenuPressed);
                        eventScript.ButtonTwoReleased += new ControllerInteractionEventHandler(OnMenuReleased);
                    }
                    else
                    {
                        eventScript.ButtonTwoPressed -= new ControllerInteractionEventHandler(OnMenuPressed);
                        eventScript.ButtonTwoReleased -= new ControllerInteractionEventHandler(OnMenuReleased);
                    }
                }
            }
        }
    }
}