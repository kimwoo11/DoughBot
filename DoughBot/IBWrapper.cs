﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IBApi;

namespace DoughBot
{
    public class IBWrapper : EWrapper
    {
        private int nextOrderId;
        EClientSocket clientSocket;
        public readonly EReaderSignal Signal;
        private Bot bot;
        public Dictionary<int, bool> ContractDetailEnd = new Dictionary<int, bool>();  // used for status of collecting option contract details
        public Dictionary<int, double> OrderStatus = new Dictionary<int, double>();

        public IBWrapper(Bot bot)
        {
            Signal = new EReaderMonitorSignal();
            clientSocket = new EClientSocket(this, Signal);
            this.bot = bot;
        }

        public EClientSocket ClientSocket
        {
            get { return clientSocket; }
            set { clientSocket = value; }
        }

        public int NextOrderId
        {
            get { return nextOrderId; }
            set { nextOrderId = value; }
        }

        public string BboExchange { get; private set; }

        public virtual void error(Exception e)
        {
            string msg = "Exception thrown: " + e;
            bot.SendText(msg);

            throw e;
        }

        public virtual void error(string str)
        {
            Console.WriteLine("Error: " + str + "\n");
            string msg = "Error: " + str + "\n";
            bot.SendText(msg);
        }


        public virtual void error(int id, int errorCode, string errorMsg)
        {
            string msg = "Error. Id: " + id + ", Code: " + errorCode + ", Msg: " + errorMsg + "\n";
            Console.WriteLine(msg);
            if (errorCode == 10190 || errorCode == 101)
            {
                bot.Disconnect();
            }
        }


        public virtual void connectionClosed()
        {
            Console.WriteLine("Connection closed.\n");
        }

        public virtual void currentTime(long time)
        {
            Console.WriteLine("Current Time: " + time + "\n");
        }


        public virtual void tickPrice(int tickerId, int field, double price, TickAttrib attribs)
        {
            Console.WriteLine("Tick Price. Ticker Id:" + tickerId + ", Field: " + field + ", Price: " + price + ", CanAutoExecute: " + attribs.CanAutoExecute +
                ", PastLimit: " + attribs.PastLimit + ", PreOpen: " + attribs.PreOpen);
        }

        public virtual void tickSize(int tickerId, int field, int size)
        {
            Console.WriteLine("Tick Size. Ticker Id:" + tickerId + ", Field: " + field + ", Size: " + size);
        }

        public virtual void tickString(int tickerId, int tickType, string value)
        {
            Console.WriteLine("Tick string. Ticker Id:" + tickerId + ", Type: " + tickType + ", Value: " + value);
        }

        public virtual void tickGeneric(int tickerId, int field, double value)
        {
            Console.WriteLine("Tick Generic. Ticker Id:" + tickerId + ", Field: " + field + ", Value: " + value);
        }

        public virtual void tickEFP(int tickerId, int tickType, double basisPoints, string formattedBasisPoints, double impliedFuture, int holdDays, string futureLastTradeDate, double dividendImpact, double dividendsToLastTradeDate)
        {
            Console.WriteLine("TickEFP. " + tickerId + ", Type: " + tickType + ", BasisPoints: " + basisPoints + ", FormattedBasisPoints: " + formattedBasisPoints + ", ImpliedFuture: " + impliedFuture + ", HoldDays: " + holdDays + ", FutureLastTradeDate: " + futureLastTradeDate + ", DividendImpact: " + dividendImpact + ", DividendsToLastTradeDate: " + dividendsToLastTradeDate);
        }
        public virtual void tickSnapshotEnd(int tickerId)
        {
            Console.WriteLine("TickSnapshotEnd: " + tickerId);
        }
        public virtual void nextValidId(int orderId)
        {
            Console.WriteLine("Next Valid Id: " + orderId);
            NextOrderId = orderId;
        }
        public virtual void deltaNeutralValidation(int reqId, DeltaNeutralContract deltaNeutralContract)
        {
            Console.WriteLine("DeltaNeutralValidation. " + reqId + ", ConId: " + deltaNeutralContract.ConId + ", Delta: " + deltaNeutralContract.Delta + ", Price: " + deltaNeutralContract.Price);
        }
        public virtual void managedAccounts(string accountsList)
        {
            Console.WriteLine("Account list: " + accountsList);
        }
        public virtual void tickOptionComputation(int tickerId, int field, double impliedVolatility, double delta, double optPrice, double pvDividend, double gamma, double vega, double theta, double undPrice)
        {
            Console.WriteLine("TickOptionComputation. TickerId: " + tickerId + ", field: " + field + ", ImpliedVolatility: " + impliedVolatility + ", Delta: " + delta
                + ", OptionPrice: " + optPrice + ", pvDividend: " + pvDividend + ", Gamma: " + gamma + ", Vega: " + vega + ", Theta: " + theta + ", UnderlyingPrice: " + undPrice);
        }

