namespace DotnetCoding.Core.Models
{
    public class ApprovalQueueItem
    {
        public int Id { get; set; }
        public ProductDetails Product { get; set; }  
        public string Reason { get; set; }  
        public Date Time RequestDate { get; set; }  
    }
}