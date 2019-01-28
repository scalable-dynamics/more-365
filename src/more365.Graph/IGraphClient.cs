using System.Threading.Tasks;

namespace more365
{
    public interface IGraphClient
    {
        Task<byte[]> DownloadFileAsPdf(string filePath);

        Task SendOutlookEmail(string subject, string content, string fromSender, params string[] toRecipients);
    }
}