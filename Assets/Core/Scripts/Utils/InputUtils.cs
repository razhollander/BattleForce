using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoreDomain.Scripts.Utils
{
    public static class InputUtils
    {
        public static bool IsOverUI()
        {
            var eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }
    }
}
