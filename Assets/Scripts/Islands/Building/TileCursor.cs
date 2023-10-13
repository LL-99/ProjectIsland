using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Islands.Building
{
    /// <summary>
    /// Cursor that may appear on specific tile to mark it as selected
    /// </summary>
    public class TileCursor : MonoBehaviour
    {
        [SerializeField] GameObject cursorObject;
        [SerializeField] Animator animator;

        [Header("Settings")]
        [SerializeField] bool active = false;
        [SerializeField] bool inActivation = false;
        [SerializeField] Vector3 targetPosition;

        /// <summary>
        /// Move the cursor to a specific position
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <returns>Could the cursor be moved?</returns>
        public bool MoveCursor(Vector3 targetPosition)
        {
            if (inActivation)
                return false;

            inActivation = true;

            this.targetPosition = targetPosition;

            if(active)
            {
                animator.SetTrigger("Deactivate");
            }
            else
            {
                ShowCursor();
            }

            return true;
        }

        /// <summary>
        /// Animate the cursor's appearance
        /// </summary>
        public void ShowCursor()
        {
            StartCoroutine(_ShowCursor());
        }

        IEnumerator _ShowCursor() {
            yield return null;

            cursorObject.SetActive(true);
            cursorObject.transform.position = targetPosition;

            animator.SetTrigger("Activate");
        }

        /// <summary>
        /// Event triggered by the 'Cursor_Disappear' animation at its end
        /// </summary>
        public void OnFinishDeactivation()
        {
            if(inActivation)
                ShowCursor();
        }

        /// <summary>
        /// Event triggered by the 'Cursor_Appear' animation at its end
        /// </summary>
        public void OnFinishActivation()
        {
            active = true;
            inActivation = false;
        }
    }
}