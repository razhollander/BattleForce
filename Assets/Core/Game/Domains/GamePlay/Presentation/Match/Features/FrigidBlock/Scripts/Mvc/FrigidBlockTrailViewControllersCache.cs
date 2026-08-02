using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc
{
    public class FrigidBlockTrailViewControllersCache
    {
        private readonly Dictionary<FrigidBlockView, FrigidBlockTrailViewController> _trailViewControllerByView = new();

        public FrigidBlockTrailViewController GetOrCreateTrailViewController(FrigidBlockView view)
        {
            if (_trailViewControllerByView.TryGetValue(view, out var trailViewController))
            {
                return trailViewController;
            }

            trailViewController = new FrigidBlockTrailViewController(view);
            _trailViewControllerByView.Add(view, trailViewController);

            return trailViewController;
        }

        public void DestroyCachedTrailMeshes()
        {
            foreach (var view in _trailViewControllerByView.Keys)
            {
                view.DestroyTrailMesh();
            }

            _trailViewControllerByView.Clear();
        }
    }
}
