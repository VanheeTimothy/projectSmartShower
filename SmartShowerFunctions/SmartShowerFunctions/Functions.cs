using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using SmartShowerFunctions.Model;

namespace SmartShowerFunctions // https://smartshowerfunctions.azurewebsites.net
{
    public static class Functions
    {
        private static string COSMOSHOST = Environment.GetEnvironmentVariable("COSMOSHOST");
        private static string COSMOSKEY = Environment.GetEnvironmentVariable("COSMOSKEY");
        private static string COSMOSDATABASE = Environment.GetEnvironmentVariable("COSMOSDATABASE");
        private static string COSMOSCOLLECTIONID = Environment.GetEnvironmentVariable("COSMOSCOLLECTIONID");
        private static string CONNECTIONSTRING = Environment.GetEnvironmentVariable("Connectionstring");


        [FunctionName("RegisterUser")]
        public static async Task<HttpResponseMessage> RegisterUser([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SmartShower/User/Reg")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                var content = await req.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(content);
                Guid clientId = Guid.NewGuid(); // >> id wordt meegeven in de APP zelf


                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        string EmailCheckSql = "select Email FROM Users Where Email = @mail";
                        command.Parameters.AddWithValue("@mail", user.Email);
                        command.CommandText = EmailCheckSql;
                        DataSet ds = new DataSet();
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(ds);
                        //connection.Close();
                        bool userAlreadyExists = ((ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0));
                        if (userAlreadyExists)
                        {
                            return req.CreateResponse(HttpStatusCode.Forbidden, true);
                        }
                        else
                        {
                            log.Info("Waarde emailcheck= ", EmailCheckSql);

                            string sql = "INSERT INTO Users VALUES(@Guid, @Name, @Password, @Email, @Color, @MaxShowerTime, @IdealTemperature, @Monitor, @Photo)";
                            command.CommandText = sql;

                            command.Parameters.AddWithValue("@Guid", clientId);
                            command.Parameters.AddWithValue("@Name", user.Name);
                            command.Parameters.AddWithValue("@Password", user.Password);
                            command.Parameters.AddWithValue("@Email", user.Email);
                            command.Parameters.AddWithValue("@Color", user.Color);
                            command.Parameters.AddWithValue("@MaxShowerTime", user.MaxShowerTime); // input is # seconden >> 1:30 = 90seconden
                            command.Parameters.AddWithValue("@IdealTemperature", user.IdealTemperature);
                            command.Parameters.AddWithValue("@Monitor", user.Monitor);
                            command.Parameters.AddWithValue("@Photo", user.Photo);
                            command.ExecuteNonQuery();
                        }


                    }
                    return req.CreateResponse(HttpStatusCode.OK, true);
                }

            }
            catch (Exception ex)
            {
#if RELEASE
                return req.CreateResponse(HttpStatusCode.InternalServerError);
#endif
#if DEBUG
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }
        }


        [FunctionName("RegisterShower")]
        public static async Task<HttpResponseMessage> RegisterShower([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SmartShower/Shower/Reg")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {

                var content = await req.Content.ReadAsStringAsync();
                var shower = JsonConvert.DeserializeObject<Shower>(content);
                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        string showerCheck = "SELECT * FROM Shower WHERE IdShower = @ShowerId;";
                        command.Parameters.AddWithValue("@ShowerId", shower.IdShower);
                        command.CommandText = showerCheck;
                        DataSet ds = new DataSet();
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(ds);
                        //connection.Close();
                        bool showerAlreadyReg = ((ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0));
                        if (showerAlreadyReg)
                        {
                            return req.CreateResponse(HttpStatusCode.Forbidden);
                        }
                        else
                        {

                            string sql = "INSERT INTO Shower VALUES(@IdShower, @WaterCost)";
                            command.CommandText = sql;
                            command.Parameters.AddWithValue("@IdShower", shower.IdShower);
                            command.Parameters.AddWithValue("@WaterCost", shower.WaterCost);
                            command.ExecuteNonQuery();
                        }

                    }
                }
                return req.CreateResponse(HttpStatusCode.OK);


            }
            catch (Exception ex)
            {
#if RELEASE
                return req.CreateResponse(HttpStatusCode.InternalServerError);
#endif
#if DEBUG
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }
        }

        [FunctionName("RegisterUserShower")]
        public static async Task<HttpResponseMessage> RegisterUserShower([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SmartShower/UserShower/Reg")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                var content = await req.Content.ReadAsStringAsync();
                var UserShower = JsonConvert.DeserializeObject<UserShower>(content);
                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        string sql = "INSERT INTO UserShower VALUES(@IdShower,@IdUser)";
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("@IdShower", UserShower.IdShower);
                        command.Parameters.AddWithValue("@IdUser", UserShower.IdUser);
                        command.ExecuteNonQuery();
                    }
                }
                return req.CreateResponse(HttpStatusCode.OK, UserShower);

            }
            catch (Exception ex)
            {
#if RELEASE
                return req.CreateResponse(HttpStatusCode.InternalServerError);
#endif
#if DEBUG
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }

        }

        [FunctionName("LoginUser")] // https://smartshowerfunctions.azurewebsites.net/api/SmartShower/User/login
        public static async Task<HttpResponseMessage> LoginUser([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SmartShower/User/login")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {

                var content = await req.Content.ReadAsStringAsync();
                var User = JsonConvert.DeserializeObject<User>(content);
                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;

                        string sql = "SELECT * FROM Users where Email like @Email and Password = @password;";
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("@Email", User.Email);
                        command.Parameters.AddWithValue("@password", User.Password);
                        command.ExecuteNonQuery();


                        DataSet ds = new DataSet();
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(ds);
                        //connection.Close();


                        bool loginSuccesFull = ((ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0));
                        if (loginSuccesFull)
                        {
                            return req.CreateResponse(HttpStatusCode.OK, ds.Tables[0].Rows[0]["IdUser"]);
                        }
                        else
                        {
                            return req.CreateResponse(HttpStatusCode.Unauthorized, 0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
#if RELEASE
                return req.CreateResponse(HttpStatusCode.InternalServerError);
#endif
#if DEBUG
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }



        }
        [FunctionName("GetUserInfo")]
        public static async Task<HttpResponseMessage> GetUserInfo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SmartShower/User/Info/")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                User userInfo = new User();
                var content = await req.Content.ReadAsStringAsync();
                var User = JsonConvert.DeserializeObject<User>(content);
                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        string sql = "SELECT Name, Email, Color, MaxShowerTime, IdealTemperature, Monitor, Photo FROM Users WHERE IdUser Like @IdUser;";
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("@IdUser", User.IdUser);
                        //command.ExecuteNonQuery();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {

                            //IdUser = Guid.TryParse(reader["IdUser"], ),
                            userInfo.Name = reader["Name"].ToString();
                            userInfo.Email = reader["Email"].ToString();
                            userInfo.Color = reader["Color"].ToString();
                            userInfo.MaxShowerTime = Convert.ToInt32(reader["MaxShowerTime"]);
                            userInfo.IdealTemperature = Convert.ToUInt32(reader["IdealTemperature"]);
                            userInfo.Monitor = Convert.ToBoolean(reader["Monitor"]);
                            userInfo.Photo = reader["Photo"].ToString();

                        }
                    }
                    return req.CreateResponse(HttpStatusCode.OK, userInfo);
                }
            }
            catch (Exception ex)
            {
#if RELEASE
                return req.CreateResponse(HttpStatusCode.InternalServerError);
#endif
#if DEBUG
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }




        }

        [FunctionName("UpdateUser")]
        public static async Task<HttpResponseMessage> UpdateUser([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "SmartShower/User/Update/")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                var content = await req.Content.ReadAsStringAsync();
                var User = JsonConvert.DeserializeObject<User>(content);
                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;

                        string sql = "UPDATE Users SET Name = @Name, Password = @Password, Email = @Email, Color = @Color, MaxShowerTime = @MaxShowerTime, IdealTemperature = @IdealTemperature, Monitor = @Monitor, Photo = @Photo WHERE IdUser = @IdUser;";
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("@Name", User.Name);
                        command.Parameters.AddWithValue("@Password", User.Password);
                        command.Parameters.AddWithValue("@Email", User.Email);
                        command.Parameters.AddWithValue("@Color", User.Color);
                        command.Parameters.AddWithValue("@MaxShowerTime", User.MaxShowerTime);
                        command.Parameters.AddWithValue("@IdealTemperature", User.IdealTemperature);
                        command.Parameters.AddWithValue("@Monitor", User.Monitor);
                        command.Parameters.AddWithValue("@Photo", User.Photo);
                        command.Parameters.AddWithValue("@IdUser", User.IdUser);
                        command.ExecuteNonQuery();


                    }

                }
                return req.CreateResponse(HttpStatusCode.OK, true);

            }
            catch (Exception ex)
            {
#if RELEASE
                return req.CreateResponse(HttpStatusCode.InternalServerError);
#endif
#if DEBUG
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }

        }

        [FunctionName("UpdateShower")]
        public static async Task<HttpResponseMessage> UpdateShower([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "SmartShower/Shower/Update/")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                var content = await req.Content.ReadAsStringAsync();
                var shower = JsonConvert.DeserializeObject<Shower>(content);
                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        string sql = "UPDATE Shower SET WaterCost = @Watercost WHERE IdShower = @showerId;";
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("@WaterCost", Math.Round(shower.WaterCost, 2));
                        command.Parameters.AddWithValue("@showerId", shower.IdShower);
                        command.ExecuteNonQuery();
                    }
                }
                return req.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception ex)
            {
#if RELEASE
                return req.CreateResponse(HttpStatusCode.InternalServerError);
#endif
#if DEBUG
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }


        }

        [FunctionName("MakeGroup")]
        public static async Task<HttpResponseMessage> MakeGroup([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SmartShower/Group/new/")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                //Guid idGroup = Guid.NewGuid(); // >> id wordt meegeven in de APP zelf
                var content = await req.Content.ReadAsStringAsync();
                var group = JsonConvert.DeserializeObject<UserGroup>(content);
                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        string sql = "INSERT INTO UserGroup VALUES(@IdGroup, @IdUser, 0);";
                        command.Parameters.AddWithValue("@IdGroup", group.IdGroup);
                        command.Parameters.AddWithValue("@IdUser", group.IdUser);
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }

                }
                return req.CreateResponse(HttpStatusCode.OK, true);

            }
            catch (Exception ex)
            {
#if RELEASE
                return req.CreateResponse(HttpStatusCode.InternalServerError);
#endif
#if DEBUG
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }
        }

        [FunctionName("DeleteGroup")]
        public static async Task<HttpResponseMessage> DeleteGroup([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "SmartShower/Group/delete/")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                var content = await req.Content.ReadAsStringAsync();
                var group = JsonConvert.DeserializeObject<UserGroup>(content);
                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        string sql = "DELETE FROM UserGroup WHERE IdGroup = @IdGroup;";
                        command.Parameters.AddWithValue("@IdGroup", group.IdGroup);
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                }
                return req.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception ex)
            {
#if RELEASE
                return req.CreateResponse(HttpStatusCode.InternalServerError);
#endif
#if DEBUG
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }
        }
        [FunctionName("DeleteUserFromGroup")]
        public static async Task<HttpResponseMessage> DeleteUserFromGroup([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "SmartShower/Group/user/delete")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                var content = await req.Content.ReadAsStringAsync();
                var group = JsonConvert.DeserializeObject<UserGroup>(content);
                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        string sql = "DELETE FROM UserGroup WHERE IdGroup = @IdGroup AND IdUser = @IdUser ;";
                        command.Parameters.AddWithValue("@IdGroup", group.IdGroup);
                        command.Parameters.AddWithValue("@IdUser", group.IdUser);
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                }
                return req.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception ex)
            {
#if RELEASE
                return req.CreateResponse(HttpStatusCode.InternalServerError);
#endif
#if DEBUG
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }

        }
        [FunctionName("SendGroupInvite")]
        public static async Task<HttpResponseMessage> SendGroupInvite([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SmartShower/Group/invite")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                var content = await req.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(content);
                var group = JsonConvert.DeserializeObject<UserGroup>(content);
                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        string dataUser = "SELECT IdUser, Name, Color, Photo FROM Users WHERE Email = @Email;";
                        command.Parameters.AddWithValue("@Email", user.Email);
                        command.CommandText = dataUser;

                        DataSet ds = new DataSet();
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(ds);
                        bool userDataFound = ((ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0));
                        if (userDataFound)
                        {
                            User p = new User()
                            {
                                IdUser = new Guid(ds.Tables[0].Rows[0]["IdUser"].ToString()),
                                Name = ds.Tables[0].Rows[0]["Name"].ToString(),
                                Color = ds.Tables[0].Rows[0]["Color"].ToString(),
                                Photo = ds.Tables[0].Rows[0]["Photo"].ToString(),

                            };
                            string sql = "INSERT INTO UserGroup VALUES(@IdGroup, @IdUser, 1);"; // invite verzonden, pending op true
                            command.Parameters.AddWithValue("@IdGroup", group.IdGroup);
                            command.Parameters.AddWithValue("@IdUser", p.IdUser);
                            command.CommandText = sql;
                            command.ExecuteNonQuery();
                            return req.CreateResponse(HttpStatusCode.OK, p);

                        }
                        else
                        {
                            return req.CreateResponse(HttpStatusCode.NotFound, true);

                        }

                    }
                }

            }
            catch (Exception ex)
            {
#if RELEASE
                return req.CreateResponse(HttpStatusCode.InternalServerError);
#endif
#if DEBUG
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }

        }

        [FunctionName("AcceptInvite")]
        public static async Task<HttpResponseMessage> AcceptInvite([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "SmartShower/Group/invite/accept")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                var content = await req.Content.ReadAsStringAsync();
                var group = JsonConvert.DeserializeObject<UserGroup>(content);
                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        string sql = "UPDATE UserGroup SET Pending = 0 WHERE IdGroup = @IdGroup AND IdUser = @IdUser;";
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("@IdGroup", group.IdGroup);
                        command.Parameters.AddWithValue("@IdUser", group.IdUser);
                        command.ExecuteNonQuery();
                    }
                    
                }
                return req.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception ex)
            {
#if RELEASE
                return req.CreateResponse(HttpStatusCode.InternalServerError);
#endif
#if DEBUG
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }
        }

        [FunctionName("GetSessions")]
        public static async Task<HttpResponseMessage> GetSessions([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SmartShower/GetSessions")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                List<Session> sessions = new List<Session>();
                var content = await req.Content.ReadAsStringAsync();
                var session = JsonConvert.DeserializeObject<Session>(content);

                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        string sql = "SELECT IdSession, IdUser, WaterUsed, MoneySaved, EcoScore, AverageTemp, Duration, Timestamp FROM Session WHERE IdUser = @IdUser AND Timestamp >= DATEADD(DAY, 0, DATEDIFF(DAY, 0, CURRENT_TIMESTAMP)) AND Timestamp <  DATEADD(DAY, 1, DATEDIFF(DAY, 0, CURRENT_TIMESTAMP));";

                        switch (session.DataLenght)
                        {
                            case 0:
                                sql = "SELECT IdSession, IdUser, WaterUsed, MoneySaved, EcoScore, AverageTemp, Duration, Timestamp FROM Session WHERE IdUser = @IdUser AND Timestamp >= DATEADD(DAY, 0, DATEDIFF(DAY, 0, CURRENT_TIMESTAMP)) AND Timestamp <  DATEADD(DAY, 1, DATEDIFF(DAY, 0, CURRENT_TIMESTAMP));";
                                break;
                            case 1:
                                sql = "SELECT IdSession, IdUser, WaterUsed, MoneySaved, EcoScore, AverageTemp, Duration, Timestamp FROM Session WHERE IdUser = @IdUser AND Timestamp  >= DATEADD(day,-7, GETDATE())";
                                break;
                            case 2:
                                sql = "SELECT IdSession, IdUser, WaterUsed, MoneySaved, EcoScore, AverageTemp, Duration, Timestamp FROM Session WHERE IdUser = @IdUser AND timestamp >= DATEADD(day,-30, getdate())  and timestamp <= getdate()";
                                break;
                        }
                        command.Parameters.AddWithValue("@IdUser", session.IdUser);
                        command.CommandText = sql;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            sessions.Add(new Session
                            {
                                IdSession = new Guid(reader["IdSession"].ToString()),
                                IdUser = new Guid(reader["IdUser"].ToString()),
                                WaterUsed = float.Parse(reader["WaterUsed"].ToString()),
                                MoneySaved = float.Parse(reader["MoneySaved"].ToString()),
                                EcoScore = float.Parse(reader["EcoScore"].ToString()),
                                AverageTemp = float.Parse(reader["AverageTemp"].ToString()),
                                Duration = Convert.ToInt32(reader["Duration"].ToString()),
                                Timestamp = Convert.ToDateTime(reader["Timestamp"])

                            });
                        }
                    }
                    return req.CreateResponse(HttpStatusCode.OK, sessions);
                }
            }
            catch (Exception ex)
            {
#if RELEASE
                return req.CreateResponse(HttpStatusCode.InternalServerError);
#endif
#if DEBUG
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }
        }

        //[FunctionName("GetSessions")]
        //public static async Task<HttpResponseMessage> GetSessions([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SmartShower/GetSessions")]HttpRequestMessage req, TraceWriter log)
        //{


        //}








        //        [FunctionName("DeclineInvite")] // decline >> zelfde effect of delete 
        //        public static async Task<HttpResponseMessage> DeclineInvite([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "SmartShower/Group/invite/delete")]HttpRequestMessage req, TraceWriter log)
        //        {
        //            try
        //            {
        //                var content = await req.Content.ReadAsStringAsync();
        //                var group = JsonConvert.DeserializeObject<UserGroup>(content);
        //                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
        //                {
        //                    connection.Open();
        //                    using (SqlCommand command = new SqlCommand())
        //                    {
        //                        command.Connection = connection;
        //                        string sql = "DELETE UserGroup WHERE IdGroup = @IdGroup AND IdUser = @IdUser;";
        //                        command.CommandText = sql;
        //                        command.Parameters.AddWithValue("@IdGroup", group.IdGroup);
        //                        command.Parameters.AddWithValue("@IdUser", group.IdUser);
        //                        command.ExecuteNonQuery();
        //                    }

        //                }
        //                return req.CreateResponse(HttpStatusCode.OK, true);
        //            }
        //            catch (Exception ex)
        //            {
        //#if RELEASE
        //                return req.CreateResponse(HttpStatusCode.InternalServerError);
        //#endif
        //#if DEBUG
        //                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //#endif
        //            }
        //        }







    }
}
