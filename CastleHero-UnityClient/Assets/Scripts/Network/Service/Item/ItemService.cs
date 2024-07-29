using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service.Item
{
    public class ItemService
    {
        public async UniTask<OpenBoxResult> OpenBox(int boxItemId, int itemQty)
        {
            var itemChartId = Storage.db.items.Id;
            var statChartId = Storage.db.stats.Id;
            var result = await BackendWrapper.OpenBox(itemChartId, statChartId, boxItemId, itemQty);

            return result.data;
        }
    }
}