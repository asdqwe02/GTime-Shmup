using System.Linq;
using JetBrains.Annotations;
using PKFramework.Core.Editor;
using PKFramework.Utils;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace PKFramework.Macro.Editor
{
    [UsedImplicitly]
    public class MacroMenu: Menu<MacroData>
    {
        public override string MenuName => "Settings/Macro";

        [UsedImplicitly]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public MacroData MacroData;

        public MacroMenu()
        {
            MacroData = ConfigHelper.GetConfig<MacroData>();
        }
        
        public override bool UseCustomMenu => true;

        [Header("Modules")]
        [UsedImplicitly]
        public bool UseOdin;
        [UsedImplicitly]
        public bool UseLidar;
        [UsedImplicitly]
        public bool UseSerialPort;

        
        [Button(ButtonSizes.Gigantic), GUIColor(0, 1, 0)]
        public void GenerateMacros()
        {
            var macros = MacroData.Macros;
            if (UseOdin)
            {
                macros.Add("ODIN_INSPECTOR");
                macros.Add("ODIN_INSPECTOR_3");
                macros.Add("ODIN_INSPECTOR_3_1");
            }

            if (UseLidar)
            {
                macros.Add("PK_USE_LIDAR_MODULE");
            }

            if (UseSerialPort)
            {
                macros.Add("PK_USE_SERIAL_PORT_MODULE");
            }
            var defines = MacroData.Macros.Aggregate("", (current, item) => $"{current}{item}; ");
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, defines);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, defines);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
        
    }
}