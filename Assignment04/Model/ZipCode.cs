using System;
using System.Collections.Generic;
using System.Text;

namespace Assignment04.Model
{
    public class ZipCode
    {
        public enum Country { Unknown=0,US, Canada }

        public ZipCode() { }
        public ZipCode(string zip, string state, string city, Country country)
        {
            Zip = zip;
            State = state;
            City = city;
            CountryCode = country;
        }

        public Country CountryCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public List<string> Cities { get; set; }
 
    }
}
