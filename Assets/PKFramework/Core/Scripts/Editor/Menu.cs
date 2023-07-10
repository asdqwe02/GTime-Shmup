using PKFramework.Utils;
using UnityEngine;

namespace PKFramework.Core.Editor
{
    public abstract class Menu<T>: Menu where T: ScriptableObject
    {
        public override string AssetPath => ConfigHelper.GetConfigPath<T>();
    }
    
    public abstract class Menu
    {
        public virtual string AssetPath
        {
            get;
        }

        public abstract string MenuName
        {
            get;
        }
        
        public virtual bool UseCustomMenu => false;
    }
}