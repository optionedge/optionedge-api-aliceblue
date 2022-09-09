using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OptionEdge.API.AliceBlue.Records
{
    public class FundsResult : BaseResponseResult
    {
        [JsonPropertyName("adhocMargin")]
        public decimal AdhocMargin { get; set; }

        [JsonPropertyName("adhocscripmargin")]
        public decimal AdhocScripMargin { get; set; }

        [JsonPropertyName("brokeragePrsnt")]
        public decimal BrokeragePresent { get; set; }

        [JsonPropertyName("cashmarginavailable")]
        public decimal CashMarginAvailable { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("cdsSpreadBenefit")]
        public decimal CDSSpreadBenefit { get; set; }

        [JsonPropertyName("cncBrokeragePrsnt")]
        public decimal CNCBrokeragePresent { get; set; }

        [JsonPropertyName("cncMarginUsed")]
        public decimal CNCMarginUsed { get; set; }

        [JsonPropertyName("cncRealizedMtomPrsnt")]
        public decimal CNCRealizedM2MPresent { get; set; }

        [JsonPropertyName("cncSellCrditPrsnt")]
        public decimal CNCSellCreditPresent { get; set; }

        [JsonPropertyName("cncUnrealizedMtomPrsnt")]
        public decimal CNCUnrealizedM2MPresent { get; set; }

        [JsonPropertyName("collateralvalue")]
        public decimal CollateralValue { get; set; }

        [JsonPropertyName("coverOrderMarginPrsnt")]
        public decimal CoverOrderMarginPresent { get; set; }

        [JsonPropertyName("credits")]
        public decimal Credits { get; set; }

        [JsonPropertyName("debits")]
        public decimal Debits { get; set; }

        [JsonPropertyName("directcollateralvalue")]
        public decimal DirectCollateralValue { get; set; }

        [JsonPropertyName("elm")]
        public decimal ExtremeLossMargin { get; set; }

        [JsonPropertyName("exchange")]
        public string Exchange { get; set; }

        [JsonPropertyName("exposuremargin")]
        public decimal ExposureMargin { get; set; }

        [JsonPropertyName("grossexposurevalue")]
        public decimal GroupExposureValue { get; set; }

        [JsonPropertyName("losslimit")]
        public decimal LossLimit { get; set; }

        [JsonPropertyName("mfamount")]
        public decimal MFAmount { get; set; }

        [JsonPropertyName("mfssAmountUsed")]
        public decimal MFAmountUsed { get; set; }

        [JsonPropertyName("multiplier")]
        public string Multiplier { get; set; }

        [JsonPropertyName("net")]
        public decimal Net { get; set; }

        [JsonPropertyName("nfoSpreadBenefit")]
        public decimal NFOSpreadBenefit { get; set; }

        [JsonPropertyName("notionalCash")]
        public decimal NotionalCash { get; set; }

        [JsonPropertyName("payoutamount")]
        public decimal PayoutAmount { get; set; }

        [JsonPropertyName("premiumPrsnt")]
        public decimal PremiumPresent { get; set; }

        [JsonPropertyName("product")]
        public string Product { get; set; }

        [JsonPropertyName("realizedMtomPrsnt")]
        public decimal RealizedM2MPresent { get; set; }

        [JsonPropertyName("rmsIpoAmnt")]
        public decimal RMSIPOAmount { get; set; }

        [JsonPropertyName("rmsPayInAmnt")]
        public decimal RMSPayinAmount { get; set; }

        [JsonPropertyName("scripbasketmargin")]
        public decimal ScripBasketMargin { get; set; }

        [JsonPropertyName("segment")]
        public string Segment { get; set; }

        [JsonPropertyName("spanmargin")]
        public decimal SpanMargin { get; set; }

        [JsonPropertyName("subtotal")]
        public decimal SubTotal { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("turnover")]
        public decimal Turnover { get; set; }

        [JsonPropertyName("unrealizedMtomPrsnt")]
        public decimal UnrealizedM2MPresent { get; set; }

        [JsonPropertyName("valueindelivery")]
        public decimal ValueInDelivery { get; set; }

        [JsonPropertyName("varmargin")]
        public decimal VarMargin { get; set; }

        [JsonPropertyName("branchAdhoc")]
        public decimal BranchAdhoc { get; set; }
    }
}
