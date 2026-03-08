using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.FieldBarriers.Scripts
{
    public class EnvironmentFieldBarrierView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private Texture2D _texture;

        public void SetMesh(Mesh mesh)
        {
            _meshFilter.sharedMesh = mesh;
        }
        
        public void SetColor(Color color)
        {
            _spriteRenderer.material.color = color;
            _meshRenderer.material.color = color;
        }

        public void SetSprite(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
        }

        public Texture2D GetTexture()
        {
            return _texture;
        }
    }
}
