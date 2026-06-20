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

            // Background
            EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));

            // RMS bar — green → yellow → red
            var fillRect = new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(rms), rect.height);
            var barColor = rms < 0.6f
                ? Color.Lerp(new Color(0.2f, 0.85f, 0.2f), new Color(0.95f, 0.85f, 0.1f), rms / 0.6f)
                : Color.Lerp(new Color(0.95f, 0.85f, 0.1f), new Color(0.95f, 0.2f, 0.1f), (rms - 0.6f) / 0.4f);
            EditorGUI.DrawRect(fillRect, barColor);

            // Peak hold marker
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

            var isPlaying = _previewSource != null && _previewSource.isPlaying;
            var rms = isPlaying ? SampleRms() : 0f;

            for (var i = 0; i < keysProp.arraySize; i++)
            {
                var keyProp = keysProp.GetArrayElementAtIndex(i);
                var valueProp = valuesProp.GetArrayElementAtIndex(i);
                var clipProp = valueProp.FindPropertyRelative("Clip");
                var volumeProp = valueProp.FindPropertyRelative("Volume");

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(keyProp.enumDisplayNames[keyProp.enumValueIndex], EditorStyles.boldLabel, GUILayout.Width(180));

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

                EditorGUILayout.EndHorizontal();

                // Show the level meter only on the currently playing clip
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
                    var newVolume = volumeProp.floatValue;
                    _previewSource.SetAudioSourceVolume(newVolume);
                }

                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
