using UnityEngine;

namespace Core
{
    public class PlayerController : MonoBehaviour
    {
        private const int FALL_COUNT_UNIT = 120;// 1ます落下するカウント数
        private const int FALL_COUNT_SPD = 10; // 落下速度
        private const int FALL_COUNT_FAST_SPD = 20; // 高速落下時の速度
        private const int GROUND_FRAMES = 50;

        private int fallCount = 0;
        private int groundFrame = GROUND_FRAMES;
        
        private LogicalInput logicalInput = new ();

        enum RotState
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3,
            
            Invalid = -1,
        }
        [SerializeField] private PuyoController[] puyoControllers = new PuyoController[2] { default!, default! };
        [SerializeField] private BoardController boardController = default!;

        private Vector2Int position; // 軸ぷよの位置
        private RotState rotate = RotState.Up;

        private static readonly Vector2Int[] RotateTbl = {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };

        private AnimationController animationController = new ();
        private Vector2Int lastPosition;
        private RotState lastRotate = RotState.Up;
        private const int TRANS_TIME = 3;
        private const int ROT_TIME = 3;

        private void SetTransition(Vector2Int pos, RotState rot, int time)
        {
            lastPosition = position;

            lastRotate = rotate;

            position = pos;
            rotate = rot;
            
            animationController.Set(time);
        }

        public void SetLogicalInput(LogicalInput reference)
        {
            logicalInput = reference;
        }
        
        void Start()
        {
            puyoControllers[0].SetPuyoType(PuyoType.Green);
            puyoControllers[1].SetPuyoType(PuyoType.Red);

            position = new Vector2Int(2, 12);
        
            puyoControllers[0].SetPos(new Vector3(position.x, position.y, 0.0f));
            Vector2Int posChild = CalcChildPuyoPos(position, rotate);
            puyoControllers[1].SetPos(new Vector3(posChild.x, posChild.y, 0.0f));
        }

        public bool Spawn(PuyoType axis, PuyoType child)
        {
            Vector2Int pos = new(2, 12);
            RotState rot = RotState.Up;
            if (!CanMove(pos, rot)) return false;

            position = lastPosition = pos;
            rotate = lastRotate = rotate;
            animationController.Set(1);
            fallCount = 0;
            groundFrame = GROUND_FRAMES;
            
            puyoControllers[0].SetPuyoType(axis);
            puyoControllers[1].SetPuyoType(child);
            
            puyoControllers[0].SetPos(new Vector3(position.x, position.y, 0.0f));
            Vector2Int posChild = CalcChildPuyoPos(position, rotate);
            puyoControllers[1].SetPos(new Vector3(posChild.x, posChild.y, 0.0f));
            
            gameObject.SetActive(true);

            return true;
        }

        

        private bool Fall(bool isFast)
        {
            fallCount -= isFast ? FALL_COUNT_FAST_SPD : FALL_COUNT_SPD;

            while (fallCount < 0)
            {
                if (!CanMove(position + Vector2Int.down, rotate))
                {
                    fallCount = 0;
                    if (0 < --groundFrame) return true;
                    
                    Settle();
                    return false;
                }

                position += Vector2Int.down;
                lastPosition += Vector2Int.down;
                fallCount += FALL_COUNT_UNIT;
            }

            return true;
        }

        private void Settle()
        {
            bool isSet0 = boardController.Settle(position, (int)puyoControllers[0].GetPuyoType());
            Debug.Assert(isSet0);
            
            bool isSet1 = boardController.Settle(CalcChildPuyoPos(position, rotate), (int)puyoControllers[1].GetPuyoType());
            Debug.Assert(isSet1);
            
            gameObject.SetActive(false);
        }

        private void Control()
        {
            if (!Fall(logicalInput.IsRaw(LogicalInput.Key.Down))) return;

            if (animationController.Update()) return;
            if (logicalInput.IsRelease(LogicalInput.Key.Right))
            {
                Translate(true);
            }
            if (logicalInput.IsRelease(LogicalInput.Key.Left))
            {
                Translate(false);
            }

            if (logicalInput.isTrigger(LogicalInput.Key.RotR))
            {
                Rotate(true);
            }

            if (logicalInput.isTrigger(LogicalInput.Key.RotL))
            {
                Rotate(false);
            }

            if (logicalInput.IsRelease(LogicalInput.Key.QuickDrop))
            {
                QuickDrop();
            }
        }
        
        private void FixedUpdate()
        {

            Control();

            Vector3 dy = Vector3.up * fallCount / FALL_COUNT_UNIT;
            float animRate = animationController.GetNormalized();
            puyoControllers[0].SetPos(dy + Interpolate(position, RotState.Invalid, lastPosition, RotState.Invalid, animRate));
            puyoControllers[1].SetPos(dy + Interpolate(position, rotate, lastPosition, lastRotate, animRate));

        }

        private Vector3 Interpolate(Vector2Int pos, RotState rot, Vector2Int posLast, RotState rotLast, float rate)
        {
            Vector3 p = Vector3.Lerp(
                new Vector3(pos.x, pos.y, 0.0f),
                new Vector3(posLast.x, posLast.y, 0.0f), rate);

            if (rot == RotState.Invalid) return p;

            float theta0 = 0.5f * Mathf.PI * (int)rot;
            float theta1= 0.5f * Mathf.PI * (int)rotLast;
            float theta = theta1 - theta0;

            if (+Mathf.PI < theta) theta -= 2.0f * Mathf.PI;
            if (theta < -Mathf.PI) theta += 2.0f * Mathf.PI;

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
            
            Settle();
        }
    }

}
