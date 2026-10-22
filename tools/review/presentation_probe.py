"""Reproduce source-level presentation findings. Does not run/render/profile Unity."""
import json
import math
from pathlib import Path
import re
import hashlib

ROOT = Path(__file__).resolve().parents[2]
actor = (ROOT / 'unity/Assets/Kesho/Runtime/ComicActor.cs').read_text()
stage = (ROOT / 'unity/Assets/Kesho/Runtime/Stage.cs').read_text()
mesh = (ROOT / 'unity/Assets/Kesho/Runtime/ComicMesh.cs').read_text()

# Read leg dimensions from the repaired arm/leg conditional dimensions.
sleeve = re.search(r'P\(t,"Sleeve",new Vector3\(0,arm\?(-?[\d.]+)f:(-?[\d.]+)f,0\),new Vector3\(arm\?[\d.]+f:[\d.]+f,arm\?([\d.]+)f:([\d.]+)f', actor)
shoe = re.search(r'P\(t,arm\?"Hand":"Shoe",new Vector3\(0,arm\?(-?[\d.]+)f:(-?[\d.]+)f,.*?new Vector3\([\d.]+f,arm\?[\d.]+f:([\d.]+)f', actor)
assert sleeve and shoe, 'Recheck probe after actor source changes'
leg_bottom = float(sleeve[2]) - float(sleeve[4]) / 2
shoe_top = float(shoe[2]) + float(shoe[3]) / 2
assert leg_bottom <= shoe_top, 'Leg/shoe continuity regression'

# Count authored character mesh parts, including each instance in the ear/eye loop.
build = actor[actor.index('public void Build'):actor.index('Transform Joint')]
face_loop = build[build.index('for(int side='):build.index('P(head,"Nose"')]
assert 'side<=1;side+=2' in face_loop
body_parts = len(re.findall(r'\bP\(', build)) + len(re.findall(r'\bP\(', face_loop)) + 2 * len(re.findall(r'=Limb\(', build))
hair_parts = {0: 1, 1: 2, 2: 8, 3: 10, 4: 2}
assert 'i<7' in actor and 'i<9' in actor
campus = stage[stage.index('void Campus()'):stage.index('void Courtyard()')]
static_cast = [(m[1], int(m[2])) for m in re.finditer(r'Person\("([^"]+)",new Vector3\([^)]*\),\d+,\d+,(\d+)\)', campus)]
assert len(static_cast) == 5 and 'Person("Queued student "+i' in campus
people = [(f'Queued student {i}', i) for i in range(3)] + static_cast + [('Player', 0)]
parts = {name: body_parts + hair_parts[hair] + 1 for name, hair in people}  # + grounding shadow
rings, sides = map(int, re.search(r'const int rings=(\d+), sides=(\d+)', mesh).groups())
triangles_per_round = rings * sides * 2  # Includes the existing degenerate cap triangles.

# Reproduce Stage.Box obstacle quantisation for the current campus's obstacle helpers.
# This explicit extraction is limited to the campus; it is not a Unity navigation test.
blocked = set()
def box(x, z, width, depth):
    for cx in range(math.floor(x-width/2-.25), math.ceil(x+width/2+.25)+1):
        for cz in range(math.floor(z-depth/2-.25), math.ceil(z+depth/2+.25)+1):
            blocked.add((cx, cz))
box(1, -7, 2.8, 1.2)  # Registration desk
box(6, -6.7, 2.8, 1.2)  # Support desk
box(8, 2, 2.4, 2)  # Kiosk
for x in [-9, 9]:
    box(x, -5, 1.6, 1.6)  # Planter
for x in [-8, 8]:
    box(x, 5, 2.2, .7)  # Bench
