using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Data;

using System.Data.SqlClient;

namespace FastLoadTestData
{
    

    class FastLoadSimManager
    {

        public static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static string szFLPath;
        static string szTempPath;
        static string szFilter;
        static DateTime dtStart;
        static DateTime dtEnd;
        static string szConfigPath;
        static int iWindowSize;
        


        public FastLoadSimManager()
        {
            string exe = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string path = System.IO.Path.GetDirectoryName(exe);
            szConfigPath = path + "\\config.xml";

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(szConfigPath);
           
            szTempPath = doc.SelectSingleNode("/Configuration/FASTLOAD/TEMPDIR").InnerText;
            szFLPath = doc.SelectSingleNode("/Configuration/FASTLOAD/FLDIR").InnerText;
            dtStart = Convert.ToDateTime(doc.SelectSingleNode("/Configuration/FASTLOAD/STARTDATE").InnerText);
            dtEnd = Convert.ToDateTime(doc.SelectSingleNode("/Configuration/FASTLOAD/ENDDATE").InnerText);
            iWindowSize = Convert.ToInt32(doc.SelectSingleNode("/Configuration/FASTLOAD/WINDOWSIZE").InnerText);
        }

        public void GenerateData()
        {

            try
            {
                System.Data.SqlClient.SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.InitialCatalog = "Runtime";
                builder.DataSource = "localhost";
                builder.UserID = "aaAdmin";
                builder.Password = "pwAdmin";


                SqlConnection sql_connection;

                SqlCommand sql_command;

                string szConnectionInfo = builder.ConnectionString;

                DataTable dt = new DataTable();

                sql_connection = new SqlConnection(szConnectionInfo);

                string szQuery = "select TagName,TagTypeName from Tag, TagType where Tag.TagType = TagType.TagTypeKey and TagName like 'Source_Test%'";

                sql_command = new SqlCommand(szQuery, sql_connection);
                sql_command.CommandType = CommandType.Text;
            
                SqlDataAdapter da = new SqlDataAdapter(sql_command);
            
                da.Fill(dt);
                dt.Columns.Add("Value", System.Type.GetType("System.Double"));
                dt.Columns.Add("TimeStamp", System.Type.GetType("System.DateTime"));

                

                

                for (DateTime dtWindowStart = dtStart; dtWindowStart < dtEnd; dtWindowStart = dtWindowStart.AddSeconds(iWindowSize))
                {
                    DataTable dtTemp = dt.Copy() ;
                    DateTime dtWindowEnd = dtWindowStart.AddSeconds(iWindowSize);
                    if (dtWindowEnd > dtEnd)
                    {
                        dtWindowEnd = dtEnd;
                    }


                    Random r = new Random();
                   
                   
                    dtTemp.Columns["Value"].Expression = System.Convert.ToString(r.NextDouble() * 200000 - 100000);
                    dtTemp.Columns["TimeStamp"].Expression = dtWindowStart.ToString("#yyyy-MM-dd HH:mm:ss.fff#");


                    ResultWriter results = new ResultWriter();
                    results.strFL = szFLPath;
                    results.strTemp = szTempPath;
                    

                    foreach (DataRow row in dtTemp.Rows)
                    {
                        FastLoadData dataItem = new FastLoadData();
                        dataItem.dtDate = System.Convert.ToDateTime(row["TimeStamp"]);
                        dataItem.strTagName = System.Convert.ToString(row["TagName"]);
                        dataItem.strQuality = System.Convert.ToString(r.Next(0, 192));
                        dataItem.strValue = System.Convert.ToString(row["Value"]);
                        results.ResultList.Add(dataItem);


                        logger.Info("{0},{1},{2}", row["TagName"], row["Value"], row["TimeStamp"]);
                    }

                    results.WriteData();




                }
                
            }
            catch (Exception exLoad)
            {

                logger.Error(exLoad, "Error Loading Tags");
            }

        }
    }
}
