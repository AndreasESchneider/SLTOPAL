using System;
using System.Collections.Generic;

namespace slToTopalModel.Model
{
    public class SlExportInvoice
    {
        public string InvoiceNumber { get; set; }

        public SlExportCustomer Customer { get; set; }

        public string InvoiceTitle { get; set; }

        public DateTime InvoiceDate { get; set; }

        public decimal TotalAmount { get; set; }

        public List<SlExportArticle> Articles { get; set; }

        public string PayslipCode { get; set; }

        public string FiscalYear { get; set; }
    }
}