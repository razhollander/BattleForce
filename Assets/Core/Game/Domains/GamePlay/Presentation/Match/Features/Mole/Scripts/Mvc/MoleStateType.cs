namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    public enum MoleStateType
    {
        InHole = 0,
        OutsideHole = 1,
        Hit = 2,
        EmergingFromHole = 3, // still hidden inside the hole, only the hole shakes
        Expiring = 4, // out of the hole and shaking in place before it goes back in, still hittable while it does
    }
}
