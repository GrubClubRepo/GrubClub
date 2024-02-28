using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SupperClub.Domain;
using System.IO;
using SupperClub.Domain.Repository;
using SupperClub.Data;
using System.Data;
using System.Text;
namespace SupperClub.Code
{
    public class ReportGenerator
    {
        private SupperClubRepository _supperClubRepository;

        public ReportGenerator()
        {
            _supperClubRepository = new SupperClubRepository();
        }

        public string GenerateReport(Report report, string reportFolder, List<Tuple<string, string>> sqlParameters = null)
        {
            if (!Directory.Exists(reportFolder))
                Directory.CreateDirectory(reportFolder);

            // Clean up any old reports on the server
            ClearOldFiles(reportFolder);

            string fileName = report.Id + "_" + Guid.NewGuid().ToString() + ".csv";
            string filePath = reportFolder + "\\" + fileName;

            DataTable dt = _supperClubRepository.RunReportQuery(report.Query, sqlParameters);
            if (dt == null)
                return null;

            // Create the CSV file to which grid data will be exported.
            StreamWriter sw = new StreamWriter(filePath, false, Encoding.GetEncoding("Windows-1252"));

            // First we will write the headers.
            int iColCount = dt.Columns.Count;
            for (int i = 0; i < iColCount; i++)
            {
                sw.Write(dt.Columns[i]);
                if (i < iColCount - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);

            // Now write all the rows.

            foreach (DataRow dr in dt.Rows)
            {
                for (int i = 0; i < iColCount; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string toWrite = dr[i].ToString();
                        // quote any fields containing commas or new line chars
                        if (toWrite.Contains(",") || toWrite.Contains("\r") || toWrite.Contains("\n") || toWrite.Contains("\r\n"))
                            toWrite = "\"" + toWrite + "\"";
                        sw.Write(toWrite);
                    }
                    if (i < iColCount - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();

            return filePath;
        }

        public static void ClearOldFiles(string path)
        {
            foreach (string f in Directory.GetFiles(path))
            {
                //If the files are older than a day we can delete them
                if (File.GetCreationTime(f) < DateTime.Now.AddDays(-1))
                {
                    File.Delete(f);
                }
            }
        }
    }
}