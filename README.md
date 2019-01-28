# more-365

Create more applications using the Microsoft 365 platform

> more-365 enables communicating with Microsoft 365 services from a server-side application (aspnetcore / Azure Functions)

For Example: Azure AD Authentication using Certificates from Key Vault, Dynamics 365 Queries + Batches, SharePoint File Upload / Download, Graph API convert to PDF + send email

License: [MIT](http://www.opensource.org/licenses/mit-license.php)

## Nuget.org
The package name is more-365, you can find it here:

...

## C# Example

```c#
public static async Task<Entity> Execute(DynamicsClient dynamics, Guid entityId)
{
    var query = dynamics.CreateQuery<Entity>()
                        .Select(e => new
                        {
                            e.CreatedBy,
                            e.CreatedOn
                        });
                        .Where(e => e.EntityId = entityId);

    var fetchXml = query.ToFetchXml();

    try
    {
        var queryResult = await dynamics.ExecuteQuery(query);
        return queryResult.FirstOrDefault();
    }
    catch (DynamicsClientException ex)
    {
        throw new Exception(ex.Details);
    }
}
```

## Interfaces

```c#
public interface IAuthenticationClient
{
    Task<AuthenticationToken> GetAuthenticationTokenAsync(string resource);

    Task<AuthenticationToken> GetAuthenticationTokenAsync(Uri resource);
}

public interface IAuthenticatedHttpClientFactory
{
    HttpClient CreateAuthenticatedHttpClient(string resource, Guid? uniqueId = null);
}

public interface IMore365ClientFactory
{
    IDynamicsClient CreateDynamicsClient(Guid? impersonateAzureADObjectId = null);

    IGraphClient CreateGraphClient();

    ISharePointClient CreateSharePointClient();
}
```

## Dynamics Interfaces

```c#
public interface IDynamicsClient
{
    Task<IEnumerable<T>> ExecuteBatch<T>(params BatchRequest[] requests);

    Task<IEnumerable<T>> ExecuteQuery<T>(string url);

    Task<T> ExecuteSingle<T>(string url);

    Task<T> Get<T>(string entitySetName, Guid id, params string[] columns);

    Task<Guid> Save(string entitySetName, object data, Guid? id = null);
}

public interface IXrmQueryExpression
{
    IXrmQueryExpression Select(params string[] attributeNames);

    IXrmQueryExpression Where(string attributeName, XrmQueryOperator expressionOperator, params object[] values);

    IXrmQueryExpression WhereAny(Action<XrmQueryExpressionWhere> or);

    IXrmQueryExpression OrderBy(string attributeName, bool isDescendingOrder = false);

    IXrmQueryExpression Join(string entityName, string attributeToName, string attributeFromName = "", bool isOuterJoin = false, string joinAlias = "");
}
```

## SharePoint & Graph Interfaces

```c#
public interface ISharePointClient
{
    Task<SharePointFolder> CreateFolder(string documentLibraryName, string folderPath);

    Task<byte[]> DownloadFile(string filePath);

    Task<string> GetFilePreviewUrl(string filePath);

    Task<SharePointFolder> GetFolder(string documentLibraryName, string folderPath = "");

    Task<SharePointFile> UploadFile(string fileName, byte[] file, string documentLibraryName, string folderPath = "");
}

public interface IGraphClient
{
    Task<byte[]> DownloadFileAsPdf(string filePath);

    Task SendOutlookEmail(string subject, string content, string fromSender, params string[] toRecipients);
}
```

## Configuration

```c#
public class More365Configuration
{
    public Uri DynamicsUrl { get; set; }

    public Uri SharePointUrl { get; set; }

    public Guid AzureADTenantId { get; set; }

    public Guid AzureADApplicationId { get; set; }

    public string AzureADAppCertificateKey { get; set; }

    public string AzureADAppClientSecretKey { get; set; }
}

public enum XrmQueryOperator
{
    Contains,
    NotContains,
    StartsWith,
    EndsWith,
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    In,
    NotIn,
    OnOrBefore,
    OnOrAfter,
    Null,
    NotNull,
    IsCurrentUser,
    IsCurrentTeam,
    IsNotCurrentUser,
    IsNotCurrentTeam
}
```