using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Networking
{
    /// <summary>
    /// Resources に依存せず PhotonNetwork.Instantiate を使うためのプレハブプール。
    /// </summary>
    public sealed class PhotonPrefabPool : MonoBehaviour, IPunPrefabPool
    {
        [SerializeField] private GameObject defaultPlayerPrefab;
        [SerializeField] private List<GameObject> prefabs = new List<GameObject>();

        private readonly Dictionary<string, GameObject> _prefabMap = new Dictionary<string, GameObject>();

        private void Awake()
        {
            RebuildMap();
            PhotonNetwork.PrefabPool = this;
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(PhotonNetwork.PrefabPool, this))
            {
                PhotonNetwork.PrefabPool = new DefaultPool();
            }
        }

        public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
        {
            if (_prefabMap.TryGetValue(prefabId, out var prefab) && prefab != null)
            {
                return Instantiate(prefab, position, rotation);
            }

            Debug.LogError($"[PhotonPrefabPool] Prefab not registered: {prefabId}");
            return null;
        }

        public void Destroy(GameObject gameObject)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        private void RebuildMap()
        {
            _prefabMap.Clear();

            if (defaultPlayerPrefab != null)
            {
                _prefabMap[defaultPlayerPrefab.name] = defaultPlayerPrefab;
            }

            foreach (var prefab in prefabs)
            {
                if (prefab == null)
                {
                    continue;
                }

                _prefabMap[prefab.name] = prefab;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            RebuildMap();
        }
#endif
    }
}
