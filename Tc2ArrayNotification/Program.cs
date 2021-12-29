using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection.Metadata;
using System.Reflection;
using System.Xml.Linq;
using TwinCAT.Ads;
using TwinCAT.Ads.Reactive;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;

namespace Tc2ArrayNotification;

internal static class Program
{
    // https://github.com/Beckhoff/TF6000_ADS_DOTNET_V4_Samples/blob/main/Sources/BaseSamples/Reactive/Program.cs
    // line:191
    private static void Main()
    {
        using (TcAdsClient client = new TcAdsClient())
        {
            // Connect to target (testing on a VM)
            client.Connect("192.168.110.254.1.1", 801);

            // Create Symbol information
            var symbolLoader = SymbolLoaderFactory.Create(client, SymbolLoaderSettings.Default);

            int eventCount = 1;

            // Reactive Notification Handler
            var valueObserver = Observer.Create<SymbolNotification>(not =>
                {
                    Console.WriteLine(string.Format("{0} {1:u} {2} = '{3}' ({4})", eventCount++, not.TimeStamp, not.Symbol.InstancePath, not.Value, not.Symbol.DataType));
                }
            );

            // Collect the symbols that are registered as Notification sources for their changed values.
            SymbolCollection notificationSymbols = new SymbolCollection();

            //IArrayInstance taskInfo = (IArrayInstance)symbolLoader.Symbols["TwinCAT_SystemInfoVarList._TaskInfo"];
            //foreach (ISymbol element in taskInfo.Elements)
            //{
            //    ISymbol cycleCount = element.SubSymbols["CycleCount"];
            //    ISymbol lastExecTime = element.SubSymbols["LastExecTime"];

            //    notificationSymbols.Add(cycleCount);
            //    notificationSymbols.Add(lastExecTime);
            //}

            //////////////////////////////////////////////////////////////////////////////
            var intVal = symbolLoader.Symbols["Main.intVal"];
            notificationSymbols.Add(intVal);
            // comment out the following two lines will work.
            var intVal2 = symbolLoader.Symbols["Main.intVal2"];
            notificationSymbols.Add(intVal2);
            //Ads Error: 1 : [NotificationReceiver:OnNotificationError()] Exception: Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection. (Parameter 'count')
            //Ads Error: 1 : [AdsValueAccessor:adsClient_AdsNotificationError()] Exception: Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection. (Parameter 'count')
            //////////////////////////////////////////////////////////////////////////////

            // Create a subscription for the first 200 Notifications on Symbol Value changes.
            IDisposable subscription = client.WhenNotification(notificationSymbols, NotificationSettings.Default).Take(200).Subscribe(valueObserver);

            Console.ReadKey(); // Wait for Key press
            subscription.Dispose(); // Dispose the Subscription
        }
    }
}