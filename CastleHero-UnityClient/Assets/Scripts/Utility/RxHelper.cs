using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.Common.Behaviours;
using RGLabs.Data;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Utility
{
    public static class RxHelper
    {
        public static void Update<T>(this ReactiveCollection<T> collection, T value, Predicate<T> findTarget)
        {
            var exist = collection.FirstOrDefault(findTarget.Invoke);
            if (exist == null)
                return;

            int index = collection.IndexOf(exist);
            if (value != null)
                collection.Insert(index, value);

            collection.Remove(exist);
        }

        public static void Update<T>(this ReactiveCollection<T> collection, IEnumerable<T> values)
        {
            collection.Clear();

            foreach (var value in values)
            {
                collection.Add(value);
            }
        }

        public static IObservable<ReactiveCollection<T>> ChangeAsObservable<T>(this ReactiveCollection<T> collection)
        {
            return Observable.Create<ReactiveCollection<T>>(observer =>
            {
                var clear = collection.ObserveReset()
                    .Subscribe(_ => observer.OnNext(collection));

                var add = collection.ObserveAdd()
                    .Subscribe(_ => observer.OnNext(collection));

                var remove = collection.ObserveRemove()
                    .Subscribe(_ => observer.OnNext(collection));

                return Disposable.Create(() =>
                {
                    clear.Dispose();
                    add.Dispose();
                    remove.Dispose();
                });
            });
        }

        public static void SubscribeMessage<T>(this MonoBehaviour behaviour, Action<T> onReceive)
        {
            MessageBroker.Default
                .Receive<T>()
                .Subscribe(onReceive)
                .AddTo(behaviour);
        }

        public static void Publish<T>(this T data) => MessageBroker.Default.Publish(data);

        public static void SubscribeButton(this MonoBehaviour behaviour, Button button, Action onClick,
            float clickThreshold = 0.25f)
        {
            behaviour.SubscribeButton(button, onClick, Storage.soundPath.button, clickThreshold);
        }
        
        public static void SubscribeButton(this MonoBehaviour behaviour, Button button, Action onClick,
            string clickSfx, float clickThreshold = 0.25f)
        {
            button
                .OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(clickThreshold))
                .Subscribe(_ =>
                {
                    if(!string.IsNullOrEmpty(clickSfx) && Context.sounds != null)
                        Context.sounds.PlaySfx(clickSfx);
                    
                    onClick.Invoke();
                })
                .AddTo(behaviour);
        }
    }
}