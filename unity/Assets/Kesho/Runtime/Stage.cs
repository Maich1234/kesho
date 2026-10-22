using System.Collections.Generic;
using UnityEngine;
using Kesho.Core;

namespace Kesho.Runtime
{
    public sealed class Stage : MonoBehaviour
    {
        public string sceneId = "Arrival";
        public readonly Dictionary<string, Transform> Targets = new Dictionary<string, Transform>();
        public readonly HashSet<Vector2Int> Blocked = new HashSet<Vector2Int>();
        public Transform Player { get; private set; }
        public Camera View { get; private set; }
        Material stone, wood, fabric;
        void Awake()
        {
            stone = Material(new Color(.60f,.55f,.43f)); wood = Material(new Color(.29f,.18f,.12f)); fabric = Material(new Color(.19f,.39f,.37f));
            RenderSettings.ambientLight = sceneId == "Night" ? new Color(.12f,.16f,.23f) : new Color(.52f,.55f,.55f);
            var light = new GameObject("Key light").AddComponent<Light>(); light.transform.SetParent(transform); light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(48,-28,0); light.intensity = sceneId == "Night" ? .3f : 1f; light.shadows = LightShadows.Soft;
            Box("Ground",new Vector3(0,-.15f,0),new Vector3(24,.3f,20),stone,false);
            if (sceneId == "Arrival")
            {
                Box("Road",new Vector3(0,.01f,5),new Vector3(24,.02f,4),Material(new Color(.20f,.24f,.25f)),false);
                Box("Bus",new Vector3(-6,1.1f,5),new Vector3(6,2.2f,2.4f),fabric,true);
                Box("Bus windows",new Vector3(-6,1.5f,3.77f),new Vector3(4.8f,.7f,.06f),Material(new Color(.12f,.23f,.29f)),false);
                Shelter(new Vector3(2,0,-4)); Person("Waiting contact",new Vector3(3,0,-2),1,1); Target("contact",new Vector3(2,0,-1));
            }
            else if (sceneId == "Courtyard")
            {
                Walls(); Box("Supper table",new Vector3(-3,.65f,-2),new Vector3(3,1.3f,1.4f),wood,true);
                Person("Sifa",new Vector3(-3,0,-4),2,2); Target("landlord",new Vector3(-1,0,0)); Target("supper",new Vector3(-3,0,0)); Target("contact",new Vector3(3,0,-1));
                for (int i=0;i<5;i++) Box("Drying cloth",new Vector3(-6+i*1.8f,2.8f,-6),new Vector3(.8f,1,.06f),fabric,false);
            }
            else if (sceneId == "Room" || sceneId == "Night")
            {
                Walls(); Box("Bed frame",new Vector3(-4,.3f,-2),new Vector3(2.4f,.6f,4),wood,true);
                Box("Blanket",new Vector3(-4,.7f,-2),new Vector3(2.3f,.2f,3.8f),fabric,false);
                Box("Pillow",new Vector3(-4,.87f,-3.2f),new Vector3(1.5f,.15f,.6f),stone,false);
                Box("Desk",new Vector3(4,.65f,-4),new Vector3(2,1.3f,1),wood,true);
                Target("bed",new Vector3(-1,0,0)); Target("phone",new Vector3(3,0,-2)); Target("door",new Vector3(0,0,4));
            }
            else if (sceneId == "Office")
            {
                Walls(); Box("Desk",new Vector3(0,.7f,-3),new Vector3(4,1.4f,1.4f),wood,true);
                Box("Ledger",new Vector3(1,1.43f,-3),new Vector3(.7f,.08f,.5f),stone,false);
                Person("Maro",new Vector3(0,0,-5),0,3); Target("clerk",new Vector3(0,0,-1)); Target("ledger",new Vector3(2,0,-1)); Target("notice",new Vector3(-5,0,1));
                Box("Free applications notice",new Vector3(-6,1.7f,-1),new Vector3(.1f,1.2f,1),stone,false);
            }
            else if (sceneId == "Dispatch")
            {
                Shelter(new Vector3(1,0,-4));
                for(int i=0;i<4;i++) Box("Sealed stock",new Vector3(-6+i*2, .6f,-5),new Vector3(1.4f,1.2f,1.4f),wood,true);
                Person("Tendo",new Vector3(4,0,-3),1,1); Target("foreman",new Vector3(3,0,-1)); Target("parcel",new Vector3(-2,0,-2));
            }
            else if (sceneId == "Campus")
            {
                Walls(); Box("Reception",new Vector3(-4,.7f,-3),new Vector3(3,1.4f,1),wood,true);
                Person("Rehema",new Vector3(3,0,-3),3,2); Target("rehema",new Vector3(3,0,-1)); Target("parcel",new Vector3(-3,0,-1));
            }
            Target("exit",new Vector3(0,0,5));
            Player=Person("Player",new Vector3(0,0,4),1,0);
            View=new GameObject("Camera").AddComponent<Camera>(); View.transform.SetParent(transform); View.tag="MainCamera";
            View.transform.position=new Vector3(12,15,16); View.transform.LookAt(new Vector3(0,0,-1)); View.fieldOfView=42;
            View.clearFlags=CameraClearFlags.SolidColor; View.backgroundColor=new Color(.12f,.19f,.22f); View.nearClipPlane=.1f; View.farClipPlane=80;
            View.gameObject.AddComponent<AudioListener>();
            var walker=Player.gameObject.AddComponent<StreetWalker>(); walker.Stage=this;
        }
        public void ApplyAppearance(LifeState state)
        {
            var body=Player.Find("Body"); var head=Player.Find("Head");
            if(body) body.GetComponent<Renderer>().sharedMaterial=Material(Outfit(state.outfit));
            if(head) head.GetComponent<Renderer>().sharedMaterial=Material(Skin(state.skin));
        }
        public bool Walkable(Vector2Int cell) { return Mathf.Abs(cell.x)<=10 && cell.y>=-7 && cell.y<=7 && !Blocked.Contains(cell); }
        void Target(string id,Vector3 pos)
        {
            var t=new GameObject(id); t.transform.SetParent(transform); t.transform.position=pos; Targets[id]=t.transform;
        }
        void Walls()
        {
            Box("Back wall",new Vector3(0,2,-8),new Vector3(22,4,.2f),fabric,true);
            Box("Side wall",new Vector3(-11,2,0),new Vector3(.2f,4,16),stone,true);
        }
        void Shelter(Vector3 p)
        {
            Box("Tin roof",p+Vector3.up*3,new Vector3(7,.15f,4),fabric,false);
            foreach(float x in new[]{-3f,3f}) Box("Post",p+new Vector3(x,1.5f,0),new Vector3(.12f,3,.12f),wood,true);
        }
        static Color Skin(int i) { return new[]{new Color(.28f,.16f,.10f),new Color(.43f,.27f,.17f),new Color(.59f,.37f,.23f),new Color(.72f,.49f,.31f)}[Mathf.Clamp(i,0,3)]; }
        static Color Outfit(int i) { return new[]{new Color(.68f,.42f,.15f),new Color(.19f,.4f,.37f),new Color(.61f,.29f,.27f),new Color(.26f,.31f,.48f)}[Mathf.Clamp(i,0,3)]; }
        Transform Person(string name,Vector3 pos,int skin,int outfit)
        {
            var root=new GameObject(name).transform; root.SetParent(transform); root.position=pos;
            Part(root,"Body",new Vector3(0,.95f,0),new Vector3(.5f,.75f,.32f),Material(Outfit(outfit)),PrimitiveType.Cube);
            Part(root,"Head",new Vector3(0,1.55f,0),Vector3.one*.42f,Material(Skin(skin)),PrimitiveType.Sphere);
            Part(root,"Left leg",new Vector3(-.14f,.3f,0),new Vector3(.2f,.6f,.22f),wood,PrimitiveType.Cube);
            Part(root,"Right leg",new Vector3(.14f,.3f,0),new Vector3(.2f,.6f,.22f),wood,PrimitiveType.Cube);
            return root;
        }
        void Part(Transform parent,string name,Vector3 position,Vector3 scale,Material material,PrimitiveType type)
        {
            var obj=GameObject.CreatePrimitive(type); obj.name=name; obj.transform.SetParent(parent); obj.transform.localPosition=position; obj.transform.localScale=scale; obj.GetComponent<Renderer>().sharedMaterial=material; Destroy(obj.GetComponent<Collider>());
        }
        void Box(string name,Vector3 pos,Vector3 scale,Material material,bool obstacle)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name=name; obj.transform.SetParent(transform); obj.transform.position=pos; obj.transform.localScale=scale; obj.GetComponent<Renderer>().sharedMaterial=material;
            if(obstacle) for(int x=Mathf.FloorToInt(pos.x-scale.x/2-.3f);x<=Mathf.CeilToInt(pos.x+scale.x/2+.3f);x++) for(int z=Mathf.FloorToInt(pos.z-scale.z/2-.3f);z<=Mathf.CeilToInt(pos.z+scale.z/2+.3f);z++) Blocked.Add(new Vector2Int(x,z));
        }
        Material Material(Color color)
        {
            var shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null) shader=Shader.Find("Standard");
            var material=new Material(shader); material.color=color; ownedMaterials.Add(material); return material;
        }
        readonly List<Material> ownedMaterials=new List<Material>();
        void OnDestroy() { foreach(var material in ownedMaterials) Destroy(material); }
    }
}
