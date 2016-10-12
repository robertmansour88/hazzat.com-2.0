using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using hazzat.com;
using System.Diagnostics;
using System.ServiceModel;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HazzatService
{
    public class DataCacheLoader
    {
        private const string HazzatServiceEndpoint = "http://hazzat.com/DesktopModules/Hymns/WebService/HazzatWebService.asmx";
        private const int HazzatServiceMaxReceivedMessageSize = 2147483647;
        private const int HazzatServiceMaxBufferPoolSize = 2147483647;
        private const int HazzatServiceMaxBufferSize = 2147483647;

        public async Task<Dictionary<string, Dictionary<string, List<string>>>> DataCacheLoader_Load()
        {
            


            return null;
        }

        private BasicHttpBinding HazzatServiceBinding
        {
            get
            {
                return new BasicHttpBinding()
                {
                    MaxReceivedMessageSize = HazzatServiceMaxReceivedMessageSize,
                    MaxBufferSize = HazzatServiceMaxBufferSize,
                    MaxBufferPoolSize = HazzatServiceMaxBufferPoolSize
                };
            }
        }

        /// <summary>
        /// A collection of hazzat.com objects
        /// </summary>
        public SeasonInfo[] Seasons { get; private set; }
        public HymnStructNameViewModel[] Hymns { get; private set; }
        public StructureInfo[] HymnsBySeason { get; private set; }
        public ServiceHymnInfo[] HazzatHymns { get; private set; }
        public ServiceHymnsContentInfo[] HymnContentInfo { get; private set; }
        public ServiceHymnsContentInfo[] HazzatHymnContentInfo { get; private set; }
        public ServiceHymnsContentInfo[] VerticalHazzatHymnContent { get; private set; }



        public void createSeasonsViewModel(bool isDateSpecific)
        {
            try
            {
                HazzatWebServiceSoapClient client = new HazzatWebServiceSoapClient(HazzatServiceBinding, new EndpointAddress(HazzatServiceEndpoint));
                client.GetSeasonsCompleted += new EventHandler<GetSeasonsCompletedEventArgs>(client_GetCompleted);
                client.GetSeasonsAsync(isDateSpecific);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void client_GetCompleted(object sender, GetSeasonsCompletedEventArgs e)
        {
            Seasons = e.Result;
        }

        public void createViewModelBySeason(int Season)
        {
            try
            {
                HazzatWebServiceSoapClient client = new HazzatWebServiceSoapClient(HazzatServiceBinding, new EndpointAddress(HazzatServiceEndpoint));
                client.GetSeasonServicesCompleted += new EventHandler<GetSeasonServicesCompletedEventArgs>(GetCompletedStructBySeason);
                client.GetSeasonServicesAsync(Season);
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void GetCompletedStructBySeason(object sender, GetSeasonServicesCompletedEventArgs e)
        {
            HymnsBySeason = e.Result;
        }

        public void FetchServiceHymns(int StructId, Action<object, GetSeasonServiceHymnsCompletedEventArgs> GetCompletedHymnsBySeason)
        {
            try
            {
                HazzatWebServiceSoapClient client = new HazzatWebServiceSoapClient(HazzatServiceBinding, new EndpointAddress(HazzatServiceEndpoint));
                client.GetSeasonServiceHymnsCompleted += new EventHandler<GetSeasonServiceHymnsCompletedEventArgs>(GetCompletedHymnsBySeason);
                client.GetSeasonServiceHymnsAsync(StructId);
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void GetCompletedHymnsBySeason(object sender, GetSeasonServiceHymnsCompletedEventArgs e)
        {
            HazzatHymns = e.Result;
        }

        public void CreateHymnTextViewModel(int itemId, Action<object, GetSeasonServiceHymnTextCompletedEventArgs> client_GetCompletedHymnInfo)
        {
            try
            {
                HazzatWebServiceSoapClient client = new HazzatWebServiceSoapClient(HazzatServiceBinding, new EndpointAddress(HazzatServiceEndpoint));

                client.GetSeasonServiceHymnTextCompleted += new EventHandler<GetSeasonServiceHymnTextCompletedEventArgs>(client_GetCompletedHymnInfo);
                client.GetSeasonServiceHymnHazzatCompleted += new EventHandler<GetSeasonServiceHymnHazzatCompletedEventArgs>(client_GetCompletedHymnHazzat);
                client.GetSeasonServiceHymnVerticalHazzatCompleted += new EventHandler<GetSeasonServiceHymnVerticalHazzatCompletedEventArgs>(client_GetCompletedHymnVerticalHazzat);

                client.GetSeasonServiceHymnTextAsync(itemId);
                client.GetSeasonServiceHymnHazzatAsync(itemId);
                client.GetSeasonServiceHymnVerticalHazzatAsync(itemId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void client_GetCompletedHymnInfo(object sender, GetSeasonServiceHymnTextCompletedEventArgs e)
        {
            HymnContentInfo = e.Result;
        }

        public void client_GetCompletedHymnHazzat(object sender, GetSeasonServiceHymnHazzatCompletedEventArgs e)
        {
            HazzatHymnContentInfo = e.Result;
        }

        private void client_GetCompletedHymnVerticalHazzat(object sender, GetSeasonServiceHymnVerticalHazzatCompletedEventArgs e)
        {
            VerticalHazzatHymnContent = e.Result;
        }
    }
}
