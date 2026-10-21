extends SceneTree
## Real finger events go through Input and Godot GUI dispatch, never pressed.emit().
var game: Node
var checks := 0
var failures: Array[String] = []
var cues: Array[String] = []

func _initialize() -> void: call_deferred("run")
func check(value: bool, message: String) -> void:
	checks+=1
	if not value: failures.append(message); printerr("FAIL: "+message); quit(1)
func scope() -> Node:
	return game.modal if is_instance_valid(game.modal) else game.ui
func find_button(value: String) -> BaseButton:
	for b in scope().find_children("*","BaseButton",true,false):
		if b.text==value and b.is_visible_in_tree(): return b
	return null
func text_exists(value: String) -> bool:
	for label in scope().find_children("*","Label",true,false):
		if value in label.text: return true
	return false
func reveal(control: Control) -> void:
	var node:=control.get_parent()
	while is_instance_valid(node):
		if node is ScrollContainer: node.ensure_control_visible(control); break
		node=node.get_parent()
	await process_frame; await process_frame
func finger(position: Vector2, down: bool, index: int = 0) -> void:
	var event:=InputEventScreenTouch.new(); event.index=index; event.pressed=down; event.position=position
	Input.parse_input_event(event)
func tap(control: Control) -> bool:
	if not is_instance_valid(control): check(false,"Requested button exists"); return false
	await reveal(control)
	var p:=control.get_global_rect().get_center()
	finger(p,true); await process_frame
	finger(p,false); await process_frame; await process_frame
	await create_timer(.19).timeout
	return true
func wait_for_modal() -> bool:
	var deadline:=Time.get_ticks_msec()+8000
	while Time.get_ticks_msec()<deadline:
		if is_instance_valid(game.modal): return true
		await process_frame
	return false
