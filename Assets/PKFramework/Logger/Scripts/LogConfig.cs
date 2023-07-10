using UnityEngine;

namespace PKFramework.Logger
{
    [CreateAssetMenu(fileName = "LogConfig.asset", menuName = "PK/Settings/Log Config")]
    public class LogConfig: ScriptableObject
    {
        [Header("Editor Console")] 
        [SerializeField]
        private bool _enableEditorConsoleLogging;
        public bool EnableEditorConsoleLogging => _enableEditorConsoleLogging;
        [SerializeField]
        private LogLevel _minimumEditorConsoleLogLevel;
        public LogLevel MinimumEditorConsoleLogLevel => _minimumEditorConsoleLogLevel;
        
        [Header("File")]
        [SerializeField]
        private bool _enableFileLogging;
        public bool EnableFileLogging => _enableFileLogging;
        [SerializeField] 
        private LogLevel _minimumFileLogLevel; 
        public LogLevel MinimumFileLogLevel => _minimumFileLogLevel;
    }

}