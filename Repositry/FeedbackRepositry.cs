using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using RemediEmr.Class;
using RemediEmr.Data.Class;
using System.Data;
using System.Dynamic;
using System.Linq.Expressions;
using static JwtService;

namespace RemediEmr.Repositry
{
    public class FeedbackRepositry
    {

        private readonly AppDbContext _DbContext;

        private readonly JwtHandler jwthand;

        private readonly IConfiguration _configuration;

        private readonly string _con;

        private readonly string _oracleConnectionString;

        //public YourServiceClassName(IConfiguration configuration)
        //{
        //    _oracleConnectionString = configuration.GetConnectionString("OracleDbContext");
        //}


        public FeedbackRepositry(AppDbContext dbContext, JwtHandler _jwthand, IConfiguration configuration)
        {
            _DbContext = dbContext;
            jwthand = _jwthand;
            _configuration = configuration;
            _con = _DbContext.Database.GetConnectionString();
            _oracleConnectionString = configuration.GetConnectionString("OracleDbContext");
        }

        private OracleConnection GetNewOracleConnection()
        {
            return new OracleConnection(_oracleConnectionString);
        }

        public async Task<dynamic> GetFeedbackQuestionsAndAnswersOPtion(string lngId)
        {
            var Finalobj = new List<dynamic>();

            var questions = await GetFeedbackQuestions(lngId);
            DataTable questionTable = (DataTable)questions.Data;

            if (questions.Status == 200 && questionTable.Rows.Count > 0)
            {
                foreach (DataRow row in questionTable.Rows)
                {
                    int fdqId = Convert.ToInt32(row["FDQ_ID"]);
                    var ans = await GetFeedbackAnswersAsync(lngId, fdqId);

                    dynamic questionItem = new System.Dynamic.ExpandoObject();
                    questionItem.FDQ_ID = fdqId;
                    questionItem.QUESTIONS = row["QUESTIONS"];
                    questionItem.FEEDBACK_Q_TYPE = row["FEEDBACK_Q_TYPE"];
                    questionItem.Answer = ans.Data;

                    Finalobj.Add(questionItem);
                }
                var orderedFinalobj = Finalobj.OrderBy(item =>
                {
                    string type = item.FEEDBACK_Q_TYPE?.ToString().ToUpper();
                    return type switch
                    {
                        "M" => 0,
                        "N" => 1,
                        "T" => 2,
                        _ => 3 // for any unexpected types
                    };
                }).ToList();

                return new { Status = 200, Data = orderedFinalobj };
            }

            return new { Status = 404, Message = "No feedback questions found." };
        }

        public async Task<dynamic> GetFeedbackQuestions(string lngId)
        {
            DataTable dataTable = new DataTable();

            string sqlQuery = @"
        SELECT 
            Q.FDQ_ID, 
            NVL(TRANSLATION, FEEDBACK_QUESTIONS) AS QUESTIONS,
            FEEDBACK_Q_TYPE
        FROM UCHMASTER.FEEDBACK_QUESTIONS Q  
        INNER JOIN UCHMASTER.FEEDBACK_QUEST_ANS_TYPES T ON T.FDQ_ID = Q.FDQ_ID
        LEFT JOIN UCHMASTER.GEN_TRANSLATION_TEXT L ON Q.FEEDBACK_QUESTIONS = L.GEN_TEXT 
            AND L.LNG_ID = :lngId
        WHERE FEEDBACK_TYPES = 'P' 
            AND NVL(Q.ACTIVE_STATUS, 'A') = 'A'
        ORDER BY Q.FDQ_ID ASC";

            try
            {
                using (var connection = GetNewOracleConnection())
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sqlQuery;
                        command.CommandType = CommandType.Text;

                        command.Parameters.Add(new OracleParameter("lngId", OracleDbType.Varchar2) { Value = lngId });

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            dataTable.Load(reader);
                        }
                    }
                }

