using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartShowerFunctions.Model
{
    public class Session
    {
        [JsonProperty("idsession")]
        public Guid IdSession { get; set; }
        [JsonProperty("iduser")]
        public Guid IdUser { get; set; }
        [JsonProperty("waterused")]
        public float WaterUsed { get; set; }
        [JsonProperty("moneysaved")]
        public float MoneySaved { get; set; }
        [JsonProperty("ecoscore")]
        public float EcoScore { get; set; }
        [JsonProperty("averagetemp")]
        public float AverageTemp { get; set; }
        [JsonProperty("duration")]
        public int Duration { get; set; }
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
        [JsonProperty("datalength")] // int om te bepalen data tijdspanne zit niet in de database
        public int DataLenght { get; set; }        
    }
}
