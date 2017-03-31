using System;
using slToTopalClient.Manager;
using TopalServerAPI;

namespace slToTopalClient.Interfaces
{
    public interface ISlTopalManager
    {
        IManager Manager { get; }

        string LastErrorMessage { get; set; }

        bool DeleteInvoice(IInvoice invoice);
        bool DeleteParty(IParty party);

        bool FillVatPostingAmount(IPosting articlePosting, string articleVatCode, decimal articleTotalAmount);
        bool FindAccount(string accountCode, out IAccount account);
        bool FindCostCenter(string costCenterCode, out ICostCenter costCenter);
        bool FindCountry(string countryCode, out ICountry country);
        bool FindFreeParty(string customerCustomerNumber, out IParty party);
        bool FindInvoice(string invoiceNumber, out IInvoice invoice);
        bool FindLanguage(string languageCode, out ILanguage language);
        bool FindPayTerm(string payTermCode, out IPayTerm payTerm);
        bool FindPayType(string manuell, out IPayType payType);
        string GetErrorMessage(int errorCode);
        IDisposable Lock();
        SlTopalLoginResult Login(string ip, string clientCode, string userName, string password);
        void Logout();
        bool SaveDebtor(IDebtor debtor);
        bool SaveInvoice(IInvoice invoice);
        bool SaveManualPayment(IPayment payment);
        bool SaveParty(IParty party);
        bool SetFiscalYear(IFiscalYear fiscalYear);
    }
}