using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.Common.InApp;
using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
using CastleHero.Data.Model;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;

namespace CastleHero.Network.Impl.Local.Services
{
    public class LocalShopService : LocalNetworkServiceBase, IShopService
    {
        private IAPManager _inApp;

        public LocalShopService(IServiceLocator sl, LocalUserDataStore store) : base(sl, store) { }

        public void RegisterIAP(IAPManager iap) => _inApp = iap;

        public string InAppPrice(string productKey, int fallBack = -1)
            => _inApp?.GetLocalizedPrice(productKey) ?? $"\uffe6 {fallBack:N0}";

        public UniTask<Result> RefreshProducts()
        {
            var get = Get();
            if (!get.IsSuccess)
                return UniTask.FromResult(Result.Error(get.error));

            var shopRecord = get.data.shopRecord;
            var products = shopRecord?.products;
            if (products == null || products.Count == 0)
                return UniTask.FromResult(Result.Complete());

            var currentTime = ServerTime.Now;
            products.ForEach(product =>
            {
                if (!Db.Shop.TryFind(product.shopId, out var entity))
                    return;

                if (entity.totalCount <= 0 || product.nextReset > currentTime)
                    return;

                product.current.byFree = 0;
                product.current.byAd = 0;
                product.current.byDefault = 0;
            });

            Save(get.data);

            return UniTask.FromResult(Result.Complete());
        }

        public UniTask<Result<Pack>> ReceiveSubscribedItems()
        {
            var get = Get();
            if (!get.IsSuccess)
                return UniTask.FromResult(Result<Pack>.Error(get.error));

            var userData = get.data;
            var currentTime = ServerTime.Now;
            if (userData.shopRecord?.products == null || userData.shopRecord.products.Count == 0)
                return UniTask.FromResult(Result<Pack>.Error(Error.InvalidRequest));

            var products = userData.shopRecord.products;
            var packs = new List<Pack>();
            products.ForEach(x =>
            {
                if (x.expireDate <= currentTime)
                    return;

                if ((currentTime.Date - x.updatedAt.Date).TotalDays <= 0)
                    return;

                if (!Db.Shop.TryFind(x.shopId, out var entity))
                    return;

                if (!Db.ShopGroup.TryFind(entity.groupId, out var groupEntity))
                    return;

                x.updatedAt = currentTime;
                packs.Add(GetPack(groupEntity));
            });

            if (packs.Count == 0)
                return UniTask.FromResult(Result<Pack>.Error(Error.InvalidRequest));

            var total = new Pack
            {
                currency = new(),
                items = new(),
                unitIds = new()
            };

            packs.ForEach(x =>
            {
                total.currency += x.currency;

                if (x.items != null)
                    total.items.Join(x.items);

                if (x.unitIds != null)
                    total.unitIds.AddRange(x.unitIds);
            });

            if (!total.currency.IsEmpty())
                userData.currency += total.currency;

            var inventory = userData.inventory;
            bool hasPackItem = total.items.Count > 0;
            bool hasSoulItem = false;

            if (total.unitIds.Count > 0)
            {
                UnitGen.AddUnits(total.unitIds, userData.characters.units, inventory.items, true,
                    out _, out bool itemAdded);

                hasSoulItem = itemAdded;
            }

            if (hasPackItem || hasSoulItem)
                inventory.items.Join(total.items);

            Save(userData);

            return UniTask.FromResult(Result<Pack>.Complete(total));
        }

        public UniTask<Result<ItemBought>> BuyItem(PaymentType type, int id)
        {
            if (!Db.Shop.TryFind(id, out var entity) ||
                !Db.ShopGroup.TryFind(entity.groupId, out var groupEntity))
                return UniTask.FromResult(Result<ItemBought>.Error(Error.DataNotFound));

            var pack = GetPack(groupEntity);

            var get = Get();
            if (!get.IsSuccess)
                return UniTask.FromResult(Result<ItemBought>.Error(get.error));

            var userData = get.data;
            var currency = userData.currency;
            var inventory = userData.inventory;
            var record = userData.shopRecord;
            if (!HasPrevItem(record, entity))
                return UniTask.FromResult(Result<ItemBought>.Error(Error.NotOpenedProduct));

            var currentTime = ServerTime.Now;
            record.products ??= new();
            var product = record.products.Find(x => x.shopId == id);
            if (product == null)
            {
                product = new Product { shopId = id };
                record.products.Add(product);
            }

            if (!TryPurchase(currency, inventory, product, entity, currentTime, type,
                    out var error, out var history))
                return UniTask.FromResult(Result<ItemBought>.Error(error));

            int duration = entity.duration;
            if (duration > 0)
            {
                var expire = product.expireDate;
                product.expireDate = expire > currentTime
                    ? expire.AddDays(duration)
                    : currentTime.AddDays(entity.duration);
            }

            record.histories ??= new();
            record.histories.Add(history);

            if (!pack.currency.IsEmpty())
                userData.currency += pack.currency;

            bool hasPackItem = pack.items.Count > 0;
            bool hasSoulItem = false;
            if (pack.unitIds.Count > 0)
            {
                UnitGen.AddUnits(pack.unitIds, userData.characters.units, inventory.items, true,
                    out _, out bool itemAdded);

                hasSoulItem = itemAdded;
            }

            if (hasPackItem || hasSoulItem)
                inventory.items.Join(pack.items);

            Save(userData);

            return UniTask.FromResult(Result<ItemBought>.Complete(new ItemBought { pack = pack }));
        }