        public virtual void accountSummary(int reqId, string account, string tag, string value, string currency)
        {
            Console.WriteLine("Acct Summary. ReqId: " + reqId + ", Acct: " + account + ", Tag: " + tag + ", Value: " + value + ", Currency: " + currency);
        }
        public virtual void accountSummaryEnd(int reqId)
        {
            Console.WriteLine("AccountSummaryEnd. Req Id: " + reqId + "\n");
        }

        public virtual void updateAccountValue(string key, string value, string currency, string accountName)
        {
            Console.WriteLine("UpdateAccountValue. Key: " + key + ", Value: " + value + ", Currency: " + currency + ", AccountName: " + accountName);
        }

        public virtual void updatePortfolio(Contract contract, double position, double marketPrice, double marketValue, double averageCost, double unrealizedPNL, double realizedPNL, string accountName)
        {
            Console.WriteLine("UpdatePortfolio. " + contract.Symbol + ", " + contract.SecType + " @ " + contract.Exchange
                + ": Position: " + position + ", MarketPrice: " + marketPrice + ", MarketValue: " + marketValue + ", AverageCost: " + averageCost
                + ", UnrealizedPNL: " + unrealizedPNL + ", RealizedPNL: " + realizedPNL + ", AccountName: " + accountName);
        }

        public virtual void updateAccountTime(string timestamp)
        {
            Console.WriteLine("UpdateAccountTime. Time: " + timestamp + "\n");
        }

        public virtual void accountDownloadEnd(string account)
        {
            Console.WriteLine("Account download finished: " + account + "\n");
        }

        public virtual void orderStatus(int orderId, string status, double filled, double remaining, double avgFillPrice, int permId, int parentId, double lastFillPrice, int clientId, string whyHeld, double mktCapPrice)
        {
            //Console.WriteLine("OrderStatus. Id: " + orderId + ", Status: " + status + ", Filled: " + filled + ", Remaining: " + remaining
            //    + ", AvgFillPrice: " + avgFillPrice + ", PermId: " + permId + ", ParentId: " + parentId + ", LastFillPrice: " + lastFillPrice + ", ClientId: " + clientId + ", WhyHeld: " + whyHeld + ", MktCapPrice: " + mktCapPrice);
            OrderStatus[orderId] = remaining;
        }

        public virtual void openOrder(int orderId, Contract contract, Order order, OrderState orderState)
        {
            Console.WriteLine("OpenOrder. PermID: " + order.PermId + ", ClientId: " + order.ClientId + ", OrderId: " + orderId + ", Account: " + order.Account +
                ", Symbol: " + contract.Symbol + ", SecType: " + contract.SecType + " , Exchange: " + contract.Exchange + ", Action: " + order.Action + ", OrderType: " + order.OrderType +
                ", TotalQty: " + order.TotalQuantity + ", CashQty: " + order.CashQty + ", LmtPrice: " + order.LmtPrice + ", AuxPrice: " + order.AuxPrice + ", Status: " + orderState.Status);
        }

        public virtual void openOrderEnd()
        {
            Console.WriteLine("OpenOrderEnd");
        }

