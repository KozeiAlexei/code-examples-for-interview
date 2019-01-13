using System.Collections.Generic;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.BusinessProcess.BillingService;

namespace InfinitySynerge.MobileForHero.BusinessServices.Billing.DTO
{
    public class TinkoffPayment_InitRequest : TinkoffMerchantApiModel
    {
        public string TerminalKey { get; set; }

        public long Amount { get; set; }

        public string OrderId { get; set; }

        public string IP { get; set; }

        public string Description { get; set; }

        public string Language { get; set; }

        public object DATA { get; set; }

        public TinkoffPayment_ReceiptModel Receipt { get; set; }

        protected override Dictionary<string, string> GetImportantPropertiesDictionary()
        {
            return new Dictionary<string, string>()
            {
                [nameof(TerminalKey)] = TerminalKey,
                [nameof(Amount)] = Amount.ToString(),
                [nameof(OrderId)] = OrderId,
                [nameof(IP)] = IP,
                [nameof(Description)] = Description,
                [nameof(Language)] = Language
            };
        }
    }

    public class TinkoffPayment_ReceiptModel
    {
        public string Email { get; set; }

        public string Phone { get; set; }

        public string Taxation { get; set; }

        public IEnumerable<TinkoffPayment_ReceiptItemModel> Items { get; set; }
    }

    public class TinkoffPayment_ReceiptItemModel
    {
        public string Name { get; set; }

        public long Price { get; set; }

        public int Quantity { get; set; }

        public long Amount { get; set; }

        public string Tax { get; set; }
    }
}
