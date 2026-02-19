using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Models;
using Core.Scripts.Utils;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc
{
    public class MatchEnvironmentSpringController
    {
        private MatchEnvironmentSpringView _view;
        private readonly MatchEnvironmentSpringModel _model;

        public MatchEnvironmentSpringController(MatchEnvironmentSpringModel model)
        {
            _model = model;
        }

        public void CreateView(MatchEnvironmentSpringView viewPrefab, Transform parent)
        {
            if (viewPrefab != null)
            {
                _view = Object.Instantiate(viewPrefab, parent);
            }
            else
            {
                var go = new GameObject("EnvironmentSpring_" + _model.Id);
                go.transform.SetParent(parent);
                _view = go.AddComponent<MatchEnvironmentSpringView>();
                var meshRenderer = go.AddComponent<MeshRenderer>();
                // Set default material (magenta/pink usually for error, but here let's try to find a default or just white)
                meshRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default")); // Simple shader
                meshRenderer.sharedMaterial.color = Color.green; // Spring color
            }

            _view.transform.position = new Vector3(_model.Position.x, _model.Position.y, 0);
            _view.transform.rotation = Quaternion.Euler(0, 0, _model.Rotation);

            // Generate Mesh
            var halfSize = _model.Size * 0.5f;
            var points = new Vector2[]
            {
                new Vector2(-halfSize.x, -halfSize.y),
                new Vector2(halfSize.x, -halfSize.y),
                new Vector2(halfSize.x, halfSize.y),
                new Vector2(-halfSize.x, halfSize.y)
            };

            var mesh = MeshUtils.BuildMesh(points);
            _view.Initialize(mesh, null); // Keep material if prefab, or what we set above
        }

        public void PlayBounceAnimation()
        {
            if (_view != null)
            {
                _view.PlayBounceAnimation();
            }
        }

        public void Destroy()
        {
            if (_view != null)
            {
                Object.Destroy(_view.gameObject);
            }
        }
    }
}
