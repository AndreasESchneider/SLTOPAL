using slToTopalClient.Customer;
using slToTopalModel.Model;
using TopalServerAPI;

namespace slToTopalClient.Interfaces
{
    public interface ISlTopalCustomer
    {
        SlTopalCustomerErrorCode LastErrorCode { get; }

        string LastErrorMessage { get; }

        bool AddOrUpdate(SlExportCustomer customer, out IParty party);

        bool Exists(string customerNumner);

        bool Find(string customerNumber, out IParty party);

        void Remove(string customerId);
    }
}