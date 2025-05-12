using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using RemediEmr.Class;
using RemediEmr.Data.Class;
using System.Data;
using static JwtService;

namespace RemediEmr.Repositry
{
    public class FeedbackRepositry
    {

        private readonly AppDbContext _DbContext;

        private readonly JwtHandler jwthand;

        private readonly IConfiguration _configuration;
        private readonly string _con;

        public FeedbackRepositry(AppDbContext dbContext, JwtHandler _jwthand, IConfiguration configuration)
        {
            _DbContext = dbContext;
            jwthand = _jwthand;
            _configuration = configuration;
            _con = _DbContext.Database.GetConnectionString();
        }

        public async Task<dynamic> GetFeedbackQuestionsAsync(string lngId)
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
                using (var connection = _DbContext.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sqlQuery;
                        command.CommandType = CommandType.Text;

                        // OracleParameter for lngId
                        var param = new OracleParameter("lngId", OracleDbType.Varchar2) { Value = lngId };
                        command.Parameters.Add(param);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            dataTable.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new { Status = 500, Message = ex.Message };
            }
            return new { Status = 200,Data=dataTable};
        }


        public async Task<dynamic> GetFeedbackAnswersAsync(string lngId, int strId)
        {
            DataTable dataTable = new DataTable();

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
                using (var connection = _DbContext.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sqlQuery;
                        command.CommandType = CommandType.Text;

                        // Add parameters
                        command.Parameters.Add(new OracleParameter("lngId", OracleDbType.Varchar2) { Value = lngId });
                        command.Parameters.Add(new OracleParameter("strId", OracleDbType.Int32) { Value = strId });

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            dataTable.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new { Status = 500, Message = ex.Message };
            }


            return new { Status = 200, Data = dataTable };
        }


        public async Task<dynamic> InsertFeedbackAnswersAsync(List<FeedbackSubmission> submissions)
        {
            string insertQuery = @"
        INSERT INTO UCHMASTER.PATI_FEEDBACK_QA
        (FQAP_ID, PATI_ID, FDQ_ID, FDA_ID, MASTER_FQAP_ID, TXT_ANSWERS)
        VALUES (UCHTRANS.SEQ_FQAP_ID.NEXTVAL, :PATI_ID, :FDQ_ID, :FDA_ID, :MASTER_FQAP_ID, :TXT_ANSWERS)";

            try
            {
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
                            command.Parameters.Clear();

                            command.Parameters.Add(new OracleParameter("PATI_ID", OracleDbType.Varchar2) { Value = item.PatiId });
                            command.Parameters.Add(new OracleParameter("FDQ_ID", OracleDbType.Int32) { Value = item.QuestionsId });
                            command.Parameters.Add(new OracleParameter("FDA_ID", OracleDbType.Int32) { Value = item.AnswersId });
                            command.Parameters.Add(new OracleParameter("MASTER_FQAP_ID", OracleDbType.Int32) { Value = item.MasterFqapId });
                            command.Parameters.Add(new OracleParameter("TXT_ANSWERS", OracleDbType.Varchar2) { Value = item.TxtAnswers ?? string.Empty });

                            await command.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log exception as needed
                throw new Exception("Error inserting feedback answers: " + ex.Message);
            }
        }


    }
}
