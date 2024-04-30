using RGLabs.Common.Pattern;
using RGLabs.Data.Repositories;
using RGLabs.Unit.Factory;
using UnityEngine;

namespace RGLabs.Common.Behaviours
{
    public class SceneBehaviour : MonoBehaviour
    {
       protected DBCollections db;

       protected InGameRepository gameRepo;
       protected UserRepository userRepo;

       protected IUnitFactory unitFactory;
       protected PoolContainer poolContainer;
    }
}