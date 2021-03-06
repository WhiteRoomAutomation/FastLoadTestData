using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Data;

using System.Data.SqlClient;
using Tedd.RandomUtils;

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



                List<ResultWriter> Writers = new List<ResultWriter>();
                Writers.Add(new ResultWriter());
                Writers.Last().strFL = szFLPath;
                Writers.Last().strTemp = szTempPath;


                Tedd.RandomUtils.FastRandom r = new Tedd.RandomUtils.FastRandom();


                for (DateTime dtWindowStart = dtStart; dtWindowStart <= dtEnd; dtWindowStart = dtWindowStart.AddSeconds(iWindowSize))
                {
                    DataTable dtTemp = dt.Copy() ;
                    DateTime dtWindowEnd = dtWindowStart.AddSeconds(iWindowSize);
                    if (dtWindowEnd > dtEnd)
                    {
                        dtWindowEnd = dtEnd;
                    }


                    
                    
                   
                    dtTemp.Columns["Value"].Expression = System.Convert.ToString(r.NextDouble() * 200000 - 100000);
                    dtTemp.Columns["TimeStamp"].Expression = dtWindowStart.ToString("#yyyy-MM-dd HH:mm:ss.fff#");


                    
                    
                    

                    foreach (DataRow row in dtTemp.Rows)
                    {
                        FastLoadData dataItem = new FastLoadData();
                        dataItem.dtDate = System.Convert.ToDateTime(row["TimeStamp"]);
                        dataItem.strTagName = System.Convert.ToString(row["TagName"]);
                        dataItem.strQuality = "192";
                        dataItem.strValue = System.Convert.ToString(row["Value"]);
                        Writers.Last().ResultList.Add(dataItem);


                        //logger.Info("{0},{1},{2}", row["TagName"], row["Value"], row["TimeStamp"]);
                    }
                    if (Writers.Last().ResultList.Count >= 10000000)
                    {
                        logger.Info("Writing to file {0}", dtWindowStart);
                        Writers.Last().WriteData();
                        Writers.Add(new ResultWriter());
                        Writers.Last().strFL = szFLPath;
                        Writers.Last().strTemp = szTempPath;
                        logger.Info("New Writer Ready");

                    }

                }

                if (Writers.Last().ResultList.Count > 0)
                {
                    Writers.Last().WriteData();
                }
               

            }
            catch (Exception exLoad)
            {

                logger.Error(exLoad, "Error Loading Tags");
            }

        }
    }
}