func run() -> void:
	Engine.max_fps=120
	root.size=Vector2i(1280,720)
	game=load("res://scenes/main.tscn").instantiate(); root.add_child(game)
	await process_frame; await process_frame
	var sound:=root.get_node("Sound")
	var life:=root.get_node("Life")
	sound.cue_played.connect(func(cue: String): cues.append(cue))
	check(Input.emulate_mouse_from_touch,"Native GUI receives finger taps")
	check(not Input.emulate_touch_from_mouse,"Mouse events do not generate duplicate fingers")
	check(sound.music.stream!=null and sound.music.stream.get_length()>40,"Original score loaded")
	check(sound.ambience.stream!=null and sound.ambience.stream.get_length()>15,"Ambience loaded")
	check(sound.music.stream.loop and sound.ambience.stream.loop,"Background audio loops")
	check(sound.voices.size()==6,"UI sound voices are bounded")
	var capture:=AudioEffectCapture.new(); AudioServer.add_bus_effect(0,capture)
	sound.play("click"); await create_timer(.15).timeout
	var mixed:=capture.get_buffer(capture.get_frames_available())
	var peak:=0.0
	for sample in mixed: peak=maxf(peak,maxf(absf(sample.x),absf(sample.y)))
	check(peak>.0001,"Audio mixer emits audible nonzero samples")
	check(find_button("Continue saved life").disabled,"No-save resume is visibly disabled")
	check(text_exists("No saved life yet"),"No-save state explains next action")
	check(game.world.paused,"Title does not advance gameplay")
	await tap(find_button("Begin a life"))
	check(is_instance_valid(game.modal) and text_exists("The city does not know you"),"Finger tap opens character creation")
	check(cues.has("press") and cues.has("click"),"Touch produces press and click feedback")
	var options:=scope().find_children("*","OptionButton",true,false)
	await tap(options[0])
	check(options[0].get_popup().visible,"Touch opens character selector")
	options[0].get_popup().hide()
	await tap(find_button("Back"))
	check(not is_instance_valid(game.modal) and game.world.paused,"Creator Back returns to paused title")
	await tap(find_button("Sound & controls"))
	check(text_exists("Sound & controls"),"Title settings open by touch")
	await tap(find_button("Mute all sound"))
	check(sound.prefs.muted,"Mute responds to touch")
	check(AudioServer.is_bus_mute(AudioServer.get_bus_index("Music")),"Mute silences music bus")
	check(AudioServer.is_bus_mute(AudioServer.get_bus_index("Effects")),"Mute silences effect bus")
	check(AudioServer.is_bus_mute(AudioServer.get_bus_index("Ambience")),"Mute silences ambience bus")
	await create_timer(.1).timeout; capture.clear_buffer(); await create_timer(.08).timeout
	var muted_samples:=capture.get_buffer(capture.get_frames_available())
	var muted_peak:=0.0
	for sample in muted_samples: muted_peak=maxf(muted_peak,maxf(absf(sample.x),absf(sample.y)))
	check(muted_peak<.0001,"Mute produces silence at the master mixer")
	await tap(find_button("Mute all sound"))
	check(not sound.prefs.muted,"Unmute responds to touch")
	var sliders:=scope().find_children("*","HSlider",true,false)
	await tap(sliders[0])
	check(float(sound.prefs.music)>0,"Music slider accepts touch")
	await tap(find_button("Reduce motion"))
	check(game.reduce_motion and game.world.reduced_motion,"Reduce motion reaches UI and world")
	await tap(find_button("Test confirmation"))
	check(cues.has("success"),"Audition triggers confirmation cue")
	var config:=ConfigFile.new(); var prefs_loaded:=config.load(sound.PREFS)==OK
	check(prefs_loaded and config.get_value("preferences","reduced_motion",false),"Preferences persist separately from life save")
	await tap(find_button("Done"))
	check(not is_instance_valid(game.modal) and game.world.paused,"Title settings return without unpausing world")
	await tap(find_button("About Kesho"))
	check(text_exists("A life before the headlines"),"About opens by touch")
	await tap(find_button("Third-party licences"))
	check(text_exists("CREDITS & LICENCES"),"Credits open by touch")
	game.navigate_back(); await process_frame; await process_frame
	check(text_exists("A life before the headlines"),"Back returns from credits to About")
	await tap(find_button("Back"))
	await tap(find_button("Begin a life"))
	await tap(find_button("Arrive in Maweni"))
	check(not life.state.is_empty() and life.state.name=="Amani","Touch commits character creation")
	check(text_exists("A place before dark"),"Arrival introduction opens")
	await tap(find_button("Step into the city"))
	check(not is_instance_valid(game.modal) and not game.world.paused,"Touch enters city")
	await tap(find_button("Contact"))
	check(await wait_for_modal(),"Touch destination walks to NPC and opens conversation")
	check(text_exists("Someone knows your name"),"Correct conversation opens")
	check(sound.scene_context=="dialogue","Dialogue ducks score")
	check(game.world.route.is_empty() and game.world.pending.is_empty(),"UI tap does not also start world movement")
	var choice: BaseButton=null
	for b in scope().find_children("*","Button",true,false):
		if b.text.begins_with("Ask what starting over"): choice=b
	await tap(choice)
	check(life.has_flag("met_contact") and life.state.history.size()==1,"Touch commits one decision")
	check(text_exists("A DECISION MADE"),"Committed choice receives visible outcome")
	await tap(find_button("Back to the street"))
	check(not game.world.paused and sound.scene_context=="city","Street restores city audio context")
	await tap(find_button("Journal"))
	check(text_exists("What will you remember"),"HUD journal receives touch")
	var notes: TextEdit=scope().find_children("*","TextEdit",true,false)[0]
	notes.text="A private note before Android Back."
	game._notification(Node.NOTIFICATION_WM_GO_BACK_REQUEST)
	await process_frame; await process_frame
	check(not is_instance_valid(game.modal),"Android Back closes journal")
	check(life.state.notes=="A private note before Android Back.","Back preserves unsaved journal text")
	await tap(find_button("Settings"))
	await tap(find_button("Show movement joystick"))
	await tap(find_button("Interface: English"))
	check(text_exists("Sound & controls") and game.world.paused,"Language change preserves modal above rebuilt HUD")
	check(game.ui.get_child(game.ui.get_child_count()-1)==game.modal,"Modal remains topmost after HUD rebuild")
	await tap(find_button("Interface: Kiswahili"))
	await tap(find_button("Done"))
	check(game.joystick_pad.visible,"Joystick preference applied")
	var stick_pos: Vector2=game.joystick_pad.global_position+Vector2(110,75)
	finger(stick_pos,true); await process_frame
	check(game.world.joystick.length()>.1,"Finger engages joystick")
	var drag:=InputEventScreenDrag.new(); drag.index=0; drag.position=Vector2(400,400); drag.relative=Vector2(200,0)
	Input.parse_input_event(drag); await process_frame
	finger(Vector2(400,400),false); await process_frame
	check(game.world.joystick==Vector2.ZERO,"Releasing outside joystick stops movement")
	check(game.world.pending.is_empty(),"Joystick touches never become destination clicks")
	await tap(find_button("Save & exit"))
	check(is_instance_valid(game.menu),"Touch Save & exit returns to title")
	check(not find_button("Continue saved life").disabled,"Saved life enables Continue")
	await tap(find_button("Continue saved life"))
	check(not is_instance_valid(game.menu) and life.has_flag("met_contact"),"Touch Continue resumes saved progress")
	sound.set_suspended(true)
	check(sound.music.stream_paused and sound.ambience.stream_paused,"Backgrounding pauses music and ambience")
	sound.set_suspended(false)
	check(not sound.music.stream_paused and not sound.ambience.stream_paused,"Foreground restores audio")
	print("KESHO TOUCH TESTS: %d checks, %d failures" % [checks,failures.size()])
	AudioServer.remove_bus_effect(0,AudioServer.get_bus_effect_count(0)-1)
	sound.shutdown(); game.queue_free(); await process_frame
	await create_timer(.15).timeout
	quit(0 if failures.is_empty() else 1)
