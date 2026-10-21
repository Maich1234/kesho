extends SceneTree
var game: Node
func _initialize() -> void: call_deferred("run")
func capture(file: String) -> void:
	await create_timer(.23).timeout
	await RenderingServer.frame_post_draw
	var img:=root.get_texture().get_image()
	var out:=OS.get_environment("KESHO_QA_DIR")
	if out.is_empty(): out=ProjectSettings.globalize_path("user://qa")
	DirAccess.make_dir_recursive_absolute(out)
	img.save_png(out.path_join(file+".png"))
func run() -> void:
	game=load("res://scenes/main.tscn").instantiate(); root.add_child(game)
	await capture("01-title")
	game.show_settings(); await capture("08-settings"); game.clear_modal()
	game.create_character(); await capture("02-character")
	var life=root.get_node("Life"); life.save_enabled=false
	life.fresh("Amani","Family household",1,0,"Checkpoints")
	game.start_play(); await capture("03-district")
	game.open_location("contact"); await capture("04-dialogue")
	game.take_choice("contact",game.story.contact.choices[0]); await capture("05-result")
	game.show_journal(); await capture("06-journal")
	game.clear_modal()
	game.world.go_to("courtyard")
	for i in range(600):
		await process_frame
		if is_instance_valid(game.modal): break
	assert(is_instance_valid(game.modal),"Walking must reach courtyard and open conversation")
	await capture("07-courtyard")
	print("KESHO VISUAL SMOKE: captured eight screens; walking interaction reached courtyard")
	root.get_node("Sound").shutdown(); game.queue_free(); await create_timer(.15).timeout; quit()
