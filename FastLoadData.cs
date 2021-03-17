using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastLoadTestData
{

    using FileHelpers;

    [DelimitedRecord(",")]
    public class FastLoadData : IComparable<FastLoadData>
    {
        public string strTagName = "";
        public string strOppType = "1";
        public DateTime dtDate;
        public string strValType = "1";
        public string strValue = "";
        public string strQuality = "";

        public int CompareTo(FastLoadData other)
        {
            return dtDate.CompareTo(other.dtDate);
        }


        public string GetExportString()
        {
            string strReturn = strTagName + "," + strOppType + "," + dtDate.ToString("yyyy/MM/dd") + "," + dtDate.ToString("HH:mm:ss.fff") + "," + strValType + "," + strValue + "," + strQuality;
            return strReturn;
        }

    }
}


