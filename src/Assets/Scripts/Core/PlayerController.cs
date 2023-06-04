using Core;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Core
{
    public class PlayerController : MonoBehaviour
    {
        enum RotState
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3,
            
            Invalid = -1,
        }
        [FormerlySerializedAs("_puyoControllers")] [SerializeField] private PuyoController[] puyoControllers = new PuyoController[2] { default!, default! };
        [SerializeField] private BoardController boardController = default!;

        private Vector2Int position; // 軸ぷよの位置
        private RotState rotate = RotState.Up;

        private static readonly Vector2Int[] RotateTbl = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };

        private AnimationController animationController = new AnimationController();
        private Vector2Int _lastPosition;
        private RotState _lastRotate = RotState.Up;
        private const float TRANS_TIME = 0.05f;
        private const float ROT_TIME = 0.05f;

        private void SetTransition(Vector2Int pos, RotState rot, float time)
        {
            _lastPosition = position;

            _lastRotate = rotate;

            position = pos;
            rotate = rot;
            
            animationController.Set(time);
        }
        
        void Start()
        {
            puyoControllers[0].SetPuyoType(PuyoType.Green);
            puyoControllers[1].SetPuyoType(PuyoType.Red);

            position = new Vector2Int(2, 12);
        
            puyoControllers[0].SetPos(new Vector3((float)position.x, (float)position.y, 0.0f));
            Vector2Int posChild = CalcChildPuyoPos(position, rotate);
            puyoControllers[1].SetPos(new Vector3((float)posChild.x, (float)posChild.y, 0.0f));
        }

        private void Control()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Translate(true);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Translate(false);
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                Rotate(true);
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                Rotate(false);
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                QuickDrop();
            }
        }
        
        void Update()
        {
            if (!animationController.Update(Time.deltaTime))
            {
                Control();
            }

            float anim_rate = animationController.GetNormalized();
            puyoControllers[0].SetPos(Interpolate(position, RotState.Invalid, _lastPosition, RotState.Invalid, anim_rate));
            puyoControllers[1].SetPos(Interpolate(position, rotate, _lastPosition, _lastRotate, anim_rate));

        }

        private Vector3 Interpolate(Vector2Int pos, RotState rot, Vector2Int pos_last, RotState rot_last, float rate)
        {
            Vector3 p = Vector3.Lerp(
                new Vector3(pos.x, pos.y, 0.0f),
                new Vector3(pos_last.x, pos_last.y, 0.0f), rate);

            if (rot == RotState.Invalid) return p;

            float theta0 = 0.5f * Mathf.PI * (float)(int)rot;
            float theta1= 0.5f * Mathf.PI * (float)(int)rot_last;
            float theta = theta1 - theta0;

            if (+Mathf.PI < theta) theta = theta - 2.0f * Mathf.PI;
            if (theta < -Mathf.PI) theta = theta + 2.0f * Mathf.PI;

            theta = theta0 + rate * theta;

            return p + new Vector3(Mathf.Sin(theta), Mathf.Cos(theta), 0.0f);
        }

        private static Vector2Int CalcChildPuyoPos(Vector2Int pos, RotState rot)
        {
            return pos + RotateTbl[(int)rot];
        }

        private bool CanMove(Vector2Int pos, RotState rot)
        {
            if (!boardController.CanSettle(pos)) return false;
            if (!boardController.CanSettle(CalcChildPuyoPos(pos, rot))) return false;

            return true;
        }

        private bool Translate(bool isRight)
        {
            Vector2Int pos = position + (isRight ? Vector2Int.right : Vector2Int.left);
            if (!CanMove(pos, rotate)) return false;

            SetTransition(pos, rotate, TRANS_TIME);
        
            return true;
        }

        private bool Rotate(bool isRight)
        {
            RotState rot = (RotState)(((int)rotate + (isRight ? +1 : +3)) & 3);

            Vector2Int pos = position;
            // 壁で回転したらずらす
            switch (rot)
            {
                case RotState.Down:
                    if (!boardController.CanSettle(pos + Vector2Int.down) ||
                        !boardController.CanSettle(pos + new Vector2Int(isRight ? 1 : -1, -1)))
                    {
                        pos += Vector2Int.up;
                    }

                    break;
                case RotState.Right:
                    if (!boardController.CanSettle(pos + Vector2Int.right))
                    {
                        pos += Vector2Int.left;
                    }

                    break;
                case RotState.Left:
                    if (!boardController.CanSettle(pos + Vector2Int.left))
                    {
                        pos += Vector2Int.right;
                    }

                    break;
                case RotState.Up:
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            if (!CanMove(pos, rot)) return false;

            SetTransition(pos, rot, ROT_TIME);
            
            return true;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void QuickDrop()
        {
            Vector2Int pos = position;
            do
            {
                pos += Vector2Int.down;
            }
            while (CanMove(pos, rotate));

            pos -= Vector2Int.down;

            position = pos;

            bool isSet0 = boardController.Settle(position, (int)puyoControllers[0].GetPuyoType());
            Debug.Assert(isSet0);
            
            bool isSet1 = boardController.Settle(CalcChildPuyoPos(position, rotate), (int)puyoControllers[1].GetPuyoType());
            Debug.Assert(isSet1);
            
            gameObject.SetActive(false);
        }
    }

}
