using UnityEngine;
using Photon.Pun;
using StarterAssets;
using Cinemachine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Networking
{
    /// <summary>
    /// ThirdPersonControllerのネットワーク対応ラッパー。
    /// photonView.IsMineに基づいてローカル/リモートの初期化を切り替える。
    /// ThirdPersonController自体は一切改変しない。
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    public class NetworkPlayerController : MonoBehaviourPun
    {
        [Header("Components")]
        [Tooltip("自動検出されない場合に手動で指定するThirdPersonController参照")]
        [SerializeField] private ThirdPersonController _thirdPersonController;

        [Tooltip("自動検出されない場合に手動で指定するStarterAssetsInputs参照")]
        [SerializeField] private StarterAssetsInputs _starterAssetsInputs;

        [Header("Remote Player")]
        [Tooltip("リモートプレイヤーの補間速度")]
        [SerializeField] private float _remoteInterpolationSpeed = 10f;

        private CharacterController _characterController;
        private Animator _animator;
        private AudioListener _audioListener;
#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif

        private bool _isInitialized;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized) return;

            // コンポーネント参照の取得
            if (_thirdPersonController == null)
                _thirdPersonController = GetComponent<ThirdPersonController>();
            if (_starterAssetsInputs == null)
                _starterAssetsInputs = GetComponent<StarterAssetsInputs>();

            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
            _audioListener = GetComponent<AudioListener>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#endif

            if (photonView.IsMine)
            {
                InitializeLocalPlayer();
            }
            else
            {
                InitializeRemotePlayer();
            }

            _isInitialized = true;
        }

        /// <summary>
        /// ローカルプレイヤーの初期化:
        /// すべてのコンポーネントを有効化し、Cinemachineカメラのターゲットを設定する
        /// </summary>
        private void InitializeLocalPlayer()
        {
            // コンポーネント有効化
            SetComponentEnabled(_thirdPersonController, true);
            SetComponentEnabled(_starterAssetsInputs, true);
            SetComponentEnabled(_characterController, true);
#if ENABLE_INPUT_SYSTEM
            SetComponentEnabled(_playerInput, true);
#endif

            if (_audioListener != null)
            {
                _audioListener.enabled = true;
            }

            // Cinemachineカメラのフォローターゲットを設定
            SetupCinemachineTarget();

            // タグ設定
            gameObject.tag = "Player";
        }

        /// <summary>
        /// リモートプレイヤーの初期化:
        /// 入力・物理系のコンポーネントを無効化し、ネットワーク同期で状態を反映する
        /// </summary>
        private void InitializeRemotePlayer()
        {
            // 入力系を無効化
            SetComponentEnabled(_thirdPersonController, false);
            SetComponentEnabled(_starterAssetsInputs, false);
#if ENABLE_INPUT_SYSTEM
            SetComponentEnabled(_playerInput, false);
#endif

            // リモートプレイヤーはCharacterControllerを無効化（PhotonTransformViewで直接Transform更新）
            if (_characterController != null)
            {
                _characterController.enabled = false;
            }

            // AudioListenerは複数あるとエラーになるため無効化
            if (_audioListener != null)
            {
                _audioListener.enabled = false;
            }
        }

        /// <summary>
        /// Cinemachineバーチャルカメラのフォローターゲットをこのプレイヤーに設定する
        /// </summary>
        private void SetupCinemachineTarget()
        {
            if (_thirdPersonController == null) return;

            // ThirdPersonControllerのCinemachineCameraTargetを使用
            var cameraTarget = _thirdPersonController.CinemachineCameraTarget;
            if (cameraTarget == null) return;

            // シーン内のCinemachineVirtualCameraを検索してフォローターゲットを設定
            var vcams = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);
            foreach (var vcam in vcams)
            {
                vcam.Follow = cameraTarget.transform;
                vcam.LookAt = cameraTarget.transform;
            }
        }

        private static void SetComponentEnabled(Behaviour component, bool enabled)
        {
            if (component != null)
            {
                component.enabled = enabled;
            }
        }

        private static void SetComponentEnabled(CharacterController component, bool enabled)
        {
            if (component != null)
            {
                component.enabled = enabled;
            }
        }
    }
}
