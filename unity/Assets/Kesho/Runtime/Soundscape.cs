using UnityEngine;
using System.Collections.Generic;

namespace Kesho.Runtime
{
    /// <summary>Authored performances only. Missing recordings stay silent and remain on the production manifest.</summary>
    public sealed class Soundscape : MonoBehaviour
    {
        AudioSource music, ambience, voice;
        AudioSource[] effects;
        int next;
        readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
        bool preferencesDirty; float preferenceDeadline;
        public float Volume { get; private set; }
        void Awake()
        {
            Volume=Mathf.Clamp01(PlayerPrefs.GetFloat("kesho.volume",.65f));
            music=Source(false); ambience=Source(false); voice=Source(false);
            music.loop=true; ambience.loop=true;
            music.clip=Clip("Sound/music"); ambience.clip=Clip("Sound/ambience");
            music.volume=Volume*.17f; ambience.volume=Volume*.3f; voice.volume=Volume;
            if(music.clip) music.Play(); if(ambience.clip) ambience.Play();
            effects=new AudioSource[6]; for(int i=0;i<effects.Length;i++) effects[i]=Source(true);
        }
        AudioSource Source(bool spatial)
        {
            var source=new GameObject("Audio voice").AddComponent<AudioSource>(); source.transform.SetParent(transform);
            source.playOnAwake=false; source.volume=0; source.mute=Volume==0; source.spatialBlend=spatial?1:0; source.minDistance=2; source.maxDistance=60; source.rolloffMode=AudioRolloffMode.Linear; return source;
        }
        AudioClip Clip(string path)
        {
            if(!clips.TryGetValue(path,out var clip)) { clip=Resources.Load<AudioClip>(path); clips[path]=clip; } return clip;
        }
        public void SetVolume(float value)
        {
            Volume=Mathf.Clamp01(value); PlayerPrefs.SetFloat("kesho.volume",Volume); preferencesDirty=true; preferenceDeadline=Time.unscaledTime+.5f;
            music.mute=ambience.mute=voice.mute=Volume==0;
            music.volume=Volume*.17f; ambience.volume=Volume*.3f; voice.volume=Volume;
            foreach(var source in effects) { source.mute=Volume==0; source.volume=Volume*.65f; }
        }
        void FlushPreferences() { if(preferencesDirty) { PlayerPrefs.Save(); preferencesDirty=false; } }
        public void Speak(string id,Vector3 position)
        {
            voice.Stop(); voice.clip=Clip("Voice/"+id); voice.transform.position=position;
            if(voice.clip) voice.Play();
        }
        public void StopVoice() { voice.Stop(); }
        public void Cue(string id,Vector3 position)
        {
            var clip=Clip("Sound/"+id); if(!clip) return;
            var source=effects[next++%effects.Length]; source.spatialBlend=id=="click"?0:1; source.transform.position=position; source.clip=clip; source.volume=Volume*.65f; source.Play();
        }
        public void SetEnvironment(string id)
        {
            var clip=Clip("Sound/"+id); if(!clip) clip=Clip("Sound/ambience");
            if(ambience.clip==clip) return; ambience.clip=clip; if(clip) ambience.Play();
        }
        void Update()
        {
            if(preferencesDirty && Time.unscaledTime>=preferenceDeadline) FlushPreferences();
            bool speaking=voice.isPlaying || (GameSession.Instance && GameSession.Instance.InConversation);
            music.volume=Mathf.MoveTowards(music.volume,Volume*(speaking?.07f:.17f),Time.unscaledDeltaTime*.3f);
            ambience.volume=Volume*(speaking?.18f:.3f); voice.volume=Volume;
        }
        void OnDestroy() { FlushPreferences(); }
        void OnApplicationPause(bool paused) { AudioListener.pause=paused; if(paused) FlushPreferences(); }
    }
}
