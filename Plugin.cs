using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using UnityEngine.Windows;

namespace SteeringWheel
{
    [HarmonyPatch]
    class LerpPatch
    {
        static readonly FieldInfo TargetInputField = typeof(sCarController).GetField("targetInput", BindingFlags.NonPublic | BindingFlags.Instance);
        static MethodInfo TargetMethod()
        {
            return typeof(sCarController).GetMethod("Move", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        static void Prefix(sCarController __instance)
        {
            
            __instance.input.x = ((Vector2) TargetInputField.GetValue(__instance)).x * (__instance.steeringSensetivity * 3);
        }

    }

    [HarmonyPatch]
    class SteeringPatch
    {
        static readonly FieldInfo TargetInputField = typeof(sCarController).GetField("targetInput", BindingFlags.NonPublic | BindingFlags.Instance);
        static MethodInfo TargetMethod()
        {
            return typeof(sCarController).GetMethod("SetInput", BindingFlags.Instance | BindingFlags.Public, null, [typeof(Vector2)], null);
        }
        static void Postfix(Vector2 input, sCarController __instance)
        {
            TargetInputField.SetValue(__instance, input);
        }

    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]

    public class SteeringWheel : BaseUnityPlugin
    {
        private void Awake()
        {
            Harmony harmony = new("SteeringWheel");
            harmony.PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
