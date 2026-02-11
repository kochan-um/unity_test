using UnityEngine;

namespace Networking
{
    /// <summary>
    /// プレイヤーのスポーン位置を示すマーカーコンポーネント。
    /// シーンに配置して使用する。
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [Tooltip("このスポーンポイントの表示用の名前")]
        [SerializeField] private string _label = "";

        public string Label => _label;
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }
    }
}
