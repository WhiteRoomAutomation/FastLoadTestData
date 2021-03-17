using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastLoadTestData
{
    class ResultWriter
    {
        public List<FastLoadData> ResultList = new List<FastLoadData>();
        public static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public string strTemp = "";
        public string strFL = "";


        public bool WriteData()
        {
            bool bReturn = false;

            ResultList.Sort();
            try
            {
                DateTime dtCurr = DateTime.Now;
                string strflOutput = strTemp + "\\" + dtCurr.ToString("yyyyMMddHHmmssFFFFFFF") + ".csv";
                //MessageBox.Show(strflOutput);
                System.IO.StreamWriter swFL = new System.IO.StreamWriter(strflOutput);
                swFL.WriteLine("ASCII");
                swFL.WriteLine(",");
                swFL.WriteLine("Administrator,1,Server Local,10,0");

                foreach (FastLoadData OutputRow in ResultList)
                {
                    swFL.WriteLine(OutputRow.GetExportString());
                }
                System.Threading.Thread.Sleep(1);

                swFL.Flush();
                swFL.Close();

                ResultList.Clear();
                ResultList.TrimExcess();

                string fileName = System.IO.Path.GetFileName(strflOutput);

                string destFile = System.IO.Path.Combine(strFL, fileName);

                try
                {

                    System.IO.File.Move(strflOutput, destFile);
                }
                catch (Exception exCopy)
                {
                    logger.Error("Error while copying file {0}: {1} ",fileName, exCopy.Message);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error while writing: {0}",ex.Message);
            }
            return bReturn;
        }
    }
}
