using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SilderValueChange : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup[] _grids;

    [SerializeField] private Slider _slider;

    [SerializeField] private Text _text;
    // Start is called before the first frame update
    void Start()
    {
        _slider.onValueChanged.AddListener(OnSliderChange);
        _grids = GameObject.FindObjectsByType<GridLayoutGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        _slider.value = Screen.height > Screen.width ? Screen.width / 4 : Screen.height / 4;
    }

    private void OnSliderChange(float val)
    {
        _text.text = val.ToString();
        foreach (var grid in _grids)
        {
            grid.cellSize = new Vector2(val, val);
        }
    }
}
