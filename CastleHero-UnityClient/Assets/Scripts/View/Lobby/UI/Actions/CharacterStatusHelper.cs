using System.Collections.Generic;
using CastleHero.Data.Model;
using CastleHero.GamePlay.Unit;
using CastleHero.GamePlay.Unit.Components;
using CastleHero.Network.Shared;
using CastleHero.Utility;

namespace CastleHero.View.Lobby.UI.Actions
{
    /// <summary>
    /// 캐릭터 기본 스탯 계산 유틸리티.
    /// PopupCharacter 등에서 사용하던 BaseStatus 로직을 재사용 가능한 정적 메서드로 분리.
    /// </summary>
    public static class CharacterStatusHelper
    {
        /// <summary>
        /// 유닛의 레벨/각성 등급 기반 기본 스탯을 계산하여 반환한다.
        /// </summary>
        public static Dictionary<Status.Type, float> BaseStatus(
            int lv, int rate, UnitEntity unit, UnitBalanceEntity balance)
        {
            balance.AdditionalStatus(lv, rate, out var additional, out _);

            return new Dictionary<Status.Type, float>
            {
                { Status.Type.Hp, unit.hp + additional.GetValueOrDefault(Status.Type.Hp) },
                { Status.Type.Atk, unit.atk + additional.GetValueOrDefault(Status.Type.Atk) },
                { Status.Type.Critical, unit.critical + additional.GetValueOrDefault(Status.Type.Critical) },
                { Status.Type.CriticalAtk, unit.criticalAtk + additional.GetValueOrDefault(Status.Type.CriticalAtk) },
                { Status.Type.AtkSpeed, unit.atkSpeed + additional.GetValueOrDefault(Status.Type.AtkSpeed) },
                { Status.Type.MoveSpeed, unit.speed + additional.GetValueOrDefault(Status.Type.MoveSpeed) },
                { Status.Type.AtkRange, unit.atkRange + additional.GetValueOrDefault(Status.Type.AtkRange) },
                { Status.Type.MoveRange, unit.moveRange + additional.GetValueOrDefault(Status.Type.MoveRange) },
            };
        }
    }
}
