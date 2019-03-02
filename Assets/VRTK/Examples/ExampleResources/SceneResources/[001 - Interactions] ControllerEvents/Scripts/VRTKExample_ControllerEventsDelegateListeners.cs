namespace VRTK.Examples
{
    using UnityEngine;

    public class VRTKExample_ControllerEventsDelegateListeners : MonoBehaviour
    {
        public VRTK_ControllerEvents controllerEvents;

        public bool triggerButtonEvents = true;
        public bool gripButtonEvents = true;
        public bool touchpadButtonEvents = true;
        public bool touchpadTwoButtonEvents = true;
        public bool buttonOneButtonEvents = true;
        public bool buttonTwoButtonEvents = true;
        public bool startMenuButtonEvents = true;

        public bool triggerAxisEvents = true;
        public bool gripAxisEvents = true;
        public bool touchpadAxisEvents = true;
        public bool touchpadTwoAxisEvents = true;

        public bool triggerSenseAxisEvents = true;
        public bool touchpadSenseAxisEvents = true;
        public bool middleFingerSenseAxisEvents = true;
        public bool ringFingerSenseAxisEvents = true;
        public bool pinkyFingerSenseAxisEvents = true;

        public bool bTrigger; //accelerate
        public bool bButton0; //fire
        public bool bLeft, bRight;
        public bool bUp, bDown;

        private void OnEnable()
        {
            controllerEvents = (controllerEvents == null ? GetComponent<VRTK_ControllerEvents>() : controllerEvents);
            if (controllerEvents == null)
            {
                VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.REQUIRED_COMPONENT_MISSING_FROM_GAMEOBJECT, "VRTK_ControllerEvents_ListenerExample", "VRTK_ControllerEvents", "the same"));
                return;
            }

            //Setup controller event listeners
            controllerEvents.TriggerPressed += DoTriggerPressed;
            controllerEvents.TriggerReleased += DoTriggerReleased;

            controllerEvents.GripPressed += DoGripPressed;
            controllerEvents.GripReleased += DoGripReleased;

            controllerEvents.TouchpadPressed += DoTouchpadPressed;
            controllerEvents.TouchpadReleased += DoTouchpadReleased;
            controllerEvents.TouchpadAxisChanged += DoTouchpadAxisChanged;
            controllerEvents.TouchpadTwoPressed += DoTouchpadTwoPressed;
            controllerEvents.TouchpadTwoReleased += DoTouchpadTwoReleased;
            controllerEvents.TouchpadTwoAxisChanged += DoTouchpadTwoAxisChanged;
            controllerEvents.TouchpadSenseAxisChanged += DoTouchpadSenseAxisChanged;

            controllerEvents.ButtonOnePressed += DoButtonOnePressed;
            controllerEvents.ButtonOneReleased += DoButtonOneReleased;

            controllerEvents.ButtonTwoPressed += DoButtonTwoPressed;
            controllerEvents.ButtonTwoReleased += DoButtonTwoReleased;

            controllerEvents.StartMenuPressed += DoStartMenuPressed;
            controllerEvents.StartMenuReleased += DoStartMenuReleased;
        }

        private void OnDisable()
        {
            if (controllerEvents != null)
            {
                controllerEvents.TriggerPressed -= DoTriggerPressed;
                controllerEvents.TriggerReleased -= DoTriggerReleased;

                controllerEvents.GripPressed -= DoGripPressed;
                controllerEvents.GripReleased -= DoGripReleased;

                controllerEvents.TouchpadPressed -= DoTouchpadPressed;
                controllerEvents.TouchpadReleased -= DoTouchpadReleased;
                controllerEvents.TouchpadAxisChanged -= DoTouchpadAxisChanged;
                controllerEvents.TouchpadTwoPressed -= DoTouchpadTwoPressed;
                controllerEvents.TouchpadTwoReleased -= DoTouchpadTwoReleased;
                controllerEvents.TouchpadTwoAxisChanged -= DoTouchpadTwoAxisChanged;
                controllerEvents.TouchpadSenseAxisChanged -= DoTouchpadSenseAxisChanged;

                controllerEvents.ButtonOnePressed -= DoButtonOnePressed;
                controllerEvents.ButtonOneReleased -= DoButtonOneReleased;

                controllerEvents.ButtonTwoPressed -= DoButtonTwoPressed;
                controllerEvents.ButtonTwoReleased -= DoButtonTwoReleased;

                controllerEvents.StartMenuPressed -= DoStartMenuPressed;
                controllerEvents.StartMenuReleased -= DoStartMenuReleased;
            }
        }

        private void LateUpdate()
        {
        }

        private void DebugLogger(uint index, string button, string action, ControllerInteractionEventArgs e)
        {
            string debugString = "Controller on index '" + index + "' " + button + " has been " + action
                                 + " with a pressure of " + e.buttonPressure + " / Primary Touchpad axis at: " + e.touchpadAxis + " (" + e.touchpadAngle + " degrees)" + " / Secondary Touchpad axis at: " + e.touchpadTwoAxis + " (" + e.touchpadTwoAngle + " degrees)";
            VRTK_Logger.Info(debugString);
        }

        private void DoTriggerPressed(object sender, ControllerInteractionEventArgs e)
        {
            bTrigger = true;

            if (triggerButtonEvents)
            {
                DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TRIGGER", "pressed", e);
            }
        }

        private void DoTriggerReleased(object sender, ControllerInteractionEventArgs e)
        {
            bTrigger = false;

            if (triggerButtonEvents)
            {
                DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TRIGGER", "released", e);
            }
        }

        private void DoGripPressed(object sender, ControllerInteractionEventArgs e)
        {
            bButton0 = true;

            if (gripButtonEvents)
            {
                DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "GRIP", "pressed", e);
            }
        }

        private void DoGripReleased(object sender, ControllerInteractionEventArgs e)
        {
            bButton0 = false;

            if (gripButtonEvents)
            {
                DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "GRIP", "released", e);
            }
        }

        private void DoTouchpadPressed(object sender, ControllerInteractionEventArgs e)
        {
            //e.touchpadAxis
            //e.touchpadTwoAxis
            //bLeft, bRight;
            //bUp, bDown;

            if (touchpadButtonEvents)
            {
                DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPAD", "pressed down", e);
            }
        }

        private void DoTouchpadReleased(object sender, ControllerInteractionEventArgs e)
        {
            if (touchpadButtonEvents)
            {
                DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPAD", "released", e);
            }
        }

        private void DoTouchpadAxisChanged(object sender, ControllerInteractionEventArgs e)
        {
            //e.touchpadAxis
            //e.touchpadTwoAxis
            //bLeft, bRight;
            //bUp, bDown;

            if (touchpadAxisEvents)
            {
                DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPAD", "axis changed", e);
            }
        }

        private void DoTouchpadTwoPressed(object sender, ControllerInteractionEventArgs e)
        {
            //e.touchpadAxis
            //e.touchpadTwoAxis
            //bLeft, bRight;
            //bUp, bDown;

            if (touchpadTwoButtonEvents)
            {
                DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPADTWO", "pressed down", e);
            }
        }

        private void DoTouchpadTwoReleased(object sender, ControllerInteractionEventArgs e)
        {
            if (touchpadTwoButtonEvents)
            {
                DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPADTWO", "released", e);
            }
        }

        private void DoTouchpadTwoAxisChanged(object sender, ControllerInteractionEventArgs e)
        {
            //e.touchpadAxis
            //e.touchpadTwoAxis
            //bLeft, bRight;
            //bUp, bDown;

            if (touchpadTwoAxisEvents)
            {
                DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPADTWO", "axis changed", e);
            }
        }

        private void DoTouchpadSenseAxisChanged(object sender, ControllerInteractionEventArgs e)
        {
            if (touchpadSenseAxisEvents)
            {
                DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "TOUCHPAD", "sense axis changed", e);
            }
        }

        private void DoButtonOnePressed(object sender, ControllerInteractionEventArgs e)
        {
            bButton0 = true;

            if (buttonOneButtonEvents)
            {
                DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "BUTTON ONE", "pressed down", e);
            }
        }

        private void DoButtonOneReleased(object sender, ControllerInteractionEventArgs e)
        {
            bButton0 = false;

            if (buttonOneButtonEvents)
            {
                DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "BUTTON ONE", "released", e);
            }
        }

        private void DoButtonTwoPressed(object sender, ControllerInteractionEventArgs e)
        {
            if (buttonTwoButtonEvents)
            {
                DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "BUTTON TWO", "pressed down", e);
            }
        }

        private void DoButtonTwoReleased(object sender, ControllerInteractionEventArgs e)
        {
            if (buttonTwoButtonEvents)
            {
                DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "BUTTON TWO", "released", e);
            }
        }

        private void DoStartMenuPressed(object sender, ControllerInteractionEventArgs e)
        {
            if (startMenuButtonEvents)
            {
                DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "START MENU", "pressed down", e);
            }
        }

        private void DoStartMenuReleased(object sender, ControllerInteractionEventArgs e)
        {
            if (startMenuButtonEvents)
            {
                DebugLogger(VRTK_ControllerReference.GetRealIndex(e.controllerReference), "START MENU", "released", e);
            }
        }

    }
}