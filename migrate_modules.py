import os
import shutil

BASE = r"C:\Users\wodud\Documents\UnityProjects\castle-hero\CastleHero-UnityClient\Assets\Scripts"

def move_with_meta(src, dst):
    src = src.replace("/", "\\")
    dst = dst.replace("/", "\\")
    os.makedirs(os.path.dirname(dst), exist_ok=True)
    if os.path.exists(src):
        shutil.move(src, dst)
        if os.path.exists(src + ".meta"):
            shutil.move(src + ".meta", dst + ".meta")
        print(f"  MOVED: {os.path.basename(src)}")
        return True
    else:
        print(f"  SKIP (not found): {src}")
        return False

def move_dir_with_meta(src, dst):
    src = src.replace("/", "\\")
    dst = dst.replace("/", "\\")
    if os.path.exists(src):
        os.makedirs(os.path.dirname(dst), exist_ok=True)
        shutil.move(src, dst)
        if os.path.exists(src + ".meta"):
            shutil.move(src + ".meta", dst + ".meta")
        print(f"  MOVED DIR: {os.path.basename(src)}")
        return True
    else:
        print(f"  SKIP DIR (not found): {src}")
        return False

print("=== Phase 1: Creating directories ===")
for d in ["Common", "Data", "View", "Network.Impl.Local"]:
    os.makedirs(os.path.join(BASE, d), exist_ok=True)
    print(f"  Created: {d}/")

# ─────────────────────────────────────────────────────────────────────────
print("\n=== Phase 2: Domain/Common → Common (non-Data, non-View) ===")
# Entire subdirectories that are clean (no Data deps)
for subdir in ["Factory", "Flow", "InApp", "Pattern", "ResourceManagement", "Secure"]:
    move_dir_with_meta(
        os.path.join(BASE, "Domain", "Common", subdir),
        os.path.join(BASE, "Common", subdir)
    )

# Sound: only non-Data-dependent files
os.makedirs(os.path.join(BASE, "Common", "Sound"), exist_ok=True)
for f in ["ISoundManager.cs", "ISoundPlayer.cs", "SoundPlayer.cs"]:
    move_with_meta(
        os.path.join(BASE, "Domain", "Common", "Sound", f),
        os.path.join(BASE, "Common", "Sound", f)
    )

# Common misc files
for f in ["Timer.cs", "UIConfig.cs", "NetworkConfig.cs", "RxHelper.cs"]:
    move_with_meta(
        os.path.join(BASE, "Domain", "Common", f),
        os.path.join(BASE, "Common", f)
    )

# Behaviours: interfaces/base classes with no Data deps
os.makedirs(os.path.join(BASE, "Common", "Behaviours"), exist_ok=True)
for f in ["CommonAnimationEvent.cs", "IPopupManager.cs", "IStageSelect.cs", "PoolItemBase.cs"]:
    move_with_meta(
        os.path.join(BASE, "Domain", "Common", "Behaviours", f),
        os.path.join(BASE, "Common", "Behaviours", f)
    )

print("\n=== Phase 3: Domain/Utility → Common/Utility (real files only) ===")
os.makedirs(os.path.join(BASE, "Common", "Utility"), exist_ok=True)
pure_utils = [
    "AddressableHelper.cs", "CollectionHelper.cs", "EnumHelper.cs",
    "MathHelper.cs", "NetworkHelper.cs", "ObjectHelper.cs",
    "PrefabPathCache.cs", "StringHelper.cs", "TaskHelper.cs", "UIHelper.cs"
]
for f in pure_utils:
    move_with_meta(
        os.path.join(BASE, "Domain", "Utility", f),
        os.path.join(BASE, "Common", "Utility", f)
    )

print("\n=== Phase 4: Domain/Shared/IUnitBehaviour → Common/Behaviours ===")
move_with_meta(
    os.path.join(BASE, "Domain", "Shared", "IUnitBehaviour.cs"),
    os.path.join(BASE, "Common", "Behaviours", "IUnitBehaviour.cs")
)

print("\n=== Phase 5: Domain/Data → Data ===")
for subdir in ["DB", "Load", "Localize", "Model", "Repositories", "Sound"]:
    move_dir_with_meta(
        os.path.join(BASE, "Domain", "Data", subdir),
        os.path.join(BASE, "Data", subdir)
    )
for f in ["ReflectionHelper.cs", "ShopHelper.cs", "Storage.cs"]:
    move_with_meta(
        os.path.join(BASE, "Domain", "Data", f),
        os.path.join(BASE, "Data", f)
    )

