using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.InputSystem;
using static SteeringWheel.SteeringWheel;

namespace SteeringWheel
{
    public class ManagedSimpleButton
    {
        public bool lastState = false;
        public string path = "";
        public SteeringWheel.mappableButtonType buttonType;
        public InputControl inputEntry;

        public bool getState(out bool aHasChanged)
        {
            bool changeState = false;
            bool retVal = false;
            if (inputEntry != null)
            {
                try
                {
                    retVal = (float)inputEntry.ReadValueAsObject() == 1 ? true : false;
                }
                catch (Exception e)
                {
                    SteeringWheel.thisLogger.LogInfo("Failed to get value for button with Path " + path + " | " + e.Message);
                }
                
                if (retVal != lastState)
                {
                    changeState = true;
                    lastState = retVal;
                }
            }

            aHasChanged = changeState;
            return retVal;
        }

        public ManagedSimpleButton(SteeringWheel.mappableButtonType aType, string aPath)
        {
            path = aPath;
            buttonType = aType;
            if (SteeringWheel.wheelDevice != null)
            {
                try
                {
                    inputEntry = SteeringWheel.wheelDevice.GetChildControl(aPath);
                }
                catch (Exception e)
                {
                    SteeringWheel.thisLogger.LogInfo("Failed to get button with Path " + aPath + " | " + e.Message);
                }
            }
        }
    }

