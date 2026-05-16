// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Rendering;
//
// namespace Core.Game.Domains.GamePlay.Presentation.Features.Environment.Shadow.Scripts
// {
//     [ExecuteInEditMode] // Allows you to see the shadow in the Scene view without pressing Play
//     public class SpriteShadowRenderer : MonoBehaviour
//     {
//         [Header("Target Setup")]
//         [SerializeField] private Camera targetCamera;
//         [SerializeField] private List<LayerMask> shadowLayers = new List<LayerMask>();
//
//         [Header("Shadow Settings")]
//         [SerializeField] private Vector2 shadowOffset = new Vector2(-0.2f, -0.2f);
//         [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.5f);
//
//         private CommandBuffer _commandBuffer;
//         private Material _shadowMaterial;
//         private Dictionary<Camera, CommandBuffer> _registeredCameras = new Dictionary<Camera, CommandBuffer>();
//
//         private void OnEnable()
//         {
//             InitializeMaterial();
//             RefreshCommandBuffer();
//         }
//
//         private void OnDisable()
//         {
//             Cleanup();
//         }
//
//         private void Update()
//         {
//             // Keep the buffer updated in case sprites move or properties change
// #if UNITY_EDITOR
//             if (!Application.isPlaying) RefreshCommandBuffer();
// #endif
//         }
//
//         private void InitializeMaterial()
//         {
//             if (_shadowMaterial == null)
//             {
//                 // We use a simple unlit tint shader. If you have custom sprite shaders, 
//                 // make sure they have a color/tint property.
//                 Shader spriteShader = Shader.Find("Sprites/Default");
//                 _shadowMaterial = new Material(spriteShader);
//             }
//         }
//
//         public void RefreshCommandBuffer()
//         {
//             if (targetCamera == null) targetCamera = Camera.main;
//             if (targetCamera == null) return;
//
//             Cleanup();
//
//             _commandBuffer = new CommandBuffer();
//             _commandBuffer.name = "Sprite Custom Shadows";
//
//             // Find all SpriteRenderers in the scene matching your layer criteria
//             SpriteRenderer[] allSprites = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
//         
//             // Combine your list of LayerMasks into a single mask
//             int combinedMask = 0;
//             foreach (var mask in shadowLayers)
//             {
//                 combinedMask |= mask.value;
//             }
//
//             // Set up the shadow drawing properties
//             _shadowMaterial.SetColor("_Color", shadowColor);
//             Matrix4x4 offsetMatrix = Matrix4x4.TRS(
//                 new Vector3(shadowOffset.x, shadowOffset.y, 0.05f), // Slightly push back in Z to prevent z-fighting
//                 Quaternion.identity, 
//                 Vector3.one
//             );
//
//             foreach (SpriteRenderer sprite in allSprites)
//             {
//                 // Check if the sprite's layer is included in our mask
//                 if (((1 << sprite.gameObject.layer) & combinedMask) != 0 && sprite.enabled && sprite.gameObject.activeInHierarchy)
//                 {
//                     // Multiply the sprite's world matrix by our offset to shift the shadow
//                     Matrix4x4 shadowMatrix = sprite.transform.localToWorldMatrix * offsetMatrix;
//                 
//                     // Draw the sprite mesh into the command buffer using our shadow material tint
//                     _commandBuffer.DrawRenderer(sprite, _shadowMaterial, 0, 0);
//                 
//                     // Force the command buffer to use our calculated matrix instead of the default renderer matrix
//                     _commandBuffer.SetTransformMatrix(shadowMatrix);
//                 }
//             }
//
//             // Inject the buffer right after the camera finishes rendering regular sprites
//             targetCamera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, _commandBuffer);
//             _registeredCameras.Add(targetCamera, _commandBuffer);
//         }
//
//         private void Cleanup()
//         {
//             foreach (var kvp in _registeredCameras)
//             {
//                 if (kvp.Key != null && kvp.Value != null)
//                 {
//                     kvp.Key.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, kvp.Value);
//                 }
//             }
//             _registeredCameras.Clear();
//             if (_commandBuffer != null)
//             {
//                 _commandBuffer.Release();
//                 _commandBuffer = null;
//             }
//         }
//     }
// }