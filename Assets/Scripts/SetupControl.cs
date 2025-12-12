
using UnityEngine;
using UnityEngine.XR;

public class SetupControl : MonoBehaviour
{
    public MotorControl motorControl;
    private bool _wasLeftgripPressed = false;
    private bool _wasRightgripPressed = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
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

            // spin the motor forward while left grip pressed
            float LgripValue = 0f;
            if (leftDevice.TryGetFeatureValue(CommonUsages.grip, out LgripValue))
            {
                bool gripPressed = LgripValue > 0.1f;

                if (gripPressed)
                {
                    // Cancel any pending delayed stop because user is manually controlling motor
   

                    if (motorControl != null)
                    {
                        motorControl.SpinForward();
                    }
                }
                else
                {
                    // grip released: if it was pressed previously, stop the motor immediately
                    if (_wasLeftgripPressed)
                    {
                        if (motorControl != null)
                        {
                            motorControl.SpinStop();
                        }

                    }
                }

                _wasLeftgripPressed = gripPressed;
            }
        }

        
        InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        // spin the motor backwards while right grip pressed
        float RgripValue = 0f;
            if (rightDevice.TryGetFeatureValue(CommonUsages.grip, out RgripValue))
            {
                bool gripPressed = RgripValue > 0.1f;

                if (gripPressed)
                {
                    // Cancel any pending delayed stop because user is manually controlling motor

                    if (motorControl != null)
                    {
                        motorControl.SpinBackward();
                    }
                }
                else
                {
                    // grip released: if it was pressed previously, stop the motor immediately
                    if (_wasRightgripPressed)
                    {
                        if (motorControl != null)
                        {
                            motorControl.SpinStop();
                        }

                    }
                }

                _wasRightgripPressed = gripPressed;
            }
        }

    }

