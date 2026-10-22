using UnityEngine;

namespace Kesho.Runtime
{
    // Original synthesized placeholders. Never presented as recorded voices or real crowd recordings.
    public static class ProceduralFoley
    {
        public static AudioClip Rustle(string name,float duration,int seed)
        {
            const int rate=22050; var samples=new float[(int)(rate*duration)]; var random=new System.Random(seed);
            float last=0;
            for(int i=0;i<samples.Length;i++)
            {
                float t=(float)i/rate,noise=(float)(random.NextDouble()*2-1); last=last*.65f+noise*.35f;
                float envelope=Mathf.Sin(Mathf.PI*t/duration)*(.25f+.75f*Mathf.Pow(Mathf.Sin(t*19),2));
                samples[i]=last*envelope*.28f;
            }
            var clip=AudioClip.Create(name,samples.Length,1,rate,false); clip.SetData(samples,0); return clip;
        }
        public static AudioClip Campus()
        {
            const int rate=22050,seconds=12; var samples=new float[rate*seconds]; var random=new System.Random(71); float air=0;
            for(int i=0;i<samples.Length;i++)
            {
                float t=(float)i/rate; air=.96f*air+.04f*(float)(random.NextDouble()*2-1);
                float bird=0; float local=t%3;
                if(local<.22f) bird=Mathf.Sin(2*Mathf.PI*(1650*t+45*Mathf.Sin(t*20)))*Mathf.Sin(Mathf.PI*local/.22f)*.025f;
                samples[i]=air*.15f+bird;
            }
            var clip=AudioClip.Create("Campus air and birds (synthesized)",samples.Length,1,rate,false); clip.SetData(samples,0); return clip;
        }
    }
}
