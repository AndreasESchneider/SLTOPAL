using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ServiceBus.Messaging;
using slClientBus;
using slToTopalClient.Customer;
using slToTopalClient.Invoice;
using slToTopalClient.Manager;
using slToTopalModel.Model;
using CloseReason = System.Windows.Forms.CloseReason;

namespace slToTopalGui
{
    public partial class MainForm : Form
    {
        private bool _formClose;

        private string _topalClient;
        private ClientBusQueue _topalInQueue;
        private ClientBusQueue _topalOutQueue;
        private string _topalServer;
        private string _topalUser;

        public MainForm()
        {
            InitializeComponent();
        }

        private void buttonQuit_Click(object sender, EventArgs e)
        {
            LogMessage("Schliesse Anwendung");

            CloseApp();
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            LogMessage("Teste Anmeldung an TOPAL...");

            using (var manager = new SlTopalManager(new Manager()))
            {
                if (TopalLogin(manager))
                {
                    LogMessage("..Anmeldung an TOPAL erfolgreich");

                    MessageBox.Show(@"Anmeldung an TOPAL erfolgreich", @"SPORTLOOP", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    return;
                }

                LogMessage($"..Anmeldung an TOPAL fehlgeschlagen mit {manager.LastErrorMessage}");
            }

            MessageBox.Show(@"Anmeldung an TOPAL fehlgeschlagen", @"SPORTLOOP", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void CloseApp()
        {
            _formClose = true;

            Close();

            Application.Exit();
        }

        private string GetSetting(string settingName, string defaultValue = "")
        {
            var setting = string.IsNullOrEmpty(ConfigurationManager.AppSettings[settingName]) ? defaultValue : ConfigurationManager.AppSettings[settingName];

            LogMessage($"Lese Einstellung {settingName} Wert {setting}");

            return setting;
        }

        private void HideForm()
        {
            SynchronizedInvoke(() =>
            {
                Hide();
            });
        }

        private bool ImportInvoice(SlExportInvoice exportInvoice)
        {
            try
            {
                using (var manager = new SlTopalManager(new Manager()))
                {
                    if (!TopalLogin(manager))
                    {
                        return false;
                    }

                    var topalCustomer = new SlTopalCustomer(manager);

                    IParty party;

                    LogMessage($"Erstelle oder aktualisere Kunde {exportInvoice.Customer.CustomerNumber}, {exportInvoice.Customer.FirstName} {exportInvoice.Customer.LastName}");

                    if (!topalCustomer.AddOrUpdate(exportInvoice.Customer, out party))
                    {
                        LogMessage($"..Erstellen von {exportInvoice.Customer.CustomerNumber} fehlgeschlagen mit {topalCustomer.LastErrorCode}:{topalCustomer.LastErrorMessage}");

                        return false;
                    }

                    var topalInvoice = new SlTopalInvoice(manager);

                    IInvoice invoice;

                    if (!topalInvoice.FindInvoice(exportInvoice.InvoiceNumber, out invoice))
                    {
                        LogMessage($"Erstelle Rechnung {exportInvoice.InvoiceNumber}");

                        if (topalInvoice.CreateInvoice(party, exportInvoice, out invoice))
                        {
                            LogMessage($"..Rechnung {exportInvoice.InvoiceNumber} wurde erstell");
                        }
                        else
                        {
                            LogMessage($"..Rechnung {exportInvoice.InvoiceNumber} konnte nicht erstellt werden mit {topalCustomer.LastErrorCode}:{topalCustomer.LastErrorMessage}");

                            return false;
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Exception on ImportInvoice for {exportInvoice.InvoiceNumber} with {ex.Message}");
            }

            return false;
        }

        private void InitializeClient()
        {
            _topalClient = GetSetting("TopalClient");
            _topalServer = GetSetting("TopalServer");
            _topalUser = GetSetting("TopalUser");

            LogMessage($"Starte Client für {_topalServer} Mandant {_topalClient} und Benutzer {_topalUser}");

            SynchronizedInvoke(() =>
            {
                Text = $@"SPORTLOOP {_topalClient} ({_topalServer}/{_topalUser})";

                notifyIconPrint.Text = $@"SPORTLOOP TOPAL Client: {_topalClient}";
            });

            StartInQueue();

            StartOutQueue();

            HideForm();
        }

        private void LogMessage(string message)
        {
            SynchronizedInvoke(() =>
            {
                Trace.TraceInformation(message);

                var addedItem = listBoxLog.Items.Add(message);

                listBoxLog.SelectedIndex = addedItem;
            });
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _topalInQueue.MessageReceived -= TopalInQueueMessageReceived;

            _topalInQueue.Dispose();

            _topalInQueue = null;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing)
            {
                return;
            }

            if (_formClose)
            {
                return;
            }

            e.Cancel = true;

            HideForm();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            HideForm();

            Task.Run(() =>
            {
                InitializeClient();
            });
        }

        private void notifyIconPrint_Click(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void ShowForm()
        {
            SynchronizedInvoke(() =>
            {
                Show();

                BringToFront();
            });
        }

        private void StartInQueue()
        {
            _topalInQueue = new ClientBusQueue(GetSetting("TopalEndpointConnectionString"));
            _topalInQueue.MessageReceived += TopalInQueueMessageReceived;

            Task.Run(() =>
            {
                var topalInQueue = GetSetting("TopalInQueueName");

                LogMessage($"Starte Eingangs-Warteschlange mit {_topalInQueue} auf {topalInQueue}");

                while (!_topalInQueue.Start(topalInQueue))
                {
                    Thread.Sleep(TimeSpan.FromMinutes(20));
                }
            });
        }

        private void StartOutQueue()
        {
            _topalOutQueue = new ClientBusQueue(GetSetting("TopalEndpointConnectionString"));

            Task.Run(() =>
            {
                var topalOutQueue = GetSetting("TopalOutQueueName");

                LogMessage($"Starte Ausgangs-Warteschlange mit {_topalInQueue} auf {topalOutQueue}");

                while (!_topalOutQueue.Start(topalOutQueue))
                {
                    Thread.Sleep(TimeSpan.FromMinutes(20));
                }
            });
        }

        private void SynchronizedInvoke(Action action)
        {
            if (!InvokeRequired)
            {
                action();
            }
            else
            {
                Invoke(action, new object[]
                {
                });
            }
        }

        private void TopalInQueueMessageReceived(object sender, BrokeredMessage message)
        {
            if (message.ContentType.Equals("SlExportInvoice", StringComparison.OrdinalIgnoreCase))
            {
                var exportInvoice = message.GetBody<SlExportInvoice>();

                if (string.IsNullOrEmpty(exportInvoice?.InvoiceNumber))
                {
                    LogMessage("..De-Serialisierung ergab null. Rechnung wird verworfen");

                    message.DeadLetter("Rechnung konnte nicht deserialisiert werden", "Deserialisierung fehlgeschlagen");

                    return;
                }

                LogMessage($"Eingang der Rechnung {exportInvoice.InvoiceNumber}");

                if (ValidateInvoice(ref exportInvoice))
                {
                    LogMessage($"..Rechnung {exportInvoice.InvoiceNumber} wurde überprüft");

                    if (ImportInvoice(exportInvoice))
                    {
                        LogMessage($"..Rechnung {exportInvoice.InvoiceNumber} wird quittiert");

                        message.Complete();
                    }
                }
            }

            if (message.ContentType.Equals("SlExportInvoicePaid", StringComparison.OrdinalIgnoreCase))
            {
                var paidInvoice = message.GetBody<SlExportInvoicePaid>();

                LogMessage($"Prüfe Zahlung der Rechnung {paidInvoice.InvoiceNumber}");

                UpdateInvoicePaidStatus(ref paidInvoice);

                if (paidInvoice.IsFound)
                {
                    LogMessage($"..Rechnung {paidInvoice.InvoiceNumber} mit Zahlungsstatus {paidInvoice.IsPaid} aktualisieren");
                }
                else
                {
                    LogMessage($"..Rechnung {paidInvoice.InvoiceNumber} wurde nicht gefunden");
                }

                _topalOutQueue.SendMessage(new BrokeredMessage(paidInvoice)
                {
                    ContentType = nameof(SlExportInvoicePaid),
                    TimeToLive = TimeSpan.FromDays(14)
                });

                message.Complete();
            }
        }

        private bool TopalLogin(SlTopalManager manager)
        {
            LogMessage($"TOPAL Login auf {_topalServer} für {_topalClient} mit Benutzer {_topalUser}");

            return manager.Login(_topalServer, _topalClient, _topalUser, string.Empty) == SlTopalLoginResult.Success;
        }

        private void UpdateInvoicePaidStatus(ref SlExportInvoicePaid paidInvoice)
        {
            using (var manager = new SlTopalManager(new Manager()))
            {
                if (TopalLogin(manager))
                {
                    var topalInvoice = new SlTopalInvoice(manager);

                    IInvoice invoice;

                    if (topalInvoice.FindInvoice(paidInvoice.InvoiceNumber, out invoice))
                    {
                        paidInvoice.IsFound = true;
                        paidInvoice.IsPaid = topalInvoice.IsPaid(paidInvoice.InvoiceNumber);
                    }
                }
            }
        }

        private bool ValidateInvoice(ref SlExportInvoice exportInvoice)
        {
            LogMessage($"Prüfe Rechnung {exportInvoice.InvoiceNumber}");

            if (string.IsNullOrEmpty(exportInvoice.InvoiceTitle))
            {
                exportInvoice.InvoiceTitle = string.Format(GetSetting("InvoiceTitle"), exportInvoice.InvoiceNumber);

                LogMessage($"..Titel auf {exportInvoice.InvoiceTitle} für {exportInvoice.InvoiceNumber}");
            }

            if (!string.IsNullOrEmpty(exportInvoice.PayslipCode))
            {
                var match = Regex.Match(exportInvoice.PayslipCode, @"(?<=\>)(.*?)(?=\+)");

                if (match.Success)
                {
                    exportInvoice.PayslipCode = match.Value;

                    LogMessage($"..ESR auf {exportInvoice.PayslipCode} für {exportInvoice.InvoiceNumber}");
                }
            }

            if (string.IsNullOrEmpty(exportInvoice.Customer.AccountCode))
            {
                exportInvoice.Customer.AccountCode = GetSetting("CustomerAccountCode");

                LogMessage($"..Kundenkonto auf {exportInvoice.Customer.AccountCode} für {exportInvoice.InvoiceNumber}");
            }

            if (string.IsNullOrEmpty(exportInvoice.Customer.PaymentTermCode))
            {
                exportInvoice.Customer.PaymentTermCode = GetSetting("CustomerPaymentTermCode");

                LogMessage($"..Zahlungskonditionen auf {exportInvoice.Customer.PaymentTermCode} für {exportInvoice.InvoiceNumber}");
            }

            if (string.IsNullOrEmpty(exportInvoice.Customer.PaymentType))
            {
                exportInvoice.Customer.PaymentType = GetSetting("CustomerPaymentType");

                LogMessage($"..Zahlungsart auf {exportInvoice.Customer.PaymentType} für {exportInvoice.InvoiceNumber}");
            }

            foreach (var article in exportInvoice.Articles.ToList())
            {
                if (article.TotalAmount <= decimal.Zero)
                {
                    exportInvoice.Articles.Remove(article);

                    LogMessage($"..Entferne Artikel {article.Text} ohne Betrag von {exportInvoice.InvoiceNumber}");

                    continue;
                }

                // kostenstelle
                if (string.IsNullOrEmpty(article.CostCenterCode))
                {
                    article.CostCenterCode = GetSetting("CostCenterCode");

                    LogMessage($"..Kostenstelle für {article.Text} auf {article.CostCenterCode} für {exportInvoice.InvoiceNumber}");
                }

                // ertragskonto
                if (string.IsNullOrEmpty(article.AccountCode))
                {
                    article.AccountCode = GetSetting("AccountCode");

                    LogMessage($"..Ertragskonto für {article.Text} auf {article.AccountCode} für {exportInvoice.InvoiceNumber}");
                }

                if (string.IsNullOrEmpty(article.VatCode))
                {
                    article.VatCode = GetSetting("ArticleVatCode");

                    LogMessage($"..MwSt Code für {article.Text} auf {article.VatCode} für {exportInvoice.InvoiceNumber}");
                }
            }

            return true;
        }
    }
}