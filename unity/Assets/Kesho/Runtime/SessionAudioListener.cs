using UnityEngine;

namespace Kesho.Runtime
{
    /// <summary>One listener exists before scene audio starts and survives camera/scene replacement.</summary>
    [DefaultExecutionOrder(100)]
    public sealed class SessionAudioListener : MonoBehaviour
    {
        static SessionAudioListener instance;
        Camera view;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Ensure()
        {
            if(instance) return;
            instance=FindFirstObjectByType<SessionAudioListener>();
            if(!instance) new GameObject("Kesho persistent audio listener").AddComponent<SessionAudioListener>();
        }
        void Awake()
        {
            if(instance && instance!=this) { Destroy(gameObject); return; }
            instance=this; DontDestroyOnLoad(gameObject);
            if(!GetComponent<AudioListener>()) gameObject.AddComponent<AudioListener>();
        }
        void LateUpdate()
        {
            if(!view) view=Camera.main;
            if(view) transform.SetPositionAndRotation(view.transform.position,view.transform.rotation);
        }
        void OnDestroy() { if(instance==this) instance=null; }
    }
}
