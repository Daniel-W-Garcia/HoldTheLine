using UnityEngine;

namespace HoldTheLine.HoldTheLine.Scripts.Core
{
    /// Add this component to any object with a Health component 
    /// to specify its type for targeted feedback systems.
    public class DamageableType : MonoBehaviour
    {
        public enum Type
        {
            Generic,
            Enemy,
            DestroyableWall,
            Obstacle,
            Player
            // Add any other types you need
        }

        [Tooltip("The type of this damageable object.")]
        public Type ObjectType = Type.Generic;
    }
}