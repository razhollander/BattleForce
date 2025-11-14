using UnityEngine;

namespace CoreDomain.Scripts.Mvc.UICamera
{
    public class UICameraView : MonoBehaviour
    {
        [field:SerializeField] public Camera Camera { get;private set; }
    }
}
