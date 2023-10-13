using Islands.Building;
using System.Collections.Generic;
using UnityEngine;

namespace Islands.Generation
{
    /// <summary>
    /// Compiled list of references to various environmental objects, such as grass, trees & rocks
    /// </summary>
    [CreateAssetMenu(fileName = "New Floraset", menuName = "Custom/Floraset")]
    public class Floraset : ScriptableObject
    {
        // How many flora objects may be spawned across a single tile at maximum
        public static readonly int PointsPerTile = 16;

        [System.Serializable]
        public struct FloraEntry
        {
            public float radius;
            public Transform prefab;
        }

        [Header("Settings")]
        [Range(0f, 1f)][SerializeField] private float density = 1f;
        [SerializeField] private int rockWeight = 1;                    // Weights used for random generation
        [SerializeField] private int treeWeight = 10;                   // Weights are relative to each other
        [SerializeField] private int grassWeight = 100;                 // Example: Rock chance for default values is ~.9% (1/111), grass chance is ~90% (100/111)

        [Header("References")]
        [SerializeField] private List<FloraEntry> rocks = new List<FloraEntry>();   // Rocks arem't plants, but I didn't want to rename the sets.
        [SerializeField] private List<FloraEntry> trees = new List<FloraEntry>();
        [SerializeField] private List<FloraEntry> grass = new List<FloraEntry>();

        public float Density => density;
        public FloraEntry GetRock(int index) => rocks[index];
        public FloraEntry GetTree(int index) => trees[index];
        public FloraEntry GetGrass(int index) => grass[index];

        /// <summary>
        /// Returns a random referenced flora entry. Uses weight settings.
        /// </summary>
        /// <param name="randomBatch"></param>
        /// <returns></returns>
        public FloraEntry GetRandomFlora(Utils.RandomBatch randomBatch)
        {
            var totalWeight = rockWeight + treeWeight + grassWeight;
            var v = randomBatch.NextF() * totalWeight;
            var roll = randomBatch.NextF();

            if (v < rockWeight)
                return rocks[Mathf.FloorToInt(roll * rocks.Count)];
            if (v < treeWeight + rockWeight)
                return trees[Mathf.FloorToInt(roll * trees.Count)];

            return grass[Mathf.FloorToInt(roll * grass.Count)];
        }

        /// <summary>
        /// Returns a random rock
        /// </summary>
        /// <param name="randomBatch"></param>
        /// <returns></returns>
        public FloraEntry GetRandomRock(Utils.RandomBatch randomBatch)
        {
            return GetRock(Mathf.FloorToInt(randomBatch.NextF() * rocks.Count));
        }
    }
}