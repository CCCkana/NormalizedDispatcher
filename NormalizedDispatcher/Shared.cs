using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NormalizedDispatcher.Shared
{
    public record StaticParameters
    {
        public double zMax1;
        public double zMax2;
        public double zMin;
        public double K;

        public StaticParameters()
        {
            zMax1 = 780.0;
            zMax2 = 773.1;
            zMin = 731.0;
            K = 8.6;
        }


    }

    public class Loader
    {
        private string basePath;
        private string folderPath;
        public Loader()
        {
            basePath = Directory.GetCurrentDirectory();
            folderPath = Path.Combine(basePath, "Data");
        }
    }
}
