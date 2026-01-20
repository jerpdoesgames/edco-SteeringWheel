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
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.INTERACT_OK, "button7"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.HONK, "button8"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.BRAKE_BACK, "button6"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.LIGHTS, "button5"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.RADIO_CHANNEL_PREVIOUS, "button22"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.RADIO_CHANNEL_NEXT, "button23"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.RADIO_SCAN, "button20"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.RADIO_SCAN, "button21"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.RADIO_ON, "button24"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.CAMERA, "hat/up"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.MAP_ITEMS, "hat/left"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.JOB_SELECTION, "hat/right"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.PAUSE, "button25"));
                        mappableButtons.Add(new ManagedSimpleButton(mappableButtonType.RESET_HOLD, "button9"));
                    }
                }
            }

            if (SteeringWheel.wheelDevice != null)
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
                                    __instance.radioInput.y = curState ? -1 : 0;
                                    __instance.radioPressed = curState;
                                    break;
                                case SteeringWheel.mappableButtonType.RADIO_SCAN:
                                    __instance.radioInput.y = curState ? 1 : 0;
                                    __instance.radioPressed = curState;
                                    break;

                                case SteeringWheel.mappableButtonType.RADIO_CHANNEL_PREVIOUS:
                                    __instance.radioInput.x = curState ? -1 : 0;
                                    __instance.radioPressed = curState;
                                    break;
                                case SteeringWheel.mappableButtonType.RADIO_CHANNEL_NEXT:
                                    __instance.radioInput.x = curState ? 1 : 0;
                                    __instance.radioPressed = curState;
                                    break;
                            }
                        }
                        // Buttons that can be held
                        switch (curButton.buttonType)
                        {
                            case SteeringWheel.mappableButtonType.BRAKE_BACK:
                                __instance.breakPressed = curState;
                                __instance.backReleased = !curState;
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
            foreach (Joystick curJoystick in UnityEngine.InputSystem.Joystick.all)
            {
                if (curJoystick.displayName == SteeringWheel.TARGET_CONTROLLER)
                {
                    float gasPedalValue = ((float)curJoystick.GetChildControl("Z").ReadValueAsObject() * -0.5f) + 0.5f;
                    float breakPedalValue = ((float)curJoystick.GetChildControl("Rz").ReadValueAsObject() * -0.5f) + 0.5f; ;
                    __instance.input.y = gasPedalValue > 0 ? gasPedalValue : breakPedalValue * -1;
                    __instance.input.x = Math.Max(-1.0f, Math.Min(curJoystick.stick.x.ReadValue() * SteeringWheel.steerMult, 1.0f));
                    break;
                }
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
            HONK
        }

        public static Joystick wheelDevice;
        public static List<ManagedSimpleButton> mappableButtons;

        public static float steerMult = 2.0f;

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