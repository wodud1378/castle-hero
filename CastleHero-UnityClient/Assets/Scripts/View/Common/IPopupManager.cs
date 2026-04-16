using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI.Popup;
namespace CastleHero.View.Common
{
    public interface IPopupManager
    {
        void Open<T>() where T : PopupBase;
        void Open<T>(params object[] parameters) where T : PopupBase;
        UniTask<T> OpenAsync<T>(params object[] parameters) where T : PopupBase;
        UniTask<T> OpenAsync<T>() where T : PopupBase;
        bool TryGetPopupIfExist<T>(out T popup) where T : PopupBase;
        void ReplaceToTop(PopupBase popup);
        UniTask Close<T>(T popup) where T : PopupBase;
        UniTask CloseAllAsync();
        void CloseAll();
    }
}
