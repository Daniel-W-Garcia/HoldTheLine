using MoreMountains.TopDownEngine;
using UnityEngine;

namespace HoldTheLine.Scripts.Spawning.Enemy
{
    [CreateAssetMenu(fileName = "CharacterConfiguration", menuName = "Scriptable Objects/CharacterConfiguration")]
    public class CharacterConfiguration : ScriptableObject
    {
        public string characterName;

        /// <summary>
        /// Short summary of the agent
        /// </summary>
        [Multiline]
        public string characterDescription;

        /// <summary>
        /// The Agent prefab that will be used on instantiation
        /// </summary>
        public Character characterPrefab;
    }
}
