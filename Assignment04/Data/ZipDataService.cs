using Assignment04.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Assignment04.Data
{
    public class ZipDataService : IZipDataService
    {
        public async Task<List<ZipCode>> GetAllZipAsync()
        {
            return await Task.Run(() =>
           {
               var zips = new List<ZipCode>();
               var currpath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location).Split('\\');
               var loc = Path.Combine(string.Join("\\", currpath.Take(currpath.Length - 3)), "Data", "zip.csv");
               using (var reader = new StreamReader(loc))
               {
                   while (!reader.EndOfStream)
                   {
                       var line = reader.ReadLine().Split(',');
                       var country = Regex.IsMatch(line[0], "^[A-Z]") ? ZipCode.Country.Canada : ZipCode.Country.US;
                       zips.Add(new ZipCode(line[0], line[2], line[1], country));
                   }
               }
               return zips;
           });
        }
    }
}
