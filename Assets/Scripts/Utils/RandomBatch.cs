using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Buffer of pregenerated random numbers used for complex procedural generations
    /// </summary>
    [System.Serializable]
    public class RandomBatch
    {
        [SerializeField] private string batchName;
        [SerializeField] private int batchSeed;
        [SerializeField] private int[] buffer;
        [SerializeField] private int bufferIndex;
        [SerializeField] private int batchMaxValue;

        /// <summary>
        /// Creates a new random batch
        /// </summary>
        /// <param name="name">Name of the batch</param>
        /// <param name="seed"></param>
        /// <param name="capacity"></param>
        /// <param name="offset"></param>
        public RandomBatch(string name, int seed, int capacity, int offset, int maxValue = int.MaxValue)
        {
            batchName = name;
            batchSeed = seed;
            buffer = new int[capacity];
            bufferIndex = 0;
            batchMaxValue = maxValue;

            // Pregenerate batch
            var rnd = new System.Random(seed);

            for (int i = 0; i < offset; i++)
                rnd.Next(maxValue);

            for (int i = 0; i < capacity; i++)
                buffer[i] = rnd.Next(maxValue);
        }
        
        /// <summary>
        /// Returns the next buffered value
        /// </summary>
        /// <returns></returns>
        public int Next()
        {
            bufferIndex = (bufferIndex + 1) % buffer.Length;
            return buffer[bufferIndex];
        }

        /// <summary>
        /// Returns the next buffered value as a ratio between 0 and 1
        /// </summary>
        /// <returns></returns>
        public float NextF()
        {
            return ((float)Next() / batchMaxValue);
        }

        public override string ToString()
        {
            return "[" + base.ToString() + "]: " + batchName + ", " + batchSeed + ", " + bufferIndex + "/" + buffer.Length;
        }
    }
}