using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using CasualGames.Operius2D.Configs;
using PKFramework.Macro;
using PKFramework.Utils;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace CasualGames.Operius2D.Editor
{
    public class Operius2DMenu : OdinMenuEditorWindow
    {
        private SpawnerConfig _spawnerConfig;

        [MenuItem("Operius2D/Main Menu")]
        private static void OpenWindow()
        {
            GetWindow<Operius2DMenu>().Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _spawnerConfig = AssetDatabase.LoadAssetAtPath<SpawnerConfig>(
                "Assets/CasualGames/Settings/Operius2DSettings/SpawnerConfig.asset");
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree
            {
                Selection =
                {
                    SupportsMultiSelect = false
                }
            };
            tree.Add("Config/Spawner", _spawnerConfig);
            tree.Add("Build", new BuildButton());
            return tree;
        }

        private class BuildButton
        {
            private static string[] Levels => new[]
            {
                "Assets/CasualGames/Scenes/Operius2DScene/Scenes/Operius2DScene.unity"
            };

            [Button(ButtonSizes.Gigantic), GUIColor(0, 1, 0)]
            public void BuildMobile()
            {
                var macros = new List<string>(ConfigHelper.GetConfig<MacroData>().Macros) { "MOBILE" };
                macros.Remove("PK_USE_SERIAL_PORT_MODULE");
                macros.Remove("ARCADE");
                var defines = macros.Aggregate("", (current, item) => $"{current}{item}; ");

                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, defines);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, defines);

                BuildPipeline.BuildPlayer(Levels, "Build/Mobile/Operius2D.apk", BuildTarget.Android,
                    BuildOptions.None);
                Directory.Delete("Build/Mobile/Operius2D_BurstDebugInformation_DoNotShip", true);
            }

            [Button(ButtonSizes.Gigantic), GUIColor(0, 1, 0)]
            public void BuildArcade()
            {
                var macros = new List<string>(ConfigHelper.GetConfig<MacroData>().Macros)
                {
                    "PK_USE_SERIAL_PORT_MODULE",
                    "ARCADE"
                };
                macros.Remove("MOBILE");
                var defines = macros.Aggregate("", (current, item) => $"{current}{item}; ");

                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, defines);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, defines);


                BuildPipeline.BuildPlayer(Levels, "Build/Arcade/Windows/Operius2D.exe", BuildTarget.StandaloneWindows64,
                    BuildOptions.None);
                Directory.Delete("Build/Arcade/Windows/Operius2D_BurstDebugInformation_DoNotShip", true);
                BuildPipeline.BuildPlayer(Levels, "Build/Arcade/Linux/Operius2D", BuildTarget.StandaloneLinux64,
                    BuildOptions.None);
                Directory.Delete("Build/Arcade/Linux/Operius2D_BurstDebugInformation_DoNotShip", true);
            }

            [Button(ButtonSizes.Gigantic), GUIColor(0, 1, 0)]
            public void BuildWeb()
            {
                BuildPipeline.BuildPlayer(Levels, "Build/Web/Operius2D", BuildTarget.WebGL,
                    BuildOptions.None);
                Directory.Delete("Build/Web/Operius2D_BurstDebugInformation_DoNotShip", true);
            }

            [Button(ButtonSizes.Gigantic), GUIColor(0, 1, 0)]
            public void BuildAll()
            {
                BuildMobile();
                BuildArcade();
                BuildWeb();
            }
        }
    }
}