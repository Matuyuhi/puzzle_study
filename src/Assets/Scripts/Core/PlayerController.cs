using Core;
using UnityEngine;

namespace Core
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PuyoController[] _puyoControllers = new PuyoController[2] { default!, default! };
        [SerializeField] private BoardController boardController = default!;

        private Vector2Int _position; // 軸ぷよの位置
        void Start()
        {
            _puyoControllers[0].SetPuyoType(PuyoType.Green);
            _puyoControllers[1].SetPuyoType(PuyoType.Red);

            _position = new Vector2Int(2, 12);
        
            _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));
            _puyoControllers[1].SetPos(new Vector3((float)_position.x, (float)_position.y + 1.0f, 0.0f));
        }
        
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Translate(true);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Translate(false);
            }
        
        }

        private bool CanMove(Vector2Int pos)
        {
            if (!boardController.CanSettle(pos)) return false;
            if (!boardController.CanSettle(pos + Vector2Int.up)) return false;

            return true;
        }

        private bool Translate(bool is_right)
        {
            Vector2Int pos = _position + (is_right ? Vector2Int.right : Vector2Int.left);
            if (!CanMove(pos)) return false;

            _position = pos;
        
            _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));
            _puyoControllers[1].SetPos(new Vector3((float)_position.x, (float)_position.y + 1.0f, 0.0f));
        
            return true;
        }
    }

}
