using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SDG.Unturned;

namespace SodBuildingUpgrader {
    class MUtilities {

        static FieldInfo ItemStorageAssetSXField = typeof(ItemStorageAsset).GetField("_storage_x", BindingFlags.Instance | BindingFlags.NonPublic);
        static FieldInfo ItemStorageAssetSYField = typeof(ItemStorageAsset).GetField("_storage_y", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void SetStorageSize(ItemStorageAsset sa, byte x, byte y) {
            if (sa != null && ItemStorageAssetSXField != null && ItemStorageAssetSYField != null) {
                ItemStorageAssetSXField.SetValue(sa, x);
                ItemStorageAssetSYField.SetValue(sa, y);
            }
        }

        static FieldInfo UseableMeleeSwingModeField = typeof(UseableMelee).GetField("swingMode", BindingFlags.Instance | BindingFlags.NonPublic);

        public static ESwingMode UseableMelee_GetSwingMode(UseableMelee um) {
            if (um != null && UseableMeleeSwingModeField != null) {
                return (ESwingMode)UseableMeleeSwingModeField.GetValue(um);
            }
            return ESwingMode.WEAK;
        }

        public static bool RemoveItemFromPlayerInventory(Player player, Asset asset) {
            if (player == null || asset == null) {
                return false;
            }
            try {
                List<PlayerInventorySearchResultV2> result = new List<PlayerInventorySearchResultV2>();
                player.inventory.FindFirstItemByAsset(result, asset);
                if (result != null && result.Count > 0) {
                    PlayerInventorySearchResultV2 inventorySearch = result[0];
                    byte index = player.inventory.getIndex(inventorySearch.Page, inventorySearch.Jar.x, inventorySearch.Jar.y);
                    player.inventory.removeItem(inventorySearch.Page, index);
                    return true;
                }
            } catch (Exception e) {
                CommandWindow.LogError("building upgrade error: exception 42278dss cought: " + e.Message);
            }
            return false;
        }


        static MethodInfo PlayerCraftingHandleCraftMethod = typeof(PlayerCrafting).GetMethod("HandleCraftRequestInternal", BindingFlags.Instance | BindingFlags.NonPublic);
        public static bool CraftItem(PlayerCrafting playerCrafting, Blueprint blueprint, bool asManyAsPossible, bool playEffect, bool bypassWorkstationRequirements) {
            if (PlayerCraftingHandleCraftMethod != null && playerCrafting != null && blueprint != null) {
                return (bool)PlayerCraftingHandleCraftMethod.Invoke(playerCrafting, new object[] { new ServerInvocationContext(), blueprint, asManyAsPossible, playEffect, bypassWorkstationRequirements });
            }
            return false;
        }

    }
}
