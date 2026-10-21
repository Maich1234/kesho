"""Original Kesho score and sound design. No samples or third-party recordings.
Regenerate with Python 3 + numpy/scipy and ffmpeg. Output deterministic stereo Ogg.
"""
from pathlib import Path
import numpy as np
from scipy.signal import butter, sosfilt
import wave, subprocess, tempfile

ROOT = Path(__file__).resolve().parents[1] / 'assets/audio'
RATE = 22050
RNG = np.random.default_rng(10422)
ROOT.mkdir(parents=True, exist_ok=True)

def note(midi): return 440 * 2 ** ((midi - 69) / 12)

def write(name, audio):
    if audio.ndim == 1: audio = np.column_stack([audio, audio])
    # Avoid clipping, retain quiet dynamics. Loops are circularly mixed below.
    peak = np.max(np.abs(audio))
    if peak > .85: audio = audio * (.85 / peak)
    pcm = np.round(audio * 32767).astype('<i2')
    with tempfile.NamedTemporaryFile(suffix='.wav') as f:
        with wave.open(f.name,'wb') as w:
            w.setnchannels(2); w.setsampwidth(2); w.setframerate(RATE); w.writeframes(pcm.tobytes())
        subprocess.run(['ffmpeg','-y','-loglevel','error','-i',f.name,'-c:a','libvorbis','-q:a','4',str(ROOT/(name+'.ogg'))], check=True)
    print(name, round(len(audio)/RATE,3), 'seconds', 'peak', round(float(np.max(np.abs(audio))),3))

def add(track, sound, at, gain=1, pan=0):
    start = round(at*RATE)
    idx=(np.arange(len(sound))+start)%len(track)
    np.add.at(track[:,0],idx,sound*gain*np.sqrt((1-pan)/2))
    np.add.at(track[:,1],idx,sound*gain*np.sqrt((1+pan)/2))

def pluck(midi, duration=2.2):
    t=np.arange(round(duration*RATE))/RATE; f=note(midi)
    attack=np.minimum(t/.004,1)
    return attack*(np.sin(2*np.pi*f*t)*np.exp(-2.8*t)+.25*np.sin(2*np.pi*f*2.003*t)*np.exp(-5*t)+.08*np.sin(2*np.pi*f*4.01*t)*np.exp(-9*t))

beat=60/88; bars=16; duration=bars*4*beat
music=np.zeros((round(duration*RATE),2))
chords=[[50,57,62,65,69],[46,53,58,62,65],[41,53,57,60,65],[48,55,60,62,67]]
pattern=[(0,2),(.75,4),(1.5,3),(2.5,1),(3.25,2)]
for bar in range(bars):
    chord=chords[(bar//2)%4]; origin=bar*4*beat
    for offset,index in pattern:
        add(music,pluck(chord[index]+12),origin+offset*beat,.105,pan=(-.3 if index%2 else .3))
    for offset in [0,2]: add(music,pluck(chord[0]-12,3),origin+offset*beat,.1)
    # Soft frame-drum pulse and sparse brushed shaker, beneath the plucked motif.
    for offset in [0,2.5]:
        t=np.arange(int(.24*RATE))/RATE
        drum=np.sin(2*np.pi*(80*t+25*.035*(1-np.exp(-t/.035))))*np.exp(-t*24)
        add(music,drum,origin+offset*beat,.055)
    for offset in [.5,1.5,2.25,3.5]:
        t=np.arange(int(.07*RATE))/RATE
        noise=RNG.normal(0,1,len(t)); noise=np.concatenate(([0],np.diff(noise)))
        add(music,noise*np.sin(np.pi*np.minimum(t/.07,1))**2*np.exp(-t*35),origin+offset*beat,.012,pan=.45)
# A slowly moving, seamless harmonic bed. Each note wraps its release into the loop.
for block in range(8):
    chord=chords[block%4]; length=8*beat+2
    t=np.arange(round(length*RATE))/RATE
    envelope=np.minimum(t/1.3,1)*np.minimum((length-t)/1.8,1)
    for midi in chord[1:4]:
        sound=np.sin(2*np.pi*note(midi)*t)*envelope
        add(music,sound,block*8*beat,.025,pan=.15)
write('maweni_first_light',music)

# Circular low-pass air, a distant motor-like rumble and occasional bird-like tones.
length=round(16*RATE)
noise=RNG.normal(0,1,length*3)
wind=sosfilt(butter(2,350,fs=RATE,output='sos'),noise)[length:2*length]
t=np.arange(length)/RATE
ambient=np.column_stack([wind, np.roll(wind,1500)])*.045
ambient += (.006*np.sin(2*np.pi*55*t)*(1+.4*np.sin(2*np.pi*t/16)))[:,None]
for at,freq in [(2.4,1800),(7.1,2100),(11.3,1700)]:
    ts=np.arange(int(.35*RATE))/RATE
    chirp=np.sin(2*np.pi*(freq*ts+300*ts*ts))*np.sin(np.pi*ts/.35)**2
    add(ambient,chirp,at,.012,pan=-.5)
# 100 ms boundary crossfade for the nonperiodic noise layer.
n=int(.1*RATE); blend=np.linspace(0,1,n)[:,None]
shared=ambient[:n]*blend+ambient[-n:]*(1-blend)
ambient[:n]=shared; ambient[-n:]=shared
write('maweni_air',ambient)

for name,notes,duration,gain in [('press',[420],.05,.12),('click',[650,980],.11,.15),('open',[392,587],.19,.13),('close',[587,392],.14,.1),('success',[523,659,784],.36,.17),('deny',[190,165],.18,.12)]:
    t=np.arange(round(duration*RATE))/RATE
    sound=np.zeros(len(t))
    for index,freq in enumerate(notes):
        start=index*duration/(len(notes)+1); local=t-start
        sound += np.where(local>=0,np.sin(2*np.pi*freq*np.maximum(local,0))*np.exp(-np.maximum(local,0)*38),0)
    sound*=np.minimum(t/.003,1)*np.minimum((duration-t)/.015,1)*gain
    write(name,sound)
t=np.arange(int(.09*RATE))/RATE
step=sosfilt(butter(2,1300,fs=RATE,output='sos'),RNG.normal(0,1,len(t)))*np.exp(-t*55)*np.minimum(t/.003,1)*.15
write('step',step)
