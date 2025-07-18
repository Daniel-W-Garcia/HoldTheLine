using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.TopDownEngine;
using UnityEngine;
// for Health

namespace HoldTheLine.Scripts.Upgrades
{
    public class UpgradeLaneManager : MonoBehaviour
    {
        [Header("Lane Configuration")]
        public List<Transform> laneSlots;
        public float moveDuration = 0.5f;

        [Header("Prefabs")]
        public GameObject wallPrefab;
        public List<GameObject> upgradePrefabs;
        [Range(0f,1f)] public float upgradeSpawnChance = 0.2f;

        private readonly List<GameObject> laneItems = new List<GameObject>();
        public IReadOnlyList<GameObject> LaneItems => laneItems;

        void Start()
        {
            InitializeLane();
        }

        void InitializeLane()
        {
            SpawnWall(50);
            SpawnUpgrade(upgradePrefabs[0]);
            SpawnWall(50);
            SpawnWall(50);
            SpawnUpgrade(upgradePrefabs[1]);

            FillToCapacity();
            RefreshInteractability();
        }

        public void OnFrontItemDestroyed()
        {
            // Remove & destroy front
            if (laneItems.Count == 0) return;

            Destroy(laneItems[0]);
            laneItems.RemoveAt(0);

            // Move everyone up
            MoveAllForward();

            // Refill & re-enable front only
            FillToCapacity();
            RefreshInteractability();
        }

        void MoveAllForward()
        {
            for (int i = 0; i < laneItems.Count; i++)
            {
                laneItems[i].transform
                    .DOMove(laneSlots[i].position, moveDuration)
                    .SetEase(Ease.OutCubic);
            }
        }

        // Fill any missing slots at the back
        void FillToCapacity()
        {
            while (laneItems.Count < laneSlots.Count)
                SpawnRandom();
        }

        void SpawnRandom()
        {
            if (Random.value < upgradeSpawnChance && upgradePrefabs.Count > 0)
                SpawnUpgrade(upgradePrefabs[Random.Range(0, upgradePrefabs.Count)]);
            else
                SpawnWall(100);  // or randomize health
        }

        public void SpawnWall(int healthAmount)
        {
            var go = InstantiateAtNextSlot(wallPrefab);
            var item = go.GetComponent<LaneItem>();
            item.ConfigureAsWall(healthAmount);
            item.SetManager(this);
        }

        public void SpawnUpgrade(GameObject upgradePrefab)
        {
            var go = InstantiateAtNextSlot(upgradePrefab);
            var item = go.GetComponent<LaneItem>();
            item.ConfigureAsUpgrade();
            item.SetManager(this);
        }

        GameObject InstantiateAtNextSlot(GameObject prefab)
        {
            if (laneItems.Count >= laneSlots.Count)
                throw new System.InvalidOperationException("Lane is full!");

            var slot = laneSlots[laneItems.Count];
            var go   = Instantiate(prefab, slot.position, slot.rotation);
            laneItems.Add(go);
            return go;
        }

        // Only the front item (index 0) will have its colliders, Health, Pickable, etc. enabled
        void RefreshInteractability()
        {
            for (int i = 0; i < laneItems.Count; i++)
            {
                var go = laneItems[i];
                
                // Colliders
                foreach (var col in go.GetComponentsInChildren<Collider>())
                    col.enabled = (i == 0);
                
                // Health (so only front can die)
                var health = go.GetComponent<Health>();
                if (health != null) health.enabled = (i == 0);

                // PickableWeapon (so only front can be picked)
                var pickable = go.GetComponent<PickableWeapon>();
                if (pickable != null) ((MonoBehaviour)pickable).enabled = (i == 0);
            }
        }
    }
}
