using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;

namespace DatabaseInterpreter.Geometry
{
    public abstract class OracleCustomTypeBase<T> : INullable, IOracleCustomType, IOracleCustomTypeFactory
    where T : OracleCustomTypeBase<T>, new()
    {
        private static string errorMessageHead = "Error converting Oracle User Defined Type to .Net Type " + typeof(T).ToString() + ", oracle column is null, failed to map to . NET valuetype, column ";
        private OracleConnection connection;
        private object udt;
        private bool isNull;

        public virtual bool IsNull
        {
            get
            {
                return this.isNull;
            }
        }

        public static T Null
        {
            get
            {
                T t = new T();
                t.isNull = true;
                return t;
            }
        }

        public IOracleCustomType CreateObject()
        {
            return new T();
        }

        protected void SetConnectionAndPointer(OracleConnection connection, object udt)
        {
            this.connection = connection;
            this.udt = udt;
        }

        public abstract void MapFromCustomObject();
        public abstract void MapToCustomObject();

        public void FromCustomObject(OracleConnection connection, object udt)
        {
            this.SetConnectionAndPointer(connection, udt);
            this.MapFromCustomObject();
        }
        public void ToCustomObject(OracleConnection connection, object udt)
        {
            this.SetConnectionAndPointer(connection, udt);
            this.MapToCustomObject();
        }

        protected void SetValue(string columnName, object value)
        {
            if (value != null)
            {
                OracleUdt.SetValue(connection, udt, columnName, value);
            }
        }
        protected void SetValue(int columnId, object value)
        {
            if (value != null)
            {
                OracleUdt.SetValue(connection, udt, columnId, value);
            }
        }

        protected U GetValue<U>(string columnName)
        {

            if (OracleUdt.IsDBNull(connection, udt, columnName))
            {
                if (default(U) is ValueType)
                {
                    throw new Exception(errorMessageHead + columnName.ToString() + " of value type " + typeof(U).ToString());
                }
                else
                {
                    return default(U);
                }
            }
            else
            {
                return (U)OracleUdt.GetValue(connection, udt, columnName);
            }
        }

        protected U GetValue<U>(int columnId)
        {
            if (OracleUdt.IsDBNull(connection, udt, columnId))
            {
                if (default(U) is ValueType)
                {
                    throw new Exception(errorMessageHead + columnId.ToString() + " of value type " + typeof(U).ToString());
                }
                else
                {
                    return default(U);
                }
            }
            else
            {
                return (U)OracleUdt.GetValue(connection, udt, columnId);
            }
        }
    }
}
