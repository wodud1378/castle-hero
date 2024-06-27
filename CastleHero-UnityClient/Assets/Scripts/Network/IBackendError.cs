using BackEnd;
using BackEnd.MultiSettings;
using Cysharp.Threading.Tasks;

namespace RGLabs.Network
{
    public interface IBackendErrorHandler
    {
        public void OnMaintenance();
        public void OnTooManyRequest();
        public void OnTooManyRequestByLocal();
        public void OnOtherDeviceLoginDetected();
        public void OnDeviceBlocked();
    }

    public static class ErrorHandlerHelper
    {
        public static void Attach(this IBackendErrorHandler handler)
        { 
            Backend.ErrorHandler.OnMaintenanceError += handler.OnMaintenance;
            Backend.ErrorHandler.OnTooManyRequestError += handler.OnTooManyRequest;
            Backend.ErrorHandler.OnTooManyRequestByLocalError += handler.OnTooManyRequestByLocal;
            Backend.ErrorHandler.OnOtherDeviceLoginDetectedError += handler.OnOtherDeviceLoginDetected;
            Backend.ErrorHandler.OnDeviceBlockError += handler.OnDeviceBlocked;
        }

        public static void Detach(this IBackendErrorHandler handler)
        {
            Backend.ErrorHandler.OnMaintenanceError -= handler.OnMaintenance;
            Backend.ErrorHandler.OnTooManyRequestError -= handler.OnTooManyRequest;
            Backend.ErrorHandler.OnTooManyRequestByLocalError -= handler.OnTooManyRequestByLocal;
            Backend.ErrorHandler.OnOtherDeviceLoginDetectedError -= handler.OnOtherDeviceLoginDetected;
            Backend.ErrorHandler.OnDeviceBlockError -= handler.OnDeviceBlocked;
        }
    }
}