"""Bake original synthesized placeholders offline; requires ffmpeg/libvorbis, Python stdlib.
These clips are not recorded voices or natural field recordings. Deterministic seeds and a
short loop-seam fade keep startup free of sample synthesis. Output: Unity Resources/Sound.
"""
from pathlib import Path
import wave, math, random, struct, subprocess, tempfile

# Offline pre-bake of original synthesized placeholders. They are not field recordings.
sound=Path(__file__).resolve().parents[2]/'unity/Assets/Kesho/Resources/Sound'
for name,duration,seed in [('paper',.45,18),('bedding',1.8,27),('campus',12,71)]:
    rng=random.Random(seed);rate=22050;last=0;data=[]
    for i in range(int(rate*duration)):
        t=i/rate;noise=rng.random()*2-1
        if name=='campus':
            last=last*.96+noise*.04;local=t%3
            bird=math.sin(2*math.pi*(1650*t+45*math.sin(t*20)))*math.sin(math.pi*local/.22)*.025 if local<.22 else 0
            # Fade loop seam; rustle envelope already returns to silence.
            value=(last*.15+bird)*min(1,t/.1,(duration-t)/.1)
        else:
            last=last*.65+noise*.35;value=last*math.sin(math.pi*t/duration)*(.25+.75*math.sin(t*19)**2)*.28
        data.append(int(max(-1,min(1,value))*32767))
    wav=Path(tempfile.gettempdir())/('kesho-bake-'+name+'.wav')
    with wave.open(str(wav),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes(struct.pack('<'+'h'*len(data),*data))
    subprocess.run(['ffmpeg','-y','-loglevel','error','-i',str(wav),'-c:a','libvorbis','-q:a','3',str(sound/(name+'.ogg'))],check=True)
    wav.unlink()
