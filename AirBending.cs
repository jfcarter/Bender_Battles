using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Player Climb|Locomotion|20120
namespace VRTK
{
    using GrabAttachMechanics;
    using UnityEngine;

    /// <summary>
    /// Air bending allows player movement based on grabbing the air around them. Because it works by grabbing, each controller should have a `VRTK_InteractGrab` and `VRTK_InteractTouch` component attached.
    /// </summary>
    /// <example>
    /// `VRTK/Examples/037_CameraRig_ClimbingFalling` shows how to set up a scene with player climbing. There are many different examples showing how the same system can be used in unique ways.
    /// </example>
    [RequireComponent(typeof(VRTK_BodyPhysics))]
    public class AirBending : MonoBehaviour
    {
        [Tooltip("Will scale movement up and down based on the player transform's scale.")]
        public bool usePlayerScale = true;

        /// <summary>
        /// Emitted when player climbing has started.
        /// </summary>
        public event PlayerClimbEventHandler PlayerClimbStarted;
        /// <summary>
        /// Emitted when player climbing has ended.
        /// </summary>
        public event PlayerClimbEventHandler PlayerClimbEnded;

        public float floatyness = 0.05f;
        public float throwMultiplier = 1.5f;
        public float maxSpeed = 10.0f;

        public bool allowInAir = false;

        private Transform playArea;
        private Vector3 startControllerScaledLocalPosition;
        private Vector3 startGrabPointLocalPosition;
        private Vector3 startPlayerAreaPosition;
        private Vector3 startPlayAreaWorldOffset;
        private GameObject grabbingController;
        private VRTK_BodyPhysics bodyPhysics;
        private Vector3 startGrabPointWorldPosition;
        private bool isClimbing;
        private float distToGround;
        private Vector3 prevVelocity;

        protected virtual void Awake()
        {
            playArea = VRTK_DeviceFinder.PlayAreaTransform();
            bodyPhysics = GetComponent<VRTK_BodyPhysics>();
            distToGround = 0.0f;
        }

        protected virtual void OnEnable()
        {
            InitListeners(true);
        }

        protected virtual void OnDisable()
        {
            Ungrab(false, 0);
            InitListeners(false);
        }

        protected virtual void Update()
        {
            if (isClimbing)
            {
                Vector3 controllerLocalOffset = GetScaledLocalPosition(grabbingController.transform) - startControllerScaledLocalPosition;
                Vector3 newPos = startGrabPointWorldPosition + startPlayAreaWorldOffset - controllerLocalOffset;
                if (newPos.y <= startPlayerAreaPosition.y ) {
                    newPos.y = startPlayerAreaPosition.y;
                }
                prevVelocity = Vector3.Lerp(prevVelocity, Vector3.zero, floatyness);
                playArea.position = Vector3.Slerp(playArea.position, newPos + prevVelocity, floatyness);
            }
        }

        private void OnPlayerClimbStarted(PlayerClimbEventArgs e)
        {
            if (PlayerClimbStarted != null)
            {
                PlayerClimbStarted(this, e);
            }
        }

        private void OnPlayerClimbEnded(PlayerClimbEventArgs e)
        {
            if (PlayerClimbEnded != null)
            {
                PlayerClimbEnded(this, e);
            }
        }

        private PlayerClimbEventArgs SetPlayerClimbEvent(uint controllerIndex)
        {
            PlayerClimbEventArgs e;
            e.controllerIndex = controllerIndex;
            e.target = null;
            return e;
        }

        private void InitListeners(bool state)
        {
            InitControllerListeners(VRTK_DeviceFinder.GetControllerLeftHand(), state);
            InitControllerListeners(VRTK_DeviceFinder.GetControllerRightHand(), state);

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
            Ungrab(false, e.controllerIndex);
        }

        private Vector3 GetScaledLocalPosition(Transform objTransform)
        {
            if (usePlayerScale)
            {
                return playArea.localRotation * Vector3.Scale(objTransform.localPosition, playArea.localScale);
            }

            return playArea.localRotation * objTransform.localPosition;
        }

        private void OnGripPressed(object sender, ControllerInteractionEventArgs e)
        {
            if (bodyPhysics.OnGround() || allowInAir)
            {
                var controller = ((VRTK_ControllerEvents)sender).gameObject;
                var actualController = VRTK_DeviceFinder.GetActualController(controller);
                Grab(actualController, e.controllerIndex);
            }
        }

        private void OnGripReleased(object sender, ControllerInteractionEventArgs e)
        {
            var controller = ((VRTK_ControllerEvents)sender).gameObject;
            var actualController = VRTK_DeviceFinder.GetActualController(controller);
            if (IsActiveClimbingController(actualController))
            {
                Ungrab(true, e.controllerIndex);
            }
        }

        private void Grab(GameObject currentGrabbingController, uint controllerIndex)
        {
            bodyPhysics.TogglePreventSnapToFloor(true);
            bodyPhysics.enableBodyCollisions = false;
            bodyPhysics.ToggleOnGround(false);

            prevVelocity = playArea.GetComponent<Rigidbody>().velocity / 5;
            isClimbing = true;
            grabbingController = currentGrabbingController;
            startControllerScaledLocalPosition = GetScaledLocalPosition(grabbingController.transform);
            startPlayAreaWorldOffset = playArea.transform.position - grabbingController.transform.position;
            startGrabPointWorldPosition = grabbingController.transform.position;
            startPlayerAreaPosition = playArea.transform.position;
            

            OnPlayerClimbStarted(SetPlayerClimbEvent(controllerIndex));
        }

        private void Ungrab(bool carryMomentum, uint controllerIndex)
        {
            bodyPhysics.TogglePreventSnapToFloor(false);
            bodyPhysics.enableBodyCollisions = true;

            if (carryMomentum)
            {
                Vector3 velocity = Vector3.zero;
                var device = VRTK_DeviceFinder.GetControllerByIndex(controllerIndex, false);

                if (device)
                {
                    velocity = -VRTK_DeviceFinder.GetControllerVelocity(device);
                    if (usePlayerScale)
                    {
                        velocity = Vector3.Scale(velocity, playArea.localScale);
                    }
                }
                Vector3 throwVelocity = Vector3.ClampMagnitude(velocity * throwMultiplier, maxSpeed);
                if (throwVelocity.y <= 1)
                {
                    throwVelocity.y = 1;
                }
                bodyPhysics.ApplyBodyVelocity(throwVelocity * throwMultiplier, true, true);
            }

            isClimbing = false;
            grabbingController = null;

            OnPlayerClimbEnded(SetPlayerClimbEvent(controllerIndex));
        }

        private bool IsActiveClimbingController(GameObject controller)
        {
            return (controller == grabbingController);
        }

        private bool IsClimbableObject(GameObject obj)
        {
            var interactObject = obj.GetComponent<VRTK_InteractableObject>();
            return (interactObject && interactObject.grabAttachMechanicScript && interactObject.grabAttachMechanicScript.IsClimbable());
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
                        eventScript.GripPressed += new ControllerInteractionEventHandler(OnGripPressed);
                        eventScript.GripReleased += new ControllerInteractionEventHandler(OnGripReleased);
                    }
                    else
                    {
                        eventScript.GripPressed -= new ControllerInteractionEventHandler(OnGripPressed);
                        eventScript.GripReleased -= new ControllerInteractionEventHandler(OnGripReleased);
                    }
                }
            }
        }
    }
}