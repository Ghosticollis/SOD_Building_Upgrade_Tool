using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDG.Framework.Modules;
using SDG.Unturned;
using UnityEngine;

namespace SodBuildingUpgrader {
    public class Main : IModuleNexus {

        // http://wiki.unity3d.com/index.php?title=Singleton
        // https://forum.unity.com/threads/singleton-monobehaviour-script.99971/
        public static Main Instance = null;

        public void initialize() {
            try {
                CommandWindow.Log("Starting SOD Building Upgrader Module...");

                Instance = this;
                GameObject obj = new GameObject();
                Transform transform = obj.transform;
                transform.name = "SodBuildingUpgraderGlobalObj";
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
                GameObject.DontDestroyOnLoad(obj);
                BuildingUpgradeTool.instance = obj.AddComponent<BuildingUpgradeTool>();

                Level.onPostLevelLoaded += (int level) => {
                    try {
                        if (level > Level.BUILD_INDEX_SETUP) {
                            BuildingUpgradeTool.OnLevelLoaded();
                        }
                    } catch (Exception e) {
                        CommandWindow.LogError("building upgrade error: exception gfghg543 got cought: " + e.Message);
                    }
                };

                MHarmony.Init();

            } catch (Exception e) {
                CommandWindow.LogError("building upgrade error: exception fg56545 got cought: " + e.Message);
            }
        }

        public void shutdown() {

        }
    }
}
