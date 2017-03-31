namespace slToTopalModel.Model
{
    public class SlExportArticle
    {
        public string AccountCode { get; set; }

        public string ArticleCode { get; set; }

        public string CostCenterCode { get; set; }

        public bool IsVatInclusive { get; set; }

        public string Text { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal Vat { get; set; }

        public string VatCode { get; set; }
    }
}