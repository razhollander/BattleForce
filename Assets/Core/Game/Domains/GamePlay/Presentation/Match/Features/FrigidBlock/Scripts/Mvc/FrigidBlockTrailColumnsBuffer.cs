namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc
{
    public class FrigidBlockTrailColumnsBuffer
    {
        private readonly FrigidBlockTrailColumn[] _columnArray;
        private readonly int _capacity;
        private int _oldestColumnIndex;
        private int _columnsCount;

        public int ColumnsCount => _columnsCount;

        public FrigidBlockTrailColumnsBuffer(int capacity)
        {
            _capacity = capacity;
            _columnArray = new FrigidBlockTrailColumn[capacity];
        }

        public FrigidBlockTrailColumn GetColumn(int orderFromOldest)
        {
            return _columnArray[(_oldestColumnIndex + orderFromOldest) % _capacity];
        }

        public void Clear()
        {
            _oldestColumnIndex = 0;
            _columnsCount = 0;
        }

        public void AddNewestColumn(FrigidBlockTrailColumn column)
        {
            if (_columnsCount == _capacity)
            {
                RemoveOldestColumn();
            }

            _columnArray[(_oldestColumnIndex + _columnsCount) % _capacity] = column;
            _columnsCount++;
        }

        public void RemoveColumnsSpawnedBefore(float spawnTimeInSeconds)
        {
            while (_columnsCount > 0 && GetColumn(0).SpawnTimeInSeconds < spawnTimeInSeconds)
            {
                RemoveOldestColumn();
            }
        }

        private void RemoveOldestColumn()
        {
            _oldestColumnIndex = (_oldestColumnIndex + 1) % _capacity;
            _columnsCount--;
        }
    }
}
