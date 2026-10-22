"""Reproduce source-level presentation findings. Does not run/render/profile Unity."""
import json
import math
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[2]
actor = (ROOT / 'unity/Assets/Kesho/Runtime/ComicActor.cs').read_text()
stage = (ROOT / 'unity/Assets/Kesho/Runtime/Stage.cs').read_text()
mesh = (ROOT / 'unity/Assets/Kesho/Runtime/ComicMesh.cs').read_text()

# Read the actual leg dimensions; both parts use the shared radius-0.5 rounded mesh.
sleeve = re.search(r'P\(t,"Sleeve",new Vector3\(0,(-?[\d.]+)f,0\),new Vector3\(arm\?[\d.]+f:[\d.]+f,([\d.]+)f', actor)
shoe = re.search(r'P\(t,arm\?"Hand":"Shoe",new Vector3\(0,arm\?(-?[\d.]+)f:(-?[\d.]+)f,.*?new Vector3\([\d.]+f,arm\?[\d.]+f:([\d.]+)f', actor)
assert sleeve and shoe, 'Recheck probe after actor source changes'
leg_bottom = float(sleeve[1]) - float(sleeve[2]) / 2
shoe_top = float(shoe[2]) + float(shoe[3]) / 2

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
assert all(abs(x) <= 10 and -7 <= z <= 7 and (x, z) not in blocked for x, z in crossing)

result = {
    'source_commit': subprocess.check_output(['git', 'log', '-1', '--format=%H', '--', 'unity/Assets/Kesho/Runtime/ComicActor.cs', 'unity/Assets/Kesho/Runtime/Stage.cs', 'unity/Assets/Kesho/Runtime/ComicMesh.cs'], cwd=ROOT, text=True).strip(),
    'method': 'source arithmetic and static construction estimate; no Unity runtime or device measurements',
    'leg_local_bottom_y': round(leg_bottom, 3),
    'shoe_local_top_y': round(shoe_top, 3),
    'gap_between_leg_and_shoe_units': round(leg_bottom-shoe_top, 3),
    'campus_default_actor_count': len(people),
    'character_mesh_parts_by_actor': parts,
    'estimated_default_character_mesh_renderer_count': sum(parts.values()),
    'submitted_triangles_per_round_mesh_including_degenerates': triangles_per_round,
    'estimated_default_character_triangles_including_shadows_and_degenerates': sum(parts.values()) * triangles_per_round,
    'walkable_example_through_kito_actor_cell': crossing,
    'camera_instant_size_values': {'exploration': 9.7, 'conversation': 5.7, 'self_or_phone': 4.4, 'character_preview': 3.4},
    'steady_idle_music_level_at_default_master_volume': round(.65 * .17, 4),
    'music_fade_seconds_from_steady_idle_to_zero_at_rate_0_3': round(.65 * .17 / .3, 3),
    'performance_measurements': {'fps': None, 'frame_times': None, 'draw_calls': None, 'memory': None, 'thermal': None}
}
print(json.dumps(result, indent=2))
