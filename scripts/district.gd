extends Node3D
## Original procedural blockout assets; replace with licensed production art later.
signal arrived(place: String)
signal travel_failed
var reduced_motion := false
var camera: Camera3D
var avatar: Node3D
var marker: MeshInstance3D
var astar := AStarGrid2D.new()
var route: Array[Vector3] = []
var pending := ""
var paused := true
var walking := false
var elapsed := 0.0
var joystick := Vector2.ZERO
var locations := {
	"contact": {"point": Vector3(0, 0, 4), "title": "CITY CONTACT", "sub": "A familiar face", "colour": Color("e6b961")},
	"courtyard": {"point": Vector3(-7, 0, 0), "title": "COURTYARD", "sub": "A place to stay", "colour": Color("d29a78")},
	"work": {"point": Vector3(6, 0, -3), "title": "WORK YARD", "sub": "Earn your first pay", "colour": Color("8eaaa0")},
	"learning": {"point": Vector3(1, 0, -5), "title": "LEARNING CENTRE", "sub": "Find a way forward", "colour": Color("dfcfab")},
	"kiosk": {"point": Vector3(6, 0, 5), "title": "NURU'S KIOSK", "sub": "Food & conversation", "colour": Color("bb7857")}}
var obstacles: Array[Rect2i] = []
var npcs: Array[Node3D] = []
const SKINS := [Color("593c2b"), Color("80563d"), Color("a67450"), Color("c38e63")]
const OUTFITS := [Color("dca94e"), Color("436f69"), Color("ab6253"), Color("728599")]

