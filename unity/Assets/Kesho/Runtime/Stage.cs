using Unity.Profiling;
using System.Collections.Generic;
using UnityEngine;
using Kesho.Core;

namespace Kesho.Runtime
{
    public sealed class Stage : MonoBehaviour
    {
        static readonly ProfilerMarker profileCombineScenery=new ProfilerMarker("Kesho.Stage.CombineScenery");
        static readonly ProfilerMarker profileAwake=new ProfilerMarker("Kesho.Stage.Awake");
        public string sceneId="Campus";
        public readonly Dictionary<string,Transform> Targets=new Dictionary<string,Transform>();
        public readonly HashSet<Vector2Int> Blocked=new HashSet<Vector2Int>();
        public Transform Player { get; private set; }
        public Camera View { get; private set; }
        public string SpeakingActorId { get; private set; } = "";
        public DayPeriod CurrentPeriod { get; private set; }
        readonly Dictionary<Color,Material> palette=new Dictionary<Color,Material>();
        readonly Dictionary<string,ComicActor> actors=new Dictionary<string,ComicActor>();
        readonly List<Mesh> combinedMeshes=new List<Mesh>();
        readonly List<TextMesh> signs=new List<TextMesh>();
        Transform scenery;
        Light sun;
        Renderer windowGlass;
        Vector3 cameraPosition,cameraLook;
        float cameraSize=9.7f;
        readonly HashSet<Vector2Int> occupied=new HashSet<Vector2Int>();
        ComicActor playerActor;
        readonly Color cream=new Color(.95f,.87f,.69f), teal=new Color(.06f,.43f,.43f), ink=new Color(.1f,.16f,.23f), red=new Color(.87f,.28f,.2f), gold=new Color(.97f,.65f,.12f), green=new Color(.27f,.49f,.29f);
        void Awake()
        {
            using(profileAwake.Auto())
            {
                SessionAudioListener.Ensure();
                scenery=new GameObject("Authored environment").transform; scenery.SetParent(transform,false);
                sun=new GameObject("Daylight").AddComponent<Light>(); sun.transform.SetParent(transform); sun.type=LightType.Directional;
                Box("Ground",new Vector3(0,-.16f,0),new Vector3(36,.3f,32),new Color(.76f,.67f,.5f));
                if(sceneId=="Campus" || sceneId=="Arrival") Campus();
                else if(sceneId=="Room" || sceneId=="Night") Room();
                else if(sceneId=="Courtyard") Courtyard();
                else LegacyLocation();
                if(!Targets.ContainsKey("exit")) Target("exit",new Vector3(0,0,5));
                Player=Person("Player",new Vector3(0,0,4),1,0,0); Player.rotation=Quaternion.Euler(0,180,0);
                playerActor=Player.GetComponent<ComicActor>();
                foreach(var actor in GetComponentsInChildren<ComicActor>()) if(actor!=playerActor) occupied.Add(StreetWalker.Cell(actor.transform.position));
                var walker=Player.gameObject.AddComponent<StreetWalker>(); walker.Stage=this;
                View=new GameObject("Camera").AddComponent<Camera>(); View.transform.SetParent(transform); View.tag="MainCamera";
                View.orthographic=true; View.orthographicSize=9.7f; View.nearClipPlane=.1f; View.farClipPlane=100; View.backgroundColor=new Color(.69f,.83f,.87f); View.clearFlags=CameraClearFlags.SolidColor;
                ResetCamera(); View.transform.position=cameraPosition; View.transform.LookAt(cameraLook);
                ApplyTimeOfDay(sceneId=="Night"?DayPeriod.Night:DayPeriod.Afternoon);
                CombineScenery();
            }
        }
        void Campus()
        {
            Box("Paved courtyard",new Vector3(0,.015f,-1),new Vector3(21,.06f,17),cream);
            // Clear central circulation spine; furniture only along edges.
            for(int x=-10;x<=10;x+=2) Box("Paving joint",new Vector3(x,.052f,-1),new Vector3(.035f,.008f,17),new Color(.79f,.73f,.59f));
            for(int z=-8;z<=6;z+=2) Box("Paving joint",new Vector3(0,.054f,z),new Vector3(21,.008f,.035f),new Color(.79f,.73f,.59f));
            Building("Admissions hall",new Vector3(0,0,-11),new Vector3(20,6,4),cream,3);
            Box("Admissions teal fascia",new Vector3(0,4.3f,-8.88f),new Vector3(20,.8f,.15f),teal);
            Sign("MAWENI COLLEGE",new Vector3(0,5.2f,-8.7f),.11f,ink);
            Sign("ADMISSIONS  /  STUDENT SERVICES",new Vector3(0,4.35f,-8.65f),.052f,Color.white);
            for(int x=-8;x<=8;x+=4) Box("Colonnade",new Vector3(x,1.9f,-8.2f),new Vector3(.35f,3.8f,.35f),cream);
            Box("Canopy",new Vector3(0,3.8f,-7.9f),new Vector3(20,.22f,2),red);
            Building("Library wing",new Vector3(-14,0,-3),new Vector3(5,5,15),new Color(.78f,.49f,.33f),2);
            Building("Student wing",new Vector3(14,0,-5),new Vector3(5,4,11),cream,2);
            Sign("LIBRARY",new Vector3(-10.9f,3.3f,-4),.07f,ink);
            for(int i=0;i<7;i++) Building("City skyline",new Vector3(-20+i*6,0,-22),new Vector3(3.7f,5+(i*7%6),3),i%2==0?teal:new Color(.56f,.66f,.65f),2);
            Desk(new Vector3(1,0,-7),"Registration desk"); Sign("REGISTRATION",new Vector3(1,1.4f,-6.45f),.045f,ink);
            Box("Document tray",new Vector3(2,1.2f,-7),new Vector3(.6f,.12f,.5f),red);
            Box("Admission papers",new Vector3(1.9f,1.29f,-7),new Vector3(.4f,.025f,.4f),Color.white);
            for(int i=0;i<3;i++) { var t=Person("Queued student "+i,new Vector3(-1.6f-i*.65f,0,-5.4f+i*.6f),i%4,i%4,i); t.rotation=Quaternion.Euler(0,160,0); if(i==0) actors["queued_student"]=t.GetComponent<ComicActor>(); }
            actors["clerk"]=Person("Admissions clerk",new Vector3(1,0,-8),2,1,2).GetComponent<ComicActor>();
            Target("clerk",new Vector3(1,0,-5));
            Kiosk(new Vector3(8,0,2)); Target("parcel",new Vector3(5,0,2));
            actors["copy_seller"]=Person("Kiosk owner",new Vector3(8,0,3.4f),3,0,4).GetComponent<ComicActor>();
            Noticeboard(new Vector3(-8,0,-2)); Target("notice",new Vector3(-7,0,-1));
            Desk(new Vector3(6,0,-6.7f),"Student support"); Sign("HELP DESK",new Vector3(6,1.4f,-6.1f),.045f,ink); Target("ledger",new Vector3(5,0,-4));
            actors["support"]=Person("Student support officer",new Vector3(6,0,-7.7f),1,3,1).GetComponent<ComicActor>();
            var mentor=Person("Rehema",new Vector3(-4,0,1),0,2,2); mentor.rotation=Quaternion.Euler(0,25,0); actors["rehema"]=mentor.GetComponent<ComicActor>(); Target("rehema",new Vector3(-3,0,1));
            var fixer=Person("Kito",new Vector3(4,0,3),2,3,3); fixer.rotation=Quaternion.Euler(0,-25,0); actors["kito"]=fixer.GetComponent<ComicActor>(); Target("contact",new Vector3(3,0,3));
            Target("phone",new Vector3(0,0,3));
            for(int i=0;i<2;i++) { Tree(new Vector3(i==0?-9:9,0,-5)); Bench(new Vector3(i==0?-8:8,0,5)); }
            Box("Road",new Vector3(0,.005f,10),new Vector3(36,.03f,5),ink);
            for(int i=-3;i<=3;i++) Box("Crossing",new Vector3(i*.7f,.03f,9),new Vector3(.4f,.015f,3),cream);
            Bus(new Vector3(-7,0,10));
            for(int side=-1;side<=1;side+=2) { Box("Gate pillar",new Vector3(side*5,1.7f,7.2f),new Vector3(.7f,3.4f,.7f),red); Box("Gate wing",new Vector3(side*8,1,7.2f),new Vector3(5,2,.12f),teal); }
            Box("Campus arch",new Vector3(0,3.7f,7.2f),new Vector3(10.7f,.4f,.5f),teal);
            Sign("YOUR NEXT CHAPTER",new Vector3(0,3.75f,7.55f),.055f,cream);
            Target("exit",new Vector3(0,0,5));
        }
        void Courtyard()
        {
            Building("Rental rooms",new Vector3(0,0,-10),new Vector3(22,4,4),cream,1);
            Box("Courtyard floor",new Vector3(0,.015f,0),new Vector3(22,.04f,16),new Color(.74f,.57f,.41f));
            for(int i=-2;i<=2;i++)
            {
                Box("Teal door",new Vector3(i*4,1.2f,-7.9f),new Vector3(1.4f,2.4f,.1f),teal);
                Sign((i+3).ToString("00"),new Vector3(i*4,2.8f,-7.7f),.055f,ink);
                Box("Washing",new Vector3(i*2.1f,2.8f,-5.8f),new Vector3(1.1f,.85f,.04f),i%2==0?red:gold);
            }
            Box("Clothesline",new Vector3(0,3.2f,-5.8f),new Vector3(12,.025f,.025f),ink);
            Desk(new Vector3(-5,0,-3),"Supper table"); Round("Serving bowl",new Vector3(-5,1.25f,-3),new Vector3(.6f,.25f,.6f),cream);
            for(int i=0;i<3;i++) Round("Water container",new Vector3(8+i*.7f,.5f,-6),new Vector3(.6f,1,.6f),i%2==0?gold:teal);
            Tree(new Vector3(8,0,2)); Bench(new Vector3(-8,0,3));
            var sifa=Person("Sifa",new Vector3(-4,0,-4),1,2,4); actors["sifa"]=sifa.GetComponent<ComicActor>();
            Target("landlord",new Vector3(-2,0,-2)); Target("supper",new Vector3(-4,0,-1)); Target("contact",new Vector3(3,0,0)); Target("phone",new Vector3(0,0,2));
            Sign("KARIBU COURT",new Vector3(0,4.2f,-8),.09f,ink);
        }
        void Room()
        {
            Box("Floor",new Vector3(0,.02f,0),new Vector3(20,.05f,15),cream);
            Box("Back wall",new Vector3(0,2,-8),new Vector3(22,4,.2f),new Color(.66f,.81f,.77f));
            Box("Side wall",new Vector3(-11,2,0),new Vector3(.2f,4,16),teal);
            Box("Window frame",new Vector3(2,2.7f,-7.8f),new Vector3(3,2,.2f),ink);
            windowGlass=ComicMesh.Part(transform,"Window glass",new Vector3(2,2.7f,-7.65f),new Vector3(2.7f,1.7f,.08f),Tint(new Color(.6f,.83f,.92f)),ComicMesh.Cube).GetComponent<Renderer>();
            Box("Window mullion",new Vector3(2,2.7f,-7.5f),new Vector3(.1f,1.7f,.1f),cream);
            Box("Bed frame",new Vector3(-4,.35f,-2),new Vector3(2.4f,.7f,4),ink,true);
            Round("Mattress",new Vector3(-4,.8f,-2),new Vector3(2.45f,.45f,3.9f),cream);
            Box("Woven blanket",new Vector3(-4,1,-1.5f),new Vector3(2.35f,.13f,2.7f),teal);
            for(int i=0;i<4;i++) Box("Blanket stripe",new Vector3(-4,1.075f,-.5f-i*.55f),new Vector3(2.36f,.015f,.1f),gold);
            Round("Pillow",new Vector3(-4,1.07f,-3.25f),new Vector3(1.7f,.3f,.7f),Color.white);
            Desk(new Vector3(5,0,-5),"Study desk"); Box("Phone",new Vector3(4.5f,1.2f,-4.8f),new Vector3(.22f,.04f,.45f),ink);
            Box("Notebook",new Vector3(5.4f,1.2f,-5),new Vector3(.65f,.05f,.8f),red);
            Box("Rug",new Vector3(1,.06f,-1),new Vector3(4,.02f,5),red);
            Box("Suitcase",new Vector3(-7,.5f,-6),new Vector3(1.3f,1,.6f),gold,true);
            Sign("ONE DAY AT A TIME",new Vector3(-5,3,-7.6f),.07f,ink);
            Target("bed",new Vector3(-1,0,0)); Target("phone",new Vector3(4,0,-3)); Target("door",new Vector3(0,0,4));
        }
        void LegacyLocation()
        {
            Building("Office",new Vector3(0,0,-10),new Vector3(22,4,4),cream,1);
            Desk(new Vector3(0,0,-4),"Service counter"); actors["clerk"]=Person("Staff",new Vector3(0,0,-5.5f),1,3,0).GetComponent<ComicActor>();
            Target("clerk",new Vector3(0,0,-2)); Target("ledger",new Vector3(3,0,-2)); Target("notice",new Vector3(-5,0,1)); Target("foreman",new Vector3(3,0,-1)); Target("parcel",new Vector3(-3,0,-1));
        }
        void Building(string name,Vector3 p,Vector3 size,Color colour,int floors)
        {
            Box(name,p+Vector3.up*size.y*.5f,size,colour);
            Box("Roof coping",p+Vector3.up*(size.y+.12f),new Vector3(size.x+.3f,.24f,size.z+.3f),ink);
            int cols=Mathf.Max(1,Mathf.FloorToInt(size.x/2));
            for(int row=0;row<floors;row++) for(int col=0;col<cols;col++)
            {
                var at=p+new Vector3((col-(cols-1)*.5f)*1.85f,1.5f+row*(size.y-1)/floors,size.z*.5f+.025f);
                Box("Window trim",at,new Vector3(1.25f,1.4f,.08f),cream); Box("Recessed window",at+Vector3.forward*.055f,new Vector3(1.05f,1.2f,.06f),teal);
                Box("Window mullion",at+Vector3.forward*.1f,new Vector3(.07f,1.2f,.05f),ink);
            }
        }
        void Desk(Vector3 p,string name)
        {
            Box(name,p+Vector3.up*1.05f,new Vector3(2.8f,.2f,1.2f),gold,true);
            for(int side=-1;side<=1;side+=2) Box("Table leg",p+new Vector3(side*1.1f,.5f,0),new Vector3(.13f,1,.8f),ink);
        }
        void Bench(Vector3 p) { Box("Bench",p+Vector3.up*.6f,new Vector3(2.2f,.15f,.7f),teal,true); Box("Bench back",p+new Vector3(0,1,-.3f),new Vector3(2.2f,.7f,.12f),teal); for(int i=-1;i<=1;i+=2) Box("Bench leg",p+new Vector3(i*.8f,.3f,0),new Vector3(.16f,.6f,.55f),ink); }
        void Tree(Vector3 p) { Round("Tree trunk",p+Vector3.up*1.7f,new Vector3(.45f,3.4f,.45f),new Color(.38f,.25f,.16f)); for(int i=0;i<3;i++) Round("Tree crown",p+new Vector3((i-1)*.75f,3.7f+(i%2)*.6f,0),new Vector3(2.8f,2.1f,2.5f),green); Box("Planter",p+Vector3.up*.2f,new Vector3(1.6f,.4f,1.6f),red,true); }
        void Noticeboard(Vector3 p)
        {
            Box("Noticeboard frame",p+Vector3.up*1.9f,new Vector3(2.2f,1.8f,.16f),ink);
            Box("Noticeboard backing",p+new Vector3(0,1.9f,.1f),new Vector3(2,1.6f,.08f),gold);
            for(int i=0;i<3;i++) Box("Paper notice",p+new Vector3((i-1)*.55f,1.8f,.16f),new Vector3(.45f,.65f,.025f),cream);
            Sign("READ BEFORE YOU PAY",p+new Vector3(0,2.48f,.2f),.038f,ink);
            for(int i=-1;i<=1;i+=2) Box("Notice post",p+new Vector3(i*.9f,.8f,0),new Vector3(.1f,1.6f,.1f),ink);
        }
        void Kiosk(Vector3 p)
        {
            Box("Tea kiosk",p+Vector3.up*.65f,new Vector3(2.4f,1.3f,2),teal,true);
            Box("Countertop",p+Vector3.up*1.35f,new Vector3(2.6f,.12f,2.2f),cream);
            Box("Awning",p+Vector3.up*2.9f,new Vector3(3,.2f,2.6f),red);
            for(int i=-1;i<=1;i+=2) Box("Awning post",p+new Vector3(i*1.1f,2.1f,0),new Vector3(.08f,1.6f,.08f),ink);
            Round("Thermos",p+new Vector3(0,1.65f,-.2f),new Vector3(.3f,.55f,.3f),gold);
            Sign("CHAI  /  COPIES",p+new Vector3(0,2.9f,1.4f),.048f,cream);
        }
        void Bus(Vector3 p)
        {
            Round("Minibus body",p+Vector3.up*1.1f,new Vector3(5,2.2f,2.2f),gold);
            Box("Bus window strip",p+new Vector3(0,1.5f,-1),new Vector3(3.8f,.7f,.08f),teal);
            for(int i=-1;i<=1;i+=2) for(int j=-1;j<=1;j+=2) Round("Tyre",p+new Vector3(i*1.5f,.4f,j*.95f),new Vector3(.7f,.7f,.3f),ink);
        }
        public Transform Person(string name,Vector3 p,int skin,int outfit,int hair)
        {
            var root=new GameObject(name).transform; root.SetParent(transform,false); root.position=p;
            root.gameObject.AddComponent<ComicActor>().Build(this,skin,outfit,hair);
            ComicMesh.Part(root,"Grounding shadow",new Vector3(0,.02f,0),new Vector3(.9f,.015f,.65f),Tint(new Color(.51f,.47f,.37f)),ComicMesh.Round);
            return root;
        }
        void Box(string name,Vector3 p,Vector3 scale,Color c,bool obstacle=false)
        {
            ComicMesh.Part(scenery,name,p,scale,Tint(c),ComicMesh.Cube);
            if(obstacle) for(int x=Mathf.FloorToInt(p.x-scale.x/2-.25f);x<=Mathf.CeilToInt(p.x+scale.x/2+.25f);x++) for(int z=Mathf.FloorToInt(p.z-scale.z/2-.25f);z<=Mathf.CeilToInt(p.z+scale.z/2+.25f);z++) Blocked.Add(new Vector2Int(x,z));
        }
        void Round(string name,Vector3 p,Vector3 scale,Color c) { ComicMesh.Part(scenery,name,p,scale,Tint(c),ComicMesh.Round); }
        void Sign(string text,Vector3 p,float size,Color colour)
        {
            var go=new GameObject(text); go.transform.SetParent(transform,false); go.transform.position=p;
            var label=go.AddComponent<TextMesh>(); label.text=text; label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.GetComponent<Renderer>().sharedMaterial=label.font.material;
            label.fontSize=64; label.characterSize=size; label.anchor=TextAnchor.MiddleCenter; label.alignment=TextAlignment.Center; label.color=colour; signs.Add(label);
        }
        void Target(string id,Vector3 p) { var t=new GameObject("Interact "+id).transform; t.SetParent(transform,false); t.position=p; Targets[id]=t; }
        public Material Tint(Color c)
        {
            if(palette.TryGetValue(c,out var value)) return value;
            var shader=Resources.Load<Shader>("Shaders/Comic"); if(!shader) shader=Shader.Find("Universal Render Pipeline/Lit");
            var material=new Material(shader); material.SetColor("_BaseColor",c); palette[c]=material; return material;
        }
        public bool Walkable(Vector2Int cell) { return Mathf.Abs(cell.x)<=10 && cell.y>=-7 && cell.y<=7 && !Blocked.Contains(cell) && !occupied.Contains(cell); }
        public void ApplyAppearance(LifeState state) { playerActor.SetAppearance(state); }
        public Vector3 HotspotPosition(string id) { return actors.TryGetValue(StoryPresentation.TargetActor(sceneId,id),out var actor)?actor.transform.position+Vector3.up*1.5f:Targets[id].position+Vector3.up*.3f; }
        public void ResetCamera() { cameraPosition=new Vector3(10,16,19); cameraLook=new Vector3(0,0,-1); cameraSize=9.7f; }
        public void Preview(bool enabled)
        {
            if(enabled) { cameraPosition=Player.position+new Vector3(3,3,6); cameraLook=Player.position+new Vector3(-1.5f,1.4f,0); cameraSize=3.4f; Player.rotation=Quaternion.Euler(0,20,0); }
            else ResetCamera();
        }
        public void Conversation(string target,bool enabled)
        {
            StopSpeaking();
            if(enabled && Targets.ContainsKey(target))
            {
                var p=Targets[target].position;
                cameraLook=(p+Player.position)*.5f+new Vector3(0,.8f,0); cameraPosition=cameraLook+new Vector3(5,6,10); cameraSize=5.7f;
                Face(Player,p);
            }
            else ResetCamera();
        }
        public void StopSpeaking()
        {
            SpeakingActorId="";
            foreach(var actor in actors.Values) actor.Talking=false;
            if(Player) Player.GetComponent<ComicActor>().Talking=false;
        }
        public Vector3 PresentSpeaker(SpeakerCue cue,string target)
        {
            Conversation(target,true);
            if(cue.kind==SpeakerKind.InPerson && actors.TryGetValue(cue.actor,out var actor))
            {
                actor.Talking=true; SpeakingActorId=cue.actor;
                var p=actor.transform.position;
                cameraLook=(p+Player.position)*.5f+Vector3.up*1.1f;
                cameraPosition=cameraLook+new Vector3(5,6,10); cameraSize=5.7f;
                Face(Player,p); Face(actor.transform,Player.position);
                return p+Vector3.up*1.5f;
            }
            if(cue.kind==SpeakerKind.Player || cue.kind==SpeakerKind.Phone || cue.kind==SpeakerKind.Offscreen)
            {
                cameraLook=Player.position+Vector3.up*1.2f; cameraPosition=cameraLook+new Vector3(4,4,7); cameraSize=4.4f;
                if(cue.kind==SpeakerKind.Player) { Player.GetComponent<ComicActor>().Talking=true; SpeakingActorId="player"; }
            }
            return Player.position+Vector3.up*1.5f;
        }
        static void Face(Transform actor,Vector3 point)
        {
            var comic=actor.GetComponent<ComicActor>(); if(comic) comic.FaceTowards(point);
        }
        public void ApplyTimeOfDay(DayPeriod period)
        {
            CurrentPeriod=period;
            bool night=period==DayPeriod.Night,evening=period==DayPeriod.Evening,morning=period==DayPeriod.Morning;
            sun.intensity=night?.18f:evening?.7f:1.1f;
            sun.color=night?new Color(.52f,.65f,1):evening?new Color(1,.74f,.5f):morning?new Color(1,.91f,.77f):Color.white;
            sun.transform.rotation=Quaternion.Euler(night?18:morning?25:evening?22:48,morning?-65:-32,0);
            RenderSettings.ambientLight=night?new Color(.25f,.3f,.45f):evening?new Color(.68f,.56f,.52f):new Color(.8f,.85f,.9f);
            var sky=night?new Color(.15f,.24f,.39f):evening?new Color(.88f,.66f,.53f):morning?new Color(.72f,.85f,.94f):new Color(.69f,.83f,.87f);
            if(View) View.backgroundColor=sky;
            if(windowGlass) windowGlass.sharedMaterial=Tint(sky);
            // Comic.shader uses a stylised light treatment; exposure keeps night/day legible and distinct.
            Shader.SetGlobalFloat("_KeshoExposure",night?.55f:evening?.85f:1f);
            Shader.SetGlobalColor("_KeshoLightTint",sun.color);
            Shader.SetGlobalColor("_KeshoAmbientTint",RenderSettings.ambientLight);
            Shader.SetGlobalFloat("_KeshoKeyStrength",sun.intensity/1.1f);
        }
        public void RestPose(bool enabled)
        {
            bool wasResting=playerActor.Resting; playerActor.SetResting(enabled);
            var shadow=Player.Find("Grounding shadow"); if(shadow) shadow.gameObject.SetActive(!enabled);
            if(enabled && (sceneId=="Room" || sceneId=="Night"))
            {
                Player.position=new Vector3(-4,1.25f,-1); Player.rotation=Quaternion.Euler(-90,0,0);
                cameraLook=new Vector3(-4,1,-2); cameraPosition=cameraLook+new Vector3(4,5,7); cameraSize=4.4f;
            }
            else if(wasResting) { var p=Player.position; p.y=0; Player.position=p; Player.rotation=Quaternion.Euler(0,180,0); }
        }
        void LateUpdate()
        {
            if(!View) return;
            float t=Comfort.ReducedMotion?1:1-Mathf.Exp(-Time.unscaledDeltaTime*5);
            View.orthographicSize=Mathf.Lerp(View.orthographicSize,cameraSize,t);
            View.transform.position=Vector3.Lerp(View.transform.position,cameraPosition,t);
            View.transform.rotation=Quaternion.Slerp(View.transform.rotation,Quaternion.LookRotation(cameraLook-View.transform.position),t);
            foreach(var sign in signs) sign.transform.rotation=View.transform.rotation;
        }
        void CombineScenery()
        {
            using(profileCombineScenery.Auto())
            {
                // One mesh per shared colour rather than one draw for every architectural detail.
                var groups=new Dictionary<Material,List<CombineInstance>>(); var parts=scenery.GetComponentsInChildren<MeshFilter>();
                foreach(var part in parts) { var mat=part.GetComponent<Renderer>().sharedMaterial; if(!groups.ContainsKey(mat)) groups[mat]=new List<CombineInstance>(); groups[mat].Add(new CombineInstance { mesh=part.sharedMesh,transform=scenery.worldToLocalMatrix*part.transform.localToWorldMatrix }); }
                foreach(var pair in groups) { var mesh=new Mesh { name="Campus colour batch",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32 }; mesh.CombineMeshes(pair.Value.ToArray()); combinedMeshes.Add(mesh); ComicMesh.Part(scenery,"Scenery batch",Vector3.zero,Vector3.one,pair.Key,mesh); }
                foreach(var part in parts) { part.gameObject.SetActive(false); Destroy(part.gameObject); }
            }
        }
        void OnDestroy() { foreach(var material in palette.Values) Destroy(material); foreach(var mesh in combinedMeshes) Destroy(mesh); }
    }
}
