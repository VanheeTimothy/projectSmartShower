using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartShowerFunctions.Model
{
    class GroupSessions
    {
        [JsonProperty("idgroup")]
        public Guid IdGroup { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("photo")]
        public string Photo { get; set; }
        [JsonProperty("iduser")]
        public Guid IdUser { get; set; }
        [JsonProperty("idsession")]
        public Guid IdSession { get; set; }
        [JsonProperty("waterused")]
        public float WaterUsed { get; set; }
        [JsonProperty("moneysaved")]
        public float MoneySaved { get; set; }
        [JsonProperty("ecoscore")]
        public float EcoScore { get; set; }
        [JsonProperty("averagetemp")]
        public float AverageTemp { get; set; }
        [JsonProperty("duration")]
        public TimeSpan Duration { get; set; }
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
