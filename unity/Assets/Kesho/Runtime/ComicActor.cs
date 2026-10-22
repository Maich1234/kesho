using UnityEngine;
using Kesho.Core;

namespace Kesho.Runtime
{
    public sealed class ComicActor : MonoBehaviour
    {
        Transform head,leftArm,rightArm,leftLeg,rightLeg,hair;
        Material skin,coat,ink,white,shoe;
        Stage stage;
        float offset, stride, motionBlend;
        StreetWalker walker;
        Quaternion facing;
        bool turning, settled;
        int appearanceSkin=-1,appearanceOutfit=-1;
        readonly System.Collections.Generic.List<Mesh> ownedMeshes=new System.Collections.Generic.List<Mesh>();
        readonly System.Collections.Generic.Dictionary<int,Transform> hairCache=new System.Collections.Generic.Dictionary<int,Transform>();
        int currentHair=-1;
        public bool Talking;
        public bool Resting { get; private set; }
        public void Build(Stage owner,int skinIndex,int outfit,int hairstyle)
        {
            stage=owner; offset=transform.position.x*.7f;
            skin=owner.Tint(Skin(skinIndex)); coat=owner.Tint(Outfit(outfit)); ink=owner.Tint(new Color(.07f,.055f,.09f)); white=owner.Tint(new Color(.98f,.93f,.81f)); shoe=owner.Tint(new Color(.17f,.2f,.26f));
            P(transform,"Body",new Vector3(0,1.38f,0),new Vector3(.72f,.97f,.43f),coat);
            P(transform,"Shirt",new Vector3(0,1.48f,.19f),new Vector3(.23f,.6f,.1f),white);
            P(transform,"Collar",new Vector3(0,1.86f,.02f),new Vector3(.36f,.15f,.34f),coat);
            P(transform,"Neck",new Vector3(0,1.91f,0),new Vector3(.24f,.26f,.25f),skin);
            head=Joint("Head",new Vector3(0,2.15f,0));
            P(head,"Face",Vector3.zero,new Vector3(.59f,.68f,.53f),skin);
            P(head,"Chin",new Vector3(0,-.23f,.065f),new Vector3(.37f,.25f,.35f),skin);
            for(int side=-1;side<=1;side+=2)
            {
                P(head,"Ear",new Vector3(side*.29f,-.02f,0),new Vector3(.12f,.22f,.13f),skin);
                P(head,"Eye white",new Vector3(side*.12f,.045f,.247f),new Vector3(.15f,.095f,.045f),white);
                P(head,"Pupil",new Vector3(side*.12f,.046f,.269f),new Vector3(.065f,.075f,.026f),ink);
                var brow=P(head,"Brow",new Vector3(side*.12f,.13f,.245f),new Vector3(.17f,.035f,.04f),ink); brow.localRotation=Quaternion.Euler(0,0,side*9);
            }
            P(head,"Nose",new Vector3(0,-.025f,.28f),new Vector3(.12f,.15f,.16f),skin);
            P(head,"Smile",new Vector3(0,-.15f,.235f),new Vector3(.18f,.027f,.05f),ink);
            leftArm=Limb("Left arm",new Vector3(-.43f,1.73f,0),coat,true); rightArm=Limb("Right arm",new Vector3(.43f,1.73f,0),coat,true);
            leftLeg=Limb("Left leg",new Vector3(-.21f,.99f,0),shoe,false); rightLeg=Limb("Right leg",new Vector3(.21f,.99f,0),shoe,false);
            P(transform,"Backpack",new Vector3(0,1.4f,-.3f),new Vector3(.59f,.65f,.3f),shoe);
            SetHair(hairstyle);
            foreach(var joint in new[]{transform,head,leftArm,rightArm,leftLeg,rightLeg}) CombineRigid(joint);
        }
        Transform Joint(string name,Vector3 p) { var t=new GameObject(name).transform; t.SetParent(transform,false); t.localPosition=p; return t; }
        Transform Limb(string name,Vector3 p,Material m,bool arm)
        {
            var t=Joint(name,p); P(t,"Sleeve",new Vector3(0,arm?-.27f:-.42f,0),new Vector3(arm?.23f:.28f,arm?.59f:.86f,.27f),m);
            P(t,arm?"Hand":"Shoe",new Vector3(0,arm?-.62f:-.84f,arm?0:.11f),new Vector3(.22f,arm?.25f:.23f,arm?.22f:.43f),arm?skin:ink); return t;
        }
        Transform P(Transform parent,string name,Vector3 p,Vector3 s,Material m) { return ComicMesh.Part(parent,name,p,s,m,ComicMesh.Round); }
        public void SetAppearance(LifeState state)
        {
            if(appearanceSkin==state.skin && appearanceOutfit==state.outfit && currentHair==state.hair) return;
            appearanceSkin=state.skin; appearanceOutfit=state.outfit;
            var newSkin=stage.Tint(Skin(state.skin));
            foreach(var renderer in GetComponentsInChildren<Renderer>()) if(renderer.sharedMaterial==skin) renderer.sharedMaterial=newSkin;
            skin=newSkin;
            var newCoat=stage.Tint(Outfit(state.outfit));
            foreach(var renderer in GetComponentsInChildren<Renderer>()) if(renderer.sharedMaterial==coat) renderer.sharedMaterial=newCoat;
            coat=newCoat; SetHair(state.hair);
        }
        public void SetHair(int style)
        {
            if(currentHair==style) return; currentHair=style;
            if(hair) hair.gameObject.SetActive(false);
            if(hairCache.TryGetValue(style,out hair)) { hair.gameObject.SetActive(true); return; }
            hair=new GameObject("Hairstyle").transform; hair.SetParent(head,false);
            P(hair,"Hairline",new Vector3(0,.24f,-.02f),new Vector3(.6f,.28f,.52f),ink);
            if(style==1) P(hair,"Natural puff",new Vector3(0,.48f,-.09f),new Vector3(.72f,.6f,.65f),ink);
            if(style==2) for(int i=0;i<7;i++) P(hair,"Braids",new Vector3((i-3)*.075f,.08f,-.24f),new Vector3(.065f,.85f,.12f),ink);
            if(style==3) for(int i=0;i<9;i++) P(hair,"Twists",new Vector3(Mathf.Sin(i*2.4f)*.22f,.37f,Mathf.Cos(i*2.4f)*.17f),new Vector3(.12f,.35f,.12f),ink);
            if(style==4) P(hair,"Loc bun",new Vector3(0,.32f,-.31f),new Vector3(.45f,.4f,.45f),ink);
            hairCache[style]=hair; CombineRigid(hair);
        }
        public void SetResting(bool resting)
        {
            Resting=resting; turning=false; settled=false;
            if(!resting) return;
            Talking=false; head.localRotation=Quaternion.identity;
            leftLeg.localRotation=rightLeg.localRotation=Quaternion.identity;
            leftArm.localRotation=Quaternion.Euler(0,0,8); rightArm.localRotation=Quaternion.Euler(0,0,-8);
        }
        public void FaceTowards(Vector3 point)
        {
            point.y=transform.position.y;
            if((point-transform.position).sqrMagnitude<.001f) return;
            facing=Quaternion.LookRotation(point-transform.position); turning=true;
        }
        void CombineRigid(Transform root)
        {
            var groups=new System.Collections.Generic.Dictionary<Material,System.Collections.Generic.List<CombineInstance>>();
            var originals=new System.Collections.Generic.List<GameObject>();
            foreach(Transform child in root)
            {
                var filter=child.GetComponent<MeshFilter>(); if(!filter) continue;
                var material=child.GetComponent<Renderer>().sharedMaterial;
                if(!groups.ContainsKey(material)) groups[material]=new System.Collections.Generic.List<CombineInstance>();
                groups[material].Add(new CombineInstance { mesh=filter.sharedMesh,transform=root.worldToLocalMatrix*child.localToWorldMatrix }); originals.Add(child.gameObject);
            }
            foreach(var pair in groups)
            {
                var mesh=new Mesh { name="Rigid character colour batch" }; mesh.CombineMeshes(pair.Value.ToArray()); ownedMeshes.Add(mesh);
                ComicMesh.Part(root,"Character batch",Vector3.zero,Vector3.one,pair.Key,mesh);
            }
            foreach(var original in originals) { original.SetActive(false); Destroy(original); }
        }
        void Start() { walker=GetComponent<StreetWalker>(); }
        void Update()
        {
            if(!head) return;
            if(Resting) { turning=false; return; }
            bool moving=walker && walker.Moving && GameSession.Instance && GameSession.Instance.CanExplore;
            if(settled && !moving && !Talking && !turning) return;
            bool reduced=Comfort.ReducedMotion;
            float blend=1-Mathf.Exp(-Time.deltaTime*12);
            motionBlend=Mathf.Lerp(motionBlend,moving?1:0,blend);
            if(walker) stride=walker.TravelledDistance*Mathf.PI/StreetWalker.StepDistance;
            float angle=Mathf.Sin(stride)*25*motionBlend;
            leftLeg.localRotation=Quaternion.Euler(angle,0,0); rightLeg.localRotation=Quaternion.Euler(-angle,0,0);
            var left=Quaternion.Euler(-angle*.65f,0,8);
            var right=Quaternion.Euler(Talking&&!reduced?-22+Mathf.Sin(Time.time*3+offset)*10:angle*.65f,0,-8);
            leftArm.localRotation=Quaternion.Slerp(leftArm.localRotation,left,blend); rightArm.localRotation=Quaternion.Slerp(rightArm.localRotation,right,blend);
            head.localRotation=Quaternion.Slerp(head.localRotation,Quaternion.Euler(Talking&&!reduced?Mathf.Sin(Time.time*2+offset)*3:0,Talking&&!reduced?Mathf.Sin(Time.time*1.7f+offset)*6:0,0),blend);
            if(turning && !moving)
            {
                transform.rotation=reduced?facing:Quaternion.Slerp(transform.rotation,facing,blend);
                if(Quaternion.Angle(transform.rotation,facing)<.1f) turning=false;
            }
            settled=!moving && !Talking && !turning && motionBlend<.001f && Quaternion.Angle(leftArm.localRotation,left)<.1f && Quaternion.Angle(rightArm.localRotation,right)<.1f && Quaternion.Angle(head.localRotation,Quaternion.identity)<.1f;
        }
        void OnDestroy() { foreach(var mesh in ownedMeshes) Destroy(mesh); }
        public static Color Skin(int i) { return new[]{new Color(.29f,.15f,.09f),new Color(.45f,.26f,.15f),new Color(.62f,.39f,.23f),new Color(.75f,.51f,.32f)}[Mathf.Clamp(i,0,3)]; }
        public static Color Outfit(int i) { return new[]{new Color(.98f,.65f,.13f),new Color(.05f,.52f,.5f),new Color(.92f,.28f,.2f),new Color(.22f,.31f,.52f)}[Mathf.Clamp(i,0,3)]; }
    }
}