func _ready() -> void:
	var world := WorldEnvironment.new()
	var env := Environment.new()
	env.background_mode = Environment.BG_COLOR
	env.background_color = Color("b5c4b9")
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color("e8d9b9")
	env.ambient_light_energy = 0.35
	env.tonemap_mode = Environment.TONE_MAPPER_LINEAR
	world.environment = env
	add_child(world)
	var sun := DirectionalLight3D.new()
	sun.rotation_degrees = Vector3(-48, -25, 0)
	sun.light_color = Color("ffdeb0")
	sun.light_energy = 0.85
	sun.shadow_enabled = true
	sun.directional_shadow_max_distance = 50
	add_child(sun)
	camera = Camera3D.new()
	camera.projection = Camera3D.PROJECTION_ORTHOGONAL
	camera.size = 24.5
	camera.position = Vector3(17, 21, 23)
	add_child(camera)
	camera.look_at(Vector3(0, 0, 0))
	box(Vector3(0, -0.45, 0), Vector3(28, .8, 24), Color("8e9276"))
	box(Vector3(0, -.025, 0), Vector3(26, .12, 20), Color("b7a78a"))
	box(Vector3(0, .05, 1.5), Vector3(23, .08, 4), Color("666b64"))
	box(Vector3(2.7, .05, 0), Vector3(3.4, .08, 19), Color("666b64"))
	for i in range(-5, 6): box(Vector3(i * 1.9, .10, 1.5), Vector3(.8, .015, .08), Color("d7cba9"))
	for i in range(5): box(Vector3(2.7, .11, -2.3 + i * .4), Vector3(2.7, .015, .2), Color("e2dac2"))
	building(Vector3(-8, 0, -4), Vector3(5, 4.6, 4.5), Color("b88062"), "MAKAZI • ROOMS")
	building(Vector3(-2, 0, -8), Vector3(5.4, 3.3, 3), Color("d9c7a0"), "LEARNING CENTRE")
	building(Vector3(8, 0, -7), Vector3(5.5, 3.6, 4.5), Color("77918a"), "DISPATCH / WORK")
	building(Vector3(-9, 0, 6), Vector3(3.7, 2.8, 3.3), Color("aa6c50"), "REPAIRS")
	building(Vector3(-3.3, 0, 8), Vector3(4, 3.8, 2.7), Color("c2ae8d"), "DUKA / GOODS")
	building(Vector3(9, 0, 7), Vector3(3.5, 2.4, 2.5), Color("bca37a"), "NURU • CHAI")
	for i in range(6):
		var x := -13.0 + i * 5.0
		box(Vector3(x, 1.6 + (i % 3), -15), Vector3(3.4, 3.2 + (i % 3) * 2, 3), Color("889b91"))
	# Kiosk canopy and stocked counter.
	box(Vector3(7, 2.0, 5.5), Vector3(3.5, .13, 2.3), Color("476b61"))
	for x in [5.5, 8.5]: box(Vector3(x, .95, 6.3), Vector3(.08, 1.9, .08), Color("735c42"))
	box(Vector3(7, .55, 5.5), Vector3(2.8, 1.1, .6), Color("845139"))
	for i in range(7): sphere(Vector3(6.1 + i * .28, 1.18, 5.5), Vector3(.15, .17, .15), Color("d69343"))
	# Clothesline, benches, planters, trees, transport and a distant city.
	box(Vector3(-7, 2.6, -1), Vector3(4, .035, .035), Color("3c4945"))
	for i in range(5): box(Vector3(-8.4 + i * .65, 2.15, -1), Vector3(.45, .75, .05), OUTFITS[i % 4])
	for pos in [Vector3(-5, 0, 4), Vector3(10, 0, -1), Vector3(-11, 0, -8), Vector3(10, 0, 3)]: tree(pos)
	for pos in [Vector3(-4, .3, -.7), Vector3(4.7, .3, 5.3)]:
		box(pos, Vector3(1.5, .15, .45), Color("78593e"))
		for dx in [-.55, .55]: box(pos + Vector3(dx, -.17, 0), Vector3(.1, .35, .4), Color("394c43"))
	van(Vector3(-4, 0, 1.2), Color("c9c6af"))
	van(Vector3(9, 0, -3), Color("d0ab4f"))
	for key in locations:
		var p: Vector3 = locations[key].point
		var ring := MeshInstance3D.new()
		var torus := TorusMesh.new()
		torus.inner_radius = .46; torus.outer_radius = .55
		ring.mesh = torus
		ring.position = p + Vector3(0, .12, 0)
		ring.material_override = material(locations[key].colour)
		add_child(ring)
		var npc := person(SKINS[npcs.size() % 4], OUTFITS[(npcs.size()+1) % 4])
		npc.position = p + Vector3(.8, 0, -.45)
		npc.rotation.y = -.6
		add_child(npc); npcs.append(npc)
		var label := Label3D.new()
		label.text = locations[key].title
		label.font_size = 48; label.pixel_size = .012
		label.position = p + Vector3(0, 2.8, 0)
		label.billboard = BaseMaterial3D.BILLBOARD_ENABLED
		label.modulate = Color("fff2d8")
		label.outline_modulate = Color("273c37")
		label.outline_size = 9
		add_child(label)
	astar.region = Rect2i(-12, -10, 25, 21)
	astar.cell_size = Vector2.ONE
	astar.diagonal_mode = AStarGrid2D.DIAGONAL_MODE_ONLY_IF_NO_OBSTACLES
	astar.update()
	for rect in obstacles:
		for x in range(rect.position.x, rect.end.x):
			for y in range(rect.position.y, rect.end.y):
				if astar.is_in_boundsv(Vector2i(x,y)): astar.set_point_solid(Vector2i(x,y))
	marker = MeshInstance3D.new()
	var m := TorusMesh.new(); m.inner_radius = .15; m.outer_radius = .24
	marker.mesh = m; marker.material_override = material(Color("edc16c"))
	marker.visible = false
	add_child(marker)
	set_avatar(0,0)

func material(colour: Color) -> StandardMaterial3D:
	var mat := StandardMaterial3D.new(); mat.albedo_color = colour; mat.roughness = .95
	return mat

