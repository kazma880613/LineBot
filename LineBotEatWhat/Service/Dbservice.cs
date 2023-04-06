using Newtonsoft.Json;
using LineBot.Object;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Security;
using System.Collections.Generic;

namespace LineBot.Service
{
    public class Dbservice
    {
        RBMQService _RBMQ = new RBMQService();
        DBConfig _dbconfig = new DBConfig();
        string DBConnString = "";

        public Dbservice()
        {
            MySqlConnectionStringBuilder sqlConnString = new MySqlConnectionStringBuilder();
            sqlConnString.UserID = _dbconfig.dbUsername;
            sqlConnString.Password = _dbconfig.dbPassword;
            sqlConnString.Database = _dbconfig.dbName;
            sqlConnString.Server = _dbconfig.dbIP;
            sqlConnString.Port = (uint)_dbconfig.dbPort;

            sqlConnString.Pooling = true;
            sqlConnString.MinimumPoolSize = 3;
            sqlConnString.MaximumPoolSize = 10;

            DBConnString = sqlConnString.ConnectionString;
        }
        public List<Response> query(string Latitude, string Longitude, string Function)
        {
            List<Response> _responseList = new List<Response>();
            List<Response> _tmpList = new List<Response>();
            MySqlConnection UserSqlConnection = new MySqlConnection();
            UserSqlConnection.ConnectionString = DBConnString;
            UserSqlConnection.Open();

            string sub_Latitude = string.Empty;
            string sub_Longitude = string.Empty;
            string cmdString = string.Empty;
            sub_Latitude = Latitude.Substring(0, 6);
            sub_Longitude = Longitude.Substring(0, 7);

            if(Function == "電動車維修站點")
            {
                cmdString = $"SELECT * FROM linebot.maintainancestation where Latitude LIKE '{Latitude}%' AND Longitude LIKE '{Longitude}%'";
                MySqlCommand DBCommand = new MySqlCommand(cmdString, UserSqlConnection);
                MySqlDataReader DBReader = DBCommand.ExecuteReader();
                while (DBReader.Read())
                {
                    Response _response = new Response();
                    _response.address = DBReader["Address"].ToString();
                    _response.station = DBReader["Station"].ToString();
                    _response.longitude = Convert.ToDouble(DBReader["Longitude"]);
                    _response.latitude = Convert.ToDouble(DBReader["Latitude"]);
                    _tmpList.Add(_response);
                }
                DBReader.Close();

                if (_tmpList.Count() == 0)
                {
                    sub_Latitude = Latitude.Substring(0, 5);
                    sub_Longitude = Longitude.Substring(0, 6);
                    cmdString = $"SELECT * FROM linebot.maintainancestation where Latitude LIKE '{sub_Latitude}%' AND Longitude LIKE '{sub_Longitude}%'";
                    DBCommand = new MySqlCommand(cmdString, UserSqlConnection);
                    DBReader = DBCommand.ExecuteReader();
                    while (DBReader.Read())
                    {
                        Response _response = new Response();
                        _response.address = DBReader["Address"].ToString();
                        _response.station = DBReader["Station"].ToString();
                        _response.longitude = Convert.ToDouble(DBReader["Longitude"]);
                        _response.latitude = Convert.ToDouble(DBReader["Latitude"]);
                        _tmpList.Add(_response);
                    }
                    DBReader.Close();
                }
            }

            if (Function == "電動車充電站點")
            {
                cmdString = $"SELECT * FROM linebot.chargingpoint where Latitude LIKE '{Latitude}%' AND Longitude LIKE '{Longitude}%'";
                MySqlCommand DBCommand = new MySqlCommand(cmdString, UserSqlConnection);
                MySqlDataReader DBReader = DBCommand.ExecuteReader();
                while (DBReader.Read())
                {
                    Response _response = new Response();
                    _response.address = DBReader["Address"].ToString();
                    _response.station = DBReader["Station"].ToString();
                    _response.longitude = Convert.ToDouble(DBReader["Longitude"]);
                    _response.latitude = Convert.ToDouble(DBReader["Latitude"]);
                    _tmpList.Add(_response);
                }
                DBReader.Close();

                if (_tmpList.Count() == 0)
                {
                    sub_Latitude = Latitude.Substring(0, 5);
                    sub_Longitude = Longitude.Substring(0, 6);
                    cmdString = $"SELECT * FROM linebot.maintainancestation where Latitude LIKE '{sub_Latitude}%' AND Longitude LIKE '{sub_Longitude}%'";
                    DBCommand = new MySqlCommand(cmdString, UserSqlConnection);
                    DBReader = DBCommand.ExecuteReader();
                    while (DBReader.Read())
                    {
                        Response _response = new Response();
                        _response.address = DBReader["Address"].ToString();
                        _response.station = DBReader["Station"].ToString();
                        _response.longitude = Convert.ToDouble(DBReader["Longitude"]);
                        _response.latitude = Convert.ToDouble(DBReader["Latitude"]);
                        _tmpList.Add(_response);
                    }
                    DBReader.Close();
                }
            }

            if (Function == "UBike站點")
            {
                cmdString = $"SELECT * FROM linebot.ubike_location where Latitude LIKE '{Latitude}%' AND Longitude LIKE '{Longitude}%'";
                MySqlCommand DBCommand = new MySqlCommand(cmdString, UserSqlConnection);
                MySqlDataReader DBReader = DBCommand.ExecuteReader();
                while (DBReader.Read())
                {
                    Response _response = new Response();
                    _response.address = DBReader["Address"].ToString();
                    _response.station = DBReader["Station"].ToString();
                    _response.longitude = Convert.ToDouble(DBReader["Longitude"]);
                    _response.latitude = Convert.ToDouble(DBReader["Latitude"]);
                    _tmpList.Add(_response);
                }
                DBReader.Close();

                if (_tmpList.Count() == 0)
                {
                    sub_Latitude = Latitude.Substring(0, 5);
                    sub_Longitude = Longitude.Substring(0, 6);
                    cmdString = $"SELECT * FROM linebot.ubike_location where Latitude LIKE '{sub_Latitude}%' AND Longitude LIKE '{sub_Longitude}%'";
                    DBCommand = new MySqlCommand(cmdString, UserSqlConnection);
                    DBReader = DBCommand.ExecuteReader();
                    while (DBReader.Read())
                    {
                        Response _response = new Response();
                        _response.address = DBReader["Address"].ToString();
                        _response.station = DBReader["Station"].ToString();
                        _response.longitude = Convert.ToDouble(DBReader["Longitude"]);
                        _response.latitude = Convert.ToDouble(DBReader["Latitude"]);
                        _tmpList.Add(_response);
                    }
                    DBReader.Close();
                }
            }
            UserSqlConnection.Close();

            _responseList = MinDistance(_tmpList, Latitude, Longitude);

            return _responseList;
        }

        private List<Response> MinDistance(List<Response> before, string Latitude, string Longitude)
        {
            List<Response> after = new List<Response>();
            for (int i = 0; i < before.Count(); i++)
            {
                before[i].distance = Math.Pow(Math.Pow(before[i].latitude - Convert.ToDouble(Latitude), 2) + Math.Pow(before[i].longitude - Convert.ToDouble(Longitude), 2), 0.5);
            }

            before = before.OrderBy(x => x.distance).ToList();

            if (before.Count > 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    after.Add(before[i]);
                }
            }
            else
            {
                after = before;
            }
            return after;
        }
    }
}
