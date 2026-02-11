using UnityEngine;

namespace WorldStreaming.Samples
{
    /// <summary>
    /// WorldStreamerシステムの使用例を示すサンプルスクリプト。
    /// プレイヤーキャラクターとして機能し、WorldStreamerが自動的にチャンクをロード/アンロードする。
    /// </summary>
    public class WorldStreamerSample : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private WorldStreamerSettings _streamerSettings;

        private CharacterController _characterController;
        private Vector3 _velocity;
        private float _gravity = -9.8f;

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
            if (_characterController == null)
            {
                _characterController = gameObject.AddComponent<CharacterController>();
            }

            // WorldStreamerの初期化
            var streamer = WorldStreamer.Instance;
            if (streamer != null)
            {
                streamer.GetSettings();
            }
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // 移動入力
            Vector3 moveDirection = (transform.forward * vertical + transform.right * horizontal).normalized;
            _velocity.x = moveDirection.x * _moveSpeed;
            _velocity.z = moveDirection.z * _moveSpeed;

            // 重力
            _velocity.y += _gravity * Time.deltaTime;

            // CharacterControllerで移動
            if (_characterController.enabled)
            {
                _characterController.Move(_velocity * Time.deltaTime);
            }
            else
            {
                transform.position += _velocity * Time.deltaTime;
            }

            // 接地判定
            if (_characterController.isGrounded)
            {
                _velocity.y = 0;
            }

            // カメラ回転（簡易版）
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            transform.Rotate(Vector3.up * mouseX * 2f);

            if (transform.Find("CameraHolder") != null)
            {
                var cameraHolder = transform.Find("CameraHolder");
                cameraHolder.Rotate(Vector3.left * mouseY * 2f);
            }
        }

        /// <summary>
        /// デバッグ用：プレイヤー位置をテレポートさせる
        /// </summary>
        public void TeleportTo(Vector3 position)
        {
            if (_characterController.enabled)
            {
                _characterController.enabled = false;
                transform.position = position;
                _characterController.enabled = true;
            }
            else
            {
                transform.position = position;
            }
        }
    }
}
