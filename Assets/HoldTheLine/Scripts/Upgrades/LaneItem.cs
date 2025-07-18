using UnityEngine;

namespace HoldTheLine.Scripts.Upgrades
{
    public class LaneItem : MonoBehaviour
    {
        private UpgradeLaneManager manager;
        public int HealthAmount { get; private set; }
        public bool IsUpgrade { get; private set; }

        public void SetManager(UpgradeLaneManager mgr) => manager = mgr;
        
        public void ConfigureAsWall(int hp)
        {
            IsUpgrade = false;
            HealthAmount = hp;
        }

        public void ConfigureAsUpgrade()
        {
            IsUpgrade = true;
        }

        public void NotifyManagerOfRemoval()
        {
            if (manager == null) return;

            // Only notify if this GameObject is literally the front slot
            if (manager.LaneItems.Count > 0 
                && manager.LaneItems[0] == gameObject)
            {
                manager.OnFrontItemDestroyed();
            }
        }
    }
}