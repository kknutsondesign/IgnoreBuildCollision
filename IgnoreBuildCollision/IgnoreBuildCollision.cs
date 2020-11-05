﻿using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

public class IgnoreBuildCollision : Mod
{

    private Harmony harmonyInstance;
    public void Start()
    {
        MyInput.Keybinds.Add("Alt", new Keybind("alt", KeyCode.LeftAlt, KeyCode.RightAlt));

        harmonyInstance = new Harmony("com.Soggylithe.IgnoreBuildCollision");
        harmonyInstance.PatchAll();

        
        Debug.Log("Ignore Build Collision successfully loaded! - Default-AltKey");
    }

    public void OnModUnload()
    {
        MyInput.Keybinds.Remove("Alt");

        harmonyInstance.UnpatchAll("com.Soggylithe.IgnoreBuildCollision");
        Destroy(gameObject);
        Debug.Log("Ignore Build Collision has been unloaded!");
    }
}

[HarmonyPatch(typeof(BlockCreator), "CanBuildBlock")]
public class HarmonyPatch_IgnoreCollisionOnAlt
{
    [HarmonyPostfix]
    static BuildError CheckForAlt(BuildError __result)
    {
        if (!MyInput.GetButton("Alt"))
        {
            return __result;
        }

        if (__result == BuildError.PositionOccupied || __result == BuildError.PositionOccupied_Same)
        {
            return BuildError.None;
        }
        return __result;
    }
}

//Fix pipe through wall functionality but not visuals
[HarmonyPatch(typeof(Pipe), "GetNearbyPipes")]
public class HarmonyPatch_ConnectPipesThroughWalls
{
    [HarmonyPrefix]
    static bool IgnoreWalls(ref List<Pipe> __result, Pipe __instance)
    {
        //Do usual method without bitmasking for neighbours
        List<Pipe> list = new List<Pipe>();
        Pipe pipe = PipeGroupManager.GetPipeFromPosition(__instance.snappedBuildingPosition + Vector3.forward * 1.5f);
        Pipe pipe2 = PipeGroupManager.GetPipeFromPosition(__instance.snappedBuildingPosition - Vector3.forward * 1.5f);
        Pipe pipe3 = PipeGroupManager.GetPipeFromPosition(__instance.snappedBuildingPosition - Vector3.right * 1.5f);
        Pipe pipe4 = PipeGroupManager.GetPipeFromPosition(__instance.snappedBuildingPosition + Vector3.right * 1.5f);
        Pipe pipe5 = PipeGroupManager.GetPipeFromPosition(__instance.snappedBuildingPosition + Vector3.up * 1.21f);
        Pipe pipe6 = PipeGroupManager.GetPipeFromPosition(__instance.snappedBuildingPosition - Vector3.up * 1.21f);

        list.AddUniqueOnly(pipe);
        list.AddUniqueOnly(pipe2);
        list.AddUniqueOnly(pipe3);
        list.AddUniqueOnly(pipe4);
        list.AddUniqueOnly(pipe5);
        list.AddUniqueOnly(pipe6);

        if (!list.ContainsItems<Pipe>())
        {
            __result = null;
        }
        else { __result = list; }

        //Dont execute origial method
        return false;
    }
}

////Fix pipe through wall visuals
//[HarmonyPatch(typeof(Block_Pipe), "I need to figure this out")]
//public class HarmonyPatch_ConnectPipeVisualsThroughWalls
//{
//    [HarmonyPrefix]
//    static bool IgnoreWallVisuals()
//    {
//        return true;
//    }
//}