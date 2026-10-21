extends Node
## Original layered score. No network, microphone, samples, or per-frame synthesis.
signal cue_played(cue: String)
const PREFS := "user://kesho_preferences.cfg"
const DEFAULTS := {"music": 0.45, "effects": 0.8, "ambience": 0.45, "muted": false, "haptics": true, "reduced_motion": false, "joystick": false, "language": "en"}
var prefs: Dictionary = DEFAULTS.duplicate(true)
var last_error := ""
var persistence_enabled := true
var music: AudioStreamPlayer
var ambience: AudioStreamPlayer
var voices: Array[AudioStreamPlayer] = []
var clips: Dictionary = {}
var voice_index := 0
var duck_tween: Tween
var scene_context := "title"
var suspended := false

func _ready() -> void:
	load_preferences()
	for bus in ["Music", "Effects", "Ambience"]:
		if AudioServer.get_bus_index(bus) < 0:
			AudioServer.add_bus()
			AudioServer.set_bus_name(AudioServer.bus_count - 1, bus)
	for cue in ["press", "click", "open", "close", "success", "deny", "step"]:
		clips[cue] = load("res://assets/audio/"+cue+".ogg")
	music = make_player("Music")
	var score: AudioStreamOggVorbis = load("res://assets/audio/maweni_first_light.ogg")
	score.loop = true; music.stream = score
	music.volume_db = -80
	ambience = make_player("Ambience")
	var air: AudioStreamOggVorbis = load("res://assets/audio/maweni_air.ogg")
	air.loop = true; ambience.stream = air; ambience.volume_db = -8
	for i in range(6): voices.append(make_player("Effects"))
	apply_preferences()
	music.play(); ambience.play()
	set_context("title", false)

func make_player(bus: String) -> AudioStreamPlayer:
	var player := AudioStreamPlayer.new(); player.bus=bus; add_child(player); return player

func load_preferences() -> void:
	var config:=ConfigFile.new()
	if config.load(PREFS) != OK: return
	for key in DEFAULTS:
		var value = config.get_value("preferences",key,DEFAULTS[key])
		if key in ["music","effects","ambience"]:
			if (value is float or value is int) and is_finite(float(value)): prefs[key]=clampf(float(value),0,1)
		elif DEFAULTS[key] is bool:
			if value is bool: prefs[key]=value
		elif key=="language" and value in ["en","sw"]: prefs[key]=value

func save_preferences() -> bool:
	if not persistence_enabled: return true
	var config:=ConfigFile.new()
	for key in prefs: config.set_value("preferences",key,prefs[key])
	var error:=config.save(PREFS)
	last_error="" if error==OK else "Preferences could not be saved on this device."
	return error==OK

func set_preference(key: String, value: Variant) -> void:
	if not DEFAULTS.has(key): return
	if key in ["music","effects","ambience"]: value=clampf(float(value),0,1)
	prefs[key]=value; apply_preferences(); save_preferences()

func apply_preferences() -> void:
	for pair in [["Music","music"],["Effects","effects"],["Ambience","ambience"]]:
		var index:=AudioServer.get_bus_index(pair[0])
		if index<0: continue
		var gain: float=prefs[pair[1]]
		AudioServer.set_bus_volume_db(index,linear_to_db(maxf(gain,.0001)))
		AudioServer.set_bus_mute(index,bool(prefs.muted) or gain<=0.001)

func play(cue: String, haptic: bool = false) -> void:
	if suspended or not clips.has(cue): return
	cue_played.emit(cue)
	if not bool(prefs.muted) and float(prefs.effects)>.001:
		var player: AudioStreamPlayer=voices[voice_index % voices.size()]
		voice_index+=1; player.stream=clips[cue]; player.pitch_scale=1.0
		player.volume_db=-8 if cue=="step" else -2
		player.play()
	if haptic and bool(prefs.haptics) and OS.has_feature("mobile"):
		Input.vibrate_handheld(12 if cue!="success" else 20)

func set_context(context: String, animate: bool = true) -> void:
	scene_context=context
	if not is_instance_valid(music): return
	if is_instance_valid(duck_tween): duck_tween.kill()
	var target: float={"title":-3.0,"city":-7.0,"dialogue":-13.0}.get(context,-7.0)
	if not animate: music.volume_db=target; return
	duck_tween=create_tween().set_parallel(true)
	duck_tween.tween_property(music,"volume_db",target,.35)
	duck_tween.tween_property(ambience,"volume_db",-17.0 if context=="dialogue" else -8.0,.35)

func set_suspended(value: bool) -> void:
	suspended=value
	if not is_instance_valid(music): return
	music.stream_paused=value; ambience.stream_paused=value
	for voice in voices:
		if value: voice.stop()

func _notification(what: int) -> void:
	if what==NOTIFICATION_APPLICATION_PAUSED or what==NOTIFICATION_APPLICATION_FOCUS_OUT: set_suspended(true)
	elif what==NOTIFICATION_APPLICATION_RESUMED or what==NOTIFICATION_APPLICATION_FOCUS_IN: set_suspended(false)

func shutdown() -> void:
	suspended=true
	if is_instance_valid(duck_tween): duck_tween.kill()
	for player in [music,ambience]+voices:
		if is_instance_valid(player): player.stop(); player.stream=null
	clips.clear()

func _exit_tree() -> void:
	shutdown()
