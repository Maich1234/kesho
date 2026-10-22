# Kesho brand exports

Original project mark: repository-root `icon.svg` (K, sun and steps/path). This is not the Shujaaz logo or third-party magazine artwork.

The repository-root `icon.svg` is the vector master; `app-icon.png` is its 1024px transparent PNG export for Unity Player Settings. The WebGL template contains matching SVG/ICO/PNG favicon exports. Raster export was generated with Inkscape; ICO sizes are 16, 32 and 48 pixels, browser touch icon 192 pixels. The vector master stays outside Unity Assets so it requires no SVG importer plugin. Keep all derivatives consistent when changing the mark. `ProjectSetup.Configure` assigns the Unity player icon; its build must run before testing installation.
