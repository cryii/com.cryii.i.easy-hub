using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryII.I.EasyHub
{
    public class EasyLuaConfig
    {
        [JsonProperty("dev_directory")] public string DevDirectory;
        [JsonProperty("ts_ui_directory")] public string TsUIDirectory;
        [JsonProperty("unity_dts_file")] public string UnityDtsFile;
    }
    
    public class EasyWorkDirectoryConfig
    {
        [JsonProperty("Assets")] public JObject Assets;
        [JsonProperty("ui_prefab_directory")] public string UIPrefabDirectory;
    }

    public class InitConfig
    {
        [JsonProperty("packages")] public Dictionary<string, string> Packages;
        [JsonProperty("work_directory")] public EasyWorkDirectoryConfig WorkDirectoryConfig;
        [JsonProperty("lua_config")] public EasyLuaConfig LuaConfig;
        
        private const string ConfigFile = "Packages/com.cryii.i.easy-hub/Assets/init_config.json";

        public static bool TryGet(out InitConfig initConfig)
        {
            var configAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(ConfigFile);
            if (configAsset == null)
            {
                Debug.LogError($"initialization file not found, it should have been located at: {ConfigFile}");
                initConfig = default;
                return false;
            }
            
            initConfig = JsonConvert.DeserializeObject<InitConfig>(configAsset.text);
            return initConfig != null;
        }
    }
}