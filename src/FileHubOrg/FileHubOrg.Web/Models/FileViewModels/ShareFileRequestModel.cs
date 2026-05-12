namespace FileHubOrg.Web.Models.FileViewModels
{
    public class ShareFileRequestModel
    {
        public Guid FileId { get; set; }
        public string[] SharedToUserIds { get; set; }
    }
}
