extends Node
## Offline authority only. Never accept this state as an online economy transaction.
signal changed
const SAVE := "user://kesho_v1.json"
const BACKUP := "user://kesho_v1.backup.json"
var state: Dictionary = {}
var last_error := ""
var rng := RandomNumberGenerator.new()
var save_enabled := true

func fresh(player_name: String, background: String, skin: int, outfit: int, mode: String, gender: String = "Man", belief: String = "Undecided", family_trait: String = "Supportive") -> void:
	rng.randomize()
	state = {"version": 1, "name": player_name.strip_edges().left(24), "background": background, "skin": skin, "outfit": outfit, "gender": gender, "belief": belief, "family_trait": family_trait, "mode": mode, "day": 1, "hour": 10, "money": 1200, "energy": 85, "flags": {}, "history": [], "notes": "", "position": [0.0, 5.5], "complete": false, "rng_seed": str(rng.seed), "rng_state": str(rng.state)}
	if state.name.is_empty(): state.name = "Amani"
	commit()

func has_flag(key: String) -> bool:
	return bool(state.get("flags", {}).get(key, false))

func available(choice: Dictionary) -> bool:
	if state.is_empty() or bool(state.get("complete", false)): return false
	if choice.has("background") and choice.background != state.background: return false
	if int(choice.get("cost", 0)) > int(state.money): return false
	if int(state.hour) + int(choice.get("hours", 0)) > 22: return false
	if int(state.energy) < int(choice.get("effort", 0)): return false
	for flag in choice.get("requires", []):
		if not has_flag(flag): return false
	for flag in choice.get("excludes", []):
		if has_flag(flag): return false
	return true

func apply(choice: Dictionary, location: String) -> Dictionary:
	if not available(choice): return {"ok": false, "text": "You need more time, energy, money, or a previous lead."}
	# Select once, persist RNG, then reveal. Reloading cannot roll the same action again.
	var outcome: Dictionary = choice
	var variants: Array = choice.get("outcomes", [])
	if not variants.is_empty():
		var roll := rng.randf()
		var cumulative := 0.0
		for variant in variants:
			cumulative += float(variant.weight)
			if roll <= cumulative:
				outcome = variant
				break
	state.money = int(state.money) - int(choice.get("cost", 0)) + int(outcome.get("gain", choice.get("gain", 0)))
	state.hour = int(state.hour) + int(choice.get("hours", 0))
	state.energy = clampi(int(state.energy) - int(choice.get("effort", 0)) + int(choice.get("restore", 0)), 0, 100)
	for flag in choice.get("sets", []): state.flags[flag] = true
	for flag in outcome.get("sets", []): state.flags[flag] = true
	var text: String = outcome.get("result", choice.get("result", ""))
	state.history.append({"id": choice.get("id", ""), "day": state.day, "hour": state.hour, "place": location, "choice": choice.get("label", ""), "result": text})
	state.rng_state = str(rng.state)
	var saved := commit()
	return {"ok": true, "text": text, "saved": saved}

func next_day() -> Dictionary:
	if not has_flag("shelter"): return {"ok": false, "text": "Arrange a bed at the courtyard before ending the day."}
	if int(state.day) >= 3:
		state.complete = true
	else:
		state.day = int(state.day) + 1
		state.hour = 8
		state.energy = 90
		state.flags.erase("meal_today")
		# Rent terms in the scene cover all three prototype nights, no invisible charges.
	commit()
	return {"ok": true, "text": ""}

func commit() -> bool:
	changed.emit()
	if not save_enabled: return true
	var temp := SAVE + ".tmp"
	var file := FileAccess.open(temp, FileAccess.WRITE)
	if file == null:
		last_error = "Could not save. Keep the game open and free device storage."
		return false
	file.store_string(JSON.stringify(state))
	file.flush()
	file.close()
	if FileAccess.file_exists(SAVE):
		var copy_error := DirAccess.copy_absolute(SAVE, BACKUP)
		if copy_error != OK:
			last_error = "Backup could not be updated."
	var error := DirAccess.rename_absolute(temp, SAVE)
	if error != OK:
		last_error = "Could not finish saving."
		return false
	last_error = ""
	return true

func load_life() -> bool:
	for path in [SAVE, BACKUP]:
		if not FileAccess.file_exists(path): continue
		var parser := JSON.new()
		if parser.parse(FileAccess.get_file_as_string(path)) != OK: continue
		var parsed = parser.data
		if not valid_state(parsed): continue
		state = parsed
		rng.seed = int(state.rng_seed)
		rng.state = int(state.rng_state)
		last_error = "" if path == SAVE else "Recovered your previous backup."
		return true
	last_error = "No valid saved life found."
	return false

func valid_state(value: Variant) -> bool:
	if not value is Dictionary: return false
	for key in ["version", "name", "background", "skin", "outfit", "mode", "day", "hour", "money", "energy", "flags", "history", "notes", "position", "complete", "rng_seed", "rng_state"]:
		if not value.has(key): return false
	if not (value.flags is Dictionary and value.history is Array and value.position is Array and value.position.size() == 2): return false
	for key in ["version", "skin", "outfit", "day", "hour", "money", "energy"]:
		if not (value[key] is int or value[key] is float): return false
		if not is_finite(float(value[key])) or float(value[key]) != float(int(value[key])): return false
	for key in ["name", "background", "mode", "notes", "rng_seed", "rng_state"]:
		if not value[key] is String: return false
	if not value.complete is bool: return false
	if value.name.length() < 1 or value.name.length() > 24 or value.notes.length() > 12000: return false
	if value.background not in ["Family household", "Independent parent", "Community upbringing"]: return false
	if value.mode not in ["Checkpoints", "Permanent consequences"]: return false
	if not value.rng_seed.is_valid_int() or not value.rng_state.is_valid_int(): return false
	for point in value.position:
		if not (point is float or point is int) or not is_finite(float(point)) or absf(float(point)) > 30: return false
	for flag in value.flags:
		if not flag is String or not value.flags[flag] is bool: return false
	for entry in value.history:
		if not entry is Dictionary: return false
		for key in ["id", "day", "hour", "place", "choice", "result"]:
			if not entry.has(key): return false
		for key in ["id", "place", "choice", "result"]:
			if not entry[key] is String: return false
		for key in ["day", "hour"]:
			if not (entry[key] is int or entry[key] is float): return false
	return int(value.version) == 1 and int(value.skin) in [0,1,2,3] and int(value.outfit) in [0,1,2,3] and int(value.money) >= 0 and int(value.energy) >= 0 and int(value.energy) <= 100 and int(value.day) in [1,2,3] and int(value.hour) >= 0 and int(value.hour) <= 22

func snapshot() -> Dictionary:
	# Snapshot is opt-in, excludes journal, secrets and exact private choices.
	return {"name": state.get("name", ""), "chapter": 1, "day": state.get("day", 1), "complete": state.get("complete", false), "background": state.get("background", ""), "kind": "unverified_offline_summary"}
