# First Hand Interactive Observation Laboratory

A tactile, sci-fi observation research pod and interactive engineering workbench inspired by Meta Reality Labs' **First Hand**. Built with Unity 6 and the Universal Render Pipeline (URP).

---

## 🚀 Key Features

### 1. Dual-Mode Playable: Desktop PC & VR Ready
- **Desktop First-Person Controller**: Smooth WASD locomotion, 1:1 camera pitch/yaw, contextual central crosshair, tactile mouse click/drag physics.
- **VR Hardware Ready**: True 1:1 metric scale, zero-disorientation VR comfort conventions (eliminated artificial pitch bobbing), 3D spatialized audio attenuation.
- **Precision Workbench Inspection Zoom**: Press **C** or hold **Right Mouse Button** to lean in and inspect console switches, energy core rings, and holographic projections with high-fidelity magnification.

### 2. Tactile Interactive Engineering Workbench
- **Spring-Loaded Physical Push Buttons (`PhysicalPushButton`)**:
  - Depress with realistic local travel and spring return damping.
  - Threshold-triggered mechanical click DSP audio.
  - Dynamic LED emission glow feedback.
  - Momentary contact and toggle switch modes.
- **Industrial Mechanical Levers & Sliders (`PhysicalLeverSlider`)**:
  - **Blast Shield Lever**: Heavy rotational hinge lever with tactile detent ratchet audio clicks.
  - **Power Output Throttle**: Linear slider for fine tuning laboratory generator output.
- **Magnetic Power Core Docking Sockets (`PowerCoreSocket`)**:
  - Three specialized receiver bays:
    1. **Primary Fusion Reactor**
    2. **Graviton Stasis Field**
    3. **Quantum Planetary Scanner**
  - Smooth magnetic capture, lock rotation alignment, and rising harmonic power chord chimes.
- **Grabbable Energy Plasma Cores (`GrabbablePowerCore`)**:
  - Three distinct cores with dynamic pulsating emission, point lighting, and containment rings:
    - **Cyan Plasma Core** (Subsystems & Shielding)
    - **Solar Amber Core** (Primary Power)
    - **Neon Magenta Core** (Graviton Field)
  - Full physics interaction: pick up, inspect in hand, drop into the world with gravity, or dock into console sockets.

### 3. Panoramic Observation Lab & Alien Vista
- **Sleek Sci-Fi Architecture**: Titanium ribbed bulkhead arches, inset neon cyan conduit strips, and dark matte metallic flooring with glowing energy runners.
- **Mechanized Blast Shield**: A heavy titanium shutter plate outside the curved panoramic window that smoothly raises and lowers via industrial servo motors when the player pulls the blast shield lever.
- **Alien Twilight Vista**:
  - Soft pinkish/lavender atmospheric twilight sky.
  - Suspended celestial ringed gas giant in the upper atmosphere.
  - Drifting floating islands with luminous pink crystal spires.
  - Cascading vertical energy waterfalls between floating rock formations.

### 4. Dynamic Tactile HUD (`FirstHandHUD`)
- **Procedural Animated Reticle**:
  - Idle state: Subtle minimalist white dot.
  - Hover state: Expanding cyan/amber target reticle ring.
  - Core Held state: Glowing indicator matching the energy core's plasma color.
- **Contextual Action Pill**: Dynamic prompts displayed cleanly below the reticle (e.g. `[Click] Press Button`, `[Hold & Drag] Adjust Lever`, `[E / Click] Pick Up Core`, `[Click / E] Eject Core`).
- **Subsystem Dashboard**: Real-time readouts for Laboratory Power Output percentage bar and Blast Shield Shutter status.
- **Emergency Klaxon Banner**: Warning banner pulsing during alert test mode.

---

## 🎮 Desktop Controls

| Action | Control |
| :--- | :--- |
| **Move** | `W`, `A`, `S`, `D` |
| **Look Around** | `Mouse Move` |
| **Interact / Press / Drag** | `Left Mouse Click` / `Hold & Drag` |
| **Pick Up / Drop / Slot Core** | `E` or `Left Mouse Click` |
| **Workbench Zoom (Inspect)** | `C` or `Right Mouse Button (Hold)` |
| **Toggle Cursor Lock** | `Esc` |

---

## 🛠 One-Click Scene Generation
To rebuild or regenerate the entire experience at any time:
1. Open the Unity Editor.
2. Navigate to the top menu bar: **`First Hand Lab` > `Build Complete Lab Experience`**.
3. The laboratory pod, workbench console, grabbable cores, alien vista, lighting, and HUD will automatically be constructed, configured, and saved to `Assets/FirstHandLab/Scenes/FirstHandLabScene.unity`.
