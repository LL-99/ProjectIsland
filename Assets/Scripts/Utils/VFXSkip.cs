using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Utils
{
    /// <summary>
    /// Class used for custom prewarming of VisualEffect objects in specific fringe cases for testing purposes only
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(VisualEffect))]
    public class VFXSkip : MonoBehaviour
    {
        VisualEffect vfx;

        public bool skipOnStart = true;

        [Header("WARNING! FRAME COUNTS >1000 MAY CRASH UNITY")]
        public uint frameCount = 0;

        private void Start()
        {
            vfx = GetComponent<VisualEffect>();

            if (skipOnStart)
            {
                StartCoroutine(Prewarm());
            }
        }

        IEnumerator Prewarm()
        {
            yield return new WaitForSeconds(.1f);
            
            vfx.Simulate(Time.deltaTime, frameCount);

            yield return new WaitForSeconds(.1f);
            
            vfx.Simulate(Time.deltaTime, frameCount);
        }
    }
}