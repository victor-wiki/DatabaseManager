using System.Collections.Generic;
using System.Xml.Linq;

namespace DatabaseConverter.Model
{
    public class DataTypeMapping
    {
        public DataTypeMappingSource Source { get; set; }
        public DataTypeMappingTarget Tareget { get; set; }
        public List<DataTypeMappingSpecial> Specials { get; set; } = new List<DataTypeMappingSpecial>();
    }

    public class DataTypeMappingSource
    {
        public string Type { get; set; }

        public DataTypeMappingSource() { }
        public DataTypeMappingSource(XElement element)
        {
            this.Type = element.Element("source").Attribute("type").Value;
        }
    }

    public class DataTypeMappingTarget
    {
        public string Type { get; set; }
        public string Length { get; set; }
        public string Precision { get; set; }
        public string Scale { get; set; }

        public DataTypeMappingTarget() { }
        public DataTypeMappingTarget(XElement element)
        {
            this.Type = element.Element("target").Attribute("type")?.Value;
            this.Length = element.Element("target").Attribute("length")?.Value;
            this.Precision = element.Element("target").Attribute("precision")?.Value;
            this.Scale = element.Element("target").Attribute("scale")?.Value;
        }
    }

    public class DataTypeMappingSpecial
    {
        public string SourceMaxLength { get; set; }
        public string SourcePrecision { get; set; }
        public string SourceScale { get; set; }
        public string Type { get; set; }
        public string TargetMaxLength { get; set; }

        public DataTypeMappingSpecial() { }
        public DataTypeMappingSpecial(XElement element)
        {
            this.SourceMaxLength = element.Attribute("sourceMaxLength")?.Value;
            this.SourcePrecision = element.Attribute("sourcePrecision")?.Value;
            this.SourceScale = element.Attribute("sourceScale")?.Value;
            this.Type = element.Attribute("type")?.Value;
            this.TargetMaxLength = element.Attribute("targetMaxLength")?.Value;
        }
    }
}
