using UnityEngine;

namespace CasualGames.Common.Input
{
    [CreateAssetMenu(menuName = "Casual Game/InputConfig", fileName = "InputConfig.asset")]
    public class InputConfig : ScriptableObject
    {
        public KeyCode InputKey;
    }
}