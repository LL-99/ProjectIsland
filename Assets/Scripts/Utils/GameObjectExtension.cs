using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Extensions for Unity's GameObject class
/// </summary>
public static class GameObjectExtension
{
    public static T GetComponentInSibling<T>(this GameObject go) where T : Component
    {
        var childComponents = go.transform.parent.GetComponentsInChildren<T>().ToList();    // Get all instances of the requested component
        childComponents.RemoveAll(c => c.gameObject == go || c.transform.parent != go.transform.parent);    // Filter out all instances on the object itself as well as those on "children" or "nephews"
        return childComponents.FirstOrDefault();
    }
}
