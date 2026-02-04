using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SDG.Unturned;
using UnityEngine;

namespace SodBuildingUpgrader {
    class MHarmony {
        public static void Init() {
            try {
                var harmony = new Harmony("SodBuildingUpgrader.2026");

                PatchPlyrGetInput.Init(harmony);

            } catch (Exception e) {
                CommandWindow.LogError("building upgrade error: Harmony exception 3424356y: " + e.Message);
                CommandWindow.LogError(e);
            }
        }
    }

    //[HarmonyPatch(typeof(SDG.Unturned.PlayerInput), nameof(SDG.Unturned.PlayerInput.getInput))]
    class PatchPlyrGetInput {
        public static void Init(Harmony harmony) {
            try {
                MethodInfo targetMethod = typeof(SDG.Unturned.PlayerInput).GetMethod("getInput", new Type[] { typeof(bool), typeof(ERaycastInfoUsage), typeof(Vector3?) });
                MethodInfo postfixRef = typeof(PatchPlyrGetInput).GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static);
                harmony.Patch(targetMethod, postfix: new HarmonyMethod(postfixRef));
            } catch (Exception e) {
                CommandWindow.LogError("building upgrade error: Harmony exception 433662s: " + e.Message);
            }
        }

        static void Postfix(ref InputInfo __result, PlayerInput __instance, bool doOcclusionCheck, ERaycastInfoUsage usage) {
            if (__instance != null && __result != null) {
                if (usage == ERaycastInfoUsage.Melee) {
                    if (__result.type == ERaycastInfoType.STRUCTURE) {
                        BuildingUpgradeTool.OnMeleeHitStructure(__instance.player, __result.transform);
                    } else if (__result.type == ERaycastInfoType.BARRICADE) {
                        BuildingUpgradeTool.OnMeleeHitBarricade(__instance.player, __result.transform);
                    }
                }
            }
        }
    }

}