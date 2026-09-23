using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Spatial;
using Newtonsoft.Json;
using System.Globalization;

namespace eTimeTrial.Web.Backend.Entities
{
    public class Race
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string RaceName { get; set; }
        public string RaceStartDate { get; set; }
        public string RaceStartTime { get; set; }
        public DateTime? RaceStart { get 
            {
                string combined = $"{RaceStartDate} {RaceStartTime}";

                // Define the expected format (adjust to your input format)
                string format = "yyyy-MM-dd HH:mm:ss";

                // Parse with invariant culture to avoid locale issues

                if(DateTime.TryParse(combined, out DateTime result))
                //if (DateTime.TryParseExact(
                //        combined,
                //        format,
                //        CultureInfo.InvariantCulture,
                //        DateTimeStyles.None,
                //        out DateTime result))
                {
                    return result;
                }
                return DateTime.UtcNow ;


            }
            set { }
        }
        public DateTime RaceFinish { get; set; }
        public int DistanceKm { get; set; }
        public int Interval { get; set; }
        public List<Team>? Teams { get; set; }
        public string Status { get; set; }
    }
}
