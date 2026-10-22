using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Kesho.Runtime
{
    public sealed class StoryHud : MonoBehaviour
    {
        RectTransform safe, panel;
        Font font;
        Text objective, hint, status, notice;
        public bool PanelOpen { get { return panel!=null; } }
        public RectTransform Panel { get { return panel; } }
        readonly Color ink=new Color(.055f,.12f,.14f,.97f), cream=new Color(.96f,.9f,.77f), gold=new Color(.84f,.65f,.32f);
        void Awake()
        {
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas=gameObject.AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1280,720); scaler.matchWidthOrHeight=.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            if(FindFirstObjectByType<EventSystem>()==null) { var e=new GameObject("Input",typeof(EventSystem),typeof(StandaloneInputModule)); e.transform.SetParent(transform); }
            safe=Rect("Safe area",transform); Stretch(safe);
            objective=Label("",safe,24); Position(objective.rectTransform,new Vector2(0,1),new Vector2(24,-25),new Vector2(740,62));
            status=Label("",safe,18); Position(status.rectTransform,new Vector2(0,1),new Vector2(24,-90),new Vector2(680,35));
            hint=Label("",safe,19); Position(hint.rectTransform,new Vector2(.5f,0),new Vector2(-370,95),new Vector2(740,42)); hint.alignment=TextAnchor.MiddleCenter;
            notice=Label("",safe,19); Position(notice.rectTransform,new Vector2(.5f,1),new Vector2(-470,-132),new Vector2(940,70)); notice.color=new Color(1f,.67f,.5f); notice.alignment=TextAnchor.MiddleCenter;
            var pause=Rect("Pause",safe); Position(pause,new Vector2(1,1),new Vector2(-145,-22),new Vector2(120,58));
            pause.gameObject.AddComponent<Image>().color=ink; pause.gameObject.AddComponent<Button>().onClick.AddListener(()=>GameSession.Instance.Settings());
            var pauseText=Label("Settings",pause,20); Stretch(pauseText.rectTransform); pauseText.alignment=TextAnchor.MiddleCenter;
        }
        void Update()
        {
            Rect area=Screen.safeArea;
            safe.anchorMin=new Vector2(area.xMin/Screen.width,area.yMin/Screen.height); safe.anchorMax=new Vector2(area.xMax/Screen.width,area.yMax/Screen.height);
            safe.offsetMin=safe.offsetMax=Vector2.zero;
        }
        public void Objective(string text,string details) { objective.text=text; status.text=details; }
        public void Hint(string text) { hint.text=text; }
        public void Notice(string text) { notice.text=text; notice.transform.SetAsLastSibling(); }
        public void Close()
        {
            if(panel) { panel.gameObject.SetActive(false); Destroy(panel.gameObject); } panel=null;
        }
        public void Open(string title,string subtitle="")
        {
            Close(); panel=Rect("Story panel",safe); panel.anchorMin=new Vector2(.14f,.05f); panel.anchorMax=new Vector2(.86f,.66f); panel.offsetMin=panel.offsetMax=Vector2.zero;
            var image=panel.gameObject.AddComponent<Image>(); image.color=ink;
            var scrollRoot=Rect("Scroll",panel); Stretch(scrollRoot); scrollRoot.offsetMin=new Vector2(24,18); scrollRoot.offsetMax=new Vector2(-24,-18);
            var scroll=scrollRoot.gameObject.AddComponent<ScrollRect>(); scroll.horizontal=false;
            var view=Rect("Viewport",scrollRoot); Stretch(view); view.gameObject.AddComponent<RectMask2D>();
            var surface=view.gameObject.AddComponent<Image>(); surface.color=Color.clear;
            var content=Rect("Content",view); content.anchorMin=new Vector2(0,1); content.anchorMax=Vector2.one; content.pivot=new Vector2(.5f,1); content.sizeDelta=Vector2.zero;
            var layout=content.gameObject.AddComponent<VerticalLayoutGroup>(); layout.spacing=12; layout.childControlWidth=true; layout.childControlHeight=true; layout.childForceExpandHeight=false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport=view; scroll.content=content; contentRoot=content;
            AddText(title,30,gold); if(subtitle!="") AddText(subtitle,19,cream);
        }
        RectTransform contentRoot;
        public Text AddText(string text,int size=24,Color? color=null)
        {
            var label=Label(text,contentRoot,size); label.color=color??cream;
            label.horizontalOverflow=HorizontalWrapMode.Wrap; label.verticalOverflow=VerticalWrapMode.Overflow;
            var element=label.gameObject.AddComponent<LayoutElement>(); element.minHeight=size+12;
            return label;
        }
        public Button Button(string title,Action action,bool primary=false)
        {
            var r=Rect(title,contentRoot); var image=r.gameObject.AddComponent<Image>(); image.color=primary?gold:new Color(.14f,.26f,.28f);
            var button=r.gameObject.AddComponent<Button>(); var colors=button.colors; colors.pressedColor=new Color(.7f,.7f,.7f); button.colors=colors;
            r.gameObject.AddComponent<LayoutElement>().minHeight=58;
            var text=Label(title,r,22); Stretch(text.rectTransform); text.alignment=TextAnchor.MiddleCenter; text.color=primary?ink:cream;
            button.onClick.AddListener(()=>
            {
                if(!button.interactable) return;
                button.interactable=false;
                if(GameSession.Instance) GameSession.Instance.Sound.Cue("click",Vector3.zero);
                StartCoroutine(ReleaseButton(button)); action();
            }); return button;
        }
        System.Collections.IEnumerator ReleaseButton(Button button)
        {
            yield return new WaitForSecondsRealtime(.2f);
            if(button) button.interactable=true;
        }
        public void Field(string initial,Action<string> changed)
        {
            var r=Rect("Name",contentRoot); r.gameObject.AddComponent<Image>().color=new Color(.12f,.22f,.25f); r.gameObject.AddComponent<LayoutElement>().minHeight=58;
            var label=Label(initial,r,23); Stretch(label.rectTransform); label.rectTransform.offsetMin=new Vector2(12,6); label.rectTransform.offsetMax=new Vector2(-12,-6);
            var input=r.gameObject.AddComponent<InputField>(); input.textComponent=label; input.characterLimit=32; input.text=initial; input.onValueChanged.AddListener(value=>changed(value));
        }
        public void Slider(string title,float value,Action<float> changed)
        {
            AddText(title,18); var r=Rect(title,contentRoot); r.gameObject.AddComponent<LayoutElement>().minHeight=44;
            var back=r.gameObject.AddComponent<Image>(); back.color=new Color(.15f,.27f,.29f);
            var thumb=Rect("Handle",r); thumb.gameObject.AddComponent<Image>().color=gold; thumb.sizeDelta=new Vector2(28,44);
            var slider=r.gameObject.AddComponent<Slider>(); slider.handleRect=thumb; slider.targetGraphic=thumb.GetComponent<Image>(); slider.value=value; slider.onValueChanged.AddListener(v=>changed(v));
        }
        public void ActionButton(string text,Action action)
        {
            if(actionRoot) Destroy(actionRoot.gameObject);
            actionRoot=Rect("Explore controls",safe); Position(actionRoot,new Vector2(1,0),new Vector2(-290,82),new Vector2(260,58));
            actionRoot.gameObject.AddComponent<Image>().color=gold; var b=actionRoot.gameObject.AddComponent<Button>(); b.onClick.AddListener(()=>action());
            var label=Label(text,actionRoot,22); Stretch(label.rectTransform); label.alignment=TextAnchor.MiddleCenter; label.color=ink;
        }
        RectTransform actionRoot;
        public void ShowAction(bool value) { if(actionRoot) actionRoot.gameObject.SetActive(value); }
        Text Label(string value,Transform parent,int size)
        {
            var r=Rect("Text",parent); var text=r.gameObject.AddComponent<Text>(); text.font=font; text.text=value; text.fontSize=size; text.color=cream; text.raycastTarget=false; text.supportRichText=false; return text;
        }
        static RectTransform Rect(string name,Transform parent) { var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent,false); return r; }
        static void Stretch(RectTransform r) { r.anchorMin=Vector2.zero; r.anchorMax=Vector2.one; r.offsetMin=r.offsetMax=Vector2.zero; }
        static void Position(RectTransform r,Vector2 anchor,Vector2 offset,Vector2 size) { r.anchorMin=r.anchorMax=anchor; r.pivot=new Vector2(0,1); r.anchoredPosition=offset; r.sizeDelta=size; }
    }
}
