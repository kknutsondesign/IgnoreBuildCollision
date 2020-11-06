using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;

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


//Fix pipe through wall 
[HarmonyPatch(typeof(BitmaskTile), "ExcludeOutOfSightNeighbours")]
public class HarmonyPatch_ConnectPipeVisualsThroughWalls
{
    [HarmonyPrefix]
    static bool IgnoreWallVisuals(BitmaskTile __instance)
    {
        if(__instance.BitmaskType == TileBitmaskType.Pipe || __instance.BitmaskType == TileBitmaskType.Pipe_Water)
            return false;

        return true;
    }
}

//Fix Host rechecking overlapping blocks on receiving client block place messages
[HarmonyPatch(typeof(BlockCreator), "Deserialize")]
public class HarmonyPatch_HostIgnoresOverlappingBlocksFromClient
{
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> IgnoreCheckForOverlapping(IEnumerable<CodeInstruction> instructions)
    {
        //Fingerprint of code to be patched
        MethodInfo isOverlappingInfo = AccessTools.Method(typeof(Block), nameof(Block.IsOverlapping));

        var codes = new List<CodeInstruction>(instructions);
        for(int i=0; i<codes.Count;i++)
        {
            //Search for fingerprint
            if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand as MethodInfo == isOverlappingInfo )
            {
                //Overwrite codeblock with Nops to remove functionality
                codes[i - 1].opcode = OpCodes.Nop;
                codes[i].opcode = OpCodes.Nop;
                codes[i + 1].opcode = OpCodes.Nop;
                break;
            }
        }

        //Give back changed instruction set
        return codes.AsEnumerable();
    }
}
