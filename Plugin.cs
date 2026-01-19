using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using Microsoft.SqlServer.Server;
using System;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Windows;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;


namespace SteeringWheel
{
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
                    __instance.input.x = curJoystick.stick.x.ReadValue();
                    break;
                }
            }
            
        }
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]

    public class SteeringWheel : BaseUnityPlugin
    {
        public const string TARGET_CONTROLLER = "G29 Driving Force Racing Wheel";

        public static bool joystickInfo = false;
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