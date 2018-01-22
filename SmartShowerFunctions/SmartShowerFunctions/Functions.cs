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
using Microsoft.Azure.Documents.Client;
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
                //Guid clientId = Guid.NewGuid(); // >> id wordt meegeven in de APP zelf


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

                            command.Parameters.AddWithValue("@Guid", user.IdUser);
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
                            command.Parameters.AddWithValue("@WaterCost", 0.005); // vaste gemiddelde prijs per liter
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
                        string showerCheck = "SELECT * FROM Shower WHERE IdShower = @ShowerId;";
                        command.Parameters.AddWithValue("@ShowerId", UserShower.IdShower);
                        command.CommandText = showerCheck;
                        DataSet ds = new DataSet();
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(ds);
                        //connection.Close();
                        bool showerAlreadyReg = ((ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0));
                        if (showerAlreadyReg)
                        {
                            string sql = "INSERT INTO UserShower VALUES(@IdShower,@IdUser)";
                            command.CommandText = sql;
                            command.Parameters.AddWithValue("@IdShower", UserShower.IdShower);
                            command.Parameters.AddWithValue("@IdUser", UserShower.IdUser);
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            return req.CreateResponse(HttpStatusCode.NotFound);

                        }


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

                        string sql = "SELECT users.IdUser, users.Name, users.Email, users.Color, users.MaxShowerTime, users.IdealTemperature, users.Monitor, users.Photo, Shower.IdShower, dbo.Shower.WaterCost FROM Users INNER JOIN dbo.UserShower ON Users.IdUser = dbo.UserShower.IdUser INNER JOIN Shower ON Shower.IdShower = UserShower.IdShower where Users.Email like @Email and Users.Password = @Password;";
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
                            return req.CreateResponse(HttpStatusCode.OK, ds.Tables[0]);
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
                            userInfo.Color = Convert.ToInt32(reader["Color"]);
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
                var group = JsonConvert.DeserializeObject<Groups>(content);
                var user = JsonConvert.DeserializeObject<User>(content);
                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        string sql = "INSERT INTO Groups VALUES(@IdGroup, @Name, @Photo);";
                        command.Parameters.AddWithValue("@IdGroup", group.IdGroup);
                        command.Parameters.AddWithValue("@Name", group.Name);
                        command.Parameters.AddWithValue("@Photo", group.Photo);
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                        string addUser = "INSERT INTO UserGroup VALUES(@GroupId, @IdUser);";
                        command.Parameters.AddWithValue("@GroupId", group.IdGroup);
                        command.Parameters.AddWithValue("@IdUser", user.IdUser);
                        command.CommandText = addUser;
                        command.ExecuteNonQuery();

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
                                Color = Convert.ToInt32(ds.Tables[0].Rows[0]["Color"]),
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
                            return req.CreateResponse(HttpStatusCode.NotFound, false);

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

        [FunctionName("GetAllGroupsFromUser")]
        public static async Task<HttpResponseMessage> GetAllGroupsFromUser([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SmartShower/getAllGroupsFromUser")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                List<Groups> GroupInfo = new List<Groups>();
                var content = await req.Content.ReadAsStringAsync();
                var group = JsonConvert.DeserializeObject<UserGroup>(content);
                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        string sql = "SELECT Groups.IdGroup, Name, Photo FROM dbo.Groups INNER JOIN UserGroup ON Groups.IdGroup = dbo.UserGroup.IdGroup WHERE IdUser = @IdUser AND dbo.UserGroup.Pending = 0;";
                        command.CommandText = sql; 
                        command.Parameters.AddWithValue("@IdUser", group.IdUser);
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            GroupInfo.Add(new Groups()
                            {
                                IdGroup = new Guid(reader["IdGroup"].ToString()),
                                Name = reader["Name"].ToString(),
                                Photo = reader["Photo"].ToString()

                            });


                        }
                    }

                }
                return req.CreateResponse(HttpStatusCode.OK, GroupInfo);

            }
            catch(Exception ex)
            {
#if RELEASE
                return req.CreateResponse(HttpStatusCode.InternalServerError);
#endif
#if DEBUG
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }
        }

        [FunctionName("GetAllFriendsFromUser")]
        public static async Task<HttpResponseMessage> GetAllFriendsFromUser([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SmartShower/GetAllFriendsFromUser")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                List<User> Friends = new List<User>();
                List<GroupSessions> FriendsWithSessionData = new List<GroupSessions>();
                var content = await req.Content.ReadAsStringAsync();
                var group = JsonConvert.DeserializeObject<UserGroup>(content);
                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        string sql = "";
                        switch (group.WithSessionData) // property van de klasse UserGroup >> zit niet in de database. 
                        {
                            case true:
                                sql = "SELECT DISTINCT UserIdTable.[IdUser], Users.[Name], Users.[Photo], Sessions.WaterUsed, Sessions.MoneySaved, Sessions.EcoScore, Sessions.AverageTemp, Sessions.Duration, Sessions.Timestamp FROM[dbo].[UserGroup] AS UserIdTable INNER JOIN[dbo].[UserGroup] AS GroupIdTable ON GroupIdTable.[IdGroup] = UserIdTable.[IdGroup] and GroupIdTable.[IdUser] = @IdUser INNER JOIN [dbo].[Users] as Users ON Users.[IdUser] = UserIdTable.[IdUser] INNER JOIN dbo.Session AS Sessions ON Sessions.IdUser = Users.IdUser;";
                                command.CommandText = sql;
                                command.Parameters.AddWithValue("@IdUser", group.IdUser);
                                SqlDataReader reader = command.ExecuteReader();
                                while (reader.Read())
                                {
                                    FriendsWithSessionData.Add(new GroupSessions
                                    {
                                        IdUser = new Guid(reader["IdUser"].ToString()),
                                        Name = reader["Name"].ToString(),
                                        Photo = reader["Photo"].ToString(),
                                        WaterUsed = float.Parse(reader["WaterUsed"].ToString()),
                                        MoneySaved = float.Parse(reader["MoneySaved"].ToString()),
                                        EcoScore = float.Parse(reader["EcoScore"].ToString()),
                                        AverageTemp = float.Parse(reader["AverageTemp"].ToString()),
                                        Duration = TimeSpan.Parse(reader["Duration"].ToString()),
                                        Timestamp = Convert.ToDateTime(reader["Timestamp"])

                                    });
                                }
                               

                                break;
                            case false:
                                sql = "SELECT DISTINCT UserIdTable.[IdUser], Users.[Name], Users.[Photo] FROM[dbo].[UserGroup] AS UserIdTable INNER JOIN[dbo].[UserGroup] AS GroupIdTable ON GroupIdTable.[IdGroup] = UserIdTable.[IdGroup] and GroupIdTable.[IdUser] = @IdUser INNER JOIN [dbo].[Users] as Users ON Users.[IdUser] = UserIdTable.[IdUser];";
                                command.CommandText = sql;
                                command.Parameters.AddWithValue("@IdUser", group.IdUser);
                                SqlDataReader readerSession = command.ExecuteReader();
                                while (readerSession.Read())
                                {
                                    Friends.Add(new User
                                    {
                                        IdUser = new Guid(readerSession["IdUser"].ToString()),
                                        Name = readerSession["Name"].ToString(),
                                        Photo = readerSession["Photo"].ToString(),

                                    });
                                }

                                break;

                        }
                        
                    }
                    if(group.WithSessionData)
                        return req.CreateResponse(HttpStatusCode.OK, FriendsWithSessionData);
                    else
                        return req.CreateResponse(HttpStatusCode.OK, Friends);

                }

            }
            catch(Exception ex)
            {
#if RELEASE
                return req.CreateResponse(HttpStatusCode.InternalServerError);
#endif
#if DEBUG
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }
        }

        [FunctionName("GetGroupRanking")]
        public static async Task<HttpResponseMessage> GetGroupRanking([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SmartShower/GetGroupRanking")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                List<GroupSessions> GroupData = new List<GroupSessions>();
                var content = await req.Content.ReadAsStringAsync();
                var session = JsonConvert.DeserializeObject<Session>(content);
                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    string sql = "";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        switch (session.DataLenght)
                        {
                            case 0:
                                sql = "SELECT dbo.UserGroup.IdGroup, dbo.Groups.Name, dbo.Groups.Photo, dbo.Session.IdUser, session.IdSession, Session.WaterUsed, Session.MoneySaved, dbo.Session.EcoScore, Session.AverageTemp,Session.Duration, dbo.Session.Timestamp FROM dbo.Session JOIN dbo.UserGroup on UserGroup.IdUser = Session.IdUser JOIN dbo.Groups ON Groups.IdGroup = UserGroup.IdGroup WHERE dbo.UserGroup.IdGroup IN(SELECT dbo.UserGroup.IdGroup FROM dbo.UserGroup WHERE iduser = @IdUser) AND Timestamp >= DATEADD(DAY, 0, DATEDIFF(DAY, 0, CURRENT_TIMESTAMP)) AND Timestamp <  DATEADD(DAY, 1, DATEDIFF(DAY, 0, CURRENT_TIMESTAMP)) ORDER BY dbo.UserGroup.IdGroup; ";
                                break;
                            case 1:
                                 sql = "SELECT dbo.UserGroup.IdGroup, dbo.Groups.Name, dbo.Groups.Photo, dbo.Session.IdUser, session.IdSession, Session.WaterUsed, Session.MoneySaved, dbo.Session.EcoScore, Session.AverageTemp,Session.Duration, dbo.Session.Timestamp FROM dbo.Session JOIN dbo.UserGroup on UserGroup.IdUser = Session.IdUser JOIN dbo.Groups ON Groups.IdGroup = UserGroup.IdGroup WHERE dbo.UserGroup.IdGroup IN(SELECT dbo.UserGroup.IdGroup FROM dbo.UserGroup WHERE iduser = @IdUser) AND dbo.Session.Timestamp >= DATEADD(DAY, -7, DATEDIFF(DAY, 0, CURRENT_TIMESTAMP)) AND dbo.Session.Timestamp <  DATEADD(DAY, 1, DATEDIFF(DAY, 0, CURRENT_TIMESTAMP)) ORDER BY dbo.UserGroup.IdGroup;";
                                break;
                            case 2:
                                sql = "SELECT dbo.UserGroup.IdGroup, dbo.Groups.Name, dbo.Groups.Photo, dbo.Session.IdUser, session.IdSession, Session.WaterUsed, Session.MoneySaved, dbo.Session.EcoScore, Session.AverageTemp,Session.Duration, dbo.Session.Timestamp FROM dbo.Session JOIN dbo.UserGroup on UserGroup.IdUser = Session.IdUser JOIN dbo.Groups ON Groups.IdGroup = UserGroup.IdGroup WHERE dbo.UserGroup.IdGroup IN(SELECT dbo.UserGroup.IdGroup FROM dbo.UserGroup WHERE iduser = @IdUser) AND dbo.Session.Timestamp >= DATEADD(DAY, -30, DATEDIFF(DAY, 0, CURRENT_TIMESTAMP)) AND dbo.Session.Timestamp <  DATEADD(DAY, 1, DATEDIFF(DAY, 0, CURRENT_TIMESTAMP)) ORDER BY dbo.UserGroup.IdGroup;";
                                break;
                        }
                        command.Connection = connection;
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("@IdUser", session.IdUser);
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            GroupData.Add(new GroupSessions
                            {
                                IdGroup = new Guid(reader["IdGroup"].ToString()),
                                Name = reader["Name"].ToString(),
                                Photo = reader["Photo"].ToString(),
                                IdUser = new Guid(reader["IdUser"].ToString()),
                                IdSession = new Guid(reader["IdSession"].ToString()),
                                WaterUsed = float.Parse(reader["WaterUsed"].ToString()),
                                MoneySaved = float.Parse(reader["MoneySaved"].ToString()),
                                EcoScore = float.Parse(reader["EcoScore"].ToString()),
                                AverageTemp = float.Parse(reader["AverageTemp"].ToString()),
                                Duration = TimeSpan.Parse(reader["Duration"].ToString()),
                                Timestamp = Convert.ToDateTime(reader["Timestamp"]),

                            });
                        }

                    }

                }
                GroupSessions result = new GroupSessions();

                result.IdGroup = GroupData[0].IdGroup;
                Guid OldId = new Guid();
                Guid IdGroup = new Guid();
                List<GroupSessions> DataForEachGroup = new List<GroupSessions>();
                for (int i = 0; i < GroupData.Count; i++)
                {
                    IdGroup = GroupData[i].IdGroup;
                    if (i > 0)
                    {
                        OldId = GroupData[i - 1].IdGroup;
                    }
                    else
                    {
                        OldId = IdGroup;
                    }

                    if(IdGroup == OldId)
                    {
                        DataForEachGroup.Add(GroupData[i]);

                    }
                    else
                    {
                        foreach(GroupSessions se in DataForEachGroup)
                        {
                            result.WaterUsed += session.WaterUsed;
                            result.MoneySaved += session.MoneySaved;
                            result.EcoScore += session.EcoScore;
                            result.Duration += session.Duration;
                            result.AverageTemp += session.AverageTemp * (float)session.Duration.TotalSeconds;
                        }
                        result.AverageTemp = result.AverageTemp / (float)result.Duration.TotalSeconds;
                        DataForEachGroup.Clear();

                    }

                }

                return req.CreateResponse(HttpStatusCode.OK, result);


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
                                sql = "SELECT IdSession, IdUser, WaterUsed, MoneySaved, EcoScore, AverageTemp, Duration, Timestamp FROM Session WHERE IdUser = @IdUser AND TIMESTAMP >= DATEADD(DAY, -7, DATEDIFF(DAY, 0, CURRENT_TIMESTAMP)) AND Timestamp <  DATEADD(DAY, 1, DATEDIFF(DAY, 0, CURRENT_TIMESTAMP));";
                                break;
                            case 2:
                                sql = "SELECT IdSession, IdUser, WaterUsed, MoneySaved, EcoScore, AverageTemp, Duration, Timestamp FROM Session WHERE IdUser = @IdUser AND TIMESTAMP >= DATEADD(DAY, -30, DATEDIFF(DAY, 0, CURRENT_TIMESTAMP)) AND Timestamp <  DATEADD(DAY, 1, DATEDIFF(DAY, 0, CURRENT_TIMESTAMP));";
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
                                Duration = TimeSpan.Parse(reader["Duration"].ToString()),
                                Timestamp = Convert.ToDateTime(reader["Timestamp"]),
                                DataLenght = Convert.ToInt32(session.DataLenght)

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

        [FunctionName("AddSessionToCosmosDb")]
        public static async Task<HttpResponseMessage> AddSessionToCosmosDb([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SmartShower/AddSession")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                // content van de body inlezen
                var content = await req.Content.ReadAsStringAsync();
                SessionCosmosDb sessionData = JsonConvert.DeserializeObject<SessionCosmosDb>(content);
                sessionData.Timestamp = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "w. Europe Standard Time");
                var client = new DocumentClient(new Uri(COSMOSHOST), COSMOSKEY);
                var docUrl = UriFactory.CreateDocumentCollectionUri(COSMOSDATABASE, COSMOSCOLLECTIONID);
                await client.CreateDocumentAsync(docUrl, sessionData);
                return req.CreateResponse(HttpStatusCode.OK, sessionData);
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

        [FunctionName("GetSessionFromCosmosDb")]
        public static HttpResponseMessage GetSessionFromCosmosDb([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "SmartShower/getSession/{idSession}")]HttpRequestMessage req, string idSession, TraceWriter log)
        {
            try
            {
                var client = new DocumentClient(new Uri(COSMOSHOST), COSMOSKEY);
                // volgende stap uri prepareren
                var docUrl = UriFactory.CreateDocumentCollectionUri(COSMOSDATABASE, COSMOSCOLLECTIONID);
                IQueryable<SessionCosmosDb> logs = client.CreateDocumentQuery<SessionCosmosDb>($"/dbs/{COSMOSDATABASE}/colls/{COSMOSCOLLECTIONID}", new FeedOptions { EnableCrossPartitionQuery = true, MaxDegreeOfParallelism = 10, MaxBufferedItemCount = 100 }).Where(p => p.IdSession == new Guid(idSession));
                
                return req.CreateResponse(HttpStatusCode.OK, logs.ToList<SessionCosmosDb>());
            }
            catch (Exception ex)
            {
#if RELEASE
                return req.CreateResponse(HttpStatusCode.InternalServerError);
#endif
#if DEBUG
                log.Info(ex.ToString());
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }
        }



        [FunctionName("CalculateSession")]
        public static async Task<HttpResponseMessage> CalculateSession([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "SmartShower/calculateSession/{idSession}")]HttpRequestMessage req, string idSession, TraceWriter log)
        {
            try
            {
                
                string url = String.Format("https://smartshowerfunctions.azurewebsites.net/api/SmartShower/getSession/{0}", idSession);
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                string result = await client.GetStringAsync(url);
                List<SessionCosmosDb> sessionData = JsonConvert.DeserializeObject<List<SessionCosmosDb>>(result);
                float averageTemp = new float();
                float waterUsed = new float();
                TimeSpan duration = sessionData[sessionData.Count - 1].Timestamp - sessionData[0].Timestamp;
                foreach (SessionCosmosDb se in sessionData)
                {
                    averageTemp += se.Temp;
                    waterUsed += se.WaterUsage * 2;
                }
                averageTemp = averageTemp / sessionData.Count();

                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        string sql = "select Users.IdUser from Users INNER JOIN UserShower ON Users.IdUser = UserShower.IdUser WHERE IdShower = @IdShower AND Users.Color = @Color;";
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("@IdShower", sessionData[0].IdShower);
                        command.Parameters.AddWithValue("@Color", sessionData[0].ProfileNumber);
                        command.ExecuteNonQuery();
                        DataSet ds = new DataSet();
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(ds);
     

                        bool idUserFound = ((ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0));
                        if (idUserFound)
                        {
                            string insertSql = "INSERT INTO Session VALUES(@IdSession, @IdUser, @WaterUsed, @MoneySaved, @EcoScore, @AverageTemp, @Duration, @Timestamp);";
                            command.CommandText = insertSql;
                            command.Parameters.AddWithValue("@IdSession", Guid.NewGuid());
                            command.Parameters.AddWithValue("@IdUser", ds.Tables[0].Rows[0]["IdUser"]);
                            command.Parameters.AddWithValue("@WaterUsed", waterUsed);
                            command.Parameters.AddWithValue("@MoneySaved", 2.2);
                            command.Parameters.AddWithValue("@EcoScore", 9.1);
                            command.Parameters.AddWithValue("@AverageTemp", averageTemp);
                            command.Parameters.AddWithValue("@Duration", duration);
                            command.Parameters.AddWithValue("@Timestamp", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "w. Europe Standard Time"));
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            return req.CreateResponse(HttpStatusCode.NotFound, false);
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
                log.Info(ex.ToString());
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }
        }

        [FunctionName("GetAvailableColors")]
        public static async Task<HttpResponseMessage> GetAvailableColors([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SmartShower/getAvailableColors")]HttpRequestMessage req,  TraceWriter log)
        {
            try
            {
                List<int> allColors = new List<int>() { 1, 2, 3, 4, 5, 6, 7 };
                List<int> colors = new List<int>();
                var content = await req.Content.ReadAsStringAsync();
                var shower = JsonConvert.DeserializeObject<Shower>(content);
                using (SqlConnection connection = new SqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        string sql = "select color from Users inner join UserShower on Users.IdUser = UserShower.IdUser where UserShower.IdShower = @IdShower;";
                        command.Parameters.AddWithValue("@IdShower", shower.IdShower);
                        command.CommandText = sql;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            colors.Add(Convert.ToInt32(reader["color"]));

                        }

                    }
                }
                foreach(int color in colors)
                {
                    allColors.Remove(color);
                }
                return req.CreateResponse(HttpStatusCode.OK, allColors);

            }
            catch (Exception ex)
            {
#if RELEASE
                return req.CreateResponse(HttpStatusCode.InternalServerError);
#endif
#if DEBUG
                log.Info(ex.ToString());
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
#endif
            }
        }










    }
}
