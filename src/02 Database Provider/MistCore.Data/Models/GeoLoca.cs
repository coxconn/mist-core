using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace MistCore.Data
{
    [DataContract(Namespace = "http://tempuri.org/")]
    public class GeoLoca
    {
        public GeoLoca() { }

        public GeoLoca(double latitude, double longitude)
        {
            this.Lat = latitude;
            this.Lon = longitude;
        }

        public GeoLoca(string latitude, string longitude)
        {
            double p = 0;
            if (double.TryParse(latitude, out p))
            {
                this.Lat = p;
            }
            if (double.TryParse(longitude, out p))
            {
                this.Lon = p;
            }
        }

        [DataMember(Name = "Lat")]
        public double Lat { get; set; }

        [DataMember(Name = "Lon")]
        public double Lon { get; set; }

    }
}
