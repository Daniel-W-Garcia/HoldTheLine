// In PickupMoverBridge.cs

using MoreMountains.TopDownEngine;
using UnityEngine;

namespace HoldTheLine.Scripts.Upgrades
{
    [RequireComponent(typeof(LaneItem))] 
    public class PickupMoverBridge : PickableWeapon
    {
        private WeaponPickupMover mover;

        protected override void Start()
        {
            base.Start();
            mover = GetComponent<WeaponPickupMover>();
        }

        protected override void Pick(GameObject picker)
        {
            if (mover != null)
            {
                mover.OnItemPickedUp();
            }
        
            GetComponent<LaneItem>().NotifyManagerOfRemoval();
            
            base.Pick(picker);
        }
    }
}