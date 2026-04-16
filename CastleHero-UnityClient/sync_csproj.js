const fs = require('fs');
const path = require('path');
const asmdefToCsproj = {
  'Assets/Scripts/Common': 'CastleHero.Common.csproj',
  'Assets/Scripts/Data': 'CastleHero.Data.csproj',
  'Assets/Scripts/Editor': 'CastleHero.Editor.csproj',
  'Assets/Scripts/GamePlay': 'CastleHero.GamePlay.csproj',
  'Assets/Scripts/Network': 'CastleHero.Network.csproj',
  'Assets/Scripts/Network.Impl.Backend': 'CastleHero.Network.Impl.Backend.csproj',
  'Assets/Scripts/Network.Impl.Local': 'CastleHero.Network.Impl.Local.csproj',
  'Assets/Scripts/View': 'CastleHero.View.csproj',
};
const roots = Object.keys(asmdefToCsproj);
function owns(root, rel) {
  if (!rel.startsWith(root + '/')) return false;
  for (const o of roots) if (o !== root && o.startsWith(root + '/') && rel.startsWith(o + '/')) return false;
  return true;
}
function walk(dir, out) {
  if (!fs.existsSync(dir)) return out;
  for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
    const f = path.join(dir, e.name).replace(/\\/g, '/');
    if (e.isDirectory()) walk(f, out);
    else if (e.isFile() && f.endsWith('.cs')) out.push(f);
  }
  return out;
}

let added = 0, removed = 0;
for (const [root, csproj] of Object.entries(asmdefToCsproj)) {
  if (!fs.existsSync(csproj)) continue;
  let c = fs.readFileSync(csproj, 'utf8');
  const before = c;

  // Remove stale entries (file doesn't exist)
  c = c.replace(/\s*<Compile Include="([^"]+)" \/>/g, (m, p) => {
    const full = path.resolve('.', p.replace(/\\/g, '/'));
    if (!fs.existsSync(full)) { removed++; return ''; }
    return m;
  });

  // Add missing entries
  const files = walk(root, []).filter(f => owns(root, f));
  const newAdds = [];
  for (const f of files) {
    const inc = f.replace(/\//g, '\\');
    if (c.includes('<Compile Include="' + inc + '"')) continue;
    newAdds.push(inc);
  }
  if (newAdds.length > 0) {
    const snip = newAdds.map(p => '    <Compile Include="' + p + '" />').join('\n') + '\n';
    c = c.replace(/(<ItemGroup>[\s\S]*?<Compile Include="[\s\S]*?)(\s*<\/ItemGroup>)/, (m, b, a) => b + '\n' + snip + a);
    added += newAdds.length;
  }

  if (c !== before) fs.writeFileSync(csproj, c);
}
console.log(`Added ${added}, Removed ${removed}`);
