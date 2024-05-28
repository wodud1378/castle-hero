using RGLabs.Common.Behaviours;
using RGLabs.Data.Repositories;

namespace RGLabs.Lobby.UI.Inventory
{
    public class InventoryPresenter
    {
        private readonly UserRepository _repository = Context.currentBehaviour.userRepo;
    }
}