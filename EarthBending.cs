using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Player Climb|Locomotion|20120
namespace VRTK
{
    using GrabAttachMechanics;
    using UnityEngine;

    public class EarthBending : MonoBehaviour
    {
        [Tooltip("Will scale movement up and down based on the player transform's scale.")]
        public bool usePlayerScale = true;

        public GameObject earthDiscs;
        public float throwMultiplier = 2.0f;
        public float hoverFollowSpeed = 0.05f;
        public float hoverDistance = 0.6f;
        public float curveSpeed = 20.0f;
        public float curveControlTime = 1.5f;

        private Transform playArea;
        private GameObject grabbedDisc;
        private Rigidbody grabbedDiscRb;
        private GameObject releasedDisc;
        private Rigidbody releasedDiscRb;
        private Vector3 releasePos;
        private Vector3 releaseDir;

        private uint controllerIndex;

        private bool client;

        protected virtual void Awake()
        {
            playArea = VRTK_DeviceFinder.PlayAreaTransform();
            if (earthDiscs == null)
            {

            }
        }

        protected virtual void OnEnable()
        {
            InitListeners(true);
        }

        protected virtual void OnDisable()
        {
            if (grabbedDisc)
            {
                UngrabDisc(false, 0);
            }
            InitListeners(false);
        }

        protected virtual void Update()
        {
            if (grabbedDisc != null)
            {
                HoverGrabbedDisc();
            }
            if (releasedDisc != null)
            {
                CurveAfterThrow();
            }
        }

        private void InitListeners(bool state)
        {
            InitControllerListeners(this.gameObject, state);

            InitTeleportListener(state);
        }

        private void InitTeleportListener(bool state)
        {
            var teleportComponent = GetComponent<VRTK_BasicTeleport>();
            if (teleportComponent)
            {
                if (state)
                {
                    teleportComponent.Teleporting += new TeleportEventHandler(OnTeleport);
                }
                else
                {
                    teleportComponent.Teleporting -= new TeleportEventHandler(OnTeleport);
                }
            }
        }

        private void OnTeleport(object sender, DestinationMarkerEventArgs e)
        {
            UngrabDisc(false, e.controllerIndex);
        }

        private Vector3 GetScaledLocalPosition(Transform objTransform)
        {
            if (usePlayerScale)
            {
                return playArea.localRotation * Vector3.Scale(objTransform.localPosition, playArea.localScale);
            }

            return playArea.localRotation * objTransform.localPosition;
        }

        private void OnTriggerPressed(object sender, ControllerInteractionEventArgs e)
        {
            if (!grabbedDisc)
            {
                var controller = ((VRTK_ControllerEvents)sender).gameObject;
                var actualController = VRTK_DeviceFinder.GetActualController(controller);
                controllerIndex = e.controllerIndex;
                GrabDisc(actualController, e.controllerIndex);
            }
        }

        private void OnTriggerReleased(object sender, ControllerInteractionEventArgs e)
        {
            if (grabbedDisc)
            {
                var controller = ((VRTK_ControllerEvents)sender).gameObject;
                var actualController = VRTK_DeviceFinder.GetActualController(controller);
                UngrabDisc(true, e.controllerIndex);
            }
        }

        private void GrabDisc(GameObject currentGrabbingController, uint controllerIndex)
        {
            grabbedDisc = GetEarthDisc();
            grabbedDisc.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.player.ID); //Changes ownership when grabbed
            grabbedDisc.transform.position = grabbedDisc.transform.position + new Vector3(0,0.1f,0);
            grabbedDiscRb = grabbedDisc.GetComponent<Rigidbody>();
            grabbedDiscRb.constraints = RigidbodyConstraints.None;
            grabbedDisc.transform.rotation = Quaternion.Euler(new Vector3 (80, 0, 0));
            grabbedDiscRb.velocity = Vector3.zero;
            grabbedDiscRb.angularVelocity = new Vector3(0.0f, 50.0f, 0.0f);
            grabbedDisc.GetComponent<Collider>().enabled = true;
        }

