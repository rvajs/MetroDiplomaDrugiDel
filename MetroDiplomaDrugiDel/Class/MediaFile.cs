using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroDiplomaDrugiDel.Class
{
    public class MediaFile
    {
        public MediaFile(string naziv, string trajanje)
        {
            Naziv = naziv;
            Trajanje = trajanje;
        }

        public string Naziv { get; set; }
        public string Trajanje { get; set; }
    }
}
