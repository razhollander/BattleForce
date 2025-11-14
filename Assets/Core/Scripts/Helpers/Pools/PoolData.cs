namespace CoreDomain.Scripts.Helpers.Pools
{
    public struct PoolData
    {
        public readonly int InitialAmount;
        public readonly int IncreaseStepAmount;

        public PoolData(int initialAmount, int increaseStepAmount)
        {
            InitialAmount = initialAmount;
            IncreaseStepAmount = increaseStepAmount;
        }
    }
}