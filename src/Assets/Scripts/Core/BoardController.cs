using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class BoardController : MonoBehaviour
    {
        public const int BOARD_WIDTH = 6;

        public const int BOARD_HEIGHT = 14;
        [SerializeField] private GameObject prefabPuyo = default!;

        private int[,] _board = new int[BOARD_HEIGHT, BOARD_WIDTH];

        private GameObject[,] _Puyos = new GameObject[BOARD_HEIGHT, BOARD_WIDTH];

        private void ClearAll()
        {
            for (int y = 0; y < BOARD_HEIGHT; y++)
            {
                for (int x = 0; x < BOARD_WIDTH; x++)
                {
                    _board[y, x] = 0;
                }
            }
        }
        void Start()
        {
            ClearAll();

            for (int y = 0; y < BOARD_HEIGHT; y++)
            {
                for (int x = 0; x < BOARD_WIDTH; x++)
                {
                    Settle(new Vector2Int(x, y), Random.Range(1, 7));
                }
            }
        }

        public static bool IsValidated(Vector2Int pos)
        {
            return 0 <= pos.x && pos.x < BOARD_WIDTH &&
                   0 <= pos.y && pos.y < BOARD_HEIGHT;
        }

        public bool CanSettle(Vector2Int pos)
        {
            if (!IsValidated(pos)) return false;

            return 0 == _board[pos.y, pos.x];
        }

        public bool Settle(Vector2Int pos, int val)
        {
            if (!IsValidated(pos)) return false;

            _board[pos.y, pos.x] = val;
            
            Debug.Assert(_Puyos[pos.y, pos.x] == null);
            Vector3 world_position = transform.position + new Vector3(pos.x, pos.y, 0.0f);
            _Puyos[pos.y, pos.x] = Instantiate(prefabPuyo, world_position, Quaternion.identity, transform);
            _Puyos[pos.y, pos.x].GetComponent<PuyoController>().SetPuyoType((PuyoType)val);

            return true;
        }
        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
