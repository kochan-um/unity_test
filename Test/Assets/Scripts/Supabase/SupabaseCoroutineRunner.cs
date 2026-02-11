using UnityEngine;

namespace Supabase
{
    /// <summary>
    /// Coroutineを実行するためのヘルパークラス
    /// サービスクラスがMonoBehaviourを継承していないため、別途提供
    /// </summary>
    public class SupabaseCoroutineRunner : MonoBehaviour
    {
        private static SupabaseCoroutineRunner _instance;

        public static SupabaseCoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SupabaseCoroutineRunner");
                    _instance = go.AddComponent<SupabaseCoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
