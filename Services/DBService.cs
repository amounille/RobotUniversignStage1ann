using RobotUniversign.DAO;

namespace RobotUniversign.Services
{
    public interface IDBService
    {
        DatabaseContext GetDatabase();
        void Close();
    }

    public class DBService : IDBService
    {
        private DatabaseContext _db;

        public DBService(DatabaseContext db)
        {
            _db = db;
        }

        public DatabaseContext GetDatabase()
        {
            return _db;
        }

        public void Close()
        {
            //_db.Dispose();
        }


    }
}
