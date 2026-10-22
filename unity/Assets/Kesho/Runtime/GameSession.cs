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
        bool playing, loading, background, settingsOpen, directorConversation;
        Action onBack, currentView;
        float saveClock, idle;
        int lineIndex;
        string spokenNode, inspectedTarget, directorResult;
        Line[] lines;
        LifeState draft;
        GameObject marker;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { if(Instance==null) new GameObject("Kesho session").AddComponent<GameSession>(); }
        void Awake()
        {
            if(Instance!=null) { Destroy(gameObject); return; } Instance=this; DontDestroyOnLoad(gameObject); Comfort.Refresh();
            Application.targetFrameRate=30; Screen.orientation=ScreenOrientation.LandscapeLeft;
            chapter=JsonUtility.FromJson<Chapter>(Resources.Load<TextAsset>("Story/opening").text); chapter.Validate();
            saves=new OfflineSave(Path.Combine(Application.persistentDataPath,"kesho-unity-v2"),chapter);
            Sound=gameObject.AddComponent<Soundscape>();
            var ui=new GameObject("Story interface",typeof(RectTransform)); ui.transform.SetParent(transform); hud=ui.AddComponent<StoryHud>();
            SceneManager.sceneLoaded+=SceneReady;
        }
        void Start() { stage=FindFirstObjectByType<Stage>(); Menu(); }
        void SceneReady(Scene scene,LoadSceneMode mode)
        {
            stage=FindFirstObjectByType<Stage>(); if(stage==null) return;
            loading=false;
            if(playing) { hud.Close(); ConfigureStage(); }
        }
        void ConfigureStage()
        {
            stage.RestPose(false); stage.Conversation("",false); stage.Preview(false); stage.ApplyAppearance(Engine.State);
            stage.ApplyTimeOfDay(StoryPresentation.TimeFor(Engine.Current));
            if(StoryPresentation.SameLayout(stage.sceneId,Engine.State.poseScene) && stage.Walkable(new Vector2Int(Mathf.RoundToInt(Engine.State.posX),Mathf.RoundToInt(Engine.State.posZ)))) stage.Player.position=new Vector3(Engine.State.posX,0,Engine.State.posZ);
            if(marker && marker.transform.parent!=stage.transform) { Destroy(marker); marker=null; }
            if(!marker) marker=ComicMesh.Part(stage.transform,"Current story destination",Vector3.zero,Vector3.one,stage.Tint(new Color(1,.7f,.12f)),ComicMesh.Cylinder).gameObject;
            marker.transform.localScale=new Vector3(.8f,.025f,.8f);

            RefreshObjective();
        }
        void RefreshObjective()
        {
            if(!playing||stage==null) return;
            var node=Engine.Current; hud.Objective(node.objective,"Day "+Engine.State.day+"  ·  C"+Engine.State.cash);
            hud.ActionButton("Go to next story moment",Interact); hud.Hint("Tap to walk. Tap people or notices to investigate.");
            if(stage.Targets.ContainsKey(node.target)) { marker.transform.position=stage.Targets[node.target].position+Vector3.up*.04f; marker.SetActive(true); }
            else { marker.SetActive(false); Debug.LogError("Missing target "+node.target+" in "+stage.sceneId); }
            Sound.SetEnvironment(node.sound);
        }
        void Menu()
        {
            TrackView(Menu); playing=false; InConversation=false; onBack=ConfirmQuit; StopWalking(); Sound.StopVoice(); hud.Close(); hud.Objective("KESHO","College opening · 0.6.1"); if(stage) { stage.StopSpeaking(); stage.Preview(false); }
            hud.Title(); hud.AddText("Maweni College. Your first day. A stranger offers a shortcut.",22);
            hud.Button("Begin a life",CreateCharacter,true);
            if(saves.Read()!=null) hud.Button("Continue",()=>Begin(saves.Read(),true));
            hud.Button("Sound & accessibility",Settings); hud.ShowAction(false);
            if(Directory.Exists(Path.Combine(Application.persistentDataPath,"kesho-unity-v1"))) hud.AddText("This rewritten opening starts a new life. Your earlier development save is kept separately.",16);
        }
        void CreateCharacter()
        {
            draft=new LifeState { node=chapter.start, chapter=chapter.id, rng=(long)((uint)DateTime.UtcNow.Ticks | 1u) };
            SelectCharacter();
        }
        void SelectCharacter()
        {
            TrackView(SelectCharacter);
            onBack=Menu; hud.Open("Who arrives today?","An original cast. Your choices shape the person.");
            hud.CastChoices(ChooseCast);
            hud.Button("Create your own character",()=>ChooseCast("Custom"));
            hud.Button("Back",Menu);
        }
        void ChooseCast(string id)
        {
            draft.character=id; draft.name=id=="Custom"?"Amani":id;
            draft.gender=id=="Nia"?"Woman":"Man"; draft.hair=id=="Nia"?1:0;
            draft.outfit=id=="Nia"?1:0; draft.background=id=="Nia"?"Independent parent":"Family household";
            CharacterForm();
        }
        void CharacterForm()
        {
            TrackView(CharacterForm);
            onBack=SelectCharacter; hud.Open("Make this life yours","Age 19 · C1,200 · Registration day");
            hud.Panel.anchorMax=new Vector2(.52f,.81f);
            if(stage) { stage.ApplyAppearance(draft); stage.Preview(true); }
            hud.AddText(draft.character=="Nia"?"Nia notices the details. What she does with them is up to you.":draft.character=="Amani"?"Amani carries his family’s hopes. You decide how he carries the pressure.":"Choose your name, appearance and family connection.",20);
            hud.Field(draft.name,value=>draft.name=value);
            hud.CycleButton(()=>"Background: "+draft.background,()=>draft.background=Cycle(WorldCatalog.Backgrounds,draft.background));
            hud.CycleButton(()=>"Gender: "+draft.gender,()=>draft.gender=Cycle(WorldCatalog.Genders,draft.gender));
            hud.CycleButton(()=>"Beliefs: "+draft.belief,()=>draft.belief=Cycle(WorldCatalog.Beliefs,draft.belief));
            hud.CycleButton(()=>"Skin tone: "+(draft.skin+1)+" / 4",()=> { draft.skin=(draft.skin+1)%4; if(stage) stage.ApplyAppearance(draft); });
            hud.CycleButton(()=>"Hair: "+WorldCatalog.Hairstyles[draft.hair],()=> { draft.hair=(draft.hair+1)%WorldCatalog.Hairstyles.Length; if(stage) stage.ApplyAppearance(draft); });
            hud.CycleButton(()=>"Outfit: "+(draft.outfit+1)+" / 4",()=> { draft.outfit=(draft.outfit+1)%4; if(stage) stage.ApplyAppearance(draft); });
            hud.CycleButton(()=>draft.permanent?"Permanent consequences":"Checkpoints",()=>draft.permanent=!draft.permanent);
            hud.Button("Enter Maweni College",()=> {
                if(string.IsNullOrWhiteSpace(draft.name)) draft.name="Amani";
                if(saves.Read()!=null) ConfirmNewLife();
                else Begin(draft);
            },true);
            hud.Button("Change starting character",SelectCharacter);
        }
        void ConfirmNewLife()
        {
            TrackView(ConfirmNewLife); onBack=CharacterForm; hud.Open("Start this new life?","Your current Unity save will be replaced.");
            hud.Button("Start new life",()=>Begin(draft),true); hud.Button("Back",CharacterForm);
        }
        static string Cycle(string[] values,string value) { return values[(Array.IndexOf(values,value)+1)%values.Length]; }
        void Begin(LifeState state,bool resumed=false)
        {
            if(state==null) { hud.Hint("No valid save was found."); return; }
            bool recovered=resumed && (saves.LastReadSource==SaveSource.Backup || saves.LastReadSource==SaveSource.InterruptedWrite);
            Engine=new StoryEngine(chapter,state); playing=true; settingsOpen=false; hud.Close(); bool saved=Save(false); LoadCurrent();
            if(recovered && saved) hud.Notice("Recovered your last valid save. Please check your latest decision.");
        }
        void LoadCurrent()
        {
            TrackView(EndConversation);
            InConversation=false; directorConversation=false; settingsOpen=false; onBack=null; StopWalking(); Sound.StopVoice(); hud.Close();
            // Reserve a context-selected moment on entering the beat, so optional hotspots cannot bypass it.
            if(Engine.PrepareMoment()) Save(false);
            if(stage!=null && stage.sceneId==Engine.Current.scene) { ConfigureStage(); return; }
            loading=true; if(marker) Destroy(marker);
            if(!Application.CanStreamedLevelBeLoaded(Engine.Current.scene))
            {
                loading=false; MissingLocation(); return;
            }
            hud.Open("Travelling…","Preparing the next location.");
            StartCoroutine(LoadLocation(Engine.Current.scene));
        }
        IEnumerator LoadLocation(string scene)
        {
            yield return null;
            yield return SceneManager.LoadSceneAsync(scene,LoadSceneMode.Single);
        }
        void MissingLocation()
        {
            TrackView(MissingLocation); onBack=Menu; hud.Open("A location is missing from this build.","Your progress is kept. This APK needs to be rebuilt.");
            hud.Button("Return to title",Menu,true);
        }
        public void Interact()
        {
            if(!CanExplore || stage==null || !stage.Targets.ContainsKey(Engine.Current.target)) return;
            var target=stage.Targets[Engine.Current.target]; var walker=stage.Player.GetComponent<StreetWalker>();
            if(Vector3.Distance(stage.Player.position,target.position)>1.65f)
            {
                string expectedNode=Engine.State.node;
                if(!walker.WalkTo(target.position,()=> { if(CanExplore && Engine.State.node==expectedNode) Interact(); })) hud.Hint("Find a clear route around the furniture.");
                return;
            }
            walker.Stop(); stage.Conversation(Engine.Current.target,true); InConversation=true; spokenNode=Engine.State.node;
            directorConversation=Engine.PrepareMoment();
            // Persist the selected event and seed before showing any options.
            if(directorConversation && !Save()) { EndConversation(); return; }
            lines=Engine.VisibleLines(); lineIndex=0; ShowLine();
        }
        public bool TryInspectScreen(Vector2 screen)
        {
            if(!CanExplore || stage==null) return false;
            string best=null; float distance=48f*Screen.height/720f;
            foreach(var pair in stage.Targets)
            {
                var projected=stage.View.WorldToScreenPoint(stage.HotspotPosition(pair.Key));
                float d=Vector2.Distance(screen,new Vector2(projected.x,projected.y));
                if(projected.z>0 && d<distance) { distance=d; best=pair.Key; }
            }
            return best!=null && TryInspect(stage.Targets[best].position);
        }
        public bool TryInspect(Vector3 point)
        {
            if(!CanExplore || stage==null) return false;
            string closest=null; float best=1.35f;
            foreach(var pair in stage.Targets) { float d=Vector3.Distance(pair.Value.position,point); if(d<best) { best=d; closest=pair.Key; } }
            if(closest==null) return false;
            if(closest==Engine.Current.target) { Interact(); return true; }
            if(StoryPresentation.Describe(stage.sceneId,closest)==null) return false;
            string target=closest, expectedNode=Engine.State.node, expectedScene=stage.sceneId;
            if(!stage.Player.GetComponent<StreetWalker>().WalkTo(stage.Targets[target].position,()=>
            {
                if(!CanExplore || Engine.State.node!=expectedNode || stage.sceneId!=expectedScene) return;
                inspectedTarget=target; ShowInspection(); Sound.Cue("paper",stage.Targets[target].position);
            })) hud.Hint("That spot is blocked. Try the nearby path.");
            return true;
        }
        void ShowInspection()
        {
            var info=StoryPresentation.Describe(stage.sceneId,inspectedTarget);
            if(info==null) { EndConversation(); return; }
            TrackView(ShowInspection); InConversation=false; onBack=EndConversation; stage.StopSpeaking(); Sound.StopVoice(); hud.Open(info.title,info.text);
            foreach(var choice in Engine.Current.choices)
            {
                var selected=choice;
                var next=chapter.Find(choice.next);
                if(next.scene!=stage.sceneId || next.target!=inspectedTarget || !Engine.Available(choice)) continue;
                hud.Button(choice.label,()=> { spokenNode=Engine.State.node; Commit(selected); },true);
            }
            hud.Button("Back to exploring",EndConversation);
        }
        void ShowLine()
        {
            if(Engine.State.node!=spokenNode) return;
            if(directorConversation && Engine.PendingMoment!=null) { ShowDirectorMoment(); return; }
            TrackView(ShowLine);
            InConversation=true; onBack=EndConversation; Sound.StopVoice();
            if(lineIndex<lines.Length)
            {
                var line=lines[lineIndex]; hud.Dialogue(Engine.Format(line.speaker),Engine.Format(line.text),Engine.Current.comic);
                var position=stage.PresentSpeaker(StoryPresentation.Speaker(Engine.Current,line.speaker),Engine.Current.target);
                Sound.Speak(line.voice,position);
                hud.Button(lineIndex==lines.Length-1?"Respond":"Continue",()=> { lineIndex++; ShowLine(); },true);
                hud.Button("Step away",EndConversation);
            }
            else
            {
                stage.StopSpeaking();
                hud.Responses(Engine.Current.terminal?"To be continued":"What do you do?");
                if(Engine.Current.terminal)
                {
                    hud.AddText("College opening complete. Your decisions are saved. Orientation and the rest of your lifetime are still in development.",22);
                    hud.Button("View your decisions",Journal,true); hud.Button("Save & title",()=> { if(Save()) Menu(); });
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
        void ShowDirectorMoment()
        {
            var moment=Engine.PendingMoment;
            if(moment==null) { directorConversation=false; ShowLine(); return; }
            TrackView(ShowDirectorMoment);
            InConversation=true; onBack=EndConversation; Sound.StopVoice();
            // These moments occur in person with Rehema or as messages on the player's phone.
            stage.PresentSpeaker(StoryPresentation.Speaker(Engine.Current,moment.speaker),Engine.Current.target);
            hud.Dialogue(Engine.Format(moment.speaker),Engine.Format(LifeDirector.Text(moment,Engine.State)),false);
            string occurrence=Engine.State.director.pendingOccurrence;
            foreach(var option in moment.choices)
            {
                var selected=option;
                var button=hud.Button(option.label+(Engine.State.cash<option.cost?" · Need C"+option.cost:""),()=>ResolveDirectorMoment(occurrence,selected.id),true);
                button.interactable=Engine.State.cash>=option.cost;
            }
            hud.Button("Think about it",EndConversation);
        }
        void ResolveDirectorMoment(string occurrence,string choiceId)
        {
            if(loading || Engine.State.node!=spokenNode || Engine.PendingMoment==null) return;
            CapturePosition();
            if(!Engine.State.permanent)
            {
                try { saves.Write(Engine.State,"checkpoint"); }
                catch(Exception e) when(e is IOException||e is UnauthorizedAccessException) { hud.Notice("Checkpoint could not be saved. Free some storage before continuing."); return; }
            }
            string result;
            if(!Engine.ResolveMoment(occurrence,choiceId,out result)) return;
            directorConversation=false; directorResult=result; bool saved=Save(); ShowDirectorResult();
            if(!saved) hud.Notice("Progress is in memory. Save failed; keep the app open.");
        }
        void ShowDirectorResult()
        {
            TrackView(ShowDirectorResult); InConversation=true; onBack=EndConversation; stage.StopSpeaking(); Sound.StopVoice(); RefreshObjective();
            hud.Responses("What follows"); hud.AddText(Engine.Format(directorResult),22);
            hud.Button("Continue",()=> { stage.Conversation(Engine.Current.target,true); lineIndex=0; ShowLine(); },true);
            hud.Button("Back to exploring",EndConversation);
        }
        void TrackView(Action render) { currentView=render; settingsOpen=false; }
        void EndConversation() { TrackView(EndConversation); if(stage) stage.Conversation("",false); InConversation=false; directorConversation=false; onBack=null; StopWalking(); Sound.StopVoice(); hud.Close(); }
        public void MovementBlocked() { hud.Hint("That path is blocked. Tap a clear space nearby."); }
        void StopWalking() { if(stage && stage.Player) stage.Player.GetComponent<StreetWalker>().Stop(); }
        public void Back()
        {
            if(loading) return;
            if(onBack!=null) onBack(); else Settings();
        }
        void ConfirmQuit()
        {
            TrackView(ConfirmQuit);
            hud.Open("Leave Kesho?"); onBack=Menu;
            hud.Button("Keep playing",Menu,true); hud.Button("Exit",()=>Application.Quit());
        }
        void Commit(Choice choice)
        {
            if(loading||spokenNode!=Engine.State.node||!Engine.Available(choice)) return;
            CapturePosition();
            if(!Engine.State.permanent)
            {
                try { saves.Write(Engine.State,"checkpoint"); }
                catch(Exception e) when(e is IOException||e is UnauthorizedAccessException) { hud.Notice("Checkpoint could not be saved. Free some storage before continuing."); return; }
            }
            if(!Engine.Apply(spokenNode,choice.id)) return;
            if(!Save()) { hud.Notice("Progress is in memory. Save failed; keep the app open."); }
            if(choice.effect=="sleep" || choice.effect=="night_resolution") StartCoroutine(SleepTransition()); else LoadCurrent();
        }
        IEnumerator SleepTransition()
        {
            loading=true; InConversation=false; stage.StopSpeaking(); stage.RestPose(true); Sound.StopVoice(); Sound.Cue("bedding",stage.Player.position);
            hud.Responses("The city settles. Tomorrow waits.");
            bool skip=false; hud.Button("Skip transition",()=>skip=true);
            float time=0; while(time<2.5f&&!skip) { if(!background) time+=Time.unscaledDeltaTime; yield return null; }
            loading=false; LoadCurrent();
        }
        void Journal()
        {
            ShowJournal(currentView??(Action)EndConversation);
        }
        void ShowJournal(Action returnTo)
        {
            TrackView(()=>ShowJournal(returnTo));
            onBack=returnTo; InConversation=false; Sound.StopVoice(); hud.Open("What you knew then");
            if(stage) stage.StopSpeaking();
            int start=Math.Max(0,Engine.State.journal.Count-8); for(int i=start;i<Engine.State.journal.Count;i++) hud.AddText(Engine.State.journal[i],18);
            hud.Button("Close",()=>returnTo(),true);
        }
        public void Settings()
        {
            if(loading || settingsOpen) return;
            ShowSettings(currentView??(Action)Menu);
        }
        void ShowSettings(Action returnTo)
        {
            TrackView(()=>ShowSettings(returnTo)); settingsOpen=true; InConversation=false; onBack=returnTo; StopWalking(); if(stage) stage.StopSpeaking();
            Sound.StopVoice(); hud.Open("Sound & comfort");
            hud.Slider("Volume",Sound.Volume,value=>Sound.SetVolume(value));
            bool reduced=PlayerPrefs.GetInt("kesho.reduce_motion",0)==1;
            hud.Button(reduced?"Reduced motion: on":"Reduced motion: off",()=> { PlayerPrefs.SetInt("kesho.reduce_motion",reduced?0:1); PlayerPrefs.Save(); Comfort.Refresh(); ShowSettings(returnTo); });
            hud.AddText("Subtitles remain visible. Story choices never have a countdown.",18);
            if(playing)
            {
                hud.Button("Your decisions",Journal);
                var checkpoint=saves.Read("checkpoint");
                if(!Engine.State.permanent && checkpoint!=null && checkpoint.lifeId==Engine.State.lifeId)
                    hud.Button("Branch from last checkpoint",()=> { Engine.RestoreCheckpoint(checkpoint); Save(false); LoadCurrent(); });
                hud.Button("Save & title",()=> { if(Save()) Menu(); });
            }
            hud.Button("Return",()=>returnTo(),true);
        }
        bool Save(bool capturePosition=true)
        {
            if(Engine==null) return true;
            try
            {
                if(capturePosition) CapturePosition();
                saves.Write(Engine.State); hud.Notice(""); return true;
            }
            catch(Exception e) when(e is IOException || e is UnauthorizedAccessException) { Debug.LogWarning("Save failed: "+e.GetType().Name); hud.Notice("Could not save. Free storage before closing; keep the app open."); return false; }
        }
        void CapturePosition()
        {
            if(Engine!=null && stage && stage.sceneId==Engine.Current.scene)
            { Engine.State.poseScene=stage.sceneId; Engine.State.posX=stage.Player.position.x; Engine.State.posZ=stage.Player.position.z; }
        }
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape)) Back();
            hud.ShowAction(CanExplore);
            if(!playing||background||loading) return;
            if(Input.anyKeyDown || Input.touchCount>0) idle=0; else idle+=Time.unscaledDeltaTime;
            if(idle<60&&(CanExplore||InConversation)) Engine.State.activeSeconds+=Time.unscaledDeltaTime;
            if(saves.TryFailure(out var saveError)) { Debug.LogWarning("Autosave failed: "+saveError.GetType().Name); hud.Notice("Autosave failed. Keep the app open and free storage before saving again."); }
            saveClock+=Time.unscaledDeltaTime;
            if(saveClock>20)
            {
                saveClock=0; CapturePosition();
                try { saves.Autosave(Engine.State); }
                catch(Exception e) when(e is IOException || e is UnauthorizedAccessException) { Debug.LogWarning("Autosave failed: "+e.GetType().Name); hud.Notice("Autosave failed. Keep the app open and free storage before saving again."); }
            }
            if(marker && !Comfort.ReducedMotion) marker.transform.Rotate(Vector3.up,Time.deltaTime*25);
        }
        void OnApplicationPause(bool pause) { background=pause; if(pause) { StopWalking(); if(playing) Save(); } }
        void OnApplicationFocus(bool focus) { background=!focus; if(!focus) { StopWalking(); if(playing) Save(); } }
        void OnApplicationQuit() { if(playing) Save(); }
        void OnDestroy() { if(saves!=null) saves.Flush(); SceneManager.sceneLoaded-=SceneReady; if(Instance==this) Instance=null; }
    }
}
