using System;
using System.Diagnostics;
using slToTopalClient.Interfaces;
using slToTopalModel.Model;
using TopalServerAPI;

namespace slToTopalClient.Customer
{
    public class SlTopalCustomer : ISlTopalCustomer
    {
        private readonly ISlTopalManager _topalManager;

        public SlTopalCustomer(ISlTopalManager topalManager)
        {
            _topalManager = topalManager;
        }

        public bool AddOrUpdate(SlExportCustomer customer, out IParty party)
        {
            party = null;

            try
            {
                using (_topalManager.Lock())
                {
                    if (_topalManager.FindFreeParty(customer.CustomerNumber, out party))
                    {
                        if (!TryUpdateParty(customer, ref party))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!TryCreateParty(customer, out party))
                        {
                            return false;
                        }
                    }

                    _topalManager.SaveParty(party);

                    if (party.IsHaveDebtor)
                    {
                        if (!TryUpdateDebtor(customer, ref party))
                        {
                            return false;
                        }

                        _topalManager.SaveDebtor(party.Debtor);
                    }
                    else
                    {
                        IDebtor debtor;

                        if (!TryCreateDebtor(customer, party, out debtor))
                        {
                            return false;
                        }

                        _topalManager.SaveDebtor(debtor);
                    }

                    return _topalManager.FindFreeParty(customer.CustomerNumber, out party);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to create or update customer {customer.CustomerNumber} with {ex.Message}");
            }

            return false;
        }

        public bool Exists(string customerNumner)
        {
            using (_topalManager.Lock())
            {
                IParty party;

                return _topalManager.FindFreeParty(customerNumner, out party);
            }
        }

        public bool Find(string customerNumber, out IParty party)
        {
            using (_topalManager.Lock())
            {
                return _topalManager.FindFreeParty(customerNumber, out party);
            }
        }

        public SlTopalCustomerErrorCode LastErrorCode { get; private set; }

        public string LastErrorMessage { get; private set; }

        public void Remove(string customerId)
        {
            using (_topalManager.Lock())
            {
                IParty party;

                if (_topalManager.FindFreeParty(customerId, out party))
                {
                    _topalManager.DeleteParty(party);
                }
            }
        }

        private bool SetLastError(string message, SlTopalCustomerErrorCode errorCode)
        {
            LastErrorMessage = message;

            LastErrorCode = errorCode;

            return false;
        }

        private bool TryCreateDebtor(SlExportCustomer customer, IParty party, out IDebtor debtor)
        {
            debtor = null;

            IAccount account;

            if (!_topalManager.FindAccount(customer.AccountCode, out account))
            {
                return SetLastError($"{nameof(TryCreateDebtor)} account {customer.AccountCode} not found for customer {customer.CustomerNumber}", SlTopalCustomerErrorCode.AccountNotFound);
            }

            IPayTerm payTerm;

            if (!_topalManager.FindPayTerm(customer.PaymentTermCode, out payTerm))
            {
                return SetLastError($"{nameof(TryCreateDebtor)} payment term {customer.PaymentTermCode} not found for customer {customer.CustomerNumber}", SlTopalCustomerErrorCode.PaymentTermNotFound);
            }

            debtor = new Debtor
            {
                PartyFID = party.ID,
                FreeCode = party.FreePartyNum,
                NoReminders = customer.NoReminders,
                Person =
                {
                    FirstName = party.Person.FirstName
                },
                PayTermFID = payTerm.ID,
                AccountFID = account.ID
            };

            IPayType payType;

            if (!_topalManager.FindPayType(customer.PaymentType, out payType))
            {
                return SetLastError($"{nameof(TryCreateDebtor)} payType {customer.PaymentType} not found for customer {customer.CustomerNumber}", SlTopalCustomerErrorCode.PaymentTypeNotFound);
            }

            var payMethod = new PayMethod
            {
                Name = customer.PaymentType,
                AccountFID = account.ID,
                PayTypeFID = payType.ID,
                PersonRoleFID = (int) PersonRole.Debtor
            };

            var payAddMethodResult = debtor.PayMethods.Add(payMethod);

            if (payAddMethodResult != 0)
            {
                return SetLastError($"{nameof(TryCreateDebtor)} pay method add failed {customer.PaymentType} for customer {customer.CustomerNumber} with error {_topalManager.GetErrorMessage(payAddMethodResult)}", SlTopalCustomerErrorCode.PaymentAddFailed);
            }

            return true;
        }

        private bool TryCreateParty(SlExportCustomer customer, out IParty party)
        {
            party = null;

            ICountry country;

            if (!_topalManager.FindCountry(customer.CountryCode, out country))
            {
                return SetLastError($"{nameof(TryCreateParty)} country {customer.CountryCode} not found for customer {customer.CustomerNumber}", SlTopalCustomerErrorCode.CountryNotFound);
            }

            ILanguage language;

            if (!_topalManager.FindLanguage(customer.LanguageCode, out language))
            {
                return SetLastError($"{nameof(TryCreateParty)} language {customer.LanguageCode} not found for customer {customer.CustomerNumber}", SlTopalCustomerErrorCode.LanguageNotFound);
            }

            party = new Party
            {
                Address1 = customer.AddressLine1,
                Address2 = customer.AddressLine2,
                City = customer.City,
                CountryFID = country.ID,
                LanguageFID = language.ID,
                FreePartyNum = customer.CustomerNumber,
                Name = customer.LastName,
                Zip = customer.Zip,
                ShortName = customer.FirstName + customer.LastName
            };

            party.Person.FirstName = customer.FirstName;
            party.Person.LastName = customer.LastName;
            party.Person.Salutation = customer.Salutation;
            party.Person.Title = customer.Title;
            party.Person.Email = customer.EMail;

            return true;
        }

        private bool TryUpdateDebtor(SlExportCustomer customer, ref IParty party)
        {
            party.Debtor.NoReminders = customer.NoReminders;

            return true;
        }

        private bool TryUpdateParty(SlExportCustomer customer, ref IParty party)
        {
            ICountry country;

            if (!_topalManager.FindCountry(customer.CountryCode, out country))
            {
                return SetLastError($"{nameof(TryUpdateParty)} country {customer.CountryCode} not found for customer {customer.CustomerNumber}", SlTopalCustomerErrorCode.CountryNotFound);
            }

            ILanguage language;

            if (!_topalManager.FindLanguage(customer.LanguageCode, out language))
            {
                return SetLastError($"{nameof(TryUpdateParty)} language {customer.LanguageCode} not found for customer {customer.CustomerNumber}", SlTopalCustomerErrorCode.LanguageNotFound);
            }

            party.Address1 = customer.AddressLine1;
            party.Address2 = customer.AddressLine2;
            party.City = customer.City;
            party.CountryFID = country.ID;
            party.LanguageFID = language.ID;

            party.Person.FirstName = customer.FirstName;
            party.Person.LastName = customer.LastName;
            party.Person.Salutation = customer.Salutation;
            party.Person.Title = customer.Title;
            party.Person.Email = customer.EMail;

            return true;
        }
    }
}