        public virtual void contractDetails(int reqId, ContractDetails contractDetails)
        {
            bot.DataDictionary[bot.IdSymbolDictionary[reqId - bot.OptionRequestIdBase]].StrikePrices.Add(contractDetails.Contract.Strike);
        }
        public void printContractMsg(Contract contract)
        {
            Console.WriteLine("\tConId: " + contract.ConId);
            Console.WriteLine("\tSymbol: " + contract.Symbol);
            Console.WriteLine("\tSecType: " + contract.SecType);
            Console.WriteLine("\tLastTradeDateOrContractMonth: " + contract.LastTradeDateOrContractMonth);
            Console.WriteLine("\tStrike: " + contract.Strike);
            Console.WriteLine("\tRight: " + contract.Right);
            Console.WriteLine("\tMultiplier: " + contract.Multiplier);
            Console.WriteLine("\tExchange: " + contract.Exchange);
            Console.WriteLine("\tPrimaryExchange: " + contract.PrimaryExch);
            Console.WriteLine("\tCurrency: " + contract.Currency);
            Console.WriteLine("\tLocalSymbol: " + contract.LocalSymbol);
            Console.WriteLine("\tTradingClass: " + contract.TradingClass);
        }
        public void printContractDetailsMsg(ContractDetails contractDetails)
        {
            Console.WriteLine("\tMarketName: " + contractDetails.MarketName);
            Console.WriteLine("\tMinTick: " + contractDetails.MinTick);
            Console.WriteLine("\tPriceMagnifier: " + contractDetails.PriceMagnifier);
            Console.WriteLine("\tOrderTypes: " + contractDetails.OrderTypes);
            Console.WriteLine("\tValidExchanges: " + contractDetails.ValidExchanges);
            Console.WriteLine("\tUnderConId: " + contractDetails.UnderConId);
            Console.WriteLine("\tLongName: " + contractDetails.LongName);
            Console.WriteLine("\tContractMonth: " + contractDetails.ContractMonth);
            Console.WriteLine("\tIndystry: " + contractDetails.Industry);
            Console.WriteLine("\tCategory: " + contractDetails.Category);
            Console.WriteLine("\tSubCategory: " + contractDetails.Subcategory);
            Console.WriteLine("\tTimeZoneId: " + contractDetails.TimeZoneId);
            Console.WriteLine("\tTradingHours: " + contractDetails.TradingHours);
            Console.WriteLine("\tLiquidHours: " + contractDetails.LiquidHours);
            Console.WriteLine("\tEvRule: " + contractDetails.EvRule);
            Console.WriteLine("\tEvMultiplier: " + contractDetails.EvMultiplier);
            Console.WriteLine("\tMdSizeMultiplier: " + contractDetails.MdSizeMultiplier);
            Console.WriteLine("\tAggGroup: " + contractDetails.AggGroup);
            Console.WriteLine("\tUnderSymbol: " + contractDetails.UnderSymbol);
            Console.WriteLine("\tUnderSecType: " + contractDetails.UnderSecType);
            Console.WriteLine("\tMarketRuleIds: " + contractDetails.MarketRuleIds);
            Console.WriteLine("\tRealExpirationDate: " + contractDetails.RealExpirationDate);
            Console.WriteLine("\tLastTradeTime: " + contractDetails.LastTradeTime);
            printContractDetailsSecIdList(contractDetails.SecIdList);
        }

