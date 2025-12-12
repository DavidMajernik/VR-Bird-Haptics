using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public enum BirdState { Flying, Perched, Ragdoll }

[RequireComponent(typeof(BirdFlying))]
public class BirdController : MonoBehaviour
{
    [Header("Scripts")]
    public BirdFlying flyingScript;
    public BirdRagdoll ragdollScript;
    public MotorControl motorControl;
    [SerializeField] private HitSounds hitSounds;
    public BirdState state;
    private float delayTime = 0.7f;

    private bool _wasLeftGripPressed = false;
    private bool _wasLeftButtonPressed = false;
    private bool _wasRightButtonPressed = false;

    // Store the child transform so we can reset it
    private Transform _visualChild;
    private Vector3 _childLocalPosition;
    private Quaternion _childLocalRotation;

    // Coroutine handle for delayed motor stop
    private Coroutine _spinStopCoroutine;

    void Start()
    {
        flyingScript = GetComponent<BirdFlying>();
        ragdollScript = GetComponentInChildren<BirdRagdoll>();

        // Store the child's initial local transform
        _visualChild = ragdollScript.transform;
        _childLocalPosition = _visualChild.localPosition;
        _childLocalRotation = _visualChild.localRotation;

        flyingScript.OnArrivedAtPerch += HandlePerchArrival;
        flyingScript.OnStartLanding += HandleStartLanding;
        SetState(BirdState.Flying);
    }

    void OnDestroy()
    {
        if (flyingScript != null)
        {
            flyingScript.OnArrivedAtPerch -= HandlePerchArrival;
            flyingScript.OnStartLanding -= HandleStartLanding;
        }
    }

    void Update()
    {
        CheckGripInput();
    }

    void CheckGripInput()
    {
        // left grip: tell bird to perch
        InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (leftDevice.isValid)
        {
            bool isPressed = false;
            if (leftDevice.TryGetFeatureValue(CommonUsages.gripButton, out isPressed))
            {
                if (isPressed && !_wasLeftGripPressed)
                {
                    TriggerPerch();
                }
                _wasLeftGripPressed = isPressed;
            }

            // spin the motor forward while left trigger pressed
            bool bPressed = false;
            if (leftDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bPressed))
            {
                bool triggerPressed = bPressed;

                if (triggerPressed)
                {
                    // Cancel any pending delayed stop because user is manually controlling motor
                    if (_spinStopCoroutine != null)
                    {
                        StopCoroutine(_spinStopCoroutine);
                        _spinStopCoroutine = null;
                    }

                    if (motorControl != null)
                    {
                        motorControl.SpinForward();
                    }
                }
                else
                {
                    // Trigger released: if it was pressed previously, stop the motor immediately
                    if (_wasLeftButtonPressed)
                    {
                        if (motorControl != null)
                        {
                            motorControl.SpinStop();
                        }

                        // Cancel any pending delayed stop to avoid unexpected behavior
                        if (_spinStopCoroutine != null)
                        {
                            StopCoroutine(_spinStopCoroutine);
                            _spinStopCoroutine = null;
                        }
                    }
                }

                _wasLeftButtonPressed = triggerPressed;
            }
        }

        // right grip: tell bird to start flying again
        InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightDevice.isValid)
        {
            bool isPressed = false;
            if (rightDevice.TryGetFeatureValue(CommonUsages.gripButton, out isPressed))
            {
                if (isPressed)
                {
                    if (state != BirdState.Flying)
                    {
                        SetState(BirdState.Flying);
                    }
                }
            }

            // spin the motor backwards while the a button pressed
            bool aPressed = false;
            if (rightDevice.TryGetFeatureValue(CommonUsages.primaryButton, out aPressed))
            {
                bool triggerPressed = aPressed;

                if (triggerPressed)
                {
                    // Cancel any pending delayed stop because user is manually controlling motor
                    if (_spinStopCoroutine != null)
                    {
                        StopCoroutine(_spinStopCoroutine);
                        _spinStopCoroutine = null;
                    }

                    if (motorControl != null)
                    {
                        motorControl.SpinBackward();
                    }
                }
                else
                {
                    // Trigger released: if it was pressed previously, stop the motor immediately
                    if (_wasRightButtonPressed)
                    {
                        if (motorControl != null)
                        {
                            motorControl.SpinStop();
                        }

                        // Cancel any pending delayed stop to avoid unexpected behavior
                        if (_spinStopCoroutine != null)
                        {
                            StopCoroutine(_spinStopCoroutine);
                            _spinStopCoroutine = null;
                        }
                    }
                }

                _wasRightButtonPressed = triggerPressed;
            }
        }

    }

    void TriggerPerch()
    {
        Debug.Log("Perching...");
        // If ragdolled, transition to flying first, then fly to perch
        if (state == BirdState.Ragdoll)
        {
            SetState(BirdState.Flying);
        }

        flyingScript.FlyToPerch();
    }

    void HandleStartLanding()
    {
        // start motor to create tension only if motorControl exists
        if (motorControl != null)
        {
            motorControl.SpinForward();
            // stop motor 
            if (_spinStopCoroutine != null)
            {
                StopCoroutine(_spinStopCoroutine);
            }
            _spinStopCoroutine = StartCoroutine(DelayedSpinStop(delayTime));
        }
    }

    void HandlePerchArrival()
    {
        SetState(BirdState.Perched);
    }

    public void SetState(BirdState newState, Vector3 impactForce = default(Vector3))
    {
        // cleanup old state
        transform.SetParent(null);

        if (state == BirdState.Flying)
        {
            flyingScript.enabled = false;
        }

        if (state == BirdState.Perched)
        {
            // Cancel any pending delayed stop when leaving Perched
            if (_spinStopCoroutine != null)
            {
                StopCoroutine(_spinStopCoroutine);
                _spinStopCoroutine = null;
            }

            _wasLeftGripPressed = false; // Reset input tracker

            // Only operate motor if motorControl is assigned
            if (motorControl != null)
            {
                motorControl.SpinBackward(); // reset tension
                // stop motor 
                if (_spinStopCoroutine != null)
                {
                    StopCoroutine(_spinStopCoroutine);
                }
                _spinStopCoroutine = StartCoroutine(DelayedSpinStop(delayTime));
            }
        }

        if (state == BirdState.Ragdoll)
        {
            ragdollScript.DisableRagdoll();
            _wasLeftGripPressed = false;

            // move parent to where the child capsule ended up
            Vector3 childWorldPos = _visualChild.position;
            Quaternion childWorldRot = _visualChild.rotation;

            // Reset child's local transform back to its original position
            _visualChild.localPosition = _childLocalPosition;
            _visualChild.localRotation = _childLocalRotation;

            // Now position the parent so the child ends up where it was
            transform.position = childWorldPos;
            transform.rotation = childWorldRot;
        }

        // set up new state
        switch (newState)
        {
            case BirdState.Flying:
                ragdollScript._animator.SetBool("isFlying", true);
                transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
                flyingScript.enabled = true;
                break;

            case BirdState.Perched:
                ragdollScript._animator.SetBool("isFlying", false);
                transform.position = flyingScript.playerPerchTarget.position;
                transform.rotation = flyingScript.playerPerchTarget.rotation;
                transform.SetParent(flyingScript.playerPerchTarget);
                break;

            case BirdState.Ragdoll:
                ragdollScript.EnableRagdoll(impactForce);
                break;
        }

        state = newState;
    }

    private IEnumerator DelayedSpinStop(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        if (motorControl != null)
        {
            motorControl.SpinStop();
        }
        _spinStopCoroutine = null;
    }
}