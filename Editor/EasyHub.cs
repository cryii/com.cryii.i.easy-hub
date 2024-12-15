using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using Newtonsoft.Json.Linq;
using CryII.I.UIFramework;

namespace CryII.I.EasyHub
{
    public static class EasyHub
    {
        [MenuItem("EasyHub/Launch/Initialize Workspace (step 1)", false, 0)]
        public static void InitializeWorkspace()
        {
            if (!InitConfig.TryGet(out var easyInitConfig)) return;

            InstallPackages(easyInitConfig);
            CreateWorkDirectory(easyInitConfig);
        }
        
        [MenuItem("EasyHub/Launch/Initialize Framework (step 2)", false, 1)]
        public static void InitializeFramework()
        {
            UIFrameworkEditor.InitializeUIFramework();
        }

        private static void InstallPackages(InitConfig initConfig)
        {
            if (initConfig.Packages == null)
            {
                Debug.LogError("failed to parse 'packages' property to dictionary!");
                return;
            }
            
            var manifestFullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Packages/manifest.json"));
            var manifestTxt = File.ReadAllText(manifestFullPath);
            var manifestJson = JObject.Parse(manifestTxt);
            var packageInstalled = manifestJson["dependencies"]!.ToObject<Dictionary<string, string>>();

            foreach (var (packageName, gitUrl) in initConfig.Packages)
            {
                if (packageInstalled!.ContainsKey(packageName))
                {
                    Debug.Log($"package already installed: {packageName}");
                    continue;
                }

                var addReq = Client.Add(gitUrl);
                if (addReq.Status == StatusCode.Failure)
                {
                    Debug.LogError(
                        $"failed to install package through git, url: {gitUrl}, error message: {addReq.Error}");
                }
                else
                {
                    Debug.Log($"successfully installed package through git: {packageName}");
                }
            }
        }

        private static void CreateWorkDirectory(InitConfig initConfig)
        {
            if (initConfig.WorkDirectoryConfig == null)
            {
                Debug.LogError("the 'work_directory' property was not found!");
                return;
            }
            
            if (initConfig.WorkDirectoryConfig.Assets == null)
            {
                Debug.LogError("the 'Assets' property was not found!");
                return;
            }

            DeepCreateWorkDirectory(initConfig.WorkDirectoryConfig.Assets, "Assets");
        }

        private static void DeepCreateWorkDirectory(JObject root, string path)
        {
            if (root == null) return;

            foreach (var jProperty in root.Properties())
            {
                var folderName = jProperty.Name;
                var folderSlug = Path.Combine(path, folderName);
                if (AssetDatabase.IsValidFolder(folderSlug))
                {
                    Debug.Log($"the folder already exists, will skip creating it: {folderSlug}");
                    continue;
                }

                var uuid = AssetDatabase.CreateFolder(path, folderName);

                DeepCreateWorkDirectory(jProperty.Value as JObject, folderSlug);

                if (string.IsNullOrEmpty(uuid))
                {
                    Debug.LogError($"failed to create folder, path is: {folderSlug}");
                }
                else
                {
                    Debug.Log($"folder was created to the following path: {folderSlug}");
                }
            }
        }
    }
}