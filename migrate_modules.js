const fs = require('fs');
const path = require('path');

const BASE = 'C:\\Users\\wodud\\Documents\\UnityProjects\\castle-hero\\CastleHero-UnityClient\\Assets\\Scripts';

function moveWithMeta(src, dst) {
  if (!fs.existsSync(src)) {
    console.log(`  SKIP: ${path.basename(src)}`);
    return false;
  }
  fs.mkdirSync(path.dirname(dst), { recursive: true });
  fs.renameSync(src, dst);
  if (fs.existsSync(src + '.meta')) fs.renameSync(src + '.meta', dst + '.meta');
  console.log(`  MOVED: ${path.basename(src)}`);
  return true;
}

function moveDirWithMeta(src, dst) {
  if (!fs.existsSync(src)) {
    console.log(`  SKIP DIR: ${path.basename(src)}`);
    return false;
  }
  fs.mkdirSync(path.dirname(dst), { recursive: true });
  fs.renameSync(src, dst);
  if (fs.existsSync(src + '.meta')) fs.renameSync(src + '.meta', dst + '.meta');
  console.log(`  MOVED DIR: ${path.basename(src)}`);
  return true;
}

function countCs(dir) {
  if (!fs.existsSync(dir)) return 0;
  let count = 0;
  function walk(d) {
    for (const f of fs.readdirSync(d)) {
      const fp = path.join(d, f);
      if (fs.statSync(fp).isDirectory()) walk(fp);
      else if (f.endsWith('.cs')) count++;
    }
  }
  walk(dir);
  return count;
}

const j = (...parts) => path.join(BASE, ...parts);

// ─── Phase 1: Create directories ───────────────────────────────────────
console.log('\n=== Phase 1: Creating directories ===');
for (const d of ['Common', 'Data', 'View', 'Network.Impl.Local']) {
  fs.mkdirSync(j(d), { recursive: true });
  console.log(`  Created: ${d}/`);
}

// ─── Phase 2: Domain/Common → Common (non-Data) ────────────────────────
console.log('\n=== Phase 2: Domain/Common → Common ===');
for (const s of ['Factory', 'Flow', 'InApp', 'Pattern', 'ResourceManagement', 'Secure']) {
  moveDirWithMeta(j('Domain','Common',s), j('Common',s));
}

// Sound: only non-Data-dep files
fs.mkdirSync(j('Common','Sound'), { recursive: true });
for (const f of ['ISoundManager.cs', 'ISoundPlayer.cs', 'SoundPlayer.cs']) {
  moveWithMeta(j('Domain','Common','Sound',f), j('Common','Sound',f));
}

// Common misc
for (const f of ['Timer.cs', 'UIConfig.cs', 'NetworkConfig.cs', 'RxHelper.cs']) {
  moveWithMeta(j('Domain','Common',f), j('Common',f));
}

// Behaviours: clean interfaces
fs.mkdirSync(j('Common','Behaviours'), { recursive: true });
for (const f of ['CommonAnimationEvent.cs', 'IPopupManager.cs', 'IStageSelect.cs', 'PoolItemBase.cs']) {
  moveWithMeta(j('Domain','Common','Behaviours',f), j('Common','Behaviours',f));
}

// ─── Phase 3: Domain/Utility → Common/Utility ──────────────────────────
console.log('\n=== Phase 3: Domain/Utility → Common/Utility ===');
fs.mkdirSync(j('Common','Utility'), { recursive: true });
for (const f of ['AddressableHelper.cs', 'CollectionHelper.cs', 'EnumHelper.cs',
                 'MathHelper.cs', 'NetworkHelper.cs', 'ObjectHelper.cs',
                 'PrefabPathCache.cs', 'StringHelper.cs', 'TaskHelper.cs', 'UIHelper.cs']) {
  moveWithMeta(j('Domain','Utility',f), j('Common','Utility',f));
}

// ─── Phase 4: IUnitBehaviour → Common/Behaviours ──────────────────────
console.log('\n=== Phase 4: IUnitBehaviour → Common/Behaviours ===');
moveWithMeta(j('Domain','Shared','IUnitBehaviour.cs'), j('Common','Behaviours','IUnitBehaviour.cs'));

// ─── Phase 5: Domain/Data → Data ──────────────────────────────────────
console.log('\n=== Phase 5: Domain/Data → Data ===');
for (const s of ['DB', 'Load', 'Localize', 'Model', 'Repositories', 'Sound']) {
  moveDirWithMeta(j('Domain','Data',s), j('Data',s));
}
for (const f of ['ReflectionHelper.cs', 'ShopHelper.cs', 'Storage.cs']) {
  moveWithMeta(j('Domain','Data',f), j('Data',f));
}

