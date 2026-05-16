using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Core.Scripts.Utils.Shadows
{
    [RequireComponent(typeof(Camera))]
    public class CustomSpriteShadowRenderer : MonoBehaviour
    {
        [Header("Shadow Settings")]
        public Color shadowColor = new Color(0f, 0f, 0f, 0.5f);
        public Vector2 shadowOffset = new Vector2(0.1f, -0.1f);
        public LayerMask shadowLayers;

        [Header("Performance")]
        public float refreshInterval = 0.5f;

        private Camera _camera;
        private CommandBuffer _commandBuffer;
        private Material _shadowMaterial;
        private float _lastRefreshTime;
        private readonly List<SpriteRenderer> _cachedRenderers = new List<SpriteRenderer>();

        public void InitEntryPoint()
        {
            _camera = GetComponent<Camera>();
            _commandBuffer = new CommandBuffer();
            _commandBuffer.name = "Custom Sprite Shadows";

            if (_shadowMaterial == null)
            {
                _shadowMaterial = new Material(Shader.Find("Sprites/Default"));
                _shadowMaterial.color = shadowColor;
            }

            _camera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, _commandBuffer);
            Camera.onPreRender += OnCameraPreRender;
            RefreshRenderers();
        }

        public void InitExitPoint()
        {
            if (_camera != null && _commandBuffer != null)
            {
                _camera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, _commandBuffer);
                _commandBuffer.Release();
            }

            if (_shadowMaterial != null)
            {
                Destroy(_shadowMaterial);
            }

            Camera.onPreRender -= OnCameraPreRender;
            _cachedRenderers.Clear();
        }

        private void RefreshRenderers()
        {
            _cachedRenderers.Clear();
            var allRenderers = FindObjectsOfType<SpriteRenderer>();
            foreach (var renderer in allRenderers)
            {
                if (((1 << renderer.gameObject.layer) & shadowLayers) != 0)
                {
                    _cachedRenderers.Add(renderer);
                }
            }
            _lastRefreshTime = Time.time;
        }

        private void OnCameraPreRender(Camera cam)
        {
            if (cam != _camera || _commandBuffer == null)
            {
                return;
            }

            if (Time.time - _lastRefreshTime > refreshInterval)
            {
                RefreshRenderers();
            }

            _commandBuffer.Clear();

            if (_shadowMaterial != null)
            {
                _shadowMaterial.color = shadowColor;
            }

            Matrix4x4 offsetMatrix = Matrix4x4.Translate(new Vector3(shadowOffset.x, shadowOffset.y, 0.01f));
            Matrix4x4 shadowViewMatrix = _camera.worldToCameraMatrix * offsetMatrix.inverse;

            _commandBuffer.SetViewMatrix(shadowViewMatrix);

            foreach (var renderer in _cachedRenderers)
            {
                if (renderer != null && renderer.gameObject.activeInHierarchy && renderer.enabled)
                {
                    _commandBuffer.DrawRenderer(renderer, _shadowMaterial, 0, 0);
                }
            }

            _commandBuffer.SetViewMatrix(_camera.worldToCameraMatrix);
        }
    }
}