        public void printContractDetailsSecIdList(List<TagValue> secIdList)
        {
            if (secIdList != null && secIdList.Count > 0)
            {
                Console.Write("\tSecIdList: {");
                foreach (TagValue tagValue in secIdList)
                {
                    Console.Write(tagValue.Tag + "=" + tagValue.Value + ";");
                }
                Console.WriteLine("}");
            }
        }
        public void printBondContractDetailsMsg(ContractDetails contractDetails)
        {
            Console.WriteLine("\tSymbol: " + contractDetails.Contract.Symbol);
            Console.WriteLine("\tSecType: " + contractDetails.Contract.SecType);
            Console.WriteLine("\tCusip: " + contractDetails.Cusip);
            Console.WriteLine("\tCoupon: " + contractDetails.Coupon);
            Console.WriteLine("\tMaturity: " + contractDetails.Maturity);
            Console.WriteLine("\tIssueDate: " + contractDetails.IssueDate);
            Console.WriteLine("\tRatings: " + contractDetails.Ratings);
            Console.WriteLine("\tBondType: " + contractDetails.BondType);
            Console.WriteLine("\tCouponType: " + contractDetails.CouponType);
            Console.WriteLine("\tConvertible: " + contractDetails.Convertible);
            Console.WriteLine("\tCallable: " + contractDetails.Callable);
            Console.WriteLine("\tPutable: " + contractDetails.Putable);
            Console.WriteLine("\tDescAppend: " + contractDetails.DescAppend);
            Console.WriteLine("\tExchange: " + contractDetails.Contract.Exchange);
            Console.WriteLine("\tCurrency: " + contractDetails.Contract.Currency);
            Console.WriteLine("\tMarketName: " + contractDetails.MarketName);
            Console.WriteLine("\tTradingClass: " + contractDetails.Contract.TradingClass);
            Console.WriteLine("\tConId: " + contractDetails.Contract.ConId);
            Console.WriteLine("\tMinTick: " + contractDetails.MinTick);
            Console.WriteLine("\tMdSizeMultiplier: " + contractDetails.MdSizeMultiplier);
            Console.WriteLine("\tOrderTypes: " + contractDetails.OrderTypes);
            Console.WriteLine("\tValidExchanges: " + contractDetails.ValidExchanges);
            Console.WriteLine("\tNextOptionDate: " + contractDetails.NextOptionDate);
            Console.WriteLine("\tNextOptionType: " + contractDetails.NextOptionType);
            Console.WriteLine("\tNextOptionPartial: " + contractDetails.NextOptionPartial);
            Console.WriteLine("\tNotes: " + contractDetails.Notes);
            Console.WriteLine("\tLong Name: " + contractDetails.LongName);
            Console.WriteLine("\tEvRule: " + contractDetails.EvRule);
            Console.WriteLine("\tEvMultiplier: " + contractDetails.EvMultiplier);
            Console.WriteLine("\tAggGroup: " + contractDetails.AggGroup);
            Console.WriteLine("\tMarketRuleIds: " + contractDetails.MarketRuleIds);
            Console.WriteLine("\tLastTradeTime: " + contractDetails.LastTradeTime);
            Console.WriteLine("\tTimeZoneId: " + contractDetails.TimeZoneId);
            printContractDetailsSecIdList(contractDetails.SecIdList);
        }

        public virtual void contractDetailsEnd(int reqId)
        {
            ContractDetailEnd[reqId] = true;
        }

        public virtual void execDetails(int reqId, Contract contract, Execution execution)
        {
            Console.WriteLine("ExecDetails. " + reqId + " - " + contract.Symbol + ", " + contract.SecType + ", " + contract.Currency + " - " + execution.ExecId + ", " + execution.OrderId + ", " + execution.Shares + ", " + execution.LastLiquidity);
        }
        public virtual void execDetailsEnd(int reqId)
        {
            Console.WriteLine("ExecDetailsEnd. " + reqId + "\n");
        }

        public virtual void commissionReport(CommissionReport commissionReport)
        {
            Console.WriteLine("CommissionReport. " + commissionReport.ExecId + " - " + commissionReport.Commission + " " + commissionReport.Currency + " RPNL " + commissionReport.RealizedPNL);
        }

        public virtual void fundamentalData(int reqId, string data)
        {
            Console.WriteLine("FundamentalData. " + reqId + "" + data + "\n");
        }

        public virtual void historicalData(int reqId, Bar bar)
        {
            bot.OnBarUpdate(reqId, bar);
        }

        public virtual void marketDataType(int reqId, int marketDataType)
        {
            Console.WriteLine("MarketDataType. " + reqId + ", Type: " + marketDataType + "\n");
        }

        public virtual void updateMktDepth(int tickerId, int position, int operation, int side, double price, int size)
        {
            Console.WriteLine("UpdateMarketDepth. " + tickerId + " - Position: " + position + ", Operation: " + operation + ", Side: " + side + ", Price: " + price + ", Size: " + size);
        }

        public virtual void updateMktDepthL2(int tickerId, int position, string marketMaker, int operation, int side, double price, int size, bool isSmartDepth)
        {
            Console.WriteLine("UpdateMarketDepthL2. " + tickerId + " - Position: " + position + ", Operation: " + operation + ", Side: " + side + ", Price: " + price + ", Size: " + size + ", isSmartDepth: " + isSmartDepth);
        }