print("\n=== Phase 6: Domain/Shared → Data/Shared ===")
os.makedirs(os.path.join(BASE, "Data", "Shared"), exist_ok=True)
for f in ["Error.cs", "Model.cs"]:
    move_with_meta(
        os.path.join(BASE, "Domain", "Shared", f),
        os.path.join(BASE, "Data", "Shared", f)
    )

print("\n=== Phase 7: View Bootstrapper files ===")
os.makedirs(os.path.join(BASE, "View", "Bootstrapper"), exist_ok=True)
for f in ["Context.cs", "Loading.cs", "SceneBehaviour.cs"]:
    move_with_meta(
        os.path.join(BASE, "Domain", "Common", "Behaviours", f),
        os.path.join(BASE, "View", "Bootstrapper", f)
    )

print("\n=== Phase 8: View Common files (Data-dependent behaviours) ===")
os.makedirs(os.path.join(BASE, "View", "Common"), exist_ok=True)
for f in ["Map.cs", "PolygonDrawer.cs", "PopupManager.cs", "StartButton.cs", "UIMain.cs"]:
    move_with_meta(
        os.path.join(BASE, "Domain", "Common", "Behaviours", f),
        os.path.join(BASE, "View", "Common", f)
    )

print("\n=== Phase 9: SoundManager → View ===")
os.makedirs(os.path.join(BASE, "View", "Sound"), exist_ok=True)
move_with_meta(
    os.path.join(BASE, "Domain", "Common", "Sound", "SoundManager.cs"),
    os.path.join(BASE, "View", "Sound", "SoundManager.cs")
)

print("\n=== Phase 10: Domain/Common/UI → View/Common/UI ===")
move_dir_with_meta(
    os.path.join(BASE, "Domain", "Common", "UI"),
    os.path.join(BASE, "View", "Common", "UI")
)

print("\n=== Phase 11: Localize UI → View ===")
os.makedirs(os.path.join(BASE, "View", "Localize"), exist_ok=True)
move_with_meta(
    os.path.join(BASE, "Domain", "Common", "Localize", "UI", "UILocalizeText.cs"),
    os.path.join(BASE, "View", "Localize", "UILocalizeText.cs")
)

print("\n=== Phase 12: Data-dependent Utility → View ===")
os.makedirs(os.path.join(BASE, "View", "Utility"), exist_ok=True)
for f in ["ItemHelper.cs"]:
    move_with_meta(
        os.path.join(BASE, "Domain", "Utility", f),
        os.path.join(BASE, "View", "Utility", f)
    )

print("\n=== Phase 13: Constants → View ===")
move_with_meta(
    os.path.join(BASE, "Domain", "Common", "Constants.cs"),
    os.path.join(BASE, "View", "Constants.cs")
)

print("\n=== Phase 14: Lobby → View/Lobby ===")
lobby_src = os.path.join(BASE, "Lobby")
lobby_dst = os.path.join(BASE, "View", "Lobby")
os.makedirs(lobby_dst, exist_ok=True)
if os.path.exists(lobby_src):
    for item in os.listdir(lobby_src):
        if item in ("CastleHero.Lobby.asmdef", "CastleHero.Lobby.asmdef.meta"):
            continue
        move_with_meta(
            os.path.join(lobby_src, item),
            os.path.join(lobby_dst, item)
        )

print("\n=== Phase 15: GamePlay UI/Behaviours → View ===")
os.makedirs(os.path.join(BASE, "View", "InGame"), exist_ok=True)
move_dir_with_meta(
    os.path.join(BASE, "GamePlay", "InGame", "UI"),
    os.path.join(BASE, "View", "InGame", "UI")
)
move_dir_with_meta(
    os.path.join(BASE, "GamePlay", "InGame", "Behaviours"),
    os.path.join(BASE, "View", "InGame", "Behaviours")
)

os.makedirs(os.path.join(BASE, "View", "Castle"), exist_ok=True)
for f in ["UICastle.cs", "UIGlobalSkillDisplay.cs"]:
    move_with_meta(
        os.path.join(BASE, "GamePlay", "Castle", f),
        os.path.join(BASE, "View", "Castle", f)
    )

print("\n=== Phase 16: Network UITest → View ===")
os.makedirs(os.path.join(BASE, "View", "Test"), exist_ok=True)
move_with_meta(
    os.path.join(BASE, "Network", "Service", "Test", "UITest.cs"),
    os.path.join(BASE, "View", "Test", "UITest.cs")
)

print("\n=== Done! ===")
# Summary
for mod in ["Common", "Data", "View", "Network.Impl.Local"]:
    cs_files = sum(1 for r, d, f in os.walk(os.path.join(BASE, mod)) for fn in f if fn.endswith(".cs"))
    print(f"  {mod}/: {cs_files} .cs files")
