namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    public enum MoleStateType
    {
        InHole = 0,
        OutsideHole = 1,
        Hit = 2,
        EmergingFromHole = 3, // still hidden inside the hole, only the hole shakes
    }
}
