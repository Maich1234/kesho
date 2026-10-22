using UnityEngine;

namespace Kesho.Runtime
{
    /// <summary>Authored performances only. Missing recordings stay silent and remain on the production manifest.</summary>
    public sealed class Soundscape : MonoBehaviour
    {
        AudioSource music, ambience, voice;
        AudioSource[] effects;
        int next;
        public float Volume { get; private set; }
        void Awake()
        {
            Volume=Mathf.Clamp01(PlayerPrefs.GetFloat("kesho.volume",.65f));
            music=Source(false); ambience=Source(false); voice=Source(false);
            music.loop=true; ambience.loop=true;
            music.clip=Resources.Load<AudioClip>("Sound/music"); ambience.clip=Resources.Load<AudioClip>("Sound/ambience");
            if(music.clip) music.Play(); if(ambience.clip) ambience.Play();
            effects=new AudioSource[6]; for(int i=0;i<effects.Length;i++) effects[i]=Source(true);
        }
        AudioSource Source(bool spatial)
        {
            var source=new GameObject("Audio voice").AddComponent<AudioSource>(); source.transform.SetParent(transform);
            source.playOnAwake=false; source.spatialBlend=spatial?1:0; source.minDistance=2; source.maxDistance=60; source.rolloffMode=AudioRolloffMode.Linear; return source;
        }
        public void SetVolume(float value) { Volume=Mathf.Clamp01(value); PlayerPrefs.SetFloat("kesho.volume",Volume); PlayerPrefs.Save(); }
        public void Speak(string id,Vector3 position)
        {
            voice.Stop(); voice.clip=Resources.Load<AudioClip>("Voice/"+id); voice.transform.position=position;
            if(voice.clip) voice.Play();
        }
        public void StopVoice() { voice.Stop(); }
        public void Cue(string id,Vector3 position)
        {
            var clip=Resources.Load<AudioClip>("Sound/"+id); if(!clip) return;
            var source=effects[next++%effects.Length]; source.spatialBlend=id=="click"?0:1; source.transform.position=position; source.clip=clip; source.volume=Volume*.65f; source.Play();
        }
        public void SetEnvironment(string id)
        {
            var clip=Resources.Load<AudioClip>("Sound/"+id); if(!clip) clip=Resources.Load<AudioClip>("Sound/ambience");
            if(ambience.clip==clip) return; ambience.clip=clip; if(clip) ambience.Play();
        }
        void Update()
        {
            bool speaking=voice.isPlaying || (GameSession.Instance && GameSession.Instance.InConversation);
            music.volume=Mathf.MoveTowards(music.volume,Volume*(speaking?.07f:.17f),Time.unscaledDeltaTime*.3f);
            ambience.volume=Volume*(speaking?.18f:.3f); voice.volume=Volume;
        }
        void OnApplicationPause(bool paused) { AudioListener.pause=paused; }
    }
}
