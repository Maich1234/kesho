using UnityEngine;
using Kesho.Core;

namespace Kesho.Runtime
{
    public sealed class ComicActor : MonoBehaviour
    {
        Transform head,leftArm,rightArm,leftLeg,rightLeg,hair;
        Material skin,coat,ink,white,shoe;
        Stage stage;
        float offset;
        int currentHair=-1;
        public bool Talking;
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
        }
        Transform Joint(string name,Vector3 p) { var t=new GameObject(name).transform; t.SetParent(transform,false); t.localPosition=p; return t; }
        Transform Limb(string name,Vector3 p,Material m,bool arm)
        {
            var t=Joint(name,p); P(t,"Sleeve",new Vector3(0,-.27f,0),new Vector3(arm?.23f:.28f,.59f,.27f),m);
            P(t,arm?"Hand":"Shoe",new Vector3(0,arm?-.62f:-.84f,arm?0:.11f),new Vector3(.22f,arm?.25f:.23f,arm?.22f:.43f),arm?skin:ink); return t;
        }
        Transform P(Transform parent,string name,Vector3 p,Vector3 s,Material m) { return ComicMesh.Part(parent,name,p,s,m,ComicMesh.Round); }
        public void SetAppearance(LifeState state)
        {
            var body=transform.Find("Body"); body.GetComponent<Renderer>().sharedMaterial=stage.Tint(Outfit(state.outfit));
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
            if(hair) { hair.gameObject.SetActive(false); Destroy(hair.gameObject); }
            hair=new GameObject("Hairstyle").transform; hair.SetParent(head,false);
            P(hair,"Hairline",new Vector3(0,.24f,-.02f),new Vector3(.6f,.28f,.52f),ink);
            if(style==1) P(hair,"Natural puff",new Vector3(0,.48f,-.09f),new Vector3(.72f,.6f,.65f),ink);
            if(style==2) for(int i=0;i<7;i++) P(hair,"Braids",new Vector3((i-3)*.075f,.08f,-.24f),new Vector3(.065f,.85f,.12f),ink);
            if(style==3) for(int i=0;i<9;i++) P(hair,"Twists",new Vector3(Mathf.Sin(i*2.4f)*.22f,.37f,Mathf.Cos(i*2.4f)*.17f),new Vector3(.12f,.35f,.12f),ink);
            if(style==4) P(hair,"Loc bun",new Vector3(0,.32f,-.31f),new Vector3(.45f,.4f,.45f),ink);
        }
        void Update()
        {
            if(!head) return;
            var walker=GetComponent<StreetWalker>(); bool moving=walker && walker.Moving && GameSession.Instance && GameSession.Instance.CanExplore;
            bool reduced=PlayerPrefs.GetInt("kesho.reduce_motion",0)==1;
            float phase=Time.time*9+offset,angle=moving?Mathf.Sin(phase)*28:0;
            leftLeg.localRotation=Quaternion.Euler(angle,0,0); rightLeg.localRotation=Quaternion.Euler(-angle,0,0);
            leftArm.localRotation=Quaternion.Euler(-angle*.65f,0,8); rightArm.localRotation=Quaternion.Euler(Talking&&!reduced?-25+Mathf.Sin(Time.time*3)*12:angle*.65f,0,-8);
            head.localRotation=Quaternion.Euler(0,Talking&&!reduced?Mathf.Sin(Time.time*2+offset)*7:0,0);
        }
        public static Color Skin(int i) { return new[]{new Color(.29f,.15f,.09f),new Color(.45f,.26f,.15f),new Color(.62f,.39f,.23f),new Color(.75f,.51f,.32f)}[Mathf.Clamp(i,0,3)]; }
        public static Color Outfit(int i) { return new[]{new Color(.98f,.65f,.13f),new Color(.05f,.52f,.5f),new Color(.92f,.28f,.2f),new Color(.22f,.31f,.52f)}[Mathf.Clamp(i,0,3)]; }
    }
}
