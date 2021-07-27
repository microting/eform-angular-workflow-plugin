namespace Workflow.Pn.Infrastructure.Models
{
    public class FieldValue
    {
        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public UploadedDataObj UploadedDataObj { get; set; }

        public int Id { get; set; }

        public int FieldId { get; set; }
    }
}