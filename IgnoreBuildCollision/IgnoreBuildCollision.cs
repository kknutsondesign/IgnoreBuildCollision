using UnityEngine;
using HarmonyLib;
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

        harmonyInstance.UnpatchAll();
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