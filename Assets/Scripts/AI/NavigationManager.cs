using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

namespace AI
{
    public enum EntitySize
    {
        tiny,
        small,
        medium,
        large,
        huge
    }

    /// <summary>
    /// Handles differently sized entity navmeshes; Currently not in use due swapping functionality towards a single larger navmesh
    /// May be used in the future to have varying fauna roam the island
    /// </summary>
    public class NavigationManager : MonoBehaviour
    {
        [System.Serializable]
        public class NavMeshConfiguration
        {
            public EntitySize size;
            public NavMeshSurface surface;
        }

        [SerializeField] private List<NavMeshConfiguration> navMeshes = new List<NavMeshConfiguration>();


        public NavMeshConfiguration GetNavMesh(EntitySize size) => navMeshes.Find(x => x.size == size);
        public List<NavMeshConfiguration> GetNavMeshes(EntitySize size) => navMeshes.FindAll(x => x.size == size);


        public void Bake()
        {
            foreach(var nm in navMeshes)
            {
                Bake(nm);
            }
        }

        public void Bake(EntitySize size)
        {
            foreach(var nm in GetNavMeshes(size))
            {
                Bake(nm);
            }
        }

        public void Bake(NavMeshConfiguration navMesh)
        {
            navMesh.surface.BuildNavMesh();
        }
    }
}