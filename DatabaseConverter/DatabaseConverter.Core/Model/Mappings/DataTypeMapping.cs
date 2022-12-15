using DatabaseInterpreter.Core;
using DatabaseInterpreter.Utility;
using System.Collections.Generic;
using System.Xml.Linq;

namespace DatabaseConverter.Model
{
    public class DataTypeMapping
    {
        public DataTypeMappingSource Source { get; set; }
        public DataTypeMappingTarget Target { get; set; }
        public List<DataTypeMappingSpecial> Specials { get; set; } = new List<DataTypeMappingSpecial>();
    }

    public class DataTypeMappingSource
    {
        public string Type { get; set; }
        public bool IsExpression { get; set; }

        public DataTypeMappingSource() { }
        public DataTypeMappingSource(XElement element)
        {
            var source = element.Element("source");
            this.Type = source.Attribute("type").Value;
            this.IsExpression = source.Attribute("isExp")?.Value == "true";
        }
    }

    public class DataTypeMappingTarget
    {
        public string Type { get; set; }
        public string Length { get; set; }
        public string Precision { get; set; }
        public string Scale { get; set; }
        public string Substitute { get; set; }
        public string Args { get; set; }
        public List<DataTypeMappingArgument> Arguments { get; set; } = new List<DataTypeMappingArgument>();

        public DataTypeMappingTarget() { }
        public DataTypeMappingTarget(XElement element)
        {
            var target = element.Element("target");
            this.Type = target.Attribute("type")?.Value;
            this.Length = target.Attribute("length")?.Value;
            this.Precision = target.Attribute("precision")?.Value;
            this.Scale = target.Attribute("scale")?.Value;
            this.Substitute = target.Attribute("substitute")?.Value;
            this.Args = target.Attribute("args")?.Value;            
        }
    }

    public struct DataTypeMappingArgument
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class DataTypeMappingSpecial
    {               
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string TargetMaxLength { get; set; }
        public string Substitute { get; set; }       
        public bool NoLength { get; set; }
        public string Precison { get; set; }
        public string Scale { get; set; }

        public DataTypeMappingSpecial() { }
        public DataTypeMappingSpecial(XElement element)
        {            
            this.Name = element.Attribute("name")?.Value;
            this.Value = element.Attribute("value")?.Value;          
            this.Type = element.Attribute("type")?.Value;
            this.TargetMaxLength = element.Attribute("targetMaxLength")?.Value;
            this.Substitute = element.Attribute("substitute")?.Value;
            this.NoLength = ValueHelper.IsTrueValue(element.Attribute("noLength")?.Value);
            this.Precison = element.Attribute("precision")?.Value;
            this.Scale = element.Attribute("scale")?.Value;
        }
    }
}
