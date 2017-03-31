using System;
using slToTopalClient.Invoice;
using slToTopalModel.Model;
using TopalServerAPI;

namespace slToTopalClient.Interfaces
{
    public interface ISlTopalInvoice
    {
        SlTopalInvoiceErrorCode LastErrorCode { get; }

        string LastErrorMessage { get; set; }

        bool CreateInvoice(IParty party, SlExportInvoice slExportInvoice, out IInvoice invoice);

        bool FindInvoice(string invoiceNumber, out IInvoice invoice);

        bool IsPaid(string invoiceNumber);

        bool Pay(string invoiceNumber, DateTime paymentDateTime, string accountCode);

        void Remove(IInvoice invoice);
    }
}