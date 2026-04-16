using System.Linq;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.View.Unit;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

namespace CastleHero.Editor
{
    public static class UnitHelper
    {
        [MenuItem("Tools/CastleHero/Unit/Animation Events Correction All")]
        public static void UpdateUnits()
        {
            CastleHeroEditor.ModifyAllPrefabsWithComponent<UnitBehaviour>(AnimationEventsCorrection);
        }

        [MenuItem("GameObject/CastleHero/Unit/Animation Events Correction")]
        public static void AnimationEventsCorrection()
        {
            var selection = Selection.gameObjects[0];
            if (!selection.TryGetComponent(out UnitBehaviour unit))
                return;

            AnimationEventsCorrection(unit);
        }

        [MenuItem("Tools/CastleHero/Unit/Merge Shadow")]
        public static void MergeShadow()
        {
            CastleHeroEditor.ModifyAllPrefabsWithComponent<UnitBehaviour>(MergeShadow);
        }

        private static void MergeShadow(UnitBehaviour unit)
        {
            var shadow = unit.transform.GetChild(0).Find("Shadow");
            if (shadow == null)
                return;
                    
            shadow.gameObject.SetActive(false);
            unit.GetComponentInChildren<SkeletonRenderSeparator>().enabled = false;
        }

        private static void AnimationEventsCorrection(UnitBehaviour unit)
        {
            var spine = unit.GetComponentInChildren<SkeletonMecanim>();
            if (spine == null)
                return;

            var events = spine.GetComponent<AnimationEvents>();
            if (events == null)
                return;

            var animator = spine.GetComponent<Animator>();
            var controller = animator.runtimeAnimatorController;
            var clips = controller.animationClips;
            const float tolerance = 0.001f;
            foreach (var clip in clips)
            {
                string begin;
                string end;
                if (clip.name.Contains("Attack"))
                {
                    begin = nameof(events.OnHit);
                    end = nameof(events.OnReleaseAttack);
                }
                else if (clip.name.Contains("Skill"))
                {
                    var so = new SerializedObject(clip);
                    var settings = so.FindProperty("m_AnimationClipSettings");
                    if (settings != null)
                    {
                        settings.FindPropertyRelative("m_LoopTime").boolValue = false;
                        so.ApplyModifiedProperties();
                        Debug.Log($"[{unit.name}] 스킬 루프 해제");
                    }

                    begin = nameof(events.OnExecuteSkill);
                    end = nameof(events.OnReleaseSkill);
                }
                else
                    continue;

                var clipEvents = AnimationUtility
                    .GetAnimationEvents(clip)
                    .GroupBy(ev => Mathf.Round(ev.time / tolerance) * tolerance)
                    .Select(group => group.First())
                    .ToArray();

                if (clipEvents.Length != 2)
                {
                    Debug.LogError($"[{unit.name}]{clip.name} 클립의 이벤트 개수가 잘못되었습니다.");
                    continue;
                }

                clipEvents[0].functionName = begin;
                clipEvents[1].functionName = end;

                AnimationUtility.SetAnimationEvents(clip, clipEvents);
            }
        }

        [MenuItem("GameObject/CastleHero/Attach Effect Body")]
        public static void AttachEffectBody()
        {
            Transform GetTransform(Transform root, string name)
            {
                var tr = root.Find(name);
                if (tr == null)
                {
                    tr = new GameObject(name).transform;
                    tr.SetParent(root);
                    tr.localPosition = Vector3.zero;
                    tr.localScale = Vector3.one;
                }

                return tr;
            }

            void AttachBoneFollower(Transform tr, SkeletonMecanim renderer, string name)
            {
                if (!tr.TryGetComponent(out BoneFollower boneFollower))
                    boneFollower = tr.gameObject.AddComponent<BoneFollower>();

                boneFollower.skeletonRenderer = renderer;
                boneFollower.boneName = name;
            }

            var selection = Selection.gameObjects[0];
            if (!selection.TryGetComponent(out UnitBehaviour unit))
                return;

            if (unit.Type == UnitBehaviour.BehaviourType.Barricade)
                return;

            var spine = unit.GetComponentInChildren<SkeletonMecanim>();
            if (spine == null)
                return;

            var body = spine.transform.Find("Body");
            if (body == null)
                return;

            // UnitBehaviour.EffectBody 는 런타임 Awake에서 GetComponentInChildren 로 자동 연결.
            // 에디터 툴은 프리팹 계층(Effect_Top/Middle/Bottom + BoneFollower) 셋업만 담당.
            if (!body.TryGetComponent(out EffectBody effectBody))
                effectBody = body.gameObject.AddComponent<EffectBody>();

            effectBody.top = GetTransform(body, "Effect_Top");
            effectBody.middle = GetTransform(body, "Effect_Middle");
            effectBody.bottom = GetTransform(body, "Effect_Bottom");

            AttachBoneFollower(effectBody.top, spine, "Effect_Top");
            AttachBoneFollower(effectBody.middle, spine, "Effect_Middle");
            AttachBoneFollower(effectBody.bottom, spine, "Effect_Bottom");

            EditorUtility.SetDirty(unit);
        }
    }
}