        public virtual void updateNewsBulletin(int msgId, int msgType, String message, String origExchange)
        {
            Console.WriteLine("News Bulletins. " + msgId + " - Type: " + msgType + ", Message: " + message + ", Exchange of Origin: " + origExchange + "\n");
        }

        public virtual void position(string account, Contract contract, double pos, double avgCost)
        {
            Console.WriteLine("Position. " + account + " - Symbol: " + contract.Symbol + ", SecType: " + contract.SecType + ", Currency: " + contract.Currency + ", Position: " + pos + ", Avg cost: " + avgCost);
        }

        public virtual void positionEnd()
        {
            Console.WriteLine("PositionEnd \n");
        }

        public virtual void realtimeBar(int reqId, long time, double open, double high, double low, double close, long volume, double WAP, int count)
        {
            Console.WriteLine("RealTimeBars. " + reqId + " - Time: " + time + ", Open: " + open + ", High: " + high + ", Low: " + low + ", Close: " + close + ", Volume: " + volume + ", Count: " + count + ", WAP: " + WAP);
        }

        public virtual void scannerParameters(string xml)
        {
            Console.WriteLine("ScannerParameters. " + xml + "\n");
        }

        public virtual void scannerData(int reqId, int rank, ContractDetails contractDetails, string distance, string benchmark, string projection, string legsStr)
        {
            Console.WriteLine("ScannerData. " + reqId + " - Rank: " + rank + ", Symbol: " + contractDetails.Contract.Symbol + ", SecType: " + contractDetails.Contract.SecType + ", Currency: " + contractDetails.Contract.Currency
                + ", Distance: " + distance + ", Benchmark: " + benchmark + ", Projection: " + projection + ", Legs String: " + legsStr);
        }

        public virtual void scannerDataEnd(int reqId)
        {
            Console.WriteLine("ScannerDataEnd. " + reqId);
        }

        public virtual void receiveFA(int faDataType, string faXmlData)
        {
            Console.WriteLine("Receing FA: " + faDataType + " - " + faXmlData);
        }

        public virtual void bondContractDetails(int requestId, ContractDetails contractDetails)
        {
            Console.WriteLine("BondContractDetails begin. ReqId: " + requestId);
            printBondContractDetailsMsg(contractDetails);
            Console.WriteLine("BondContractDetails end. ReqId: " + requestId);
        }
        public virtual void historicalDataEnd(int reqId, string startDate, string endDate)
        {
            Console.WriteLine("HistoricalDataEnd - " + reqId + " from " + startDate + " to " + endDate);
        }
        public virtual void verifyMessageAPI(string apiData)
        {
            Console.WriteLine("verifyMessageAPI: " + apiData);
        }
        public virtual void verifyCompleted(bool isSuccessful, string errorText)
        {
            Console.WriteLine("verifyCompleted. IsSuccessfule: " + isSuccessful + " - Error: " + errorText);
        }
        public virtual void verifyAndAuthMessageAPI(string apiData, string xyzChallenge)
        {
            Console.WriteLine("verifyAndAuthMessageAPI: " + apiData + " " + xyzChallenge);
        }
        public virtual void verifyAndAuthCompleted(bool isSuccessful, string errorText)
        {
            Console.WriteLine("verifyAndAuthCompleted. IsSuccessful: " + isSuccessful + " - Error: " + errorText);
        }

        public virtual void displayGroupList(int reqId, string groups)
        {
            Console.WriteLine("DisplayGroupList. Request: " + reqId + ", Groups" + groups);
        }

        public virtual void displayGroupUpdated(int reqId, string contractInfo)
        {
            Console.WriteLine("displayGroupUpdated. Request: " + reqId + ", ContractInfo: " + contractInfo);
        }

        public virtual void positionMulti(int reqId, string account, string modelCode, Contract contract, double pos, double avgCost)
        {
            Console.WriteLine("Position Multi. Request: " + reqId + ", Account: " + account + ", ModelCode: " + modelCode + ", Symbol: " + contract.Symbol + ", SecType: " + contract.SecType + ", Currency: " + contract.Currency + ", Position: " + pos + ", Avg cost: " + avgCost + "\n");
        }

        public virtual void positionMultiEnd(int reqId)
        {
            Console.WriteLine("Position Multi End. Request: " + reqId + "\n");
        }