        private bool HasPrevItem(ShopRecordDto record, ShopItemEntity entity) =>
            entity.prevItem == 0 ||
            record.products?.Find(x => x.shopId == entity.prevItem) != null;

        private bool TryPurchase(CurrencyDto currency, InventoryDto inventory, Product product, ShopItemEntity entity,
            DateTime currentTime, PaymentType type, out Error error, out ShopRecordDto.History history)
        {
            history = null;

            if (!IsProductTimeValid(currentTime, entity))
            {
                error = Error.InvalidDate;
                return false;
            }

            product.updatedAt = currentTime;

            if (ShopHelper.IsSoldOut(entity, product, out bool byDefault, out bool byAd, out bool byFree))
            {
                error = Error.SoldOut;
                return false;
            }

            bool soldOutByType = true;
            switch (type)
            {
                case PaymentType.Default:
                    soldOutByType = byDefault;
                    if (!soldOutByType &&
                        !Purchase(currency, inventory, entity.costId, entity.costValue, out error))
                        return false;

                    product.current.byDefault = soldOutByType
                        ? product.current.byDefault
                        : product.current.byDefault + 1;

                    ++product.total.byDefault;
                    break;
                case PaymentType.Ad:
                    soldOutByType = byAd;
                    product.current.byAd = soldOutByType
                        ? product.current.byAd
                        : product.current.byAd + 1;

                    ++product.total.byAd;
                    break;
                case PaymentType.Free:
                    soldOutByType = byFree;
                    product.current.byFree = soldOutByType
                        ? product.current.byFree
                        : product.current.byFree + 1;

                    ++product.total.byFree;
                    break;
            }

            if (soldOutByType)
            {
                error = Error.SoldOutByType;
                return false;
            }

            if (ShopHelper.IsSoldOut(entity, product, out _, out _, out _))
                product.nextReset = currentTime.AddDays(entity.resetDays);

            error = Error.None;
            history = new ShopRecordDto.History
            {
                id = entity.Id,
                type = (int)type,
                time = currentTime
            };

            return true;
        }

        private bool Purchase(CurrencyDto currency, InventoryDto inventory, int id, int amount, out Error error)
        {
            if (id == 0)
            {
                error = Error.None;
                return true;
            }

            if (id.IsCurrency())
            {
                if (!currency.TryConsume(id, amount))
                {
                    error = Error.NotEnoughCurrency;
                    return false;
                }
            }
            else
            {
                if (!inventory.items.TryConsumeItem(id, amount))
                {
                    error = Error.NotEnoughItem;
                    return false;
                }
            }

            error = Error.None;
            return true;
        }

        private bool IsProductTimeValid(DateTime time, ShopItemEntity entity)
        {
            if (entity.startDate == default)
                return false;

            bool started = time >= entity.startDate;
            if (entity.endDate == default)
                return started;

            return started && time < entity.endDate;
        }

        private Pack GetPack(ShopItemGroupEntity entity)
        {
            int index = 0;
            var currency = new CurrencyDto();
            var items = new List<IItem>();
            var unitIds = new List<int>();
            while (index.IsValidIndex(entity.ids, entity.quantities))
            {
                int id = entity.ids[index];
                int quantity = entity.quantities[index];

                if (id / 10000 == 1)
                    unitIds.Add(id);
                else
                    ItemGen.NewItems(id, quantity, currency, items);

                ++index;
            }

            return new Pack
            {
                currency = currency,
                items = items,
                unitIds = unitIds
            };
        }
    }
}
