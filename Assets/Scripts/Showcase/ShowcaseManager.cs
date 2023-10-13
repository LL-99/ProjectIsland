using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Showcase {

    /// <summary>
    /// Maintains the camera orbit around the island
    /// </summary>
    public class ShowcaseManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera orbitCam;
        [SerializeField] private CinemachineSmoothPath orbitPath;
        [SerializeField] private Transform orbitCamLookAnchor;

        [Header("Settings")]
        [SerializeField] private bool orbitIsPaused = false;    // Is the orbit paused?

        [SerializeField] private float orbitHeight = 10f;       // The cameras vertical offset above the orbit in world units
        [SerializeField] private float orbitMaxHeight = 100f;   // The maximum height at which the camera is above the islands center
        [SerializeField] private float orbitSpeed = 10f;        // The orbit speed in world units per second
        [SerializeField] private float orbitPosition = 0f;      // The current orbit position in world units

        [SerializeField] private float radiusMultiplier = 1.5f; // Used to define the initial distance from the island depending on its size

        /// <summary>
        /// Initializes the Cinemachine orbit
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        public void Setup(Vector3 center, float radius)
        {
            orbitPath.transform.position = center;

            // CinemachineSmoothPath automatically creates a smooth path between all given waypoints -> just add the "corners"
            orbitPath.m_Waypoints = new CinemachineSmoothPath.Waypoint[]
            {
                new CinemachineSmoothPath.Waypoint() { position = Vector3.right * radius * radiusMultiplier, roll = 0f },
                new CinemachineSmoothPath.Waypoint() { position = Vector3.forward * radius * radiusMultiplier, roll = 0f },
                new CinemachineSmoothPath.Waypoint() { position = Vector3.left * radius * radiusMultiplier, roll = 0f },
                new CinemachineSmoothPath.Waypoint() { position = Vector3.back * radius * radiusMultiplier, roll = 0f },
            };

            // Set the path resolution to just below the maximum to ensure an actually smooth path
            orbitPath.m_Resolution = 99;

            // Make sure we have an object to look at and place it in the center of the island
            if (!orbitCamLookAnchor)
                orbitCamLookAnchor = new GameObject("Cam Anchor").transform;

            orbitCamLookAnchor.parent = transform;
            orbitCamLookAnchor.position = center;

            orbitCam.LookAt = orbitCamLookAnchor;

            // Set up the initial height offset
            var dolly = orbitCam.GetCinemachineComponent<CinemachineTrackedDolly>();
            dolly.m_PathOffset = Vector3.up * orbitHeight;

            // Update the path resolution once more due to some otherwise weird Cinemachine interactions that may happen when the island size is changed at runtime
            StartCoroutine(DelayedOrbitResolutionFix());
        }

        /// <summary>
        /// Helper function to force an acceptable orbit resolution
        /// </summary>
        /// <returns></returns>
        IEnumerator DelayedOrbitResolutionFix()
        {
            yield return null;

            orbitPath.m_Resolution = 100;
        }

        /// <summary>
        /// Update the orbit
        /// </summary>
        private void Update()
        {
            if (!orbitCam || !orbitCam.enabled || orbitIsPaused)
                return;

            // Update position and apply it to the dolly
            orbitPosition += orbitSpeed * Time.deltaTime;
            orbitPosition %= orbitPath.PathLength;

            var dolly = orbitCam.GetCinemachineComponent<CinemachineTrackedDolly>();
            dolly.m_PathPosition = orbitPosition;

            orbitHeight += Input.GetAxis("Vertical") * orbitSpeed * Time.deltaTime;                 // Apply the input to the height offset
            orbitHeight = Mathf.Clamp(orbitHeight, -orbitMaxHeight * .99f, orbitMaxHeight * .99f);  // Clamp to avoid unwanted behaviour
            dolly.m_PathOffset = Vector3.up * orbitHeight;                                          // Apply the height offset to the path

            var radiusHeightScale = Mathf.Cos(.5f * Mathf.PI * (orbitHeight / orbitMaxHeight));     // Adapt the radius scale to simulate a spherical behaviour when changing height
            orbitPath.transform.localScale = new Vector3(radiusHeightScale, 1f, radiusHeightScale);
        }
    }
}