        public virtual void accountUpdateMulti(int reqId, string account, string modelCode, string key, string value, string currency)
        {
            Console.WriteLine("Account Update Multi. Request: " + reqId + ", Account: " + account + ", ModelCode: " + modelCode + ", Key: " + key + ", Value: " + value + ", Currency: " + currency + "\n");
        }

        public virtual void accountUpdateMultiEnd(int reqId)
        {
            Console.WriteLine("Account Update Multi End. Request: " + reqId + "\n");
        }

        public void securityDefinitionOptionParameter(int reqId, string exchange, int underlyingConId, string tradingClass, string multiplier, HashSet<string> expirations, HashSet<double> strikes)
        {
            Console.WriteLine("Security Definition Option Parameter. Reqest: {0}, Exchange: {1}, Undrelying contract id: {2}, Trading class: {3}, Multiplier: {4}, Expirations: {5}, Strikes: {6}",
                              reqId, exchange, underlyingConId, tradingClass, multiplier, string.Join(", ", expirations), string.Join(", ", strikes));
        }

        public void securityDefinitionOptionParameterEnd(int reqId)
        {
            Console.WriteLine("Security Definition Option Parameter End. Request: " + reqId + "\n");
        }
        public void connectAck()
        {
            if (ClientSocket.AsyncEConnect)
                ClientSocket.startApi();
        }
        public void softDollarTiers(int reqId, SoftDollarTier[] tiers)
        {
            Console.WriteLine("Soft Dollar Tiers:");

            foreach (var tier in tiers)
            {
                Console.WriteLine(tier.DisplayName);
            }
        }
        public void familyCodes(FamilyCode[] familyCodes)
        {
            Console.WriteLine("Family Codes:");

            foreach (var familyCode in familyCodes)
            {
                Console.WriteLine("Account ID: {0}, Family Code Str: {1}", familyCode.AccountID, familyCode.FamilyCodeStr);
            }
        }
        public void symbolSamples(int reqId, ContractDescription[] contractDescriptions)
        {
            string derivSecTypes;
            Console.WriteLine("Symbol Samples. Request Id: {0}", reqId);

            foreach (var contractDescription in contractDescriptions)
            {
                derivSecTypes = "";
                foreach (var derivSecType in contractDescription.DerivativeSecTypes)
                {
                    derivSecTypes += derivSecType;
                    derivSecTypes += " ";
                }
                Console.WriteLine("Contract: conId - {0}, symbol - {1}, secType - {2}, primExchange - {3}, currency - {4}, derivativeSecTypes - {5}",
                    contractDescription.Contract.ConId, contractDescription.Contract.Symbol, contractDescription.Contract.SecType,
                    contractDescription.Contract.PrimaryExch, contractDescription.Contract.Currency, derivSecTypes);
            }
        }
        public void mktDepthExchanges(DepthMktDataDescription[] depthMktDataDescriptions)
        {
            Console.WriteLine("Market Depth Exchanges:");

            foreach (var depthMktDataDescription in depthMktDataDescriptions)
            {
                Console.WriteLine("Depth Market Data Description: Exchange: {0}, Security Type: {1}, Listing Exch: {2}, Service Data Type: {3}, Agg Group: {4}",
                    depthMktDataDescription.Exchange, depthMktDataDescription.SecType,
                    depthMktDataDescription.ListingExch, depthMktDataDescription.ServiceDataType,
                    depthMktDataDescription.AggGroup != Int32.MaxValue ? depthMktDataDescription.AggGroup.ToString() : ""
                    );
            }
        }
        public void tickNews(int tickerId, long timeStamp, string providerCode, string articleId, string headline, string extraData)
        {
            Console.WriteLine("Tick News. Ticker Id: {0}, Time Stamp: {1}, Provider Code: {2}, Article Id: {3}, headline: {4}, extraData: {5}", tickerId, timeStamp, providerCode, articleId, headline, extraData);
        }
        public void smartComponents(int reqId, Dictionary<int, KeyValuePair<string, char>> theMap)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("==== Smart Components Begin (total={0}) reqId = {1} ====\n", theMap.Count, reqId);

