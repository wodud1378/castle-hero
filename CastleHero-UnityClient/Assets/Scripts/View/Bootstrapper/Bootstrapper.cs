using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.Common;
using CastleHero.Common.Flow;
using CastleHero.Common.Pattern;
using CastleHero.Common.Sound;
using CastleHero.Data.DB;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.GamePlay.Unit.Components;
using CastleHero.GamePlay.Unit.Effects;
using CastleHero.GamePlay.Unit.Factory;
using CastleHero.View.Common;
using CastleHero.View.Common.UI;
using CastleHero.View.Sound;
using CastleHero.View.Unit;
using UnityEngine;

using CastleHero.Network.Service;
using CastleHero.Data.UseCases;
using CastleHero.View.Castle.Actions;
using CastleHero.View.InGame.Actions;
using CastleHero.View.Lobby.Actions;
using CastleHero.View.Lobby.Prepare.Actions;
using CastleHero.View.Lobby.Shop.Actions;
using CastleHero.View.Lobby.UI.Actions;
using CastleHero.View.Lobby.UI.Inventory.Actions;

namespace CastleHero.View.Bootstrapper
{
    public static class Bootstrapper
    {
        public static readonly Queue<Func<UniTask>> OnLoadCompleteQueue = new();

        public static async UniTask<Context> LoadAsync(ContextView view)
        {
            Time.timeScale = 1f;

            var sl = ServiceLocator.Instance;

            try
            {
                var context = new Context();
                sl.Register(context);
                sl.Register(context.Back);
                sl.Register(context.Transition);
                sl.Register(context.LobbyTransition);

                var gameConstants = view.GameConstants != null
                    ? view.GameConstants
                    : ScriptableObject.CreateInstance<GameConstants>();
                sl.Register(gameConstants);

                var poolContainer = new PoolContainer();
                sl.Register(poolContainer);
                sl.Register<ISoundManager>(view.SoundManager);
                sl.Register(new EffectBuilder(poolContainer));
                sl.Register<IUnitRendererFactory>(new UnitRendererFactory());

                var db = sl.Get<IDBProvider>();
                var userRepo = sl.Get<IUserRepository>();
                sl.Register(new UnitFactory(poolContainer, db, userRepo));

                sl.Register(view.UILock);
                sl.Register(view.ToolTip);
                sl.Register(view.StartButton);
                view.StartButton.StageSelect.Init();
                sl.Register<IPopupManager>(view.PopupManager);

                // --- Action / UseCase 등록 ---
                sl.Register(new EntranceManager(sl));
                sl.Register(new SummonAction(sl));
                sl.Register(new CharacterGrowthAction(sl));
                sl.Register(new RefineAction(sl));
                sl.Register(new EquipAction(sl));
                sl.Register(new ShopAction(sl));
                sl.Register(new GameStartAction(sl));
                sl.Register(new GameClearAction(sl));
                sl.Register(new LobbyAction(sl));
                sl.Register(new CastleLvUpAction(sl));
                sl.Register(new InventoryItemActions(
                    sl.Get<IPopupManager>(),
                    db,
                    sl.Get<INetworkServiceProvider>()));

                try
                {
                    while (OnLoadCompleteQueue.Count > 0)
                    {
                        var task = OnLoadCompleteQueue.Dequeue();
                        if (task != null) await task();
                    }
                }
                finally
                {
                    OnLoadCompleteQueue.Clear();
                }

                SetEntranceTransition(context,
                    sl.Get<EntranceHolder>(),
                    userRepo,
                    sl.Get<ISoundManager>(),
                    sl.Get<SoundPath>());

                return context;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
            finally
            {
                Loading.SceneReady?.TrySetResult();
            }
        }

        private static void SetEntranceTransition(Context context, EntranceHolder entranceHolder, IUserRepository userRepo, ISoundManager sounds, SoundPath soundPath)
        {
            var entrance = entranceHolder.Current;
            if (entrance.state == State.InGame)
            {
                userRepo.Entrance.Value = entrance.gameEntrance;
                return;
            }

            context.Transition.CurrentState = entrance.state;
            context.LobbyTransition.CurrentState = LobbyState.Main;

            sounds.PlayBgm(soundPath.lobbyBgm);
        }
    }
}
