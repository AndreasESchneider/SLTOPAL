using System;
using System.Diagnostics;
using System.Linq;
using slToTopalClient.Interfaces;
using TopalServerAPI;

namespace slToTopalClient.Manager
{
    public class SlTopalManager : ISlTopalManager, IDisposable
    {
        private IClient _client;

        private static object ManagerLock { get; } = new object();

        public SlTopalManager(IManager manager)
        {
            Manager = manager;
        }

        public void Dispose()
        {
            Manager?.Dispose();
        }

        public bool DeleteInvoice(IInvoice invoice)
        {
            return Manager.DeleteInvoice(invoice) == 0;
        }

        public bool DeleteParty(IParty party)
        {
            return Manager.DeleteParty(party) == 0;
        }

        public bool FillVatPostingAmount(IPosting articlePosting, string articleVatCode, decimal articleTotalAmount)
        {
            var loadVatCodesResult = Manager.LoadVATCodes();

            if (loadVatCodesResult != 0)
            {
                return false;
            }

            var vat = Manager.FindVAT(articleVatCode);

            if (vat == null)
            {
                return false;
            }

            Manager.FillVATPostingAmount(articlePosting, vat, articleTotalAmount, articleTotalAmount);

            return true;
        }

        public bool FindAccount(string accountCode, out IAccount account)
        {
            Manager.LoadAccounts();

            account = Manager.FindAccount(accountCode);

            return account != null;
        }

        public bool FindCostCenter(string costCenterCode, out ICostCenter costCenter)
        {
            Manager.LoadCostCenters();

            costCenter = Manager.FindCostCenter(costCenterCode);

            return costCenter != null;
        }

        public bool FindCountry(string countryCode, out ICountry country)
        {
            Manager.LoadCountries();

            country = Manager.FindCountry(countryCode);

            return country != null;
        }

        public bool FindFreeParty(string customerCustomerNumber, out IParty party)
        {
            Manager.LoadParties();

            return Manager.FindFreeParty(customerCustomerNumber, out party) == 0;
        }

        public bool FindInvoice(string invoiceNumber, out IInvoice invoice)
        {
            Manager.LoadInvoices();

            var findFreeInvoiceResult = Manager.FindFreeInvoice(invoiceNumber, true, out invoice);

            if (findFreeInvoiceResult != 0 && findFreeInvoiceResult != 6201)
            {
                return false;
            }

            Manager.LoadInvoice(invoice);

            return findFreeInvoiceResult == 0;
        }

        public bool FindLanguage(string languageCode, out ILanguage language)
        {
            Manager.LoadLanguages();

            language = Manager.FindLanguage(languageCode);

            return language != null;
        }

        public bool FindPayTerm(string payTermCode, out IPayTerm payTerm)
        {
            payTerm = null;

            var loadPayTermsResult = Manager.LoadPayTerms();

            if (loadPayTermsResult != 0)
            {
                return false;
            }

            payTerm = Manager.FindPayTerm(payTermCode);

            return payTerm != null;
        }

        public bool FindPayType(string paymentTypeName, out IPayType payType)
        {
            payType = null;

            var loadPayTypesResult = Manager.LoadPayTypes();

            if (loadPayTypesResult != 0)
            {
                return false;
            }

            payType = Manager.FindPayType(paymentTypeName);

            return payType != null;
        }

        public string GetErrorMessage(int errorCode)
        {
            return Manager.GetErrorMessage(errorCode);
        }

        public string LastErrorMessage { get; set; }

        public IDisposable Lock()
        {
            return new SlTopalLockToken(ManagerLock);
        }

        public SlTopalLoginResult Login(string ip, string clientCode, string userName, string password)
        {
            try
            {
                if (Manager.IsLogined)
                {
                    Manager.Logout();
                }

                string message;

                if (Manager.Login(ip, userName, password, out message))
                {
                    Manager.LoadGlobalClients();

                    _client = Manager.Clients.OfType<IClient>().SingleOrDefault(pr => pr.Code.Equals(clientCode, StringComparison.OrdinalIgnoreCase));

                    if (_client == null)
                    {
                        return SlTopalLoginResult.ClientNotFound;
                    }

                    Manager.SetGlobalClient(_client);

                    return SlTopalLoginResult.Success;
                }

                LastErrorMessage = message;

                if (message.Equals("Unbekannter Benutzer oder falsches Passwort", StringComparison.OrdinalIgnoreCase))
                {
                    return SlTopalLoginResult.WrongCredentials;
                }

                if (message.Equals("Invalid user login and/or password", StringComparison.OrdinalIgnoreCase))
                {
                    return SlTopalLoginResult.WrongCredentials;
                }

                if (message.StartsWith("A connection attempt failed"))
                {
                    return SlTopalLoginResult.ConnectionFailed;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Exception for instance {ip} for client {clientCode} and user {userName} with {ex.Message}");
            }

            return SlTopalLoginResult.Unknown;
        }

        public void Logout()
        {
            if (Manager.IsLogined)
            {
                Manager.Logout();
            }
        }

        public IManager Manager { get; }

        public bool SaveDebtor(IDebtor debtor)
        {
            return Manager.SaveDebtor(debtor) == 0;
        }

        public bool SaveInvoice(IInvoice invoice)
        {
            return Manager.SaveInvoice(invoice) == 0;
        }

        public bool SaveManualPayment(IPayment payment)
        {
            return Manager.SaveManualPayment(payment) == 0;
        }

        public bool SaveParty(IParty party)
        {
            return Manager.SaveParty(party) == 0;
        }

        public bool SetFiscalYear(IFiscalYear fiscalYear)
        {
            if (Manager.FiscalYear.ID == fiscalYear.ID)
            {
                return false;
            }

            Manager.LoadFiscalYears(_client);

            return Manager.SetGlobalFiscalYear(fiscalYear) == 0;
        }

        public bool FindFiscalYear(string searchFiscalYear, out IFiscalYear fiscalYear)
        {
            Manager.LoadFiscalYears(_client);

            fiscalYear = _client.FiscalYears.OfType<IFiscalYear>().SingleOrDefault(pr => pr.Name.Equals(searchFiscalYear, StringComparison.OrdinalIgnoreCase));

            return fiscalYear != null;
        }
    }
}