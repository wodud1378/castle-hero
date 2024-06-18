using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RGLabs.Unit.Behaviours;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

namespace RGLabs.Editor
{
    public static class UnitHelper
    {
        [MenuItem("GameObject/RGLabs/Animation Events Correction")]
        public static void AnimationEventsCorrection()
        {
            var selection = Selection.gameObjects[0];
            if (!selection.TryGetComponent(out UnitBehaviour unit))
                return;
            
            var spine = unit.GetComponentInChildren<SkeletonMecanim>();
            if (spine == null)
                return;
            
            var events = spine.GetComponent<AnimationEvents>();
            if (events == null)
                return;

            var animator = spine.GetComponent<Animator>();
            var controller = animator.runtimeAnimatorController;
            var clips = controller.animationClips;

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
                    begin = nameof(events.OnExecuteSkill);
                    end = nameof(events.OnReleaseSkill);
                }
                else
                    continue;

                var clipEvents = AnimationUtility
                    .GetAnimationEvents(clip)
                    .GroupBy(ev => ev.time)
                    .Select(group => group.First())
                    .ToArray();

                if (clipEvents.Length != 2)
                {
                    Debug.LogError($"{clip.length} 클립의 이벤트 개수가 잘못되었습니다.");
                    continue;
                }

                clipEvents[0].functionName = begin;
                clipEvents[1].functionName = end;
                
                AnimationUtility.SetAnimationEvents(clip, clipEvents);
            }
        }
        
        [MenuItem("GameObject/RGLabs/Attach Effect Body")]
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
                if(!tr.TryGetComponent(out BoneFollower boneFollower))
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

            var type = typeof(UnitBehaviour);
            var property = type.GetProperty("EffectBody", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property == null)
                return;

            var method = property.GetSetMethod(nonPublic: true);
            if (method == null)
                return;

            if (!body.TryGetComponent(out EffectBody effectBody))
                effectBody = body.gameObject.AddComponent<EffectBody>();
            
            effectBody.top = GetTransform(body, "Effect_Top");
            effectBody.middle = GetTransform(body, "Effect_Middle");
            effectBody.bottom = GetTransform(body, "Effect_Bottom");
            
            AttachBoneFollower(effectBody.top, spine, "Effect_Top");
            AttachBoneFollower(effectBody.middle, spine, "Effect_Middle");
            AttachBoneFollower(effectBody.bottom, spine, "Effect_Bottom");
            
            property.SetValue(unit, effectBody);
            
            EditorUtility.SetDirty(unit);
        }
    }
}