        private void HoverGrabbedDisc()
        {
            Vector3 targetPos = transform.position + Vector3.ProjectOnPlane((transform.up * -hoverDistance),Vector3.up);
            grabbedDisc.transform.position = Vector3.Lerp(grabbedDisc.transform.position, targetPos, hoverFollowSpeed);
        }

        private void CurveAfterThrow()
        {
            var device = VRTK_DeviceFinder.GetControllerByIndex(controllerIndex, false);
            Vector3 curveDir = Vector3.Cross(releaseDir, Vector3.up);
            Vector3 difference = device.transform.position - releasePos;
            difference = Vector3.ProjectOnPlane(difference, releaseDir);
            releasedDiscRb.AddForce(difference * curveSpeed);
        }

        private IEnumerator CountdownReleaseDisc()
        {
            yield return new WaitForSeconds(curveControlTime);
            releasedDisc = null;
            releasedDiscRb = null;
        }

        private void UngrabDisc(bool carryMomentum, uint controllerIndex)
        {
            if (carryMomentum)
            {
                Vector3 velocity = Vector3.zero;
                var device = VRTK_DeviceFinder.GetControllerByIndex(controllerIndex, false);
                grabbedDisc.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
                

                if (device)
                {
                    velocity = VRTK_DeviceFinder.GetControllerVelocity(device);
                    if (usePlayerScale)
                    {
                        velocity = Vector3.Scale(velocity, playArea.localScale);
                    }
                }
                grabbedDiscRb.angularVelocity = new Vector3(0.0f, 100.0f, 0.0f);
                grabbedDiscRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
                Vector3 throwVelocity = (velocity * throwMultiplier);
                grabbedDiscRb.velocity = (velocity * throwMultiplier);
                releaseDir = throwVelocity.normalized;
            }
            releasedDisc = grabbedDisc;
            releasedDiscRb = grabbedDiscRb;
            releasePos = VRTK_DeviceFinder.GetControllerByIndex(controllerIndex, false).transform.position;
            StartCoroutine(CountdownReleaseDisc());
            grabbedDisc = null;
            grabbedDiscRb = null;
        }

        private void InitControllerListeners(GameObject controller, bool state)
        {
            if (controller)
            {
                
                var eventScript = controller.transform.parent.GetComponentInChildren<VRTK_ControllerEvents>();
                if (eventScript)
                {
                    
                    if (state)
                    {
                        eventScript.TriggerPressed += new ControllerInteractionEventHandler(OnTriggerPressed);
                        eventScript.TriggerReleased += new ControllerInteractionEventHandler(OnTriggerReleased);
                    }
                    else
                    {
                        eventScript.TriggerPressed -= new ControllerInteractionEventHandler(OnTriggerPressed);
                        eventScript.TriggerReleased -= new ControllerInteractionEventHandler(OnTriggerReleased);
                    }
                }
            }
        }

        private GameObject GetEarthDisc()
        {
            if (ViveManager.Instance.head.transform.parent.parent.gameObject.tag == "RedPlayer")
            {
                earthDiscs = GameObject.Find("EarthDiscsRed");
            }
            else
            {
                earthDiscs = GameObject.Find("EarthDiscsBlue");
            }


            Transform closestDisc = earthDiscs.transform.GetChild(0);
            float closestDistance = (closestDisc.transform.position - transform.position).magnitude;

            foreach (Transform disc in earthDiscs.transform)
            {
                float distance = (disc.transform.position - transform.position).magnitude;
                if (distance < closestDistance && !disc.CompareTag("ThrownDisc"))
                {
                    closestDisc = disc;
                    closestDistance = distance;
                }
            }
            closestDisc.tag = "ThrownDisc";
            return closestDisc.gameObject;
        }

    }
}