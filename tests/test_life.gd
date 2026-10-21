extends SceneTree
var failures: Array[String] = []
var checks := 0
var life: Node
var story: Dictionary

func check(value: bool, message: String) -> void:
	checks+=1
	if not value: failures.append(message); printerr("FAIL: "+message)

func _initialize() -> void:
	call_deferred("run")

func find_choice(place: String, id: String) -> Dictionary:
	for choice in story[place].choices:
		if choice.id==id: return choice
	return {}

func run() -> void:
	life=load("res://scripts/life.gd").new(); life.save_enabled=false; root.add_child(life)
	story=JSON.parse_string(FileAccess.get_file_as_string("res://data/opening.json"))
	life.fresh("Test","Family household",0,0,"Checkpoints")
	check(not life.next_day().ok,"No ending day without shelter")
	check(not life.available(find_choice("learning","apply")),"Official application needs notice")
	life.apply(find_choice("contact","meet"),"contact")
	check(not life.available(find_choice("contact","meet")),"Contact reward cannot repeat")
	life.apply(find_choice("courtyard","relative_bed"),"courtyard")
	check(life.state.money==1050,"Shelter debits once")
	check(not life.available(find_choice("courtyard","relative_bed")),"Shelter cannot be charged twice")
	life.apply(find_choice("work","shift"),"work")
	check(life.state.money==1410 and life.state.hour==15,"Work advances time and pays correctly")
	check(not life.available(find_choice("work","shift")),"One-off shift cannot be farmed")
	life.state.hour=21
	check(not life.available(find_choice("learning","campus")),"Late actions cannot exceed closing time")
	life.state.hour=10; life.state.money=50
	check(not life.available(find_choice("learning","broker")),"Insufficient funds cannot be spent")
	life.state.money=1000; life.state.energy=0
	check(not life.available(find_choice("work","delivery")),"No effort without energy")
	life.next_day(); check(life.state.day==2 and life.state.energy==90,"Rest advances day and restores energy")
	# Restoring a pre-choice RNG checkpoint must resolve the same choice identically.
	life.rng.seed=98765
	var before: Dictionary=life.state.duplicate(true)
	var random_state: int=life.rng.state
	var a: Dictionary=life.apply(find_choice("work","shortcut"),"work")
	life.state=before.duplicate(true); life.rng.state=random_state
	var b: Dictionary=life.apply(find_choice("work","shortcut"),"work")
	check(a.text==b.text,"Checkpoint RNG cannot be rerolled by reload")
	var gains: Dictionary={}
	for seed_value in range(100):
		life.fresh("Test","Family household",0,0,"Checkpoints"); life.rng.seed=seed_value
		var result: Dictionary=life.apply(find_choice("work","shortcut"),"work")
		gains[result.text]=true
		check(life.state.money>=0 and life.state.hour<=22,"Random run valid %d" % seed_value)
	check(gains.size()==2,"Different life seeds produce both errand outcomes")
	check(not life.valid_state({"version":1}),"Malformed saves rejected")
	var invalid: Dictionary=life.state.duplicate(true); invalid.notes=12
	check(not life.valid_state(invalid),"Invalid journal type rejected")
	invalid=life.state.duplicate(true); invalid.background="Missing background"
	check(not life.valid_state(invalid),"Unknown background rejected")
	invalid=life.state.duplicate(true); invalid.position=["broken",0]
	check(not life.valid_state(invalid),"Invalid coordinates rejected")
	var ids: Dictionary={}
	for scene in story.values():
		for choice in scene.choices:
			check(not ids.has(choice.id),"Unique decision ID: "+choice.id)
			ids[choice.id]=true
			var total:=0.0
			for outcome in choice.get("outcomes",[]): total+=float(outcome.weight)
			if choice.has("outcomes"): check(is_equal_approx(total,1.0),"Normalised outcome weights: "+choice.id)
	check(not life.snapshot().has("notes") and not life.snapshot().has("history"),"Public summary excludes private journal and choices")
	# Exercise every background, with both work and education routes.
	for bg in ["Family household","Independent parent","Community upbringing"]:
		life.fresh("Test",bg,0,0,"Checkpoints")
		life.apply(find_choice("contact","meet"),"contact")
		var shelter_id: String={"Family household":"relative_bed","Independent parent":"friend_bed","Community upbringing":"community_bed"}[bg]
		life.apply(find_choice("courtyard",shelter_id),"courtyard")
		life.apply(find_choice("courtyard","family_talk"),"courtyard")
		life.apply(find_choice("learning","official"),"learning")
		var waiver: Dictionary=life.apply(find_choice("learning","waiver"),"learning")
		check(waiver.ok and life.has_flag("training_application"),"Waiver route reachable: "+bg)
		life.next_day(); life.next_day(); life.next_day()
		check(life.state.complete,"Chapter completes: "+bg)
		check(not life.available(find_choice("work","shift")),"Completed chapter cannot be mutated")
	# File roundtrip + backup recovery. Isolated by test invocation's XDG_DATA_HOME.
	life.fresh("Roundtrip","Family household",1,2,"Checkpoints")
	life.save_enabled=true
	check(life.commit(),"Atomic primary save succeeds")
	life.state.money=777; check(life.commit(),"Backup save succeeds")
	life.state={}; check(life.load_life() and life.state.money==777,"Primary roundtrip")
	var file:=FileAccess.open(life.SAVE,FileAccess.WRITE); file.store_string("{broken"); file.close()
	check(life.load_life() and life.state.money==1200,"Corrupt primary recovers backup")
	check(life.last_error.contains("backup"),"Backup recovery reported")
	# Restore a valid primary for a clean test directory.
	life.commit()
	var district=load("res://scripts/district.gd").new(); root.add_child(district)
	await process_frame
	for key in district.locations:
		var path: Array=district.astar.get_id_path(Vector2i(0,6),district.cell(district.locations[key].point))
		check(not path.is_empty(),"Destination reachable: "+key)
	for a_key in district.locations:
		for b_key in district.locations:
			var path: Array=district.astar.get_id_path(district.cell(district.locations[a_key].point),district.cell(district.locations[b_key].point))
			check(not path.is_empty(),"Connected destinations: "+a_key+" to "+b_key)
	print("KESHO TESTS: %d checks, %d failures" % [checks,failures.size()])
	district.queue_free(); life.queue_free(); await process_frame
	quit(0 if failures.is_empty() else 1)
