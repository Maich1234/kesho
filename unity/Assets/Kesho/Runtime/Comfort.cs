using UnityEngine;
namespace Kesho.Runtime
{
    public static class Comfort
    {
        static bool loaded, reduced;
        public static bool ReducedMotion { get { if(!loaded) Refresh(); return reduced; } }
        public static void Refresh() { reduced=PlayerPrefs.GetInt("kesho.reduce_motion",0)==1; loaded=true; }
    }
}
