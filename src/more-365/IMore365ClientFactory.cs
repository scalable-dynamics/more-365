using System;

namespace more365
{
    public interface IMore365ClientFactory
    {
        IDynamicsClient CreateDynamicsClient(Guid? impersonateAzureADObjectId = null);

        IGraphClient CreateGraphClient();

        ISharePointClient CreateSharePointClient();
    }
}