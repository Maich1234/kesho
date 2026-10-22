using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Kesho.Core;

namespace Kesho.Runtime
{
    public sealed class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }
        public StoryEngine Engine { get; private set; }
        public Soundscape Sound { get; private set; }
        public bool InConversation { get; private set; }
        public bool CanExplore { get { return playing && !loading && !background && !hud.PanelOpen; } }
        Chapter chapter;
        OfflineSave saves;
        StoryHud hud;
        Stage stage;
        bool playing, loading, background;
        float saveClock, idle;
        int lineIndex;
        string spokenNode;
        Line[] lines;
        LifeState draft;
        GameObject marker;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { if(Instance==null) new GameObject("Kesho session").AddComponent<GameSession>(); }
        void Awake()
        {
            if(Instance!=null) { Destroy(gameObject); return; } Instance=this; DontDestroyOnLoad(gameObject);
            Application.targetFrameRate=30; Screen.orientation=ScreenOrientation.LandscapeLeft;
            chapter=JsonUtility.FromJson<Chapter>(Resources.Load<TextAsset>("Story/opening").text); chapter.Validate();
            saves=new OfflineSave(Path.Combine(Application.persistentDataPath,"kesho-unity-v1"),chapter);
            Sound=gameObject.AddComponent<Soundscape>();
            var ui=new GameObject("Story interface"); ui.transform.SetParent(transform); hud=ui.AddComponent<StoryHud>();
            SceneManager.sceneLoaded+=SceneReady;
        }
        void Start() { stage=FindFirstObjectByType<Stage>(); Menu(); }
        void SceneReady(Scene scene,LoadSceneMode mode)
        {
            stage=FindFirstObjectByType<Stage>(); if(stage==null) return;
            loading=false;
            if(playing) ConfigureStage();
        }
        void ConfigureStage()
        {
            stage.ApplyAppearance(Engine.State);
            if(Engine.State.poseScene==stage.sceneId && stage.Walkable(new Vector2Int(Mathf.RoundToInt(Engine.State.posX),Mathf.RoundToInt(Engine.State.posZ)))) stage.Player.position=new Vector3(Engine.State.posX,0,Engine.State.posZ);
            if(marker) Destroy(marker);
            marker=GameObject.CreatePrimitive(PrimitiveType.Cylinder); marker.name="Current story destination"; Destroy(marker.GetComponent<Collider>());
            marker.transform.localScale=new Vector3(.8f,.025f,.8f);
            var shader=Shader.Find("Universal Render Pipeline/Lit"); markerMaterial=new Material(shader?shader:Shader.Find("Standard")); markerMaterial.color=new Color(.9f,.67f,.27f); marker.GetComponent<Renderer>().sharedMaterial=markerMaterial;
            RefreshObjective();
        }
        Material markerMaterial;
        void RefreshObjective()
        {
            if(!playing||stage==null) return;
            var node=Engine.Current; hud.Objective(node.objective,"Day "+Engine.State.day+"  ·  C"+Engine.State.cash);
            hud.ActionButton("Approach / interact",Interact); hud.Hint("Tap the ground to walk. Follow the gold marker.");
            if(stage.Targets.ContainsKey(node.target)) { marker.transform.position=stage.Targets[node.target].position+Vector3.up*.04f; marker.SetActive(true); }
            else { marker.SetActive(false); Debug.LogError("Missing target "+node.target+" in "+stage.sceneId); }
            Sound.SetEnvironment(node.sound);
        }
        void Menu()
        {
            playing=false; InConversation=false; hud.Close(); hud.Objective("KESHO","A Place Before Dark · Unity development chapter");
            hud.Open("A city can change your life.","Who gets to decide how?");
            hud.Button("Begin a life",CreateCharacter,true);
            if(saves.Read()!=null) hud.Button("Continue",()=>Begin(saves.Read()));
            hud.Button("Sound & accessibility",Settings); hud.ShowAction(false);
        }
        void CreateCharacter()
        {
            draft=new LifeState { node=chapter.start, chapter=chapter.id, rng=(long)((uint)DateTime.UtcNow.Ticks | 1u) };
            CharacterForm();
        }
        void CharacterForm()
        {
            hud.Open("Your first morning","19 years old. C1,200. An interview, not a guarantee.");
            hud.Field(draft.name,value=>draft.name=value);
            hud.Button("Background: "+draft.background,()=> { draft.background=Cycle(WorldCatalog.Backgrounds,draft.background); CharacterForm(); });
            hud.Button("Gender: "+draft.gender,()=> { draft.gender=Cycle(WorldCatalog.Genders,draft.gender); CharacterForm(); });
            hud.Button("Beliefs: "+draft.belief,()=> { draft.belief=Cycle(WorldCatalog.Beliefs,draft.belief); CharacterForm(); });
            hud.Button("Skin tone: "+(draft.skin+1)+" / 4",()=> { draft.skin=(draft.skin+1)%4; if(stage) stage.ApplyAppearance(draft); CharacterForm(); });
            hud.Button("Outfit: "+(draft.outfit+1)+" / 4",()=> { draft.outfit=(draft.outfit+1)%4; if(stage) stage.ApplyAppearance(draft); CharacterForm(); });
            hud.Button(draft.permanent?"Permanent consequences":"Checkpoints",()=> { draft.permanent=!draft.permanent; CharacterForm(); });
            hud.Button("Arrive in Maweni",()=> {
                if(string.IsNullOrWhiteSpace(draft.name)) draft.name="Amani";
                if(saves.Read()!=null) { hud.Open("Start this new life?","Your current Unity save will be replaced."); hud.Button("Start new life",()=>Begin(draft),true); hud.Button("Back",CharacterForm); }
                else Begin(draft);
            },true);
            hud.Button("Back",Menu);
        }
        static string Cycle(string[] values,string value) { return values[(Array.IndexOf(values,value)+1)%values.Length]; }
        void Begin(LifeState state)
        {
            if(state==null) { hud.Hint("No valid save was found."); return; }
            Engine=new StoryEngine(chapter,state); playing=true; hud.Close(); Save(false); LoadCurrent();
        }
        void LoadCurrent()
        {
            InConversation=false; Sound.StopVoice(); hud.Close();
            if(stage!=null && stage.sceneId==Engine.Current.scene) { if(markerMaterial) Destroy(markerMaterial); ConfigureStage(); return; }
            loading=true; if(markerMaterial) Destroy(markerMaterial); if(marker) Destroy(marker);
            SceneManager.LoadSceneAsync(Engine.Current.scene,LoadSceneMode.Single);
        }
        public void Interact()
        {
            if(!CanExplore || stage==null || !stage.Targets.ContainsKey(Engine.Current.target)) return;
            var target=stage.Targets[Engine.Current.target]; var walker=stage.Player.GetComponent<StreetWalker>();
            if(Vector3.Distance(stage.Player.position,target.position)>1.65f) { if(!walker.WalkTo(target.position)) hud.Hint("Find a clear route around the furniture."); return; }
            walker.Stop(); InConversation=true; spokenNode=Engine.State.node; lines=Engine.VisibleLines(); lineIndex=0; ShowLine();
        }
        void ShowLine()
        {
            if(Engine.State.node!=spokenNode) return;
            Sound.StopVoice();
            if(lineIndex<lines.Length)
            {
                var line=lines[lineIndex]; hud.Open(Engine.Format(line.speaker)); hud.AddText(Engine.Format(line.text),25);
                Sound.Speak(line.voice,stage.Targets[Engine.Current.target].position);
                hud.Button(lineIndex==lines.Length-1?"Respond":"Continue",()=> { lineIndex++; ShowLine(); },true);
                hud.Button("Step away",EndConversation);
            }
            else
            {
                hud.Open(Engine.Current.terminal?"The first night":"Your response");
                if(Engine.Current.terminal)
                {
                    hud.AddText("End of this development chapter. Your choices are saved.",22);
                    hud.Button("View your decisions",Journal,true); hud.Button("Save & title",()=> { Save(); Menu(); });
                }
                foreach(var choice in Engine.Current.choices)
                {
                    var selected=choice;
                    var button=hud.Button(choice.label+(Engine.Available(choice)?"":" · "+Engine.UnavailableReason(choice)),()=>Commit(selected),true);
                    button.interactable=Engine.Available(choice);
                }
                if(!Engine.Current.terminal) hud.Button("Think about it",EndConversation);
            }
        }
        void EndConversation() { InConversation=false; Sound.StopVoice(); hud.Close(); }
        void Commit(Choice choice)
        {
            if(loading||spokenNode!=Engine.State.node) return;
            if(!Engine.State.permanent)
            {
                try { saves.Write(Engine.State,"checkpoint"); }
                catch(Exception e) when(e is IOException||e is UnauthorizedAccessException) { hud.Hint("Checkpoint could not be saved. Please free some storage."); return; }
            }
            if(!Engine.Apply(spokenNode,choice.id)) return;
            if(!Save()) { hud.Hint("Progress is in memory. Save failed; keep the app open."); }
            if(choice.effect=="sleep" || choice.effect=="night_resolution") StartCoroutine(SleepTransition()); else LoadCurrent();
        }
        IEnumerator SleepTransition()
        {
            loading=true; InConversation=false; Sound.StopVoice(); Sound.Cue("bedding",stage.Player.position);
            hud.Open("Night","The room settles around you."); hud.AddText("[Bedding rustles. The courtyard quietens.]",20);
            bool skip=false; hud.Button("Skip transition",()=>skip=true);
            float time=0; while(time<2.5f&&!skip) { time+=Time.unscaledDeltaTime; yield return null; }
            loading=false; LoadCurrent();
        }
        void Journal()
        {
            hud.Open("What you knew then");
            int start=Math.Max(0,Engine.State.journal.Count-8); for(int i=start;i<Engine.State.journal.Count;i++) hud.AddText(Engine.State.journal[i],18);
            hud.Button("Close",EndConversation,true);
        }
        public void Settings()
        {
            if(loading) return;
            Sound.StopVoice(); hud.Open("Sound & comfort");
            hud.Slider("Volume",Sound.Volume,value=>Sound.SetVolume(value));
            bool reduced=PlayerPrefs.GetInt("kesho.reduce_motion",0)==1;
            hud.Button(reduced?"Reduced motion: on":"Reduced motion: off",()=> { PlayerPrefs.SetInt("kesho.reduce_motion",reduced?0:1); PlayerPrefs.Save(); Settings(); });
            hud.AddText("Subtitles remain visible. Story choices never have a countdown.",18);
            if(playing)
            {
                hud.Button("Your decisions",Journal);
                var checkpoint=saves.Read("checkpoint");
                if(!Engine.State.permanent && checkpoint!=null && checkpoint.lifeId==Engine.State.lifeId)
                    hud.Button("Branch from last checkpoint",()=> { Engine.RestoreCheckpoint(checkpoint); Save(); LoadCurrent(); });
                hud.Button("Save & title",()=> { Save(); Menu(); });
            }
            hud.Button("Return",()=> { if(playing) EndConversation(); else Menu(); },true);
        }
        bool Save(bool capturePosition=true)
        {
            if(Engine==null) return true;
            try
            {
                if(capturePosition && stage && stage.sceneId==Engine.Current.scene) { Engine.State.poseScene=stage.sceneId; Engine.State.posX=stage.Player.position.x; Engine.State.posZ=stage.Player.position.z; }
                saves.Write(Engine.State); return true;
            }
            catch(Exception e) when(e is IOException || e is UnauthorizedAccessException) { Debug.LogWarning("Save failed: "+e.GetType().Name); hud.Hint("Could not save. Free storage before closing."); return false; }
        }
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape) && !loading) Settings();
            hud.ShowAction(CanExplore);
            if(!playing||background||loading) return;
            if(Input.anyKeyDown || Input.touchCount>0) idle=0; else idle+=Time.unscaledDeltaTime;
            if(idle<60&&(CanExplore||InConversation)) Engine.State.activeSeconds+=Time.unscaledDeltaTime;
            saveClock+=Time.unscaledDeltaTime; if(saveClock>20) { saveClock=0; Save(); }
            if(marker && PlayerPrefs.GetInt("kesho.reduce_motion",0)==0) marker.transform.Rotate(Vector3.up,Time.deltaTime*25);
        }
        void OnApplicationPause(bool pause) { background=pause; if(pause && playing) Save(); }
        void OnApplicationFocus(bool focus) { background=!focus; }
        void OnApplicationQuit() { if(playing) Save(); }
        void OnDestroy() { SceneManager.sceneLoaded-=SceneReady; if(Instance==this) Instance=null; if(markerMaterial) Destroy(markerMaterial); }
    }
}