func box(pos: Vector3, size: Vector3, colour: Color, parent: Node3D = self) -> MeshInstance3D:
	var mesh := BoxMesh.new(); mesh.size = size
	var obj := MeshInstance3D.new(); obj.mesh = mesh; obj.material_override = material(colour); obj.position = pos
	parent.add_child(obj)
	return obj

func sphere(pos: Vector3, size: Vector3, colour: Color, parent: Node3D = self) -> MeshInstance3D:
	var obj := MeshInstance3D.new(); var mesh := SphereMesh.new()
	mesh.radius = .5; mesh.height = 1; mesh.radial_segments = 12; mesh.rings = 6
	obj.mesh = mesh; obj.scale = size; obj.position = pos; obj.material_override = material(colour)
	parent.add_child(obj)
	return obj

func building(pos: Vector3, size: Vector3, colour: Color, title: String) -> void:
	box(pos + Vector3(0, size.y/2, 0), size, colour)
	box(pos + Vector3(0, size.y + .1, 0), Vector3(size.x + .25, .22, size.z + .3), Color("606e65"))
	box(pos + Vector3(0, .9, size.z/2 + .02), Vector3(.85, 1.8, .08), Color("3d4c46"))
	for x in [-size.x*.32, size.x*.32]:
		box(pos + Vector3(x, 1.8, size.z/2 + .04), Vector3(.8, 1.0, .08), Color("3f6564"))
		box(pos + Vector3(x, 1.8, size.z/2 + .1), Vector3(.05, 1.0, .03), Color("c6bb9b"))
	var sign := Label3D.new(); sign.text = title; sign.font_size = 42; sign.pixel_size = .006
	sign.position = pos + Vector3(0, 2.7, size.z/2 + .1); sign.modulate = Color("fff2d8")
	add_child(sign)
	obstacles.append(Rect2i(Vector2i(floori(pos.x-size.x/2), floori(pos.z-size.z/2)), Vector2i(ceili(size.x)+1, ceili(size.z)+1)))

func tree(pos: Vector3) -> void:
	box(pos+Vector3(0,1.2,0),Vector3(.22,2.4,.22),Color("6d5c41"))
	sphere(pos+Vector3(0,2.5,0),Vector3(2.7,1.6,2.4),Color("617d54"))
	sphere(pos+Vector3(.6,2.9,.1),Vector3(1.6,1.1,1.5),Color("81915b"))

func van(pos: Vector3, colour: Color) -> void:
	box(pos+Vector3(0,.8,0),Vector3(2.8,1.35,1.25),colour)
	box(pos+Vector3(.7,1.2,.65),Vector3(.65,.5,.04),Color("3b6264"))
	box(pos+Vector3(-.5,1.2,.65),Vector3(1.3,.5,.04),Color("3b6264"))
	box(pos+Vector3(0,.55,.65),Vector3(2.8,.12,.04),Color("b96f43"))
	for x in [-.9,.9]:
		for z in [-.65,.65]: sphere(pos+Vector3(x,.35,z),Vector3(.55,.55,.2),Color("293a37"))

func person(skin: Color, outfit: Color) -> Node3D:
	var person_node := Node3D.new()
	box(Vector3(0,.94,0),Vector3(.44,.64,.28),outfit,person_node)
	sphere(Vector3(0,1.53,0),Vector3(.38,.45,.36),skin,person_node)
	sphere(Vector3(0,1.69,-.04),Vector3(.39,.2,.34),Color("242c27"),person_node)
	for side in [-1,1]:
		var leg := box(Vector3(side*.13,.33,0),Vector3(.18,.62,.2),Color("35474a"),person_node)
		leg.name = "LegL" if side == -1 else "LegR"
		var arm := box(Vector3(side*.32,.92,0),Vector3(.14,.65,.18),skin,person_node)
		arm.name = "ArmL" if side == -1 else "ArmR"
	box(Vector3(0,1,-.22),Vector3(.35,.45,.17),Color("7b5139"),person_node)
	return person_node

