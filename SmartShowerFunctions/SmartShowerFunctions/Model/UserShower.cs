using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartShowerFunctions.Model
{
    class UserShower
    {
        [JsonProperty("idshower")]
        public Guid IdShower { get; set; }
        [JsonProperty("iduser")]
        public Guid IdUser { get; set; }

    }
}
