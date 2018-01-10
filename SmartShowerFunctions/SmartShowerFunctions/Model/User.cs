using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartShowerFunctions.Model
{
    public class User
    {
        [JsonProperty("iduser")]
        public Guid IdUser { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("color")]
        public string Color { get; set; }
        [JsonProperty("maxshowertime")]
        public int MaxShowerTime { get; set; }
        [JsonProperty("idealtemperature")]
        public float IdealTemperature { get; set; }
        [JsonProperty("monitor")]
        public bool Monitor { get; set; }
        [JsonProperty("photo")]
        public string Photo { get; set; }


    }
}
