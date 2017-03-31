using System;
using System.Diagnostics;
using slToTopalClient.Interfaces;
using slToTopalModel.Model;
using TopalServerAPI;

namespace slToTopalClient.Invoice
{
    public class SlTopalInvoice : ISlTopalInvoice
    {
        private readonly ISlTopalManager _topalManager;

        public SlTopalInvoice(ISlTopalManager topalManager)
        {
            _topalManager = topalManager;
        }

        public bool CreateInvoice(IParty party, SlExportInvoice slExportInvoice, out IInvoice invoice)
        {
            using (_topalManager.Lock())
            {
                if (!TryCreateInvoice(party, slExportInvoice, out invoice))
                {
                    return false;
                }

                _topalManager.SaveInvoice(invoice);

                return true;
            }
        }

        public bool FindInvoice(string invoiceNumber, out IInvoice invoice)
        {
            invoice = null;

            try
            {
                using (_topalManager.Lock())
                {
                    return _topalManager.FindInvoice(invoiceNumber, out invoice);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"FindInvoice for {invoiceNumber} with {ex.Message}");
            }

            return false;
        }

        public bool IsPaid(string invoiceNumber)
        {
            using (_topalManager.Lock())
            {
                IInvoice invoice;

                var postingAmount = decimal.Zero;

                if (!_topalManager.FindInvoice(invoiceNumber, out invoice))
                {
                    return false;
                }

                foreach (IPosting invoicePaymentPosting in invoice.PaymentPostings)
                {
                    postingAmount += invoicePaymentPosting.Amount;
                }

                return invoice.Balance - postingAmount <= decimal.Zero;
            }
        }

        public SlTopalInvoiceErrorCode LastErrorCode { get; private set; }

        public string LastErrorMessage { get; set; }

        public bool Pay(string invoiceNumber, DateTime paymentDateTime, string accountCode)
        {
            using (_topalManager.Lock())
            {
                IInvoice invoice;
                IAccount account;

                if (!_topalManager.FindInvoice(invoiceNumber, out invoice))
                {
                    return SetLastError($"{nameof(Pay)} invoice not found {invoiceNumber} ", SlTopalInvoiceErrorCode.InvoiceNotFound);
                }

                if (!_topalManager.FindAccount(accountCode, out account))
                {
                    return SetLastError($"{nameof(Pay)} account {accountCode} not found for invoice {invoiceNumber} ", SlTopalInvoiceErrorCode.AccountNotFound);
                }

                IPayment payment = new Payment();

                payment.PayInvoice(invoice);

                payment.AccountFID = account.ID;
                payment.Transaction.DocDate = paymentDateTime;
                payment.PartyFID = invoice.PartyFID;
                payment.Text = "API Bezahlung";

                _topalManager.SaveManualPayment(payment);

                return true;
            }
        }

        public void Remove(IInvoice invoice)
        {
            using (_topalManager.Lock())
            {
                _topalManager.DeleteInvoice(invoice);
            }
        }

        private string GetArticlePostingText(IParty exportInvoice, SlExportInvoice slExportInvoice, SlExportArticle article)
        {
            return $"{exportInvoice.PartyNum}, {slExportInvoice.Customer.LastName}, {slExportInvoice.Customer.FirstName} Rechnung {slExportInvoice.InvoiceNumber}, {article.Text}";
        }

        private string GetPaymentPostingText(IParty exportInvoice, SlExportInvoice slExportInvoice)
        {
            return $"{exportInvoice.PartyNum}, {slExportInvoice.Customer.LastName}, {slExportInvoice.Customer.FirstName} Rechnung {slExportInvoice.InvoiceNumber}";
        }

        private bool SetLastError(string message, SlTopalInvoiceErrorCode errorCode)
        {
            LastErrorMessage = message;

            LastErrorCode = errorCode;

            return false;
        }

        private bool TryCreateInvoice(IParty party, SlExportInvoice slExportInvoice, out IInvoice invoice)
        {
            invoice = new TopalServerAPI.Invoice
            {
                FreeInvoiceNum = slExportInvoice.InvoiceNumber,
                InvoiceDate = slExportInvoice.InvoiceDate,
                PartyFID = party.ID,
                PersonRoleFID = (int) PersonRole.Debtor,
                Text = slExportInvoice.InvoiceTitle,
                TotalAmount = slExportInvoice.TotalAmount,
                PayMethodFID = ((IPayMethod) party.Debtor.PayMethods[0]).ID,
                PayslipCode = slExportInvoice.PayslipCode
            };

            invoice.Transaction.DocDate = slExportInvoice.InvoiceDate;

            IPosting paymentPosting = new Posting();

            var paymethods = party.Debtor.PayMethods;

            paymentPosting.AccountFID = ((IPayMethod) paymethods[0]).AccountFID;
            paymentPosting.Text = GetPaymentPostingText(party, slExportInvoice);
            paymentPosting.FreeCode = slExportInvoice.InvoiceNumber;
            paymentPosting.Amount = slExportInvoice.TotalAmount;
            paymentPosting.IsDebit = true;
            paymentPosting.PostingTypeFID = (int) PostingType.Invoice;
            paymentPosting.IsInclusive = true;

            invoice.Transaction.Postings.Add(paymentPosting);

            foreach (var article in slExportInvoice.Articles)
            {
                IPosting articlePosting = new Posting();

                IAccount account;

                if (!_topalManager.FindAccount(article.AccountCode, out account))
                {
                    return SetLastError($"{nameof(TryCreateInvoice)} account {article.AccountCode} not found for article code {article.ArticleCode}", SlTopalInvoiceErrorCode.AccountNotFoundForArticle);
                }

                articlePosting.AccountFID = account.ID;
                articlePosting.Text = GetArticlePostingText(party, slExportInvoice, article);
                articlePosting.FreeCode = article.ArticleCode;
                articlePosting.Amount = article.TotalAmount;
                articlePosting.IsDebit = false;
                articlePosting.PostingTypeFID = (int) PostingType.Invoice;
                articlePosting.IsInclusive = article.IsVatInclusive;

                if (!string.IsNullOrEmpty(article.CostCenterCode))
                {
                    ICostCenter costCenter;

                    if (_topalManager.FindCostCenter(article.CostCenterCode, out costCenter))
                    {
                        articlePosting.CostCenterFID = costCenter.ID;
                    }
                }

                if (!_topalManager.FillVatPostingAmount(articlePosting, article.VatCode, article.TotalAmount))
                {
                    return SetLastError($"{nameof(TryCreateInvoice)} vat {article.VatCode} not found for article code {article.ArticleCode}", SlTopalInvoiceErrorCode.VatNotFoundForArticle);
                }

                invoice.Transaction.Postings.Add(articlePosting);
            }

            return true;
        }
    }
}