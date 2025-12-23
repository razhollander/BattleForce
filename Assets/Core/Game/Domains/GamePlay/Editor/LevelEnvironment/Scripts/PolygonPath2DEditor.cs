// PolygonPath2DEditor.cs  (put under an "Editor" folder)
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PolygonPath2D))]
public class PolygonPath2DEditor : Editor
{
    PolygonPath2D _path;
    int _selected = -1;

    // Controls
    const float PickSize = 8f;     // clickable dot size
    const float InsertDist = 0.08f; // max distance to segment to insert

    void OnEnable() => _path = (PolygonPath2D)target;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Scene Controls", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Shift + LMB: add point (or insert on nearest segment)\n" +
            "LMB: select point\n" +
            "Drag selected point with handle\n" +
            "Ctrl + LMB (or Backspace): delete selected point",
            MessageType.Info);

        _path.Closed = EditorGUILayout.Toggle("Closed", _path.Closed);

        if (GUILayout.Button("Reverse Winding"))
        {
            Undo.RecordObject(_path, "Reverse Polygon Points");
            _path.Points.Reverse();
            EditorUtility.SetDirty(_path);
        }

        if (GUILayout.Button("Clear"))
        {
            Undo.RecordObject(_path, "Clear Polygon Points");
            _path.Points.Clear();
            _selected = -1;
            EditorUtility.SetDirty(_path);
        }

        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        if (_path == null) return;

        Event e = Event.current;
        var t = _path.transform;

        // Draw polyline with indices
        Handles.color = Color.yellow;
        for (int i = 0; i < _path.Points.Count; i++)
        {
            Vector3 wp = t.TransformPoint(_path.Points[i]);
            float size = HandleUtility.GetHandleSize(wp) * 0.06f;

            // clickable dot
            Handles.color = (i == _selected) ? Color.cyan : Color.yellow;
            if (Handles.Button(wp, Quaternion.identity, size, size, Handles.DotHandleCap))
            {
                _selected = i;
                GUI.changed = true;
            }

            // index label
            Handles.Label(wp + Vector3.up * size, i.ToString());
        }

        Handles.color = Color.yellow;
        for (int i = 0; i < _path.Points.Count - 1; i++)
            Handles.DrawLine(t.TransformPoint(_path.Points[i]), t.TransformPoint(_path.Points[i + 1]));

        if (_path.Closed && _path.Points.Count >= 3)
            Handles.DrawLine(t.TransformPoint(_path.Points[^1]), t.TransformPoint(_path.Points[0]));

        // Move selected point handle
        if (_selected >= 0 && _selected < _path.Points.Count)
        {
            Vector3 wp = t.TransformPoint(_path.Points[_selected]);
            EditorGUI.BeginChangeCheck();
            Vector3 newWp = Handles.PositionHandle(wp, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_path, "Move Polygon Point");
                Vector3 lp = t.InverseTransformPoint(newWp);
                _path.Points[_selected] = new Vector2(lp.x, lp.y);
                EditorUtility.SetDirty(_path);
            }
        }

        // Keyboard delete
        if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Backspace || e.keyCode == KeyCode.Delete))
        {
            TryDeleteSelected();
            e.Use();
        }

        // Shift+Click to add/insert point
        if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
        {
            Vector3 wp = MouseToWorldOnXY(t, e.mousePosition);
            Vector3 lp3 = t.InverseTransformPoint(wp);
            Vector2 lp = new Vector2(lp3.x, lp3.y);

            Undo.RecordObject(_path, "Add Polygon Point");

            int insertIndex = FindClosestSegmentInsertIndex(_path, lp, out float dist);
            if (insertIndex != -1 && dist <= InsertDist)
            {
                _path.Points.Insert(insertIndex, lp);
                _selected = insertIndex;
            }
            else
            {
                _path.Points.Add(lp);
                _selected = _path.Points.Count - 1;
            }

            EditorUtility.SetDirty(_path);
            e.Use();
        }

        // Ctrl+Click to delete point (if clicking one)
        if (e.type == EventType.MouseDown && e.button == 0 && e.control)
        {
            int idx = PickPointIndex(_path, e.mousePosition);
            if (idx != -1)
            {
                _selected = idx;
                TryDeleteSelected();
                e.Use();
            }
        }
    }

    void TryDeleteSelected()
    {
        if (_selected < 0 || _selected >= _path.Points.Count) return;
        Undo.RecordObject(_path, "Delete Polygon Point");
        _path.Points.RemoveAt(_selected);
        _selected = Mathf.Clamp(_selected, 0, _path.Points.Count - 1);
        EditorUtility.SetDirty(_path);
    }

    // --- helpers ---

    static Vector3 MouseToWorldOnXY(Transform t, Vector2 mousePos)
    {
        // Intersect mouse ray with plane Z = transform.position.z (works for 2D-in-XY authoring)
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
        Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, t.position.z));
        plane.Raycast(ray, out float enter);
        return ray.GetPoint(enter);
    }

    static int PickPointIndex(PolygonPath2D path, Vector2 mousePos)
    {
        Transform t = path.transform;
        float best = float.MaxValue;
        int bestIdx = -1;

        for (int i = 0; i < path.Points.Count; i++)
        {
            Vector3 wp = t.TransformPoint(path.Points[i]);
            float d = HandleUtility.DistanceToCircle(wp, HandleUtility.GetHandleSize(wp) * 0.06f);
            if (d < best && d < PickSize)
            {
                best = d;
                bestIdx = i;
            }
        }
        return bestIdx;
    }

    static int FindClosestSegmentInsertIndex(PolygonPath2D path, Vector2 p, out float bestDist)
    {
        bestDist = float.MaxValue;
        int bestInsert = -1;

        int count = path.Points.Count;
        if (count < 2) return -1;

        // segments i -> i+1
        for (int i = 0; i < count - 1; i++)
        {
            float d = DistancePointToSegment(p, path.Points[i], path.Points[i + 1]);
            if (d < bestDist)
            {
                bestDist = d;
                bestInsert = i + 1;
            }
        }

        // closing segment
        if (path.Closed && count >= 3)
        {
            float d = DistancePointToSegment(p, path.Points[^1], path.Points[0]);
            if (d < bestDist)
            {
                bestDist = d;
                bestInsert = count; // insert at end (between last and first)
            }
        }

        return bestInsert;
    }

    static float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(p - a, ab) / Mathf.Max(1e-8f, ab.sqrMagnitude);
        t = Mathf.Clamp01(t);
        Vector2 proj = a + t * ab;
        return Vector2.Distance(p, proj);
    }
}