                return new { Status = 200, Data = dataTable };
            }
            catch (Exception ex)
            {
                return new { Status = 500, Message = ex.Message };
            }
        }

        public async Task<dynamic> GetFeedbackAnswersAsync(string lngId, int strId)
        {
            var result = new List<Dictionary<string, object>>();

            string sqlQuery = @"
        SELECT 
            A.FDA_ID,
            NVL(TRANSLATION, FEEDBACK_ANSWERS) AS ANSWERS,
            IMG_ANSWERS,
            FEEDBACK_TYPE,
            CSSCLASS_ANSWERS,
            IMG_ANSWERS_WEB
        FROM UCHMASTER.FEEDBACK_QUESTIONS_ANSWERS QA 
        INNER JOIN UCHMASTER.FEEDBACK_ANSWERS A ON QA.FDA_ID = A.FDA_ID
        LEFT JOIN UCHMASTER.GEN_TRANSLATION_TEXT L ON A.FEEDBACK_ANSWERS = L.GEN_TEXT 
            AND L.LNG_ID = :lngId
        WHERE NVL(A.ACTIVE_STATUS, 'A') = 'A' 
            AND QA.FDQ_ID = :strId
        ORDER BY A.FDA_ID ASC";

            try
            {
                using (var connection = GetNewOracleConnection())
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sqlQuery;
                        command.CommandType = CommandType.Text;

                        command.Parameters.Add(new OracleParameter("lngId", OracleDbType.Varchar2) { Value = lngId });
                        command.Parameters.Add(new OracleParameter("strId", OracleDbType.Int32) { Value = strId });

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var item = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    item[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                }
                                result.Add(item);
                            }
                        }
                    }
                }

                return new { Status = 200, Data = result };
            }
            catch (Exception ex)
            {
                return new { Status = 500, Message = ex.Message };
            }
        }


        public async Task<dynamic> InsertFeedbackAnswersAsync(List<FeedbackSubmission> submissions)
        {
            string insertQuery = @"
        INSERT INTO UCHMASTER.PATI_FEEDBACK_QA
        (FQAP_ID, PATI_ID, FDQ_ID, FDA_ID, MASTER_FQAP_ID, TXT_ANSWERS)
        VALUES (UCHTRANS.SEQ_FQAP_ID.NEXTVAL, :PATI_ID, :FDQ_ID, :FDA_ID, :MASTER_FQAP_ID, :TXT_ANSWERS)";

            try
            {
                var fdqid = await GetNextMasterFqapId();
                var fid = fdqid.Data;
                using (var connection = _DbContext.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = insertQuery;
                        command.Transaction = transaction;

                        foreach (var item in submissions)
                        {
                            if(item.AnswersId!=0)
                            {

                        
                                command.Parameters.Clear();

                                command.Parameters.Add(new OracleParameter("PATI_ID", OracleDbType.Varchar2) { Value = item.PatiId });
                                command.Parameters.Add(new OracleParameter("FDQ_ID", OracleDbType.Int32) { Value = item.QuestionsId });
                                command.Parameters.Add(new OracleParameter("FDA_ID", OracleDbType.Int32) { Value = item.AnswersId });
                                command.Parameters.Add(new OracleParameter("MASTER_FQAP_ID", OracleDbType.Int32) { Value = fid });
                                command.Parameters.Add(new OracleParameter("TXT_ANSWERS", OracleDbType.Varchar2) { Value = item.TxtAnswers ?? string.Empty });

                                await command.ExecuteNonQueryAsync();
                            }
                        }

                        transaction.Commit();
                    }
                }

                return new { Status = 200, Message = "Submitted Successfully" };
            }
            catch (Exception ex)
            {
                // Log exception as needed

                return new { Status = 500, Message = ex.Message };
            }
        }


        public async Task<dynamic> GetPatiIdByRandomKey(string randomKey)
        {
            string patiId = null;

            using (OracleConnection conn = new OracleConnection(_con))
            {
                try
                {
                    conn.Open();

                    string query = "SELECT PATI_ID FROM UCHMASTER.OPN_PATIENT WHERE Random_key = :randomKey";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("randomKey", randomKey));

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                patiId = reader["PATI_ID"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle the error appropriately
                    return new { Status = 500, Message = ex.Message };
                }
            }

            return new { Status = 200, Data = patiId };
        }

        public async Task<dynamic> GetPatientDetails(string randomKey)
        {
            dynamic patient = null;

            using (var conn = new OracleConnection(_con))
            {
                try
                {


                    string query = @"
                SELECT 
                    PATI_ID,  
                    UCHTRANS.FN_GET_FORMATTED_OP_NO(PATI_OPNO) AS PATI_OPNO, 
                    PATI_FIRST_NAME || DECODE(NVL(PATI_LAST_NAME, ''), '', '', ' ' || PATI_LAST_NAME) AS PATI_NAME, 
                    UCHADMIN.GET_AGE_SHORT(TO_CHAR(PATI_BIRTH_DATE,'DD/MM/YYYY'),' Y',' M',' D') AS PATI_AGE, 
                    PATI_GENDER AS GENDER,
                    DECODE(PATI_GENDER,'M','MALE','F','FEMALE','OTHER') AS PATI_GENDER, 
                    PATI_MOBILE, 
                    PATI_EMAIL,
                    IDCARD_NAME || DECODE(NVL(CPRNO, ''), '', '', '-' || CPRNO) AS CPRNO
                FROM UCHMASTER.OPN_PATIENT p
                LEFT JOIN UCHMASTER.GEN_NATIONAL_ID_CARD CD ON p.CPR_ID = CD.CPR_ID 
                LEFT JOIN UCHMASTER.OPN_IDENTITY_CARD ty ON ty.IDCARD_ID = p.IDCARD_ID 
                WHERE RANDOM_KEY = :randomKey";

                    using (var cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("randomKey", randomKey));
                        conn.Open();

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                patient = new ExpandoObject();
                                var patientDict = (IDictionary<string, object>)patient;

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    patientDict[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {

                    return new { Status = 500, Message = ex.Message };
                }

                return new { Status = 200, Data = patient };
            }
        }

        public async Task<dynamic> GetNextMasterFqapId()
        {
            int nextId = 0;

            using (var conn = new OracleConnection(_con))
            {
                string query = "SELECT NVL(MAX(MASTER_FQAP_ID), 0) + 1 AS MASTER_FQAP_ID FROM UCHMASTER.PATI_FEEDBACK_QA";

                using (var cmd = new OracleCommand(query, conn))
                {
                    try
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        nextId = Convert.ToInt32(result);
                    }
                    catch (Exception ex)
                    {
                        // Handle or log error as needed

                        return new { Status = 500, Message = ex.Message };
                    }
                }
            }

            return new { Status = 200, Data = nextId };
        }




    }
}
