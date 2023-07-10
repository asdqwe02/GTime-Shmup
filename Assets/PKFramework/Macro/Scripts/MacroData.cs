using System.Collections.Generic;
using UnityEngine;

namespace PKFramework.Macro
{
    [System.Serializable]
    public class MacroData : ScriptableObject
    {
        [SerializeField]
        private List<string> _macros = new();
        public List<string> Macros => _macros;
        
    }
}