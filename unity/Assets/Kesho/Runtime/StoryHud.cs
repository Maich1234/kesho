using Unity.Profiling;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Kesho.Runtime
{
    public sealed class StoryHud : MonoBehaviour
    {
        static readonly ProfilerMarker profileOpen=new ProfilerMarker("Kesho.StoryHud.Open");
        RectTransform safe,panel,contentRoot,actionRoot,mission;
        Font font; Text objective,hint,status,notice;
        Rect previousSafe; int previousWidth,previousHeight;
        RectTransform panelShell,poolRoot;
        CanvasGroup panelGroup;
        ScrollRect panelScroll;
        Coroutine reveal;
        float nextClick;
        Action exploreAction;
        readonly System.Collections.Generic.Dictionary<string,System.Collections.Generic.Queue<RectTransform>> pool=new System.Collections.Generic.Dictionary<string,System.Collections.Generic.Queue<RectTransform>>();
        readonly System.Collections.Generic.Dictionary<RectTransform,string> activeControls=new System.Collections.Generic.Dictionary<RectTransform,string>();
        bool Click()
        {
            if(Time.unscaledTime<nextClick || (panel && !panelGroup.interactable)) return false;
            nextClick=Time.unscaledTime+.2f;
            if(GameSession.Instance) GameSession.Instance.Sound.Cue("click",Vector3.zero);
            return true;
        }
        RectTransform Control(string kind)
        {
            RectTransform r;
            if(pool.TryGetValue(kind,out var items) && items.Count>0) { r=items.Dequeue(); r.SetParent(contentRoot,false); r.gameObject.SetActive(true); }
            else r=Rect(kind,contentRoot);
            activeControls[r]=kind; return r;
        }
        readonly Color ink=new Color(.075f,.12f,.18f),cream=new Color(1,.965f,.86f),gold=new Color(1,.7f,.12f),teal=new Color(.03f,.39f,.4f);
        public bool PanelOpen { get { return panel!=null; } }
        public RectTransform Panel { get { return panel; } }
        void Awake()
        {
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas=gameObject.AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1280,720); scaler.matchWidthOrHeight=.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            if(FindFirstObjectByType<EventSystem>()==null) { var e=new GameObject("Input",typeof(EventSystem),typeof(StandaloneInputModule)); e.transform.SetParent(transform); }
            safe=Rect("Safe area",transform); Stretch(safe);
            mission=Rect("Mission card",safe); Position(mission,new Vector2(0,1),new Vector2(22,-20),new Vector2(600,96)); Paper(mission,cream);
            objective=Label("",mission,23); Stretch(objective.rectTransform); objective.rectTransform.offsetMin=new Vector2(18,33); objective.rectTransform.offsetMax=new Vector2(-16,-12); objective.fontStyle=FontStyle.Bold;
            status=Label("",mission,16); Stretch(status.rectTransform); status.rectTransform.offsetMin=new Vector2(18,7); status.rectTransform.offsetMax=new Vector2(-16,-60); status.color=teal;
            hint=Label("",safe,18); Position(hint.rectTransform,new Vector2(0,0),new Vector2(24,83),new Vector2(650,58)); hint.color=ink; hint.gameObject.AddComponent<Outline>().effectColor=cream;
            notice=Label("",safe,19); Position(notice.rectTransform,new Vector2(.5f,1),new Vector2(-420,-125),new Vector2(840,66)); notice.color=new Color(.65f,.13f,.09f); notice.alignment=TextAnchor.MiddleCenter; notice.gameObject.AddComponent<Outline>().effectColor=cream;
            var pause=Rect("Pause",safe); Position(pause,new Vector2(1,1),new Vector2(-142,-20),new Vector2(120,58)); Paper(pause,cream);
            var button=pause.gameObject.AddComponent<Button>(); button.onClick.AddListener(()=> { if(Click() && GameSession.Instance) GameSession.Instance.Settings(); });
            var text=Label("PAUSE",pause,18); Stretch(text.rectTransform); text.alignment=TextAnchor.MiddleCenter; text.fontStyle=FontStyle.Bold;
        }
        void Update()
        {
            Rect area=Screen.safeArea; if(Screen.width==0||Screen.height==0) return;
            if(area==previousSafe && Screen.width==previousWidth && Screen.height==previousHeight) return;
            previousSafe=area; previousWidth=Screen.width; previousHeight=Screen.height;
            safe.anchorMin=new Vector2(area.xMin/Screen.width,area.yMin/Screen.height); safe.anchorMax=new Vector2(area.xMax/Screen.width,area.yMax/Screen.height); safe.offsetMin=safe.offsetMax=Vector2.zero;
        }
        public void Objective(string text,string details) { objective.text=text; status.text=details; }
        public void Hint(string text) { hint.text=text; }
        public void Notice(string text) { notice.text=text; notice.transform.SetAsLastSibling(); }
        public void Close()
        {
            if(reveal!=null) { StopCoroutine(reveal); reveal=null; }
            if(panelShell)
            {
                foreach(Transform child in contentRoot)
                {
                    var rect=child as RectTransform;
                    if(activeControls.TryGetValue(rect,out var kind))
                    {
                        if(!pool.ContainsKey(kind)) pool[kind]=new System.Collections.Generic.Queue<RectTransform>();
                        pool[kind].Enqueue(rect);
                    }
                    else { child.gameObject.SetActive(false); Destroy(child.gameObject); }
                }
                foreach(var item in activeControls) { item.Key.gameObject.SetActive(false); item.Key.SetParent(poolRoot,false); }
                activeControls.Clear();
                foreach(Transform child in panelShell) if(child.name=="Comic close-up") { child.gameObject.SetActive(false); Destroy(child.gameObject); }
                panelShell.gameObject.SetActive(false);
            }
            panel=null; hint.gameObject.SetActive(true);
        }
        public void Open(string title,string subtitle="")
        {
            using(profileOpen.Auto())
            {
                Close(); hint.gameObject.SetActive(false);
                if(!panelShell)
                {
                    poolRoot=Rect("UI pool",safe); poolRoot.gameObject.SetActive(false);
                    panelShell=Rect("Story panel",safe); Paper(panelShell,cream);
                    var scrollRoot=Rect("Scroll",panelShell); Stretch(scrollRoot);
                    panelScroll=scrollRoot.gameObject.AddComponent<ScrollRect>(); panelScroll.horizontal=false; panelScroll.movementType=ScrollRect.MovementType.Clamped; panelScroll.scrollSensitivity=28;
                    var view=Rect("Viewport",scrollRoot); Stretch(view); view.gameObject.AddComponent<RectMask2D>(); view.gameObject.AddComponent<Image>().color=Color.clear;
                    contentRoot=Rect("Content",view); contentRoot.anchorMin=new Vector2(0,1); contentRoot.anchorMax=Vector2.one; contentRoot.pivot=new Vector2(.5f,1); contentRoot.sizeDelta=Vector2.zero;
                    var layout=contentRoot.gameObject.AddComponent<VerticalLayoutGroup>(); layout.spacing=10; layout.childControlWidth=true; layout.childControlHeight=true; layout.childForceExpandHeight=false;
                    contentRoot.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
                    panelScroll.viewport=view; panelScroll.content=contentRoot;
                    panelGroup=panelShell.gameObject.AddComponent<CanvasGroup>();
                }
                panel=panelShell; panel.gameObject.SetActive(true);
                panel.anchorMin=new Vector2(.05f,.06f); panel.anchorMax=new Vector2(.65f,.81f); panel.offsetMin=panel.offsetMax=Vector2.zero;
                var scrollRect=(RectTransform)panelScroll.transform; scrollRect.offsetMin=new Vector2(24,20); scrollRect.offsetMax=new Vector2(-24,-20);
                panelScroll.StopMovement(); contentRoot.anchoredPosition=Vector2.zero;
                AddText(title,30,ink).fontStyle=FontStyle.Bold; if(subtitle!="") AddText(subtitle,20,teal);
                reveal=StartCoroutine(Reveal(panelGroup)); notice.transform.SetAsLastSibling();
            }
        }
        System.Collections.IEnumerator Reveal(CanvasGroup group)
        {
            group.alpha=1; group.interactable=true; group.blocksRaycasts=true;
            if(Comfort.ReducedMotion) yield break;
            group.alpha=0; group.interactable=false;
            while(group && group.alpha<1) { group.alpha=Mathf.Min(1,group.alpha+Time.unscaledDeltaTime*8); yield return null; }
            if(group) group.interactable=true;
        }
        public void Title()
        {
            Open("KESHO","Every shortcut has a story."); panel.anchorMax=new Vector2(.46f,.79f); panel.anchorMin=new Vector2(.05f,.17f);
        }
        public void Dialogue(string speaker,string text,bool illustrated)
        {
            Open(speaker); panel.anchorMin=new Vector2(.055f,.04f); panel.anchorMax=new Vector2(.945f,.48f);
            if(illustrated && (speaker=="Kito" || speaker=="Rehema"))
            {
                var portrait=Rect("Comic close-up",panel); portrait.anchorMin=new Vector2(0,0); portrait.anchorMax=new Vector2(0,1); portrait.pivot=new Vector2(0,.5f); portrait.offsetMin=new Vector2(8,8); portrait.offsetMax=new Vector2(220,-8);
                Atlas(portrait,speaker); var scroll=panel.Find("Scroll").GetComponent<RectTransform>(); scroll.offsetMin=new Vector2(244,18);
            }
            AddText(text,25);
        }
        public void Responses(string title)
        {
            Open(title); panel.anchorMin=new Vector2(.12f,.04f); panel.anchorMax=new Vector2(.88f,.50f);
        }
        public void CastChoices(Action<string> choose)
        {
            var row=Rect("Choose an illustrated character",contentRoot); row.gameObject.AddComponent<LayoutElement>().minHeight=244;
            for(int i=0;i<2;i++)
            {
                string who=i==0?"Amani":"Nia";
                var card=Rect(who,row); card.anchorMin=new Vector2(i*.5f,0); card.anchorMax=new Vector2((i+1)*.5f,1); card.offsetMin=new Vector2(5,0); card.offsetMax=new Vector2(-5,0); Paper(card,gold);
                var portrait=Rect("Portrait",card); Stretch(portrait); portrait.offsetMin=new Vector2(3,48); portrait.offsetMax=new Vector2(-3,-3); Atlas(portrait,who);
                var label=Label(who,card,23); Position(label.rectTransform,new Vector2(.5f,0),new Vector2(-90,44),new Vector2(180,40)); label.alignment=TextAnchor.MiddleCenter; label.fontStyle=FontStyle.Bold;
                card.gameObject.AddComponent<Button>().onClick.AddListener(()=> { if(Click()) choose(who); });
            }
        }
        void Atlas(RectTransform r,string who)
        {
            var image=r.gameObject.AddComponent<RawImage>(); image.texture=Resources.Load<Texture2D>("Art/cast"); image.raycastTarget=false;
            int index=who=="Nia"?1:who=="Rehema"?2:who=="Kito"?3:0;
            image.uvRect=new Rect(index%2*.5f,index<2?.5f:0,.5f,.5f);
            var fitter=r.gameObject.AddComponent<AspectRatioFitter>(); fitter.aspectMode=AspectRatioFitter.AspectMode.FitInParent; fitter.aspectRatio=1;
            // Layout containers own height; portrait cells use a child to avoid competing layouts.
            if(r.parent==contentRoot) { Destroy(fitter); image.uvRect=new Rect(index%2*.5f,(index<2?.5f:0)+.14f,.5f,.32f); }
        }
        public Text AddText(string text,int size=24,Color? color=null)
        {
            var r=Control("Pooled text"); var label=r.GetComponent<Text>();
            if(!label) { label=r.gameObject.AddComponent<Text>(); label.font=font; label.raycastTarget=false; label.supportRichText=false; r.gameObject.AddComponent<LayoutElement>(); }
            label.text=text; label.fontSize=size; label.fontStyle=FontStyle.Normal; label.color=color??ink; label.horizontalOverflow=HorizontalWrapMode.Wrap; label.verticalOverflow=VerticalWrapMode.Overflow;
            r.GetComponent<LayoutElement>().minHeight=size+10; return label;
        }
        public Button Button(string title,Action action,bool primary=false)
        {
            var r=Control("Pooled button"); var button=r.GetComponent<Button>();
            if(!button)
            {
                Paper(r,cream); button=r.gameObject.AddComponent<Button>(); r.gameObject.AddComponent<LayoutElement>().minHeight=60;
                var text=Label("",r,21); Stretch(text.rectTransform); text.rectTransform.offsetMin=new Vector2(14,6); text.rectTransform.offsetMax=new Vector2(-14,-6); text.alignment=TextAnchor.MiddleLeft; text.fontStyle=FontStyle.Bold;
            }
            r.GetComponent<Image>().color=primary?gold:new Color(.88f,.92f,.86f);
            r.GetComponentInChildren<Text>().text=title;
            var colors=button.colors; colors.pressedColor=new Color(.72f,.8f,.8f); colors.disabledColor=new Color(.7f,.7f,.7f,.6f); button.colors=colors;
            button.interactable=true; button.onClick.RemoveAllListeners();
            button.onClick.AddListener(()=> { if(!button.interactable || !Click()) return; action(); }); return button;
        }
        public void CycleButton(Func<string> label,Action changed)
        {
            Button control=null;
            control=Button(label(),()=> { changed(); control.GetComponentInChildren<Text>().text=label(); });
        }
        public void Field(string initial,Action<string> changed)
        {
            var r=Rect("Name",contentRoot); Paper(r,Color.white); r.gameObject.AddComponent<LayoutElement>().minHeight=58;
            var label=Label(initial,r,23); Stretch(label.rectTransform); label.rectTransform.offsetMin=new Vector2(12,6); label.rectTransform.offsetMax=new Vector2(-12,-6);
            var input=r.gameObject.AddComponent<InputField>(); input.textComponent=label; input.characterLimit=32; input.text=initial; input.onValueChanged.AddListener(value=>changed(value));
        }
        public void Slider(string title,float value,Action<float> changed)
        {
            AddText(title,18); var r=Rect(title,contentRoot); r.gameObject.AddComponent<LayoutElement>().minHeight=44; r.gameObject.AddComponent<Image>().color=teal;
            var thumb=Rect("Handle",r); thumb.gameObject.AddComponent<Image>().color=gold; thumb.sizeDelta=new Vector2(32,44);
            var slider=r.gameObject.AddComponent<Slider>(); slider.handleRect=thumb; slider.targetGraphic=thumb.GetComponent<Image>(); slider.value=value; slider.onValueChanged.AddListener(v=>changed(v));
        }
        public void ActionButton(string text,Action action)
        {
            exploreAction=action;
            if(!actionRoot)
            {
                actionRoot=Rect("Explore controls",safe); Position(actionRoot,new Vector2(1,0),new Vector2(-298,85),new Vector2(274,62)); Paper(actionRoot,gold);
                actionRoot.gameObject.AddComponent<Button>().onClick.AddListener(()=> { if(Click()) exploreAction?.Invoke(); });
                var label=Label("",actionRoot,22); Stretch(label.rectTransform); label.alignment=TextAnchor.MiddleCenter; label.fontStyle=FontStyle.Bold;
            }
            actionRoot.GetComponentInChildren<Text>().text=text;
        }
        public void ShowAction(bool value) { if(actionRoot) actionRoot.gameObject.SetActive(value); }
        void Paper(RectTransform r,Color colour)
        {
            r.gameObject.AddComponent<Image>().color=colour; var outline=r.gameObject.AddComponent<Outline>(); outline.effectColor=ink; outline.effectDistance=new Vector2(2,-2);
            var shadow=r.gameObject.AddComponent<Shadow>(); shadow.effectColor=new Color(.06f,.1f,.13f,.3f); shadow.effectDistance=new Vector2(4,-5);
        }
        Text Label(string value,Transform parent,int size)
        {
            var r=Rect("Text",parent); var text=r.gameObject.AddComponent<Text>(); text.font=font; text.text=value; text.fontSize=size; text.color=ink; text.raycastTarget=false; text.supportRichText=false; return text;
        }
        static RectTransform Rect(string name,Transform parent) { var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent,false); return r; }
        static void Stretch(RectTransform r) { r.anchorMin=Vector2.zero; r.anchorMax=Vector2.one; r.offsetMin=r.offsetMax=Vector2.zero; }
        static void Position(RectTransform r,Vector2 anchor,Vector2 offset,Vector2 size) { r.anchorMin=r.anchorMax=anchor; r.pivot=new Vector2(0,1); r.anchoredPosition=offset; r.sizeDelta=size; }
    }
}
