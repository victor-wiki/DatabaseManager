using Oracle.ManagedDataAccess.Types;
using System;
using System.Text;

namespace DatabaseInterpreter.Geometry
{
    [OracleCustomTypeMapping("MDSYS.ST_GEOMETRY")]
    public class StGeometry : OracleCustomTypeBase<StGeometry>
    {        
        [OracleObjectMappingAttribute("GEOM")]
        public SdoGeometry Geom { get; set; }

        public override void MapFromCustomObject()
        {
            this.SetValue(0, this.Geom);
        }

        public override void MapToCustomObject()
        {
            this.Geom = this.GetValue<SdoGeometry>(0);
        }

        public override string ToString()
        {
            return this.GetText();
        }

        private string GetText()
        {
            if (this == null)
            {
                throw new NullReferenceException("Cannot write Well-Known Text: geometry was null");
            }

            StringBuilder sb = new StringBuilder();           

            if (this.Geom != null)
            {
                return this.Geom.ToString();
            }

            return sb.ToString();
        }
    }
}
