using more365.SharePoint;
using System.Threading.Tasks;

namespace more365
{
    public interface ISharePointClient
    {
        Task<SharePointFolder> CreateFolder(string documentLibraryName, string folderPath);

        Task<byte[]> DownloadFile(string filePath);

        Task<string> GetFilePreviewUrl(string filePath);

        Task<SharePointFolder> GetFolder(string documentLibraryName, string folderPath = "");

        Task<SharePointFile> UploadFile(string fileName, byte[] file, string documentLibraryName, string folderPath = "");
    }
}