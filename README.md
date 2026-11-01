# Wheel Of Vertigo

A wheel-of-fortune take on Critical Strike's card game, built for the Vertigo Games developer demo.
Unity 6000.3.23f1, uGUI with TextMeshPro, DOTween and Sprite Atlas V2. The target is Android, landscape.

## How it plays

- Each zone has an 8-slice wheel. Spin it to win the reward the indicator stops on.
- Normal zones use the **bronze** wheel. One of its slices is a **bomb**, and it takes everything collected in the run.
- Every 5th zone is a **safe zone** with a **silver** wheel and no bomb.
- Every 30th zone is a **super zone** with a **golden** wheel, special rewards and no bomb.
  - Reaching one also **unlocks a guaranteed weapon**. It is shown full screen, and **Claim** adds it to the run's rewards.
  - The SUPER ZONE card shows the weapon the next super zone unlocks.
- Reward amounts grow by 15% of their base value every zone.
- **Exit** is enabled at any zone while the wheel is idle, as in Critical Strike. It asks for confirmation first, then shows what you walked away with. Safe and super zones are the ones without a bomb.
- Hitting the bomb shows the bomb popup. **Restart** begins a new run at zone 1.

## Architecture

The code follows Model-View-Presenter with a plain C# domain:

```
GameConfig (SO) ──► GamePresenter ──► Views (MonoBehaviours, no game logic)
                        │  ▲               WheelView, ZoneBarView, ZoneInfoView,
                        ▼  │ events        RewardPanelView, RewardFlyAnimator, popups
                    GameSession (plain C#, unit-tested)
                        ├─ ZoneRules        zone type, leave rule, reward growth
                        ├─ IWheelFactory    builds the wheel for a zone from configs
                        ├─ ISlicePicker     picks the landing slice (seeded random)
                        ├─ ISuperZoneRewards which weapon each super zone unlocks
                        └─ RewardInventory  rewards collected this run
```

- **`GameSession`** is a small state machine: `Ready → Spinning → Collecting → Ready`, or `→ Busted` / `→ CashedOut`. Arriving at a super zone goes through `Unlocking` until the weapon is claimed. It decides every outcome before any animation plays.
  - It depends only on interfaces, so tests drive it with a fixed picker and fake wheels.
  - It has no Unity or DOTween dependencies.
- **`GamePresenter`** is the composition root. It turns view events into session calls and sequences the animations: spin, then reward fly-in, then zone bar slide, then wheel tier change. While a step is still animating it blocks input.
- **Views** are passive. Each one finds its own references from child names in `OnValidate` (`TransformExtensions.AssignIfFound`). Buttons are wired in code with `AddListener`; the Inspector has no OnClick events.

### Assemblies

- **`WheelOfVertigo.Core`** holds the data ScriptableObjects, the domain and the utilities. It doesn't reference DOTween or the UI.
- **`WheelOfVertigo.Tests.EditMode`** is the EditMode test assembly (62 tests). Run it from the Test Runner.
- The UI scripts compile into `Assembly-CSharp`, because DOTween's modules compile into `firstpass`.

## Editing content

Everything is data, set in the Inspector:

| Asset | What it controls |
|---|---|
| `Data/game_config.asset` | Safe and super intervals, reward growth, which wheel each zone type uses, the super zone unlocks (in order, repeating), bomb icon, seed (0 = random) |
| `Data/Wheels/wheel_*.asset` | Wheel art, title, subtitle, and each slice's type, reward and base amount |
| `Data/Rewards/reward_*.asset` | A reward's name, icon and kind |
| `Data/wheel_spin_settings.asset` | Spin duration, turns, ease, landing jitter, indicator tick and feedback punches |

Both configs check themselves in `OnValidate`:
- `WheelConfig` warns about a wrong slice count or a reward slice with no reward.
- `GameConfig` warns if a safe or super wheel contains a bomb, if the normal wheel has none, or if the super zone unlock list is empty or has a gap.

## UI rules

| Rule | How it's met |
|---|---|
| Canvas scaler Expand | `ui_canvas_main` uses Scale With Screen Size at 1920x1080 with Expand. Panels are anchored to their edges, and a `SafeAreaFitter` keeps the HUD clear of notches. |
| TextMeshPro | All text uses Oswald Bold SDF with a static atlas. |
| `_value` suffix | Every image or text that changes at runtime ends in `_value`, e.g. `ui_text_wheel_title_value`. |
| General → specific names | `ui_<type>_<area>_<detail>`, e.g. `ui_image_wheel_indicator_value`. |
| No needless Raycast Target / Maskable | These are only on for button graphics, popup dims (marked with `RaycastBlocker`) and graphics under a `RectMask2D`. |
| Animations not on root transforms | Tweens run on `*_content` children. Popup roots only toggle active. |
| Buttons set in OnValidate, no editor OnClick | See each view's `OnValidate` and its `OnEnable`/`OnDisable`. |
| Sliced sprites, no stretched images | Frames, panels and buttons are 9-sliced. Icons use Preserve Aspect. |
| Sprite Atlas | `Art/Atlases/atlas_ui`, `atlas_icons` and `atlas_renders` (the full-size weapon renders), ASTC 6x6 on Android. |

**Tools > Wheel Of Vertigo > Validate UI** checks the open scene and every prefab against these rules. **Fix Raycast Target And Maskable** turns off both flags where they aren't needed.

## Performance notes

- The spinning wheel and the fly layer each have their own nested `Canvas`. Their per-frame movement doesn't rebuild the static UI.
- Fly-in icons and reward entries are pooled.
- Zone bar cells are a fixed set that is reused as the bar scrolls, so it covers any number of zones.
- The zone bar has two strips that slide together: the plain numbers behind, and a strip masked to the current-zone frame with each zone's panel and current-zone number colour. The next zone's colours wipe in with the motion instead of switching when it stops.
- Reward icon textures are capped at 256 px, since they never draw larger than about 110 px.

## Project layout

```
Assets/_Project/
  Art/        Sprites, Atlases
  Data/       game_config, wheel_spin_settings, Wheels/, Rewards/
  Fonts/      Oswald Bold SDF (+ outline material)
  Prefabs/UI/ Items/, Panels/, Popups/
  Scenes/     scene_game
  Scripts/    Core/ (asmdef), UI/{Common,Presenters,Views}, Editor/
  Tests/      EditMode/
```

The zone info cards share one prefab, `ui_item_zone_info_card`, with its `ZoneInfoCardView`. The safe and super cards are prefab variants of it that override only their colours, label, number size and reward cell, so the card is laid out and fixed in one place.

## Building

Use **File > Build Profiles > Android > Build**. The scene list contains `scene_game`. The build uses IL2CPP for ARM64, and the package id is `com.keremadali.wheelofvertigo`.
