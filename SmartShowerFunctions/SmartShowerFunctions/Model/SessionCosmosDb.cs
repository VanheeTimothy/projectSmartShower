using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartShowerFunctions.Model
{
    class SessionCosmosDb
    {
        [JsonProperty("idsession")]
        public string IdSession { get; set; }
        [JsonProperty("idshower")]
        public Guid IdShower { get; set; }
        [JsonProperty("profilenumber")]
        public int ProfileNumber { get; set; }
        [JsonProperty("tijdfase")]
        public int TijdFase { get; set; }
        [JsonProperty("temp")]
        public float Temp { get; set; }
        [JsonProperty("waterusage")]
        public int WaterUsage { get; set; }
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
