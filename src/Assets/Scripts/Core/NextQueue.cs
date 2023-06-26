using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class NextQueue
    {
        private enum Constants
        {
            PUYO_TYPE_MAX = 4,
            PUYO__NEXT_HISTORIES = 2
        }

        private Queue<Vector2Int> nexts = new();

        Vector2Int CreateNext()
        {
            return new Vector2Int(
                Random.Range(0, (int)Constants.PUYO_TYPE_MAX) + 1,
                Random.Range(0, (int)Constants.PUYO_TYPE_MAX) + 1
            );
        }

        public void Initialize()
        {
            for (int t = 0; t < (int)Constants.PUYO__NEXT_HISTORIES; t++)
            {
                nexts.Enqueue(CreateNext());
            }
        }

        public Vector2Int Update()
        {
            Vector2Int next = nexts.Dequeue();
            nexts.Enqueue(CreateNext());

            return next;
        }

        public void Each(System.Action<int, Vector2Int> cb)
        {
            int idx = 0;
            foreach (Vector2Int n in nexts)
            {
                cb(idx++, n);
            }
        }
    }
}