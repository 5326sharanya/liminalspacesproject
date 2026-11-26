# liminalspacesproject

## 360 Panorama Skeleton

`Assets/Scenes/PanoramaViewer.unity` is a standalone monoscopic 360 viewer: the base for the
First Hand-inspired experience, kept separate from the liminal-space game. `LiminalGameManager`
skips its automatic world setup whenever a `PanoramaViewer` is present in the scene, so opening
this scene does not spawn the liminal world on Play.

**Open:** `Assets/Scenes/PanoramaViewer.unity`, or `Liminal Space > Open 360 Panorama Scene`.

**Regenerate:** `Liminal Space > Build 360 Panorama Scene` — rebuilds the rig, camera, skybox and
build-settings entry from scratch.

### Controls

| Device | Look | Recenter |
| --- | --- | --- |
| Laptop | Drag with the left mouse button | Right-click or `R` |
| Phone | Drag with a finger, plus gyroscope | Double-tap |

One full screen height of dragging turns the view 180°, so the feel is the same on a phone and
on the laptop. The drag is "grab the world": the image follows your finger.

### Your own panorama

The scene currently shows the summit photo `Assets/LiminalSpace/Textures/MountainSummit.jpg`.

Two kinds of image work:

- **A normal panorama photo** (wide and short, like the summit shot) — use the
  `LiminalSpace/Skybox Panorama Band` shader, which keeps the photo at its real size in a band
  around the horizon and paints sky above and ground below. This is what `MountainSummit.jpg` uses.
- **A full 360 image** (a 2:1 equirectangular photo sphere or `RenderTexture` from a
  `VideoPlayer`) — it covers the whole sphere, so the band settings do not apply.

To use another photo: replace the image file, then run `Liminal Space > Apply Mountain Panorama`
(or set the `Panorama` slot on `PanoramaSkybox` by hand). If the mountains look too tall or too
flat, select `Assets/Settings/MountainPanorama.mat` and change **Photo Vertical FOV** (55° is a
good start for a stitched panorama). **Mirror Tiling** hides the seam where the photo's two ends
meet, at the cost of showing the same view every 180°.

With nothing assigned, a generated dusk-gradient placeholder is used so the scene is never empty.

### Tuning on a phone

- View turns the wrong way: toggle `Invert Gyro Yaw` / `Invert Gyro Pitch` on `PanoramaViewer`.
- The gyro is re-anchored automatically when the screen rotates.
- Frame rate target, v-sync off and keep-screen-awake are under `Platform`.

### Build an APK

`Liminal Space > Build 360 Panorama APK` builds `Builds/Android/PanoramaViewer.apk`: ARM64 IL2CPP,
Android 7.1+ (API 25), bundle id `com.liminalspace.panoramaviewer`, rendered with the mobile
pipeline. The same build runs from the command line:

```
Unity.exe -batchmode -nographics -projectPath <project> -buildTarget Android \
  -executeMethod LiminalSpace.Editor.PanoramaAndroidBuild.BuildApk -logFile build.log
```

The first build takes around 20 minutes on this laptop (IL2CPP plus Gradle); later ones are faster.
The script restores your editor quality level once the APK is built.

To show the viewer on a laptop instead, `Liminal Space > Build 360 Panorama for Windows` builds
`Builds/Windows/PanoramaViewer.exe` (keep it next to its `PanoramaViewer_Data` folder). You can also
just open the scene and press Play in the editor.
