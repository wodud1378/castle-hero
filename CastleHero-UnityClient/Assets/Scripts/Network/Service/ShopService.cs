using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common.InApp;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;
using RGLabs.Utility;

namespace RGLabs.Network.Service
{
    public class ShopService : NetworkServiceBase
    {
        public IAPManager InApp { get; private set; }

        public void RegisterIAP(IAPManager iap) => InApp = iap;
        
        public async UniTask<Result> RefreshProducts()
        {
            var get = await GetTable<ShopRecordDto>(Table.ShopRecord);
            if (!get.IsSuccess)
                return Result.Error(get.error);

            var shopRecord = get.data;
            var products = shopRecord.products;
            if (products == null || products.Count == 0)
                return Result.Complete();

            var getTime = await GetServerTime();
            if (!getTime.IsSuccess)
                return Result.Error(getTime.error);

            var currentTime = getTime.data;
            products.ForEach(product =>
            {
                if (!Storage.db.shop.TryFind(product.shopId, out var entity))
                    return;

                if (entity.totalCount <= 0 || product.nextReset > currentTime)
                    return;

                product.current.byFree = 0;
                product.current.byAd = 0;
                product.current.byDefault = 0;
            });

            var update = await UpdateTable(Table.ShopRecord, shopRecord);
            if (!update.IsSuccess)
                return Result.Error(update.error);

            Storage.userRepository.shopRecord.Update(shopRecord);
            return Result.Complete();
        }

        public async UniTask<Result<Pack>> ReceiveSubscribedItems()
        {
            var get = await GetTables(Table.Currency, Table.Inventory, Table.Character, Table.ShopRecord);
            if (!get.IsSuccess)
                return Result<Pack>.Error(get.error);

            var userData = get.data;
            var getServerTime = await GetServerTime();
            if (!getServerTime.IsSuccess)
                return Result<Pack>.Error(getServerTime.error);

            var currentTime = getServerTime.data;
            if (userData.shopRecord?.products == null || userData.shopRecord.products.Count == 0)
                return Result<Pack>.Error(Error.InvalidRequest);

            var products = userData.shopRecord.products;
            var packs = new List<Pack>();
            products.ForEach(x =>
            {
                if (x.expireDate <= currentTime)
                    return;

                if ((currentTime.Date - x.updatedAt.Date).TotalDays <= 0)
                    return;

                if (!Storage.db.shop.TryFind(x.shopId, out var entity))
                    return;

                if (!Storage.db.shopGroup.TryFind(entity.groupId, out var groupEntity))
                    return;

                x.updatedAt = currentTime;
                packs.Add(GetPack(groupEntity));
            });

            if (packs.Count == 0)
                return Result<Pack>.Error(Error.InvalidRequest);

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

            var tables = new Dictionary<Table, object> { { Table.ShopRecord, userData.shopRecord } };
            if (!total.currency.IsEmpty())
            {
                userData.currency += total.currency;
                tables.Add(Table.Currency, userData.currency);
            }

            bool hasPackItem = total.items.Count > 0;
            bool hasSoulItem = false;
            var inventory = userData.inventory;

            if (total.unitIds.Count > 0)
            {
                UnitGen.AddUnits(total.unitIds, userData.characters.units, inventory.items, true,
                    out int newUnitStartIndex, out bool itemAdded);

                hasSoulItem = itemAdded;

                if (newUnitStartIndex != -1)
                    tables.Add(Table.Character, userData.characters);
            }

            if (hasPackItem || hasSoulItem)
            {
                inventory.items.Join(total.items);
                tables.Add(Table.Inventory, inventory);
            }

            var update = await UpdateTables(tables);
            return update.IsSuccess
                ? Result<Pack>.Complete(total)
                : Result<Pack>.Error(update.error);
        }

        public async UniTask<Result<ItemBought>> BuyItem(PaymentType type, int id)
        {
            if (!Storage.db.shop.TryFind(id, out var entity) ||
                !Storage.db.shopGroup.TryFind(entity.groupId, out var groupEntity))
                return Result<ItemBought>.Error(Error.DataNotFound);

            var pack = GetPack(groupEntity);
            bool hasUnit = pack.unitIds.Count > 0;
            var readTables = new List<Table>
            {
                Table.Currency,
                Table.Inventory,
                Table.ShopRecord
            };

            if (hasUnit)
                readTables.Add(Table.Character);

            var get = await GetTables(readTables);
            if (!get.IsSuccess)
                return Result<ItemBought>.Error(get.error);

            var userData = get.data;
            var currency = userData.currency;
            var inventory = userData.inventory;
            var record = userData.shopRecord;
            if (!HasPrevItem(record, entity))
                return Result<ItemBought>.Error(Error.NotOpenedProduct);

            var getTime = await GetServerTime();
            if (!getTime.IsSuccess)
                return Result<ItemBought>.Error(getTime.error);

            var currentTime = getTime.data;
            record.products ??= new();
            var product = record.products.Find(x => x.shopId == id);
            if (product == null)
            {
                product = new Product { shopId = id };
                record.products.Add(product);
            }

            if (!TryPurchase(currency, inventory, product, entity, currentTime, type,
                    out var error, out var history))
                return Result<ItemBought>.Error(error);

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

            var tables = new Dictionary<Table, object> { { Table.ShopRecord, record } };
            if (!pack.currency.IsEmpty())
            {
                currency += pack.currency;
                tables.Add(Table.Currency, currency);
            }

            bool hasPackItem = pack.items.Count > 0;
            bool hasSoulItem = false;
            if (hasUnit)
            {
                UnitGen.AddUnits(pack.unitIds, userData.characters.units, inventory.items, true,
                    out int newUnitStartIndex, out bool itemAdded);

                hasSoulItem = itemAdded;

                if (newUnitStartIndex != -1)
                    tables.Add(Table.Character, userData.characters);
            }

            if (hasPackItem || hasSoulItem)
            {
                inventory.items.Join(pack.items);
                tables.Add(Table.Inventory, inventory);
            }

            var update = await UpdateTables(tables);
            return update.IsSuccess
                ? Result<ItemBought>.Complete(new() { pack = pack })
                : Result<ItemBought>.Error(update.error);
        }

        private bool HasPrevItem(ShopRecordDto record, ShopItemEntity entity) =>
            entity.prevItem == 0 ||
            record.products?.Find(x => x.shopId == entity.prevItem) != null;

        private bool TryPurchase(CurrencyDto currency, InventoryDto inventory, Product product, ShopItemEntity entity,
            DateTime currentTime, PaymentType type, out Error error, out ShopRecordDto.History history)
        {
            history = null;

            if (!IsProductTImeValid(currentTime, entity))
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
            // 임시 인앱 결제 성공처리.
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

        private bool IsProductTImeValid(DateTime time, ShopItemEntity entity)
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