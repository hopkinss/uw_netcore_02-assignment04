using Assignment04.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Assignment04.Data
{
    public interface IZipDataService
    {
        Task<List<ZipCode>> GetAllZipAsync();
    }
}