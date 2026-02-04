using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SDG.NetTransport;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace SodBuildingUpgrader {
    class BuildingUpgradeTool : MonoBehaviour {
        public static BuildingUpgradeTool instance = null;

        //public static bool bEnable = false;
        static Guid upgradeTool = Guid.Empty;

        static Dictionary<Guid, List<ItemAsset>> possibleUpgrades = new Dictionary<Guid, List<ItemAsset>>();

        static bool bUseMouseRightClickOnly = false;

        public static void OnLevelLoaded() {
            try {
                ItemStorageAsset pineCrate = Assets.find<ItemStorageAsset>(new Guid("be8da1aa6deb44ecb4546ce7102fae41"));
                if (pineCrate != null && pineCrate.storage_x == 8 && pineCrate.storage_y == 5) {
                    MUtilities.SetStorageSize(pineCrate, 7, 6); // to make it compatible with locker
                }

                bool bVanillaMap = false;
                string mapName_SmallLetter = Provider.map.ToLowerInvariant();
                if (mapName_SmallLetter == "pei" || mapName_SmallLetter == "russia" || mapName_SmallLetter == "washington" || mapName_SmallLetter == "germany" || mapName_SmallLetter == "yukon" || mapName_SmallLetter == "france") {
                    bVanillaMap = true;
                }

                string fileName = (bVanillaMap ? "Vanilla" : Provider.map);
                string filePath = System.IO.Path.Combine(UnturnedPaths.RootDirectory.FullName, "Modules", "SodBuildingUpgrader", "config", fileName + ".config");
                if (!File.Exists(filePath)) {
                    return;
                }

                string[] lines = File.ReadAllLines(filePath);
                if (lines.Length == 0) {
                    return;
                }

                List<List<string>> allGroups = new List<List<string>>();
                List<string> currentGroup = new List<string>();
                for (int i = 0; i < lines.Length; i++) {
                    string line = lines[i].Trim();
                    if (line.StartsWith("#")) {
                        continue;
                    }

                    Match m = Regex.Match(line, @"^([\w]{32})\s", RegexOptions.IgnoreCase);
                    if (m.Success) {
                        currentGroup.Add(m.Groups[1].Value);
                    } else {
                        m = Regex.Match(line, @"^upgrade tool\s*:\s*([\w]{32})\s*$", RegexOptions.IgnoreCase);
                        if (m.Success) {
                            if (upgradeTool != Guid.Empty) {
                                CommandWindow.LogError("building upgrade error: multiple upgrade tool guids found at upgrade tool config file");
                                return;
                            }
                            upgradeTool = new System.Guid(m.Groups[1].Value);
                        } else {
                            if (line == "Use_Mouse_Right_Click_Only") {
                                bUseMouseRightClickOnly = true;
                            }
                        }
                    }

                    // note for self: don't put the following inside "else" for the previous if of the regex
                    if (line.Length == 0 || i == lines.Length - 1) {
                        if (currentGroup.Count > 1) {
                            allGroups.Add(new List<string>(currentGroup));
                            currentGroup.Clear();
                        }
                    }
                }

                if (upgradeTool == Guid.Empty) {
                    CommandWindow.LogError("building upgrade error: upgrate tool guid is not set in upgrade tool config file");
                    return;
                }

                foreach (var group in allGroups) {
                    for (int i = 0; i < group.Count; i++) {
                        for (int j = i + 1; j < group.Count; j++) {
                            AddUpgradeOption(new Guid(group[i]), new Guid(group[j]));
                        }
                    }
                }

            } catch (Exception e) {
                CommandWindow.LogError("building upgrade error: exception gfde4h3 got caught: " + e.Message);
            }
        }

        static void AddUpgradeOption(Guid source, Guid target) {
            var sa = Assets.find<ItemAsset>(target);
            if (sa != null) {
                if (!possibleUpgrades.ContainsKey(source)) {
                    possibleUpgrades.Add(source, new List<ItemAsset>());
                }

                if (!possibleUpgrades[source].Contains(sa)) {
                    possibleUpgrades[source].Add(sa);
                }
            }
        }

        public static void OnMeleeHitBarricade(Player player, Transform barricade) {
            try {
                if (player?.equipment?.asset?.GUID == upgradeTool && upgradeTool != Guid.Empty) {
                    if (barricade == lastChecked && (Provider.time - lastCheckedTime) < 3) { // this keep here, don't take to the begining of the fuction cuz first we need to confirm this is the upgrade tool to disable damage (shouldAllow = false)
                        return;
                    }

                    if (bUseMouseRightClickOnly) {
                        UseableMelee um = player.equipment.useable as UseableMelee;
                        if (um != null && MUtilities.UseableMelee_GetSwingMode(um) != ESwingMode.STRONG) { // only work with right click, to not confuse none experienced players, cuz arid hammer is used for repairing to (same as blowtorch) 
                            return;
                        }
                    }

                    BarricadeDrop drop = null;
                    if (BarricadeManager.tryGetRegion(barricade, out byte x, out byte y, out ushort plant, out var barricadeRegion)) {
                        if (plant == ushort.MaxValue) { // not on vehicle
                            drop = barricadeRegion.FindBarricadeByRootTransform(barricade);
                        }
                    }
                    if (drop != null && drop.asset != null && possibleUpgrades.TryGetValue(drop.asset.GUID, out var upgradeTo)) {
                        BarricadeData sd = drop.GetServersideData();
                        if (sd != null && (sd.owner == player.channel.owner.playerID.steamID.m_SteamID || player.quests.isMemberOfGroup(new CSteamID(sd.group)))) {
                            instance?.StartCoroutine(UpgradeBarricadeAtNextTick(drop, x, y, upgradeTo, player));
                        }
                    }
                }

                lastChecked = barricade;
                lastCheckedTime = Provider.time;
            } catch (Exception e) {
                CommandWindow.LogError("building upgrade error: exception ca776eoi3 cought: " + e.Message);
            }
        }

        static Transform lastChecked = null;
        static uint lastCheckedTime = 0;
        public static void OnMeleeHitStructure(Player player, Transform structure) {
            try {
                if (player?.equipment?.asset?.GUID == upgradeTool && upgradeTool != Guid.Empty) {
                    if (structure == lastChecked && (Provider.time - lastCheckedTime) < 3) { // this keep here, don't take to the begining of the fuction cuz first we need to confirm this is the upgrade tool to disable damage (shouldAllow = false)
                        return;
                    }

                    if (bUseMouseRightClickOnly) {
                        UseableMelee um = player.equipment.useable as UseableMelee;
                        if (um != null && MUtilities.UseableMelee_GetSwingMode(um) != ESwingMode.STRONG) { // only work with right click, to not confuse none experienced players, cuz arid hammer is used for repairing to (same as blowtorch) 
                            return;
                        }
                    }

                    StructureDrop drop = null;
                    if (StructureManager.tryGetRegion(structure, out byte x, out byte y, out var structureRegion)) {
                        drop = structureRegion.FindStructureByRootTransform(structure);
                    }
                    if (drop != null && drop.asset != null && possibleUpgrades.TryGetValue(drop.asset.GUID, out var upgradeTo)) {
                        StructureData sd = drop.GetServersideData();
                        if (sd != null && (sd.owner == player.channel.owner.playerID.steamID.m_SteamID || player.quests.isMemberOfGroup(new CSteamID(sd.group)))) {
                            instance?.StartCoroutine(UpgradeStructAtNextTick(drop, x, y, upgradeTo, player));
                        }
                    }
                }

                lastChecked = structure;
                lastCheckedTime = Provider.time;
            } catch (Exception e) {
                CommandWindow.LogError("building upgrade error: exception js6633s cought: " + e.Message);
            }
        }

        static IEnumerator UpgradeStructAtNextTick(StructureDrop drop, byte x, byte y, List<ItemAsset> upgradeTo, Player player) {
            //yield return null;
            ItemStructureAsset targetAsset = null;
            foreach (ItemAsset asset in upgradeTo) {
                if (FindOrCraftItem(player, asset)) {
                    targetAsset = asset as ItemStructureAsset;
                    break;
                }
            }

            if (targetAsset == null) {
                yield break;
            }

            yield return null; // AtNextTick

            PerformStructUpgrade(drop, x, y, targetAsset, player);
        }

        static IEnumerator UpgradeBarricadeAtNextTick(BarricadeDrop drop, byte x, byte y, List<ItemAsset> upgradeTo, Player player) {
            //yield return null;
            ItemBarricadeAsset targetAsset = null;
            foreach (ItemAsset asset in upgradeTo) {
                if (FindOrCraftItem(player, asset)) {
                    targetAsset = asset as ItemBarricadeAsset;
                    break;
                }
            }

            if (targetAsset == null) {
                yield break;
            }

            yield return null; // AtNextTick

            PerformBarricadeUpgrade(drop, x, y, targetAsset, player);
        }

        static void PerformStructUpgrade(StructureDrop drop, byte x, byte y, ItemStructureAsset targetAsset, Player player) {
            try {
                if (!player.inventory.HasItemByAsset(targetAsset)) { // anohter check cuz it is possible when items got auto crafted then it dropped on ground if no inventory space. also this is new tick. so good to do another check at this tick to be 100% safe
                    return;
                }
                if (drop.GetNetId() == NetId.INVALID) { // that mean it got removed in the previous tick or in the current tick before reaching here
                    return;
                }

                StructureData sd = drop.GetServersideData();
                //if (sd != null && StructureManager.tryGetRegion(drop.model, out var x, out var y, out var region)) {
                if (sd != null) {
                    var oldStrctAsset = drop.asset;
                    ushort newStructHealth = (ushort)(targetAsset.health * ((float)sd.structure.health / (float)oldStrctAsset.health));
                    if (newStructHealth < 1) {
                        newStructHealth = 1;
                    }
                    Structure newStructure = new Structure(targetAsset, newStructHealth);
                    Vector3 angle = drop.model.transform.rotation.eulerAngles;
                    bool bSuccess = StructureManager.dropStructure(newStructure, drop.model.position, angle.x, angle.y, angle.z, sd.owner, sd.group);
                    if (bSuccess) {
                        MUtilities.RemoveItemFromPlayerInventory(player, targetAsset);
                        StructureManager.destroyStructure(drop, x, y, (drop.model.position).normalized * 100f, true);
                        player.inventory.forceAddItem(new Item(oldStrctAsset, EItemOrigin.CRAFT), false);
                        StructureDrop newDrop = StructureManager.regions[x, y].drops.Last();
                        var newSData = newDrop.GetServersideData();
                        SendHealth.Invoke(newDrop.GetNetId(), ENetReliability.Unreliable, Provider.GatherClientConnectionsMatchingPredicate((SteamPlayer client) => client.player != null && (client.playerID.steamID.m_SteamID == newSData.owner || client.player.quests.groupID.m_SteamID == newSData.group) && Regions.checkArea(x, y, client.player.movement.region_x, client.player.movement.region_y, StructureManager.STRUCTURE_REGIONS)), (byte)Mathf.RoundToInt((float)(int)newSData.structure.health / (float)(int)newDrop.asset.health * 100f));
                    }
                }
            } catch (Exception e) {
                CommandWindow.LogError("building upgrade error: exception g346rstafe cought: " + e.Message);
            }
        }
        static readonly ClientInstanceMethod<byte> SendHealth = ClientInstanceMethod<byte>.Get(typeof(StructureDrop), "ReceiveHealth");

        static void PerformBarricadeUpgrade(BarricadeDrop drop, byte x, byte y, ItemBarricadeAsset targetAsset, Player player) {
            try {
                if (!player.inventory.HasItemByAsset(targetAsset)) { // anohter check cuz it is possible when items got auto crafted then it dropped on ground if no inventory space. also this is new tick. so good to do another check at this tick to be 100% safe
                    return;
                }
                if (drop.GetNetId() == NetId.INVALID) { // that mean it got removed in the previous tick or in the current tick before reaching here
                    return;
                }
                BarricadeData oldBData = drop.GetServersideData();
                if (oldBData != null) {
                    var oldAsset = drop.asset;
                    ushort newHealth = (ushort)(targetAsset.health * ((float)oldBData.barricade.health / (float)oldAsset.health));
                    if (newHealth < 1) {
                        newHealth = 1;
                    }
                    Barricade newBarricade = new Barricade(targetAsset, newHealth, oldBData.barricade.state);
                    Vector3 angle = drop.model.transform.rotation.eulerAngles;
                    bool bSuccess = BarricadeManager.dropNonPlantedBarricade(newBarricade, drop.model.position, drop.model.rotation, oldBData.owner, oldBData.group);
                    if (bSuccess) {
                        MUtilities.RemoveItemFromPlayerInventory(player, targetAsset);
                        InteractableStorage iStrg = drop.model.GetComponent<InteractableStorage>();
                        if (iStrg != null) {
                            iStrg.despawnWhenDestroyed = true; // to not drop items on ground
                        }
                        BarricadeManager.destroyBarricade(drop, x, y, ushort.MaxValue);
                        player.inventory.forceAddItem(new Item(oldAsset, EItemOrigin.CRAFT), false);
                    }
                }
            } catch (Exception e) {
                CommandWindow.LogError("building upgrade error: exception 4356772sy cought: " + e.Message);
            }
        }

        // todo: also search on the ground next to him maybe?
        static bool FindOrCraftItem(Player player, ItemAsset sa) {
            try {
                if (player == null || sa == null) {
                    return false;
                }
                bool bResult = false;
                if (player.inventory.HasItemByAsset(sa.GUID)) {
                    bResult = true;
                } else {
                    if (sa.blueprints != null) {
                        for (byte i = 0; i < sa.blueprints.Count; i++) {
                            Blueprint blueprint = sa.blueprints[i];
                            if (blueprint.Operation != EBlueprintOperation.RepairTargetItem && blueprint.outputs != null) {
                                bool bTry = false;
                                foreach (var output in blueprint.outputs) {
                                    if (blueprint.outputs[0].IsItem(sa)) {
                                        bTry = true;
                                        break;
                                    }
                                }
                                if (bTry) {
                                    if (MUtilities.CraftItem(player.crafting, blueprint, false, true, true)) {
                                        bResult = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                return bResult;
            } catch (Exception e) {
                CommandWindow.LogError("building upgrade error: exception 345ssu72 cought: " + e.Message);
                return false;
            }
        }

    }
}
