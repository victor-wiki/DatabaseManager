using CsvHelper.Configuration;
using System.Collections.Generic;
using System.IO;

namespace DatabaseManager.FileUtility
{
    public class CsvReader : BaseReader
    {

        public CsvReader(ImportDataInfo info) : base(info) { }


        public override DataReadResult Read()
        {
            DataReadResult result = new DataReadResult();

            Dictionary<int, Dictionary<int, object>> dict = new Dictionary<int, Dictionary<int, object>>();

            string filePath = this.info.FilePath;
            bool firstRowIsColumnName = this.info.FirstRowIsColumnName;

            using (StreamReader textReader = new StreamReader(filePath))
            {
                CsvConfiguration configuration = new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture);

                configuration.HasHeaderRecord = firstRowIsColumnName;
                configuration.Delimiter = ",";

                CsvHelper.CsvReader reader = new CsvHelper.CsvReader(textReader, configuration);

                reader.Read();

                if (firstRowIsColumnName)
                {
                    reader.ReadHeader();
                }

                int columnCount = reader.ColumnCount;

                int index = 0;

                while (reader.Read())
                {
                    Dictionary<int, object> dictRow = new Dictionary<int, object>();

                    for (int i = 0; i < columnCount; i++)
                    {
                        string value = reader.GetField(i);

                        dictRow.Add(i, value);
                    }

                    dict.Add(index, dictRow);

                    index++;
                }

                result.HeaderColumns = reader.HeaderRecord;
            }

            result.Data = dict;

            return result;
        }
    }
}
