using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartShowerFunctions.Model
{
    public class Shower
    {
        [JsonProperty("idshower")]
        public Guid IdShower { get; set; }
        [JsonProperty("watercost")]
        public float WaterCost { get; set; }

    }
}