func set_avatar(skin: int, outfit: int) -> void:
	var pos := Vector3(0,0,5.5)
	if is_instance_valid(avatar): pos=avatar.position; avatar.queue_free()
	avatar = person(SKINS[clampi(skin,0,3)],OUTFITS[clampi(outfit,0,3)])
	avatar.position=pos; add_child(avatar)
	var identity_ring := MeshInstance3D.new()
	var ring_mesh := TorusMesh.new(); ring_mesh.inner_radius=.32; ring_mesh.outer_radius=.38
	identity_ring.mesh=ring_mesh; identity_ring.material_override=material(Color("e6b961")); identity_ring.position.y=.08; avatar.add_child(identity_ring)

func go_to(place: String) -> void:
	if paused or not locations.has(place): return
	pending=place
	walk_to(locations[place].point)

func cell(pos: Vector3) -> Vector2i:
	return Vector2i(roundi(pos.x), roundi(pos.z))

func walk_to(pos: Vector3) -> void:
	var end := cell(pos)
	if not astar.is_in_boundsv(end) or astar.is_point_solid(end):
		pending=""; travel_failed.emit(); return
	var start := cell(avatar.position)
	if not astar.is_in_boundsv(start) or astar.is_point_solid(start):
		pending=""; travel_failed.emit(); return
	route.clear()
	for point in astar.get_id_path(start,end): route.append(Vector3(point.x,0,point.y))
	marker.position=Vector3(end.x,.13,end.y); marker.visible=not route.is_empty()

func _unhandled_input(event: InputEvent) -> void:
	if paused: return
	# GUI and world share the primary emulated mouse. Never process raw touch twice.
	var pressed := false
	var screen := Vector2.ZERO
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT: pressed=true; screen=event.position
	if not pressed: return
	var origin := camera.project_ray_origin(screen)
	var direction := camera.project_ray_normal(screen)
	if absf(direction.y) < .001: return
	var hit := origin + direction * (-origin.y/direction.y)
	pending=""
	for key in locations:
		if hit.distance_to(locations[key].point) < 1.8: pending=key; hit=locations[key].point; break
	walk_to(hit)

func _process(delta: float) -> void:
	elapsed += delta
	if is_instance_valid(marker) and marker.visible and not reduced_motion: marker.scale=Vector3.ONE*(1.0+.08*sin(elapsed*5))
	if paused: return
	var input := Input.get_vector("ui_left","ui_right","ui_up","ui_down") + joystick
	if input.length()>.1:
		pending=""; route.clear(); marker.visible=false
		var dir := camera.global_basis.x * input.x + Vector3(camera.global_basis.z.x,0,camera.global_basis.z.z).normalized()*input.y
		dir.y=0; dir=dir.normalized()
		var next := avatar.position + dir * delta * 3
		if astar.is_in_boundsv(cell(next)) and not astar.is_point_solid(cell(next)): avatar.position=next
		avatar.rotation.y=atan2(dir.x,dir.z); walking=true
	elif not route.is_empty():
		var target: Vector3=route[0]
		var diff:=target-avatar.position
		if diff.length()<.08: route.pop_front()
		else:
			avatar.position=avatar.position.move_toward(target,delta*3.5)
			avatar.rotation.y=lerp_angle(avatar.rotation.y,atan2(diff.x,diff.z),delta*10)
		walking=true
	else:
		walking=false; marker.visible=false
		if not pending.is_empty():
			var dest:=pending; pending=""; arrived.emit(dest)
	if is_instance_valid(avatar):
		avatar.get_node("LegL").rotation.x=sin(elapsed*11)*.4 if walking else 0.0
		avatar.get_node("LegR").rotation.x=-sin(elapsed*11)*.4 if walking else 0.0
		avatar.get_node("ArmL").rotation.x=-sin(elapsed*11)*.3 if walking else 0.0
		avatar.get_node("ArmR").rotation.x=sin(elapsed*11)*.3 if walking else 0.0
