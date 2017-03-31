namespace slToTopalModel.Model
{
    public class SlExportCustomer
    {
        public string AccountCode { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string City { get; set; }

        public string CountryCode { get; set; }

        public string CustomerNumber { get; set; }

        public string EMail { get; set; }

        public string FirstName { get; set; }

        public string LanguageCode { get; set; }

        public string LastName { get; set; }

        public bool NoReminders { get; set; }

        public string PaymentTermCode { get; set; }

        public string PaymentType { get; set; }

        public string Salutation { get; set; }

        public string Title { get; set; }

        public string Zip { get; set; }

        public SlExportCustomer()
        {
            PaymentType = "Manuell";
        }
    }
}