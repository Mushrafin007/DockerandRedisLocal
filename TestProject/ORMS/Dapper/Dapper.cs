using Dapper;
using System.Data;
using System.Data.SqlClient;
namespace TestProject.ORMS.Dapper
{
    public class Dapper
    {
        private readonly string _SqlCon;
        public Dapper(string ConnectionString)
        {
            _SqlCon = ConnectionString;
        }
        public IEnumerable<T> ExecuteQuery<T>(string ProcedureName, object parameters = null) where T : class
        {
            try
            {
                using (IDbConnection db = new SqlConnection(_SqlCon))
                {
                    return db.Query<T>(ProcedureName, parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch { }
            return null;
        }
    }
}