            foreach (var item in theMap)
            {
                sb.AppendFormat("bit number: {0}, exchange: {1}, exchange letter: {2}\n", item.Key, item.Value.Key, item.Value.Value);
            }

            sb.AppendFormat("==== Smart Components Begin (total={0}) reqId = {1} ====\n", theMap.Count, reqId);

            Console.WriteLine(sb);
        }
        public void tickReqParams(int tickerId, double minTick, string bboExchange, int snapshotPermissions)
        {
            Console.WriteLine("id={0} minTick = {1} bboExchange = {2} snapshotPermissions = {3}", tickerId, minTick, bboExchange, snapshotPermissions);

            BboExchange = bboExchange;
        }
        public void newsProviders(NewsProvider[] newsProviders)
        {
            Console.WriteLine("News Providers:");

            foreach (var newsProvider in newsProviders)
            {
                Console.WriteLine("News provider: providerCode - {0}, providerName - {1}",
                    newsProvider.ProviderCode, newsProvider.ProviderName);
            }
        }
        public void newsArticle(int requestId, int articleType, string articleText)
        {
            Console.WriteLine("News Article. Request Id: {0}, ArticleType: {1}", requestId, articleType);
            if (articleType == 0)
            {
                Console.WriteLine("News Article Text: {0}", articleText);
            }
            else if (articleType == 1)
            {
                Console.WriteLine("News Article Text: article text is binary/pdf and cannot be displayed");
            }
        }
        public void historicalNews(int requestId, string time, string providerCode, string articleId, string headline)
        {
            Console.WriteLine("Historical News. Request Id: {0}, Time: {1}, Provider Code: {2}, Article Id: {3}, headline: {4}", requestId, time, providerCode, articleId, headline);
        }
        public void historicalNewsEnd(int requestId, bool hasMore)
        {
            Console.WriteLine("Historical News End. Request Id: {0}, Has More: {1}", requestId, hasMore);
        }
        public void headTimestamp(int reqId, string headTimestamp)
        {
            Console.WriteLine("Head time stamp. Request Id: {0}, Head time stamp: {1}", reqId, headTimestamp);
        }
        public void histogramData(int reqId, HistogramEntry[] data)
        {
            Console.WriteLine("Histogram data. Request Id: {0}, data size: {1}", reqId, data.Length);
            data.ToList().ForEach(i => Console.WriteLine("\tPrice: {0}, Size: {1}", i.Price, i.Size));
        }
        public void historicalDataUpdate(int reqId, Bar bar)
        {
            bot.OnBarUpdate(reqId, bar);
        }
        public void rerouteMktDataReq(int reqId, int conId, string exchange)
        {
            Console.WriteLine("Re-route market data request. Req Id: {0}, ConId: {1}, Exchange: {2}", reqId, conId, exchange);
        }
        public void rerouteMktDepthReq(int reqId, int conId, string exchange)
        {
            Console.WriteLine("Re-route market depth request. Req Id: {0}, ConId: {1}, Exchange: {2}", reqId, conId, exchange);
        }

        public void marketRule(int marketRuleId, PriceIncrement[] priceIncrements)
        {
            Console.WriteLine("Market Rule Id: " + marketRuleId);
            foreach (var priceIncrement in priceIncrements)
            {
                Console.WriteLine("Low Edge: {0}, Increment: {1}", ((decimal)priceIncrement.LowEdge).ToString(), ((decimal)priceIncrement.Increment).ToString());
            }
        }
        public void pnl(int reqId, double dailyPnL, double unrealizedPnL, double realizedPnL)
        {
            Console.WriteLine("PnL. Request Id: {0}, Daily PnL: {1}, Unrealized PnL: {2}, Realized PnL: {3}", reqId, dailyPnL, unrealizedPnL, realizedPnL);
        }
        public void pnlSingle(int reqId, int pos, double dailyPnL, double unrealizedPnL, double realizedPnL, double value)
        {
            Console.WriteLine("PnL Single. Request Id: {0}, Pos {1}, Daily PnL {2}, Unrealized PnL {3}, Realized PnL: {4}, Value: {5}", reqId, pos, dailyPnL, unrealizedPnL, realizedPnL, value);
        }
        public void historicalTicks(int reqId, HistoricalTick[] ticks, bool done)
        {
            //foreach (var tick in ticks)
            //{
            //    Console.WriteLine("Historical Tick. Request Id: {0}, Time: {1}, Price: {2}, Size: {3}", reqId, Util.UnixSecondsToString(tick.Time, "yyyyMMdd-HH:mm:ss zzz"), tick.Price, tick.Size);
            //}
        }

