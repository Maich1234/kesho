using System.Collections.Generic;
using UnityEngine;

namespace Kesho.Runtime
{
    // Explicit meshes: no CreatePrimitive, implicit collider, or native stripping dependency.
    public static class ComicMesh
    {
        static Mesh cube, round, cylinder;
        public static Mesh Cube { get { if(!cube) cube=BuildCube(); return cube; } }
        public static Mesh Round { get { if(!round) round=BuildRound(); return round; } }
        public static Mesh Cylinder { get { if(!cylinder) cylinder=BuildCylinder(); return cylinder; } }
        public static Transform Part(Transform parent,string name,Vector3 position,Vector3 scale,Material material,Mesh mesh)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));
            go.transform.SetParent(parent,false); go.transform.localPosition=position; go.transform.localScale=scale;
            go.GetComponent<MeshFilter>().sharedMesh=mesh; go.GetComponent<MeshRenderer>().sharedMaterial=material;
            return go.transform;
        }
        static Mesh Finish(string name,List<Vector3> v,List<int> t)
        {
            var m=new Mesh { name=name }; m.SetVertices(v); m.SetTriangles(t,0); m.RecalculateNormals(); m.RecalculateBounds(); return m;
        }
        static Mesh BuildCube()
        {
            var v=new List<Vector3>(); var t=new List<int>();
            Vector3[] normals={Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};
            foreach(var n in normals)
            {
                var u=Vector3.Cross(n,Mathf.Abs(n.y)>.5f?Vector3.forward:Vector3.up)*.5f;
                var w=Vector3.Cross(n,u); int k=v.Count;
                v.Add(n*.5f-u-w); v.Add(n*.5f+u-w); v.Add(n*.5f+u+w); v.Add(n*.5f-u+w);
                t.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});
            }
            return Finish("Kesho bevel-free block",v,t);
        }
        static Mesh BuildRound()
        {
            const int rings=8, sides=12; var v=new List<Vector3>(); var t=new List<int>();
            for(int r=0;r<=rings;r++) for(int s=0;s<=sides;s++)
            {
                float a=Mathf.PI*r/rings,b=Mathf.PI*2*s/sides;
                v.Add(new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b))*.5f);
            }
            for(int r=0;r<rings;r++) for(int s=0;s<sides;s++)
            { int k=r*(sides+1)+s; t.AddRange(new[]{k,k+1,k+sides+1,k+1,k+sides+2,k+sides+1}); }
            return Finish("Kesho rounded form",v,t);
        }
        static Mesh BuildCylinder()
        {
            const int sides=16; var v=new List<Vector3>(); var t=new List<int>();
            for(int s=0;s<sides;s++)
            {
                float a=Mathf.PI*2*s/sides,b=Mathf.PI*2*(s+1)/sides;
                Vector3 p=new Vector3(Mathf.Cos(a)*.5f,0,Mathf.Sin(a)*.5f), q=new Vector3(Mathf.Cos(b)*.5f,0,Mathf.Sin(b)*.5f);
                int k=v.Count; v.AddRange(new[]{p-Vector3.up*.5f,p+Vector3.up*.5f,q+Vector3.up*.5f,q-Vector3.up*.5f});
                t.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});
                k=v.Count; v.AddRange(new[]{Vector3.up*.5f,q+Vector3.up*.5f,p+Vector3.up*.5f}); t.AddRange(new[]{k,k+1,k+2});
                k=v.Count; v.AddRange(new[]{-Vector3.up*.5f,p-Vector3.up*.5f,q-Vector3.up*.5f}); t.AddRange(new[]{k,k+1,k+2});
            }
            return Finish("Kesho cylinder",v,t);
        }
    }
}