// ─── Phase 6: Domain/Shared → Data/Shared ─────────────────────────────
console.log('\n=== Phase 6: Domain/Shared → Data/Shared ===');
fs.mkdirSync(j('Data','Shared'), { recursive: true });
for (const f of ['Error.cs', 'Model.cs']) {
  moveWithMeta(j('Domain','Shared',f), j('Data','Shared',f));
}

// ─── Phase 7: View Bootstrapper ───────────────────────────────────────
console.log('\n=== Phase 7: View Bootstrapper files ===');
fs.mkdirSync(j('View','Bootstrapper'), { recursive: true });
for (const f of ['Context.cs', 'Loading.cs', 'SceneBehaviour.cs']) {
  moveWithMeta(j('Domain','Common','Behaviours',f), j('View','Bootstrapper',f));
}

// ─── Phase 8: View Common (Data-dependent behaviours) ─────────────────
console.log('\n=== Phase 8: View Common files ===');
fs.mkdirSync(j('View','Common'), { recursive: true });
for (const f of ['Map.cs', 'PolygonDrawer.cs', 'PopupManager.cs', 'StartButton.cs', 'UIMain.cs']) {
  moveWithMeta(j('Domain','Common','Behaviours',f), j('View','Common',f));
}

// ─── Phase 9: SoundManager → View ─────────────────────────────────────
console.log('\n=== Phase 9: SoundManager → View ===');
fs.mkdirSync(j('View','Sound'), { recursive: true });
moveWithMeta(j('Domain','Common','Sound','SoundManager.cs'), j('View','Sound','SoundManager.cs'));

// ─── Phase 10: Domain/Common/UI → View/Common/UI ──────────────────────
console.log('\n=== Phase 10: Domain/Common/UI → View/Common/UI ===');
moveDirWithMeta(j('Domain','Common','UI'), j('View','Common','UI'));

// ─── Phase 11: Localize UI → View ─────────────────────────────────────
console.log('\n=== Phase 11: Localize UI → View ===');
fs.mkdirSync(j('View','Localize'), { recursive: true });
moveWithMeta(j('Domain','Common','Localize','UI','UILocalizeText.cs'), j('View','Localize','UILocalizeText.cs'));

// ─── Phase 12: Data-dependent utility → View ──────────────────────────
console.log('\n=== Phase 12: Data-dependent Utility → View ===');
fs.mkdirSync(j('View','Utility'), { recursive: true });
moveWithMeta(j('Domain','Utility','ItemHelper.cs'), j('View','Utility','ItemHelper.cs'));

// ─── Phase 13: Constants → View ───────────────────────────────────────
console.log('\n=== Phase 13: Constants → View ===');
moveWithMeta(j('Domain','Common','Constants.cs'), j('View','Constants.cs'));

// ─── Phase 14: Lobby → View/Lobby ─────────────────────────────────────
console.log('\n=== Phase 14: Lobby → View/Lobby ===');
fs.mkdirSync(j('View','Lobby'), { recursive: true });
const lobbyDir = j('Lobby');
if (fs.existsSync(lobbyDir)) {
  for (const item of fs.readdirSync(lobbyDir)) {
    if (item.includes('CastleHero.Lobby.asmdef')) continue;
    moveWithMeta(path.join(lobbyDir, item), j('View','Lobby',item));
  }
}

// ─── Phase 15: GamePlay UI → View ─────────────────────────────────────
console.log('\n=== Phase 15: GamePlay UI/Behaviours → View ===');
fs.mkdirSync(j('View','InGame'), { recursive: true });
moveDirWithMeta(j('GamePlay','InGame','UI'), j('View','InGame','UI'));
moveDirWithMeta(j('GamePlay','InGame','Behaviours'), j('View','InGame','Behaviours'));

fs.mkdirSync(j('View','Castle'), { recursive: true });
for (const f of ['UICastle.cs', 'UIGlobalSkillDisplay.cs']) {
  moveWithMeta(j('GamePlay','Castle',f), j('View','Castle',f));
}

// ─── Phase 16: Network UITest → View ──────────────────────────────────
console.log('\n=== Phase 16: Network UITest → View ===');
fs.mkdirSync(j('View','Test'), { recursive: true });
moveWithMeta(j('Network','Service','Test','UITest.cs'), j('View','Test','UITest.cs'));

// ─── Summary ───────────────────────────────────────────────────────────
console.log('\n=== Done! Summary ===');
for (const mod of ['Common', 'Data', 'View', 'Network.Impl.Local', 'GamePlay', 'Network', 'Domain']) {
  console.log(`  ${mod}/: ${countCs(j(mod))} .cs files`);
}
