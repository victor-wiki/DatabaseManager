using CsvHelper.Configuration;
using System.Collections.Generic;
using System.IO;

namespace DatabaseManager.FileUtility
{
    public class CsvReader : BaseReader
    {

        public CsvReader(SourceFileInfo info) : base(info) { }


        public override DataReadResult Read(bool onlyReadHeader = false)
        {
            DataReadResult result = new DataReadResult();           

            string filePath = this.info.FilePath;
            bool firstRowIsColumnName = this.info.FirstRowIsColumnName;

            using (StreamReader textReader = new StreamReader(filePath))
            {
                CsvConfiguration configuration = new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture);

                configuration.HasHeaderRecord = firstRowIsColumnName;
                configuration.Delimiter = ",";

                CsvHelper.CsvReader reader = new CsvHelper.CsvReader(textReader, configuration);

                if (firstRowIsColumnName)
                {
                    reader.Read();
                    reader.ReadHeader();
                }

                result.HeaderColumns = reader.HeaderRecord;

                if(!onlyReadHeader)
                {
                    int index = 0;

                    Dictionary<int, Dictionary<int, object>> dict = new Dictionary<int, Dictionary<int, object>>();

                    while (reader.Read())
                    {
                        Dictionary<int, object> dictRow = new Dictionary<int, object>();

                        for (int i = 0; i < reader.ColumnCount; i++)
                        {
                            string value = reader.GetField(i);

                            dictRow.Add(i, value);
                        }

                        dict.Add(index, dictRow);

                        index++;
                    }

                    result.Data = dict;
                }               
            }           

            return result;
        }
    }
}
