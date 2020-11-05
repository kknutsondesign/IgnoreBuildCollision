using UnityEngine;
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

[HarmonyPatch(typeof(Pipe), "GetNearbyPipes")]
public class HarmonyPatch_ConnectPipesThroughWalls
{
    [HarmonyPostfix]
    static void IgnoreWalls(ref List<Pipe> __result, Pipe ___pipe, Pipe ___pipe2, Pipe ___pipe3, Pipe ___pipe4, Pipe ___pipe5, Pipe ___pipe6)
    {
        List<Pipe> list = new List<Pipe>();

        list.AddUniqueOnly(___pipe);
        list.AddUniqueOnly(___pipe2);
        list.AddUniqueOnly(___pipe3);
        list.AddUniqueOnly(___pipe4);
        list.AddUniqueOnly(___pipe5);
        list.AddUniqueOnly(___pipe6);

        if (!list.ContainsItems<Pipe>())
        {
            __result = null;
        }
        else { __result = list; }
    }
}