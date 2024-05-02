using System.Collections.Generic;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Pattern;
using RGLabs.Unit.Behaviours;

namespace RGLabs.Unit
{
    public class ProjectileLauncher
    {
        private readonly UnitBehaviour _unit;
        private readonly AddressablePool<PoolItemBase> _pool;

        public ProjectileLauncher(UnitBehaviour unit, string prefab)
        {
            _unit = unit;
            _pool = unit.Container.Get(prefab);
        }

        public async void Launch(UnitBehaviour target)
        {
            var projectile = await _pool.Get() as Projectile;
            if (projectile == null)
                return;

            projectile.Container = _unit.Container;
            projectile.transform.position = _unit.Center;
            projectile.Fire(target);
        }

        public void Launch(IEnumerable<UnitBehaviour> targets)
        {
            foreach (var target in targets)
            {
                Launch(target);
            }
        }
    }
}