namespace CastleHero.Network.Impl.Backend
{
    public interface IBackendErrorHandler
    {
        public void OnMaintenance();
        public void OnTooManyRequest();
        public void OnTooManyRequestByLocal();
        public void OnOtherDeviceLoginDetected();
        public void OnDeviceBlocked();
    }

    public static class BackendErrorHandlerExtensions
    {
        public static void Attach(this IBackendErrorHandler handler)
        {
            global::BackEnd.Backend.ErrorHandler.OnMaintenanceError += handler.OnMaintenance;
            global::BackEnd.Backend.ErrorHandler.OnTooManyRequestError += handler.OnTooManyRequest;
            global::BackEnd.Backend.ErrorHandler.OnTooManyRequestByLocalError += handler.OnTooManyRequestByLocal;
            global::BackEnd.Backend.ErrorHandler.OnOtherDeviceLoginDetectedError += handler.OnOtherDeviceLoginDetected;
            global::BackEnd.Backend.ErrorHandler.OnDeviceBlockError += handler.OnDeviceBlocked;
        }

        public static void Detach(this IBackendErrorHandler handler)
        {
            global::BackEnd.Backend.ErrorHandler.OnMaintenanceError -= handler.OnMaintenance;
            global::BackEnd.Backend.ErrorHandler.OnTooManyRequestError -= handler.OnTooManyRequest;
            global::BackEnd.Backend.ErrorHandler.OnTooManyRequestByLocalError -= handler.OnTooManyRequestByLocal;
            global::BackEnd.Backend.ErrorHandler.OnOtherDeviceLoginDetectedError -= handler.OnOtherDeviceLoginDetected;
            global::BackEnd.Backend.ErrorHandler.OnDeviceBlockError -= handler.OnDeviceBlocked;
        }
    }
}
