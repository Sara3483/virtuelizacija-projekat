using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaterijaContract;
using System.IO;
using System.Globalization;

namespace VirtuelizacijaProjekat
{
    public class EisFileReader
    {
        private string rootPath;
        private string logPath;

        public EisFileReader(string rootPath)
        {
            this.rootPath = rootPath;
            logPath = "invalid_rows.log";
        }

        public List<FileInfo> GetCsv()
        {
            DirectoryInfo rootDirectory = new DirectoryInfo(rootPath);
            if(!rootDirectory.Exists)
            {
                throw new DirectoryNotFoundException("Root directory does not exist: " + rootDirectory);
            }

            FileInfo[] allFiles = rootDirectory.GetFiles("*.csv", SearchOption.AllDirectories);
            List<FileInfo> eisFiles = new List<FileInfo>();
            foreach(FileInfo file in allFiles)
            {
                if(file.FullName.Contains("EIS measurements") &&
                    file.FullName.Contains("Hioki") &&
                    (file.FullName.Contains("Test_1") || file.FullName.Contains("Test_2")))
                {
                    eisFiles.Add(file);
                }
            }
            return eisFiles;
        }

        public List<EisSample> ReadSamples(FileInfo file)
        {
            List<EisSample> samples = new List<EisSample>();

            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line;
                int row = 0;
                bool header = true;

                while((line = reader.ReadLine()) != null)
                {
                    if(header)
                    {
                        header = false;
                        continue;
                    }

                    if(string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if(samples.Count >= 39)
                    {
                        LogInvalidRow(file.FullName, line, "More than 39 rows found inside file.");
                        Console.WriteLine();
                        continue;
                    }

                    string[] parts = line.Split(',');
                    if(parts.Length < 6)
                    {
                        LogInvalidRow(file.FullName, line, "Not enough data in the row.");
                        Console.WriteLine();
                        continue;
                    }

                    try
                    {
                        EisSample sample = new EisSample
                        {
                            RowIndex = row,
                            FrequencyHz = double.Parse(parts[0], CultureInfo.InvariantCulture),
                            R_ohm = double.Parse(parts[1], CultureInfo.InvariantCulture),
                            X_ohm = double.Parse(parts[2], CultureInfo.InvariantCulture),
                            T_degC = double.Parse(parts[4], CultureInfo.InvariantCulture),
                            Range_ohm = double.Parse(parts[5], CultureInfo.InvariantCulture),
                            TimestampLocal = DateTime.Now
                        };

                        samples.Add(sample);
                        row++;
                    }
                    catch (Exception e)
                    {
                        LogInvalidRow(file.FullName, line, e.Message);
                        Console.WriteLine();
                    }
                }
            }
            Console.WriteLine($"{file.Name} processed. Valid rows: {samples.Count}");
            Console.WriteLine();
            return samples;
        }

        public EisMeta CreateMeta(FileInfo file, int totalRows)
        {
            string noExtension = Path.GetFileNameWithoutExtension(file.Name);
            string[] parts = noExtension.Split('_');
            if(parts.Length < 4)
            {
                LogInvalidRow(file.FullName, file.Name, "Format of the file name is invalid.");
                return new EisMeta
                {
                    BatteryId = "N/A",
                    TestId = "N/A",
                    SoCPercentage = -1,
                    FileName = file.Name,
                    TotalRows = totalRows
                };
            }

            string batteryId = parts[0] + "_" + parts[1];
            int soc = int.Parse(parts[3], CultureInfo.InvariantCulture);
            string testId = ExtractTestId(file.FullName);

            return new EisMeta
            {
                BatteryId = batteryId,
                TestId = testId,
                SoCPercentage = soc,
                FileName = file.Name,
                TotalRows = totalRows
            };
        }

        private string ExtractTestId(string path)
        {
            string[] parts = path.Split(Path.DirectorySeparatorChar);
            foreach(string part in parts)
            {
                if(part.StartsWith("Test_"))
                {
                    return part;
                }
            }

            return "N/A";
        }

        private void LogInvalidRow(string filePath, string row, string reason)
        {
            File.AppendAllText(logPath,
                $"{DateTime.Now}: File={filePath}, Row={row}, Reason={reason}" +
                $"{Environment.NewLine}");
        }
    }
}
