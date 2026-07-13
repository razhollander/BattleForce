using System;
using System.Collections.Generic;
using Core.Scripts.Services.AudioService;
using Core.Scripts.Utils;
using UnityEditor;
using UnityEngine;

namespace Core.Scripts.Editor
{
    [CustomEditor(typeof(AudioClipsScriptableObject), true)]
    public class AudioClipsScriptableObjectEditor : UnityEditor.Editor
    {
        private const int SAMPLE_COUNT = 256;
        private const float METER_HEIGHT = 8f;
        private const float PEAK_HOLD_SECONDS = 1.5f;

        private static GameObject _previewObject;
        private static AudioSource _previewSource;
        private static readonly float[] _samples = new float[SAMPLE_COUNT];
        private static float _peakLevel;
        private static double _peakHoldTime;

        private int _selectedAddTypeIndex;

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (_previewSource != null && _previewSource.isPlaying)
            {
                Repaint();
            }
        }

        private static AudioSource GetOrCreatePreviewSource()
        {
            if (_previewSource != null)
            {
                return _previewSource;
            }

            _previewObject = new GameObject("__AudioPreview__") { hideFlags = HideFlags.HideAndDontSave };
            _previewSource = _previewObject.AddComponent<AudioSource>();
            return _previewSource;
        }

        private static void StopPreview()
        {
            if (_previewSource != null)
            {
                _previewSource.Stop();
            }

            _peakLevel = 0f;
        }

        private static void PlayPreview(AudioClip clip, float effectiveVolume)
        {
            var source = GetOrCreatePreviewSource();
            source.Stop();
            source.clip = clip;
            source.volume = effectiveVolume;
            source.Play();
            _peakLevel = 0f;
            _peakHoldTime = EditorApplication.timeSinceStartup;
        }

        private static float SampleRms()
        {
            if (_previewSource == null || !_previewSource.isPlaying)
            {
                return 0f;
            }

            _previewSource.GetOutputData(_samples, 0);
            var sum = 0f;
            foreach (var s in _samples)
            {
                sum += s * s;
            }

            return Mathf.Sqrt(sum / SAMPLE_COUNT);
        }

        private static void DrawLevelMeter(float rms)
        {
            var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(METER_HEIGHT), GUILayout.ExpandWidth(true));

            EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));

            var fillRect = new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(rms), rect.height);
            var barColor = rms < 0.6f
                ? Color.Lerp(new Color(0.2f, 0.85f, 0.2f), new Color(0.95f, 0.85f, 0.1f), rms / 0.6f)
                : Color.Lerp(new Color(0.95f, 0.85f, 0.1f), new Color(0.95f, 0.2f, 0.1f), (rms - 0.6f) / 0.4f);
            EditorGUI.DrawRect(fillRect, barColor);

            if (rms >= _peakLevel)
            {
                _peakLevel = rms;
                _peakHoldTime = EditorApplication.timeSinceStartup;
            }
            else if (EditorApplication.timeSinceStartup - _peakHoldTime > PEAK_HOLD_SECONDS)
            {
                _peakLevel = Mathf.MoveTowards(_peakLevel, 0f, Time.deltaTime * 0.5f);
            }

            var peakX = rect.x + rect.width * Mathf.Clamp01(_peakLevel);
            EditorGUI.DrawRect(new Rect(peakX - 1f, rect.y, 2f, rect.height), Color.white);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var audioClipsSO = (AudioClipsScriptableObject)target;

            DrawPropertiesExcluding(serializedObject, "AudioClips");

            EditorGUILayout.LabelField("Audio Clips", EditorStyles.boldLabel);

            if (audioClipsSO.AudioClips == null)
            {
                EditorGUILayout.HelpBox("AudioClips dictionary is null.", MessageType.Warning);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            var audioClipsProp = serializedObject.FindProperty("AudioClips");
            if (audioClipsProp == null)
            {
                EditorGUILayout.HelpBox("Could not find AudioClips property.", MessageType.Warning);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            var keysProp = audioClipsProp.FindPropertyRelative("m_keys");
            var valuesProp = audioClipsProp.FindPropertyRelative("m_values");

            if (keysProp == null || valuesProp == null)
            {
                DrawDefaultInspector();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            // --- Add row ---
            var unusedTypes = GetUnusedTypes(keysProp);
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = unusedTypes.Count > 0;
            _selectedAddTypeIndex = Mathf.Clamp(_selectedAddTypeIndex, 0, Mathf.Max(0, unusedTypes.Count - 1));
            var displayNames = unusedTypes.ConvertAll(t => t.ToString()).ToArray();
            _selectedAddTypeIndex = EditorGUILayout.Popup(_selectedAddTypeIndex, displayNames);
            if (GUILayout.Button("Add", GUILayout.Width(50)) && unusedTypes.Count > 0)
            {
                var newType = unusedTypes[_selectedAddTypeIndex];
                var newIndex = keysProp.arraySize;
                keysProp.InsertArrayElementAtIndex(newIndex);
                valuesProp.InsertArrayElementAtIndex(newIndex);
                keysProp.GetArrayElementAtIndex(newIndex).intValue = (int)newType;
                var newValueProp = valuesProp.GetArrayElementAtIndex(newIndex);
                newValueProp.FindPropertyRelative("Clip").objectReferenceValue = null;
                newValueProp.FindPropertyRelative("Volume").floatValue = 0f;
                _selectedAddTypeIndex = 0;
                serializedObject.ApplyModifiedProperties();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            // --- Entries ---
            var isPlaying = _previewSource != null && _previewSource.isPlaying;
            var rms = isPlaying ? SampleRms() : 0f;
            var removeIndex = -1;

            for (var i = 0; i < keysProp.arraySize; i++)
            {
                var keyProp = keysProp.GetArrayElementAtIndex(i);
                var valueProp = valuesProp.GetArrayElementAtIndex(i);
                var clipProp = valueProp.FindPropertyRelative("Clip");
                var volumeProp = valueProp.FindPropertyRelative("Volume");

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();

                var keyLabel = keyProp.enumValueIndex >= 0 && keyProp.enumValueIndex < keyProp.enumDisplayNames.Length
                    ? keyProp.enumDisplayNames[keyProp.enumValueIndex]
                    : $"<missing enum value: {keyProp.intValue}>";
                EditorGUILayout.LabelField(keyLabel, EditorStyles.boldLabel, GUILayout.Width(180));

                var clip = (AudioClip)clipProp.objectReferenceValue;
                var volume = volumeProp.floatValue;
                var effectiveVolume = (volume + 1f) / 2f;

                GUI.enabled = clip != null;
                if (GUILayout.Button("▶", GUILayout.Width(28)))
                {
                    PlayPreview(clip, effectiveVolume);
                }
                GUI.enabled = true;
                if (GUILayout.Button("■", GUILayout.Width(28)))
                {
                    StopPreview();
                }

                if (clip != null)
                {
                    EditorGUILayout.LabelField($"vol: {effectiveVolume:F2}", GUILayout.Width(60));
                }

                GUILayout.FlexibleSpace();

                var prevColor = GUI.color;
                GUI.color = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button("−", GUILayout.Width(22)))
                {
                    removeIndex = i;
                }
                GUI.color = prevColor;

                EditorGUILayout.EndHorizontal();

                var thisClipPlaying = isPlaying && _previewSource.clip == clip;
                if (thisClipPlaying)
                {
                    DrawLevelMeter(rms);
                }

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(clipProp, new GUIContent("Clip"));

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(volumeProp, new GUIContent("Volume"));
                if (EditorGUI.EndChangeCheck() && isPlaying && _previewSource.clip == clip)
                {
                    _previewSource.SetAudioSourceVolume(volumeProp.floatValue);
                }

                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            if (removeIndex >= 0)
            {
                if (isPlaying && _previewSource.clip == (AudioClip)valuesProp.GetArrayElementAtIndex(removeIndex).FindPropertyRelative("Clip").objectReferenceValue)
                {
                    StopPreview();
                }

                keysProp.DeleteArrayElementAtIndex(removeIndex);
                valuesProp.DeleteArrayElementAtIndex(removeIndex);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static List<AudioClipType> GetUnusedTypes(SerializedProperty keysProp)
        {
            var used = new HashSet<int>();
            for (var i = 0; i < keysProp.arraySize; i++)
            {
                used.Add(keysProp.GetArrayElementAtIndex(i).intValue);
            }

            var result = new List<AudioClipType>();
            foreach (AudioClipType value in Enum.GetValues(typeof(AudioClipType)))
            {
                if (value == AudioClipType.None)
                {
                    continue;
                }

                if (!used.Contains((int)value))
                {
                    result.Add(value);
                }
            }

            return result;
        }
    }
}
