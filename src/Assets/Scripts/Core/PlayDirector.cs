using UnityEngine;

namespace Core
{
    public class PlayDirector : MonoBehaviour
    {
        [SerializeField] private GameObject player = default!;
        private PlayerController playerController = null;
        private LogicalInput logicalInput = new();

        private NextQueue nextQueue = new();

        [SerializeField] private PuyoPair[] nextPuyoPairs = { default!, default! };
        
        private static readonly KeyCode[] KeyCodeTbl = {
            KeyCode.RightArrow,
            KeyCode.LeftArrow,
            KeyCode.X,
            KeyCode.Z,
            KeyCode.UpArrow,
            KeyCode.DownArrow
        };

        private void UpdateNextsView()
        {
            nextQueue.Each((int idx, Vector2Int n) =>
            {
                nextPuyoPairs[idx++].SetPuyoType((PuyoType)n.x, (PuyoType)n.y);
            });
        }

        private void Start()
        {
            playerController = player.GetComponent<PlayerController>();
            logicalInput.Clear();
            playerController.SetLogicalInput(logicalInput);
            
            nextQueue.Initialize();
            Spawn(nextQueue.Update());
        }
        
        private void UpdateInput()
        {
            LogicalInput.Key inputDev = 0;

            for (int i = 0; i < (int)LogicalInput.Key.MAX; i++)
            {
                if (Input.GetKey(KeyCodeTbl[i]))
                {
                    inputDev |= (LogicalInput.Key)(1 << i);
                }
            }
            logicalInput.Update(inputDev);
        }

        private void FixedUpdate()
        {
            UpdateInput();
            
            if (!player.activeSelf)
            {
                Spawn(nextQueue.Update());
                UpdateNextsView();
            }
        }

        private bool Spawn(Vector2Int next) => playerController.Spawn((PuyoType)next[0], (PuyoType)next[1]);
    }
}