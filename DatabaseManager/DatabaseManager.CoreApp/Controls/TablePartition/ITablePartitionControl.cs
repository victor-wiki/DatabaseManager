using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using System.Threading.Tasks;

namespace DatabaseManager.Controls
{
    public interface ITablePartitionControl
    {
        Task LoadData(DbInterpreter dbInterpreter, Table table);
        Task Reload();
    }
}
