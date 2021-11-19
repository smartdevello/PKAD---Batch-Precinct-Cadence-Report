using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace PKAD___Batch_Precinct_Cadence_Report
{
    public class BatchPrecinctData
    {
        [Name("ID")]
        public int id { get; set; }

        [Name("Folder")]
        public string folder { get; set; }

        [Name("Name")]
        public string name { get; set; }

        [Name("Right Info")]
        public string printer_id { get; set; }

        [Name("Left Info")]
        public string left_info { get; set; }

        [Name("MOT")]
        public int mot { get; set; }

        [Name("POT")]
        public int pot { get; set; }


        [Name("GOT")]
        public int got { get; set; }

        [Name("IED")]
        public int ied { get; set; }

        [Name("GOP")]
        public double gop { get; set; }

        [Name("GONZ")]
        public double gonz { get; set; }
    }
}
