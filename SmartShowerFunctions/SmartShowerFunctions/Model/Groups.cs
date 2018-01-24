using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartShowerFunctions.Model
{
    class Groups
    {
        [JsonProperty("idgroup")]
        public Guid IdGroup { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("photo")]
        public string Photo { get; set; }

    }
}
