namespace slToTopalModel.Model
{
    public class SlExportInvoicePaid
    {
        public string InvoiceNumber { get; set; }

        public bool IsFound { get; set; }

        public bool IsPaid { get; set; }

        public string Reference { get; set; }
    }
}