    [HarmonyPatch]
    class InputPatch
    {
        static MethodInfo TargetMethod()
        {
            return typeof(sInputManager).GetMethod("GetInput", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        static void Postfix(sInputManager __instance)
        {
            if (SteeringWheel.wheelDevice == null)
            {
                foreach (Joystick curJoystick in UnityEngine.InputSystem.Joystick.all)
                {
                    if (curJoystick.displayName == SteeringWheel.TARGET_CONTROLLER)
                    {
                        SteeringWheel.thisLogger.LogInfo("controller found");
                        SteeringWheel.wheelDevice = curJoystick;

                        mappableButtons = new List<ManagedSimpleButton>();
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.HONK, "button5"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.BRAKE_BACK, "button6"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.LIGHTS, "button7"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.LIGHTS, "button8"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.LIGHTS, "button11"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.LIGHTS, "button12"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.RADIO_CHANNEL_PREVIOUS, "button23"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.RADIO_CHANNEL_NEXT, "button22"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.RADIO_SCAN, "button20"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.RADIO_ON, "button24"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.CAMERA, "button21"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.MAP_ITEMS, "/button2"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.JOB_SELECTION, "button4"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.PAUSE, "button10"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.PAUSE, "button25"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.RESET_HOLD, "button9"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.MOVEMENT_X_TRIPLE, "button13"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.MOVEMENT_Y_DOUBLE, "button14"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.MOVEMENT_X_DOUBLE, "button15"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.MOVEMENT_Y_HALF, "button16"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.MOVEMENT_X_HALF, "button17"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.MOVEMENT_Y_REVERSE, "button18"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.INTERACT_OK, "trigger"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.BRAKE_BACK, "button3"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.PLAYER_INPUT_UP, "hat/up"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.PLAYER_INPUT_DOWN, "hat/down"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.PLAYER_INPUT_LEFT, "hat/left"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.PLAYER_INPUT_RIGHT, "hat/right"));
                    }
                }
            }
            else
            {
                foreach (ManagedSimpleButton curButton in SteeringWheel.mappableButtons)
                {
                    if (curButton.inputEntry != null)
                    {
                        bool hasChanged;
                        bool curState = curButton.getState(out hasChanged);

                        // Buttons that shouldn't be held
                        if (hasChanged)
                        {
                            switch (curButton.buttonType)
                            {
                                case SteeringWheel.mappableButtonType.INTERACT_OK:
                                    __instance.selectPressed = curState;
                                    __instance.selectReleased = !curState;
                                    break;
                                case SteeringWheel.mappableButtonType.HONK:
                                    __instance.hornPressed = curState;
                                    break;
                                case SteeringWheel.mappableButtonType.PAUSE:
                                    __instance.pausePressed = curState;
                                    break;
                                case SteeringWheel.mappableButtonType.LIGHTS:
                                    __instance.headlightsPressed = curState;
                                    break;
                                case SteeringWheel.mappableButtonType.MAP_ITEMS:
                                    __instance.mapPressed = curState;
                                    break;
                                case SteeringWheel.mappableButtonType.CAMERA:
                                    __instance.cameraPressed = curState;
                                    break;
                                case SteeringWheel.mappableButtonType.JOB_SELECTION:
                                    __instance.inventoryPressed = curState;
                                    __instance.inventoryReleased = !curState;
                                    break;
                                case SteeringWheel.mappableButtonType.RESET_HOLD:
                                    __instance.resetHeld = curState;
                                    break;
                                case SteeringWheel.mappableButtonType.RADIO_ON:
                                    __instance.radioInput.y = curState ? -1 : __instance.radioInput.y;
                                    if (curState) { __instance.radioPressed = true; }
                                    break;
                                case SteeringWheel.mappableButtonType.RADIO_SCAN:
                                    __instance.radioInput.y = curState ? 1 : __instance.radioInput.y;
                                    if (curState) { __instance.radioPressed = true; }
                                    break;
                                case SteeringWheel.mappableButtonType.RADIO_CHANNEL_PREVIOUS:
                                    __instance.radioInput.x = curState ? -1 : __instance.radioInput.x;
                                    if (curState) { __instance.radioPressed = true; }
                                    break;
                                case SteeringWheel.mappableButtonType.RADIO_CHANNEL_NEXT:
                                    __instance.radioInput.x = curState ? 1 : __instance.radioInput.x;
                                    if (curState) { __instance.radioPressed = true; }
                                    break;

                                case SteeringWheel.mappableButtonType.MOVEMENT_X_DOUBLE:
                                    SteeringWheel.steerMult = curState ? 2.0f : 1.0f;
                                    break;
                                case SteeringWheel.mappableButtonType.MOVEMENT_Y_HALF:
                                    SteeringWheel.gasMult = curState ? 0.5f : 1.0f;
                                    break;
                                case SteeringWheel.mappableButtonType.MOVEMENT_X_TRIPLE:
                                    SteeringWheel.steerMult = curState ? 3.0f : 1.0f;
                                    break;
                                case SteeringWheel.mappableButtonType.MOVEMENT_Y_DOUBLE:
                                    SteeringWheel.gasMult = curState ? 2.0f : 1.0f;
                                    break;
                                case SteeringWheel.mappableButtonType.MOVEMENT_X_HALF:
                                    SteeringWheel.steerMult = curState ? 0.5f : 1.0f;
                                    break;
                                case SteeringWheel.mappableButtonType.MOVEMENT_Y_REVERSE:
                                    SteeringWheel.gasMult = curState ? -1.0f : 1.0f;
                                    break;
                            }
                        }

                        // Buttons that can be held
                        switch (curButton.buttonType)
                        {
                            case SteeringWheel.mappableButtonType.BRAKE_BACK:
                                __instance.breakPressed = curState ? true : __instance.breakPressed;
                                __instance.backReleased = !curState || __instance.backReleased;
                                break;
                            case SteeringWheel.mappableButtonType.PLAYER_INPUT_UP:
                                if (curState)
                                {
                                    __instance.playerInput.y = 1;
                                    __instance.radioInput.y = 1; // radioInput without radioPressed = menu input
                                }
                                break;
                            case SteeringWheel.mappableButtonType.PLAYER_INPUT_DOWN:
                                if (curState)
                                {
                                    __instance.playerInput.y = -1;
                                    __instance.radioInput.y = -1; // radioInput without radioPressed = menu input
                                }
                                break;
                            case SteeringWheel.mappableButtonType.PLAYER_INPUT_LEFT:
                                if (curState)
                                {
                                    __instance.playerInput.x = -1;
                                    __instance.radioInput.x = -1; // radioInput without radioPressed = menu input
                                }
                                break;
                            case SteeringWheel.mappableButtonType.PLAYER_INPUT_RIGHT:
                                if (curState)
                                {
                                    __instance.playerInput.x = 1;
                                    __instance.radioInput.x = 1; // radioInput without radioPressed = menu input
                                }
                                break;
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch]
    class SteeringPatch
    {
        static MethodInfo TargetMethod()
        {
            return typeof(sCarController).GetMethod("Move", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        static void Prefix(sCarController __instance)
        {
            if (SteeringWheel.wheelDevice != null)
            {
                float gasPedalValue = ((float)SteeringWheel.wheelDevice.GetChildControl("Z").ReadValueAsObject() * -0.5f) + 0.5f;
                float breakPedalValue = ((float)SteeringWheel.wheelDevice.GetChildControl("Rz").ReadValueAsObject() * -0.5f) + 0.5f;
                float xValue = SteeringWheel.wheelDevice.stick.x.ReadUnprocessedValue();
                if (xValue < SteeringWheel.deadZonePercent && xValue > SteeringWheel.deadZonePercent * -1)
                {
                    xValue = 0.0f;
                }

                __instance.input.y = gasPedalValue > 0 ? gasPedalValue * SteeringWheel.gasMult : breakPedalValue * SteeringWheel.gasMult * -1;
                __instance.input.x = Math.Max(-1.0f, Math.Min(xValue * SteeringWheel.steerMultBase * SteeringWheel.steerMult, 1.0f));
            }
        }
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]

    public class SteeringWheel : BaseUnityPlugin
    {
        public const string TARGET_CONTROLLER = "G29 Driving Force Racing Wheel";
        public enum mappableButtonType
        {
            INTERACT_OK,
            BRAKE_BACK,
            MAP_ITEMS,
            PAUSE,
            JOB_SELECTION,
            CAMERA,
            RESET_HOLD,
            LIGHTS,
            RADIO_ON,
            RADIO_CHANNEL_PREVIOUS,
            RADIO_CHANNEL_NEXT,
            RADIO_SCAN,
            HONK,
            MOVEMENT_X_TRIPLE, //  Sets wheel to 3.0 for huge sensitivity (Gear 1, top-left)
            MOVEMENT_X_DOUBLE, //  Sets wheel to 2.0 for extra movement (Gear 3, top-center)
            MOVEMENT_X_HALF, //  Super smooth scale for wheel (Gear 5)
            MOVEMENT_Y_DOUBLE, //  2.0 scale for gas (Gear 2, bottom-left)
            MOVEMENT_Y_HALF, //  Smoother acceleration / breaking (Gear 4, bottom-center)
            MOVEMENT_Y_REVERSE, //  Reverse gear (gear 6, bottom-right / button18, though it should be 19 - unsure why my G29 is missing the 7th gear slot?)
            PLAYER_INPUT_UP,    // Generic directional input
            PLAYER_INPUT_RIGHT,    // Generic directional input
            PLAYER_INPUT_DOWN,    // Generic directional input
            PLAYER_INPUT_LEFT,    // Generic directional input
        }

        public static Joystick wheelDevice;
        public static List<ManagedSimpleButton> mappableButtons;

        public static float steerMultBase = 1.0f;
        public static float steerMult = 1.0f;
        public static float gasMult = 1.0f;
        // G29 defaults to a max of 900 degrees, though the game's maximum rotation is roughly 800
        // G29 tends to center under about 10 degrees (closer to 4-6)
        public static float deadZonePercent = 0.0125f;

        public static ManualLogSource thisLogger;
        private void Awake()
        {
            thisLogger = Logger;
            Harmony harmony = new("SteeringWheel");
            harmony.PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded! - jerp update");
        }
    }
}