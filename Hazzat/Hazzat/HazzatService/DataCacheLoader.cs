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

        public bool isOtherLoaded = false;

        private Dictionary<string, Dictionary<string, Dictionary<KeyValuePair<int, string>, Dictionary<string, string>>>> tempCache;


        public void DataCacheLoader_Load()
        {
            tempCache = new Dictionary<string, Dictionary<string, Dictionary<KeyValuePair<int, string>, Dictionary<string, string>>>>();

            createSeasonsViewModel(true);
            createSeasonsViewModel(false);
        }

        public Dictionary<string, Dictionary<string, Dictionary<KeyValuePair<int, string>, Dictionary<string, string>>>> GetCache()
        {
            return tempCache;
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

            if (isOtherLoaded)
            {
                lock (tempCache)
                {
                    foreach (var season in Seasons)
                    {
                        tempCache.Add(season.Name, null);
                        createViewModelBySeason(season.ItemId);
                    }
                }
            }
            else {
                isOtherLoaded = true;
            }
        }

        public void createViewModelBySeason(int Season)
        {
            try
            {
                HazzatWebServiceSoapClient client = new HazzatWebServiceSoapClient(HazzatServiceBinding, new EndpointAddress(HazzatServiceEndpoint));
                client.GetSeasonServicesCompleted += new EventHandler<GetSeasonServicesCompletedEventArgs>(GetCompletedStructsBySeason);
                client.GetSeasonServicesAsync(Season);
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void GetCompletedStructsBySeason(object sender, GetSeasonServicesCompletedEventArgs e)
        {
            HymnsBySeason = e.Result;
            lock (tempCache)
            {
                lock (HymnsBySeason)
                {
                    foreach (var Struct in HymnsBySeason)
                    {
                        foreach (var keyValue in tempCache)
                        {
                            if (keyValue.Key == Struct.Season_Name)
                            {
                                keyValue.Value.Add(Struct.Name, null);
                                GetServiceHymns(Struct.ItemId); 
                            }
                        }
                    }
                }
            }
        }

        public void GetServiceHymns(int StructId)
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
            lock (tempCache)
            {
                lock (HazzatHymns)
                {
                    foreach (var Hymn in HazzatHymns)
                    {
                        foreach (var keyValue in tempCache.Values)
                        {
                            foreach (var keyValue2 in keyValue)
                            {
                                if (keyValue2.Key == Hymn.Structure_Name)
                                {
                                    keyValue2.Value.Add(new KeyValuePair<int, string>(Hymn.ItemId, Hymn.Title), null);
                                    CreateHymnTextViewModel(Hymn.ItemId);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CreateHymnTextViewModel(int itemId)
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

            lock (tempCache)
            {
                lock (HymnContentInfo)
                {
                    foreach (var Hymn in HymnContentInfo)
                    {
                        foreach (var keyValue in tempCache.Values)
                        {
                            foreach (var keyValue2 in keyValue)
                            {
                                foreach (var keyvalue3 in keyValue2.Value)
                                {
                                    if (keyvalue3.Key.Equals(new KeyValuePair<int, string>(Hymn.ItemId, Hymn.Title)))
                                    {
                                        keyvalue3.Value.Add("CopticText", Hymn.Content_Coptic);
                                        keyvalue3.Value.Add("EnglishText", Hymn.Content_English);
                                        keyvalue3.Value.Add("ArabicText", Hymn.Content_Arabic);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void client_GetCompletedHymnHazzat(object sender, GetSeasonServiceHymnHazzatCompletedEventArgs e)
        {
            HazzatHymnContentInfo = e.Result;

            lock (tempCache)
            {
                lock (HazzatHymnContentInfo)
                {
                    foreach (var Hymn in HazzatHymnContentInfo)
                    {
                        foreach (var keyValue in tempCache.Values)
                        {
                            foreach (var keyValue2 in keyValue)
                            {
                                foreach (var keyvalue3 in keyValue2.Value)
                                {
                                    if (keyvalue3.Key.Equals(new KeyValuePair<int, string>(Hymn.ItemId, Hymn.Title)))
                                    {
                                        keyvalue3.Value.Add("CopticHazzat", Hymn.Content_Coptic);
                                        keyvalue3.Value.Add("EnglishHazzat", Hymn.Content_English);
                                        keyvalue3.Value.Add("ArabicHazzat", Hymn.Content_Arabic);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void client_GetCompletedHymnVerticalHazzat(object sender, GetSeasonServiceHymnVerticalHazzatCompletedEventArgs e)
        {
            VerticalHazzatHymnContent = e.Result;

            lock (tempCache)
            {
                lock (VerticalHazzatHymnContent)
                {
                    foreach (var Hymn in VerticalHazzatHymnContent)
                    {
                        foreach (var keyValue in tempCache.Values)
                        {
                            foreach (var keyValue2 in keyValue)
                            {
                                foreach (var keyvalue3 in keyValue2.Value)
                                {
                                    if (keyvalue3.Key.Equals(new KeyValuePair<int, string>(Hymn.ItemId, Hymn.Title)))
                                    {
                                        keyvalue3.Value.Add("CopticVerticalHazzat", Hymn.Content_Coptic);
                                        keyvalue3.Value.Add("EnglishVerticalHazzat", Hymn.Content_English);
                                        keyvalue3.Value.Add("ArabicVerticalHazzat", Hymn.Content_Arabic);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
