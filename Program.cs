using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;
using System.Configuration;

namespace ConsoleApp5
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string JsonData = await GetCowinDataByDistrict();
         
            Root allData = JsonConvert.DeserializeObject<Root>(JsonData);
            var result = allData.centers.Where(a => a.sessions.Contains(a.sessions.Find(p => p.min_age_limit == 18 && p.available_capacity >= 0)));
            List<int> pincodes = new List<int>();

            if (!result.Any())
            {
                Console.WriteLine("No vaccine for 18+");
            }
            foreach (var info in result)
            {
                Console.WriteLine(string.Format("Hospital name : {0}, Pin : {1}", info.name, info.pincode));
                foreach (var session in info.sessions)
                {
                    if(session.available_capacity > 0)
                    {
                        if(!pincodes.Contains(info.pincode))
                        {
                            pincodes.Add(info.pincode);
                        }
                    }
                    Console.WriteLine(string.Format("Session capacity : {0}, date : {1}, Vaccine : {2}, Age : {3}", Convert.ToString(session.available_capacity), session.date, session.vaccine, Convert.ToString(session.min_age_limit)));
                }
            }
            Console.WriteLine("------------------------------------");
            foreach (var pin in pincodes)
            {
                string dataByPin = await GetCowinDataByPin(pin);
                Root rootDataByPin = JsonConvert.DeserializeObject<Root>(dataByPin);
                var resultByPin = rootDataByPin.centers.Where(a => a.sessions.Contains(a.sessions.Find(p => p.min_age_limit == 18 && p.available_capacity >= 0)));
                foreach (var info in resultByPin)
                {
                    Console.WriteLine(string.Format("Hospital name : {0}, Pin : {1}", info.name, info.pincode));
                    foreach (var session in info.sessions)
                    {
                        Console.WriteLine(string.Format("Session capacity : {0}, date : {1}, Vaccine : {2}, Age : {3}", Convert.ToString(session.available_capacity), session.date, session.vaccine, Convert.ToString(session.min_age_limit)));
                    }
                }
            }

            Thread.Sleep(60000);

        }

        public static async Task<String> GetCowinDataByDistrict()
        {
            
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            HttpClient client = new HttpClient();
            string districtCode = ConfigurationSettings.AppSettings.Get("DistrictCode");
            HttpResponseMessage response = await client.GetAsync(String.Format("https://cdn-api.co-vin.in/api/v2/appointment/sessions/calendarByDistrict?district_id={1}&date={0}", date, districtCode));
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        public static async Task<String> GetCowinDataByPin(int pincode)
        {
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(String.Format("https://cdn-api.co-vin.in/api/v2/appointment/sessions/calendarByPin?pincode={0}&date={1}", Convert.ToString(pincode), date));
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Session
    {
        public string session_id { get; set; }
        public string date { get; set; }
        public Decimal available_capacity { get; set; }
        public int min_age_limit { get; set; }
        public string vaccine { get; set; }
        public List<string> slots { get; set; }
    }

    public class VaccineFee
    {
        public string vaccine { get; set; }
        public string fee { get; set; }
    }

    public class Center
    {
        public int center_id { get; set; }
        public string name { get; set; }
        public string state_name { get; set; }
        public string district_name { get; set; }
        public string block_name { get; set; }
        public int pincode { get; set; }
        public int lat { get; set; }
        public int @long { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string fee_type { get; set; }
        public List<Session> sessions { get; set; }
        public List<VaccineFee> vaccine_fees { get; set; }
    }

    public class Root
    {
        public List<Center> centers { get; set; }
    }



}
