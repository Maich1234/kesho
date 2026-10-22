using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kesho.Runtime
{
    public sealed class StreetWalker : MonoBehaviour
    {
        public Stage Stage;
        public const float StepDistance=1.2f;
        readonly Queue<Vector3> route=new Queue<Vector3>();
        float step;
        public float TravelledDistance { get; private set; }
        Action onArrival;
        public bool Moving { get { return route.Count>0; } }
        public void Stop() { route.Clear(); onArrival=null; }
        public bool WalkTo(Vector3 destination, Action arrived=null)
        {
            Stop();
            var start=Cell(transform.position); var end=Cell(destination);
            if(!Stage.Walkable(start)||!Stage.Walkable(end)) return false;
            var frontier=new Queue<Vector2Int>(); var parent=new Dictionary<Vector2Int,Vector2Int>(); frontier.Enqueue(start); parent[start]=start;
            var dirs=new[]{Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left};
            while(frontier.Count>0)
            {
                var cell=frontier.Dequeue(); if(cell==end) break;
                foreach(var dir in dirs) { var next=cell+dir; if(Stage.Walkable(next)&&!parent.ContainsKey(next)) { parent[next]=cell; frontier.Enqueue(next); } }
            }
            if(!parent.ContainsKey(end)) return false;
            var reverse=new List<Vector3>(); for(var at=end;at!=start;at=parent[at]) reverse.Add(new Vector3(at.x,0,at.y)); reverse.Reverse(); route.Clear(); foreach(var p in reverse) route.Enqueue(p);
            onArrival=arrived; return true;
        }
        public static Vector2Int Cell(Vector3 p) { return new Vector2Int(Mathf.RoundToInt(p.x),Mathf.RoundToInt(p.z)); }
        void Update()
        {
            var session=GameSession.Instance; if(session==null||!session.CanExplore) { return; }
            if(Input.GetMouseButtonDown(0) && !(EventSystem.current && EventSystem.current.IsPointerOverGameObject()) && !(Input.touchCount>0 && EventSystem.current && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)))
            {
                Ray ray=Stage.View.ScreenPointToRay(Input.mousePosition); var ground=new Plane(Vector3.up,Vector3.zero); float distance;
                if(!session.TryInspectScreen(Input.mousePosition) && ground.Raycast(ray,out distance)) { var point=ray.GetPoint(distance); if(!session.TryInspect(point) && !WalkTo(point)) session.MovementBlocked(); }
            }
            if(route.Count>0)
            {
                Vector3 direction=route.Peek()-transform.position;
                var previous=transform.position;
                transform.position=Vector3.MoveTowards(transform.position,route.Peek(),Time.deltaTime*3.5f);
                if(direction.sqrMagnitude>.001f) transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(direction),Time.deltaTime*12);
                if(Vector3.Distance(transform.position,route.Peek())<.04f) route.Dequeue();
                float travelled=Vector3.Distance(previous,transform.position); TravelledDistance+=travelled;
                step+=travelled; if(step>=StepDistance) { step%=StepDistance; session.Sound.Cue("step",transform.position); }
            }
            if(route.Count==0 && onArrival!=null)
            {
                var callback=onArrival; onArrival=null; callback();
            }

        }
    }
}
