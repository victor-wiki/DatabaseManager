using Oracle.ManagedDataAccess.Types;

namespace DatabaseInterpreter.Geometry
{
    [OracleCustomTypeMapping("MDSYS.ST_POINT")]
    public class StPoint : StGeometry
    {

    }
}