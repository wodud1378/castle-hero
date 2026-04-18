using CastleHero.Data.Model;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UniRx;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Components
{
    /// <summary>
    /// 유닛의 순수 데이터/상태만 보관하는 plain C# 클래스.
    /// 전투 로직은 CombatController 에 위치한다.
    /// </summary>
    public class UnitState
    {
        public enum Teams
        {
            Monster,
            Character,
        }

        public enum States
        {
            Prepare,
            Idle,
            Move,
            Return,
            Attack,
            Skill,
            Dead,
        }

        public readonly ReactiveProperty<States> State;
        public readonly ReactiveProperty<bool> OnRest;

        public Teams Team { get; private set; }
        public Elemental Elemental;
        public LayerMask EnemyLayerMask;
        public LayerMask AlleyLayerMask;

        public bool EnableRecover;

        public UnitState()
        {
            Elemental = new();
            State = new(States.Prepare);
            OnRest = new();
        }

        public void Init(UnitInfo info, UnitEntity data)
        {
            Team = data.Id / 10000 == 1 ? Teams.Character : Teams.Monster;
            EnemyLayerMask = UnitHelper.EnemyLayerMask(data.Id, data.atkLayer);
            AlleyLayerMask = UnitHelper.AlleyLayerMask(data.Id);

            Elemental.atkType = data.elementalAtk;
            Elemental.defType = data.elementalDef;

            State.Value = States.Prepare;
        }
    }
}
