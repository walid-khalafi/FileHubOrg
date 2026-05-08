namespace FileHubOrg.Web.Models.FileViewModels
{
    public class IndexViewModel
    {
        public Guid? labelId { get; set; }
        public string? labelName { get; set; }
       public List<Domain.Entities.File.FileMetaData> files { get; set; }
    }
}