        public void historicalTicksBidAsk(int reqId, HistoricalTickBidAsk[] ticks, bool done)
        {
            foreach (var tick in ticks)
            {
                Console.WriteLine("Historical Tick Bid/Ask. Request Id: {0}, Time: {1}, Price Bid: {2}, Price Ask: {3}, Size Bid: {4}, Size Ask: {5}, Bid/Ask Tick Attribs: {6} ",
                    reqId, Util.UnixSecondsToString(tick.Time, "yyyyMMdd-HH:mm:ss zzz"), tick.PriceBid, tick.PriceAsk, tick.SizeBid, tick.SizeAsk, tick.TickAttribBidAsk.toString());
            }
        }

        public void historicalTicksLast(int reqId, HistoricalTickLast[] ticks, bool done)
        {
            //foreach (var tick in ticks)
            //{
            //    Console.WriteLine("Historical Tick Last. Request Id: {0}, Time: {1}, Price: {2}, Size: {3}, Exchange: {4}, Special Conditions: {5}, Last Tick Attribs: {6} ",
            //        reqId, Util.UnixSecondsToString(tick.Time, "yyyyMMdd-HH:mm:ss zzz"), tick.Price, tick.Size, tick.Exchange, tick.SpecialConditions, tick.TickAttribLast.toString());
            //}
        }

        public void tickByTickAllLast(int reqId, int tickType, long time, double price, int size, TickAttribLast tickAttribLast, string exchange, string specialConditions)
        {
            bot.OnTickUpdate(reqId, time, price);
        }

        public void tickByTickBidAsk(int reqId, long time, double bidPrice, double askPrice, int bidSize, int askSize, TickAttribBidAsk tickAttribBidAsk)
        {
            Console.WriteLine("Tick-By-Tick. Request Id: {0}, TickType: BidAsk, Time: {1}, BidPrice: {2}, AskPrice: {3}, BidSize: {4}, AskSize: {5}, BidPastLow: {6}, AskPastHigh: {7}",
                reqId, Util.UnixSecondsToString(time, "yyyyMMdd-HH:mm:ss zzz"), bidPrice, askPrice, bidSize, askSize, tickAttribBidAsk.BidPastLow, tickAttribBidAsk.AskPastHigh);
        }

        public void tickByTickMidPoint(int reqId, long time, double midPoint)
        {
            Console.WriteLine("Tick-By-Tick. Request Id: {0}, TickType: MidPoint, Time: {1}, MidPoint: {2}",
                reqId, Util.UnixSecondsToString(time, "yyyyMMdd-HH:mm:ss zzz"), midPoint);
        }
        public void orderBound(long orderId, int apiClientId, int apiOrderId)
        {
            Console.WriteLine("Order bound. Order Id: {0}, Api Client Id: {1}, Api Order Id: {2}", orderId, apiClientId, apiOrderId);
        }
        public virtual void completedOrder(Contract contract, Order order, OrderState orderState)
        {
            Console.WriteLine("CompletedOrder. PermID: " + order.PermId + ", ParentPermId: " + Util.LongMaxString(order.ParentPermId) + ", Account: " + order.Account + ", Symbol: " + contract.Symbol + ", SecType: " + contract.SecType +
                " , Exchange: " + contract.Exchange + ", Action: " + order.Action + ", OrderType: " + order.OrderType + ", TotalQty: " + order.TotalQuantity +
                ", CashQty: " + order.CashQty + ", FilledQty: " + order.FilledQuantity + ", LmtPrice: " + order.LmtPrice + ", AuxPrice: " + order.AuxPrice + ", Status: " + orderState.Status +
                ", CompletedTime: " + orderState.CompletedTime + ", CompletedStatus: " + orderState.CompletedStatus);
        }
        public virtual void completedOrdersEnd()
        {
            Console.WriteLine("CompletedOrdersEnd");
        }

    }
}