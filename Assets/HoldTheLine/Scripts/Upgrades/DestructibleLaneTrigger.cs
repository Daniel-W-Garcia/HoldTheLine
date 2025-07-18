using MoreMountains.TopDownEngine;
using UnityEngine;

namespace HoldTheLine.Scripts.Upgrades
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(LaneItem))]
    public class DestructibleLaneTrigger : MonoBehaviour
    {
        private Health health;
        private LaneItem laneItem;

        void Awake()
        {
            health = GetComponent<Health>();
            laneItem = GetComponent<LaneItem>();
        }

        void OnEnable()
        {
            health.OnDeath += OnItemDestroyed;
        }

        void OnDisable()
        {
            health.OnDeath -= OnItemDestroyed;
        }

        private void OnItemDestroyed()
        {
            laneItem.NotifyManagerOfRemoval();
        }
    }
}