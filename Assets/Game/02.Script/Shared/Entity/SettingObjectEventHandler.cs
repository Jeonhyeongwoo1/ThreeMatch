using UnityEngine;
using UnityEngine.EventSystems;

namespace ThreeMatch.Shared.Entity
{
    public class SettingObjectEventHandler : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject(-1))
                {
                    if (EventSystem.current.currentSelectedGameObject == null) gameObject.SetActive(false);
                }
            }
        }
    }
}