crossing = [(3, 3), (4, 3), (5, 3)]
# Read static actor positions plus the authored queue's three positions.
occupied = {(round(float(m[1])), round(float(m[2]))) for m in re.finditer(r'Person\("[^"]+",new Vector3\((-?[\d.]+)f?,0,(-?[\d.]+)f?\)', campus)}
occupied.update((round(-1.6-i*.65), round(-5.4+i*.6)) for i in range(3))
assert (4,3) in occupied and '!occupied.Contains(cell)' in stage
assert 'CombineRigid(joint)' in actor and 'CombineRigid(hair)' in actor
# Rigid material batches: torso 4, head 3, arms 2 each, legs 2 each, hair 1, ground shadow 1.
# This remains an authored estimate, not Frame Debugger output.
batched_renderers_per_actor = 4+3+2*2+2*2+1+1
assert 'View.orthographicSize=Mathf.Lerp' in stage
assert not re.search(r'View\.orthographicSize=(3.4|4.4|5.7)f',stage)
# Verify every authored campus approach remains connected to spawn after reserving people.
from collections import deque
reachable={(0,4)}; frontier=deque(reachable)
while frontier:
    x,z=frontier.popleft()
    for dx,dz in [(0,1),(1,0),(0,-1),(-1,0)]:
        nxt=(x+dx,z+dz)
        if abs(nxt[0])<=10 and -7<=nxt[1]<=7 and nxt not in blocked and nxt not in occupied and nxt not in reachable:
            reachable.add(nxt); frontier.append(nxt)
approaches={m[1]:(round(float(m[2])),round(float(m[3]))) for m in re.finditer(r'Target\("([^"]+)",new Vector3\((-?[\d.]+)f?,0,(-?[\d.]+)f?\)',campus)}
assert approaches and all(cell in reachable for cell in approaches.values()), 'NPC reservations blocked a story destination'
audio=(ROOT/'unity/Assets/Kesho/Runtime/Soundscape.cs').read_text()
assert 'music.mute=ambience.mute=voice.mute=Volume==0' in audio
assert 'ProceduralFoley.' not in audio


result = {
    'source_sha256': {str(path.relative_to(ROOT)): hashlib.sha256(path.read_bytes()).hexdigest()
        for path in [ROOT/'unity/Assets/Kesho/Runtime'/name for name in ['ComicActor.cs','Stage.cs','ComicMesh.cs','Soundscape.cs']]},
    'method': 'source arithmetic and static construction estimate; no Unity runtime or device measurements',
    'leg_local_bottom_y': round(leg_bottom, 3),
    'shoe_local_top_y': round(shoe_top, 3),
    'gap_between_leg_and_shoe_units': round(max(0,leg_bottom-shoe_top), 3),
    'leg_shoe_overlap_units': round(shoe_top-leg_bottom,3),
    'campus_default_actor_count': len(people),
    'authored_mesh_parts_by_actor_before_rigid_batching': parts,
    'estimated_default_character_mesh_renderer_count_before_rigid_batching': sum(parts.values()),
    'estimated_default_character_mesh_renderer_count_after_rigid_batching': len(people)*batched_renderers_per_actor,
    'submitted_triangles_per_round_mesh_including_degenerates': triangles_per_round,
    'estimated_default_character_triangles_including_shadows_and_degenerates': sum(parts.values()) * triangles_per_round,
    'all_campus_approach_points_reachable': True,
    'campus_approach_point_count': len(approaches),
    'previous_crossing_route': crossing,
    'previous_crossing_route_is_now_walkable': all((x,z) not in occupied and (x,z) not in blocked for x,z in crossing),
    'camera_blended_target_sizes': {'exploration': 9.7, 'conversation': 5.7, 'self_or_phone': 4.4, 'character_preview': 3.4},
    'steady_idle_music_level_at_default_master_volume': round(.65 * .17, 4),
    'mute_is_immediate_at_source': True,
    'startup_synthesized_samples': 0,
    'performance_measurements': {'fps': None, 'frame_times': None, 'draw_calls': None, 'memory': None, 'thermal': None}
}
print(json.dumps(result, indent=2))
