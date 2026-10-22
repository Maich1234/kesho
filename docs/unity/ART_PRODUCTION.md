# College art production

Asset: `unity/Assets/Kesho/Resources/Art/cast.png` (1254 × 1254 RGB PNG).
Produced with the built-in image-generation tool, copied unchanged into the Unity project. The atlas is read as four UV rectangles by StoryHud. It supplies original Amani/Nia character-selection portraits and Rehema/Kito comic conversation close-ups. Customised player dialogue uses the live 3D character rather than showing an inaccurate fixed portrait.

## Generation prompt

Use case: illustration-story. Production game asset, not mockup. Generate a square 1536x1536 portrait atlas for original Kenyan-inspired adult comic characters in a mobile life game called Kesho. EXACT 2 by 2 grid of four square bust portraits, each fills its own quadrant with a plain warm cream background, no gutters, no borders, NO TEXT anywhere. Top-left: Amani, 19-year-old Black African man, dark brown skin, short tight fade haircut, mustard yellow varsity jacket over white tee, expressive hopeful eyes, carrying backpack strap, friendly determined expression. Top-right: Nia, 19-year-old Black African woman, medium brown skin, gathered natural afro puff, teal hoodie, small gold earrings, alert amused expression, backpack strap. Bottom-left: Rehema, 23-year-old Black African female student mentor, deep brown skin, neatly braided hair tied back, coral red shirt, blue lanyard, confident skeptical brow, expressive face. Bottom-right: Kito, 28-year-old Black African male unofficial campus fixer, medium brown skin, short twisted hair, dark navy open collar shirt with orange undershirt, thin moustache, one raised eyebrow, charming ambiguous smile. All portraits chest-up, head entirely inside quadrant, faces large and distinct. Bold expressive black ink contours, clean cel-shaded rich flat colours, subtle hand-ink texture, contemporary East African youth comic aesthetic, professional published graphic novel character art. Natural individual facial features, no caricature of ethnicity, no real people, no brand logos, no speech bubbles. Keep same scale and consistent warm upper-left lighting across the four portraits.

The tool returned 1254 × 1254 despite the requested nominal dimensions. The runtime uses proportional UV rectangles and does not depend on 1536 pixels. The artwork is an illustration asset, not a rendered screenshot or claim of final 3D character fidelity.

## In-engine art

ComicMesh supplies reusable explicit meshes. Stage composes campus architecture and furniture, and combines static scenery by colour. ComicActor adds articulated bodies, faces and selectable hair. Resources/Shaders/Comic.shader supplies three-band cel lighting and edge darkening. These source assets require Unity and phone visual approval; generated portraits alone do not validate the scene.
