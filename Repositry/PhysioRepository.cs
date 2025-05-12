using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using RemediEmr.Class;
using RemediEmr.Data.Class;
using RemediEmr.Data.DbModel;
using System.Configuration;
using System.Data;
using System.Dynamic;
using System.Globalization;
using static JwtService;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
//using System.Net.Http.httpc;

namespace RemediEmr.Repositry
{
    public class PhysioRepository
    {
        private readonly AppDbContext _DbContext;

        private readonly JwtHandler jwthand;
        private readonly IConfiguration _configuration;
        string sttime;
        string edtime;


        public PhysioRepository(AppDbContext dbContext, JwtHandler _jwthand, IConfiguration configuration)
        {
            _DbContext = dbContext;
            jwthand = _jwthand;
            _configuration = configuration;

        }
        public async Task<dynamic> LoginCheck(string username, string password)
        {
            try
            {
                var data = await (from a in _DbContext.HRM_EMPLOYEE
                                  where a.EMP_ACTIVE_STATUS == 'A' &&
                                a.EMP_LOGIN_NAME == username &&
                                a.EMP_PASSWORD == password

                                  select new
                                  {
                                      a.EMP_ID,
                                      a.EMP_OFFL_NAME,
                                      a.EMP_LOGIN_NAME,
                                      a.EMP_PASSWORD
                                  })
                   .ToListAsync();


                if (data != null && data.Count > 0)
                {
                    var userdat = new UserTocken
                    {
                        AUSR_ID = data[0].EMP_ID,
                        USERNAME = data[0].EMP_LOGIN_NAME,
                        PASSWORD = data[0].EMP_PASSWORD
                    };

                    var token = jwthand.GenerateToken(userdat);

                    //check exiting userdetail in loginsettings
                    var dat = await _DbContext.LOGIN_SETTINGS.Where(x => x.USERID == data[0].EMP_ID).ToListAsync();
                    var existingDATA = new UCHMASTER_LoginSettings();

                    if (dat.Count > 0)
                    {

                        existingDATA = dat[0];
                    }
                    else
                    {
                        existingDATA = null;
                    }
                    //if data exist then edit the table

                    if (existingDATA != null)
                    {
                        existingDATA.TOKEN = token;
                        existingDATA.GENERATEDATE = DateTime.Now;
                        _DbContext.SaveChanges();

                    }
                    //if no existing data exist then add new data to teh tablw
                    else
                    {
                        var newlogin = new UCHMASTER_LoginSettings
                        {
                            USERID = userdat.AUSR_ID,
                            TOKEN = token,
                            GENERATEDATE = DateTime.Now,
                        };

                        _DbContext.LOGIN_SETTINGS.Add(newlogin);
                        _DbContext.SaveChanges();
                    }

                    var msgsuccsess = new DefaultMessage.Message1
                    {
                        Status = 200,
                        Message = token
                    };
                    return msgsuccsess;


                }
                else
                {
                    var msg = new DefaultMessage.Message1
                    {
                        Status = 600,
                        Message = "Invalid Username and Password"

                    };
                    return msg;
                }

            }
            catch (Exception ex)
            {
                var msg1 = new DefaultMessage.Message1
                {
                    Status = 500,
                    Message = ex.Message
                };
                return msg1;
            }

        }

        public async Task<dynamic> GetUserDetails(UserTocken ut)
        {
            try
            {

                var result = (from emp in _DbContext.HRM_EMPLOYEE
                              where emp.EMP_ID == ut.AUSR_ID
                              select new
                              {
                                  emp.EMP_ID,
                                  emp.EMP_OFFL_NAME,
                                  emp.EMP_CONT_PER_PHONE,

                              }).ToList();
                return result;

            }
            catch (Exception ex)
            {
                var msg1 = new DefaultMessage.Message1
                {
                    Status = 500,
                    Message = ex.Message
                };
                return msg1;

            }
        }


        public async Task<dynamic> GetAllUserBranches(UserTocken ut)
        {
            try
            {

                var result = (from b in _DbContext.HRM_BRANCH
                              join bl in _DbContext.HRM_EMPLOYEE_BRANCH_LINK on b.BRANCH_ID equals bl.BRANCH_ID
                              join hr in _DbContext.HRM_EMPLOYEE_HR on bl.EMP_ID equals hr.EMP_ID
                              join emp in _DbContext.HRM_EMPLOYEE on hr.EMP_ID equals emp.EMP_ID_HR

                              where b.ACTIVE_STATUS == "A"
                                    && emp.EMP_LOGIN_NAME == ut.USERNAME
                                    && emp.EMP_PASSWORD == ut.PASSWORD
                              orderby b.BRANCH_ID
                              select new
                              {
                                  b.BRANCH_ID,
                                  b.BRCH_NAME
                              }).ToList();
                return result;

            }
            catch (Exception ex)
            {
                var msg1 = new DefaultMessage.Message1
                {
                    Status = 500,
                    Message = ex.Message
                };
                return msg1;

            }
        }

        public async Task<dynamic> GetSectionId()
        {
            try
            {
                //var data = await _DbContext.EMR_ADMIN_USERS
                //    .Where(x => x.AUSR_USERNAME == username
                //    && x.AUSR_PWD == password
                //    && x.AUSR_STATUS != "D")
                //    .ToListAsync();


                var SectonId = _configuration["SectionId"];



                return new { SectionId = SectonId };




            }
            catch (Exception ex)
            {
                var msg1 = new DefaultMessage.Message1
                {
                    Status = 500,
                    Message = ex.Message
                };
                return msg1;
            }

        }


        //userwntedreports
        public async Task<dynamic> GetAppMenuAsync(string userId)
        {
            try
            {
                // Define the SQL statement to call the stored procedure
                var sql = "BEGIN UCHTRANS.SP_MIS_APP_MENU(:USER_ID, :STROUT); END;";

                // Define the parameters
                var userIdParam = new OracleParameter("USER_ID", OracleDbType.Varchar2) { Value = userId };
                var strOutParam = new OracleParameter("STROUT", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };

                // Execute the stored procedure
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = System.Data.CommandType.Text;

                    // Add parameters to the command
                    cmd.Parameters.Add(userIdParam);
                    cmd.Parameters.Add(strOutParam);

                    await _DbContext.Database.GetDbConnection().OpenAsync();

                    using (var reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        var results = new List<dynamic>();

                        while (await reader.ReadAsync())
                        {
                            var result = new
                            {
                                TabName = reader["TAB_NAME"] != DBNull.Value ? reader["TAB_NAME"].ToString() : null,
                                Link = reader["LINK"] != DBNull.Value ? reader["LINK"].ToString() : null,
                                Priority = reader["PRIORITY"] != DBNull.Value ? Convert.ToInt32(reader["PRIORITY"]) : (int?)null
                            };

                            results.Add(result);
                        }

                        return results;
                    }
                }
            }
            catch (Exception ex)
            {
                var msg1 = new DefaultMessage.Message1
                {
                    Status = 500,
                    Message = ex.Message
                };
                return msg1;
            }
        }




        public async Task<List<dynamic>> GetPatientListDatewiseAsync(string empId, string sctId = null, string? name = null, string? mobile = null,
    string? cprNo = null, string? visitType = null, int groupId = 0, DateTime? date = null,
    string? listType = null, string? levelId = null, string branchId = null)
        {
            try
            {
                var sql = "BEGIN UCHEMR.EMR_SP_PATI_LIST_DATEWISE(:EMPID, :SCTID, :NAME, :MOBILE, :CPRNO, :VISITTYPE, :P_GRPID, :P_DATE, :P_LIST_TYPE, :P_LEVEL_ID, :P_BRANCH_ID, :STROUT); END;";

                var parameters = new List<OracleParameter>
                    {
                        new OracleParameter("EMPID", OracleDbType.Varchar2) { Value = empId },
                        new OracleParameter("SCTID", OracleDbType.Varchar2) { Value = sctId },
                        new OracleParameter("NAME", OracleDbType.Varchar2) { Value = name ?? (object)DBNull.Value },
                        new OracleParameter("MOBILE", OracleDbType.Varchar2) { Value = mobile ?? (object)DBNull.Value },
                        new OracleParameter("CPRNO", OracleDbType.Varchar2) { Value = cprNo ?? (object)DBNull.Value },
                        new OracleParameter("VISITTYPE", OracleDbType.Varchar2) { Value = visitType ?? (object)DBNull.Value },
                        new OracleParameter("P_GRPID", OracleDbType.Int32) { Value = groupId },
                        new OracleParameter("P_DATE", OracleDbType.Date) { Value = date ?? (object)DBNull.Value },
                        new OracleParameter("P_LIST_TYPE", OracleDbType.Char) { Value = listType ?? (object)DBNull.Value },
                        new OracleParameter("P_LEVEL_ID", OracleDbType.Varchar2) { Value = levelId ?? (object)DBNull.Value },
                        new OracleParameter("P_BRANCH_ID", OracleDbType.Varchar2) { Value = branchId },
                        new OracleParameter("STROUT", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
                    };

                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddRange(parameters.ToArray());

                    await _DbContext.Database.GetDbConnection().OpenAsync();

                    using (var reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        var results = new List<dynamic>();
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                Type = reader["TYPE"]?.ToString(),
                                DoctorId = reader["OPVDTLS_DOCTOR_ID"]?.ToString(),
                                RowNumber = reader["ROWNUMBER"]?.ToString(),
                                CreateDate = reader["CREATE_DATE"]?.ToString(),
                                RegistrationType = reader["REGTY_ID"]?.ToString(),
                                RegistrationTime = reader["REG_TIME"]?.ToString(),
                                VisitTime = reader["VST_TIME"]?.ToString(),
                                DocumentId = reader["EMR_DOC_ID"]?.ToString(),
                                CPRId = reader["CPR_ID"]?.ToString(),
                                ClaimId = reader["CLAIM_ID"]?.ToString(),
                                VisitId = reader["OPVISIT_ID"]?.ToString(),
                                VisitType = reader["VISITTYPE"]?.ToString(),
                                VIPStatus = reader["VIP_STS"]?.ToString(),
                                PatientId = reader["PATI_ID"]?.ToString(),
                                PatientOpNo = reader["PATI_OPNO"]?.ToString(),
                                Observation = reader["OBSERVATION"]?.ToString(),
                                PendingPayment = reader["CONSULTATION_PENDING_PAYMENT"]?.ToString(),
                                PatientFirstName = reader["PATI_FIRST_NAME"]?.ToString(),
                                PatientName = reader["PATIENTNAME"]?.ToString(),
                                DoctorName = reader["DOCT_NAME"]?.ToString(),
                                Gender = reader["GENDER"]?.ToString(),
                                DepartmentId = reader["DEPARTMENT_ID"]?.ToString(),
                                DepartmentName = reader["DEPARTMENT_NAME"]?.ToString(),
                                CaseSheetGender = reader["CASE_SHEET_GENDER"]?.ToString(),
                                VisitStatus = reader["VISIT_STS"]?.ToString(),
                                AllergyDetails = reader["ALLERGY_DTLS"]?.ToString(),
                                Remarks = reader["REMARKS"]?.ToString(),
                                Age = reader["AGE"]?.ToString(),
                                TokenNumber = reader["TOCKEN_NO"]?.ToString(),
                                TokenType = reader["TOKEN_TYPE"]?.ToString(),
                                Address = reader["ADDR"]?.ToString(),
                                Mobile = reader["PATI_MOBILE"]?.ToString(),
                                BookingTokenType = reader["BK_TOKEN_TYPE"]?.ToString(),
                                DetailsId = reader["OPVDTLS_ID"]?.ToString(),
                                NextTokenStatus = reader["NEXT_TOKEN_STS"]?.ToString(),
                                TokenWarning = reader["TOKEN_WAR"]?.ToString(),
                                TokenCallStatus = reader["TOKEN_CALL_STS"]?.ToString(),
                                BookingVisitDate = reader["BK_VISIT_DATE"]?.ToString(),
                                VitalsSaved = reader["VITALS_SAVED"]?.ToString(),
                                GroupStatus = reader["GRP_STATUS"]?.ToString(),
                                PatientTypeId = reader["PATI_TYPE_ID"]?.ToString(),
                                TeleAppointment = reader["IS_TELE_APPOINTMENT"]?.ToString(),
                                ChatSessionId = reader["CHAT_SESSION_ID"]?.ToString(),
                                PatientActive = reader["PATIENT_ACTIVE"]?.ToString(),
                                EndTime = reader["END_TIME"]?.ToString(),
                                InsuranceStatus = reader["INSURANCE"]?.ToString(),
                                VitalsImage = reader["VITALS_IMG"]?.ToString(),
                                LuxuryPackageImage = reader["LUXURY_PKG_IMG"]?.ToString(),
                                ClinicTriageImage = reader["CLINIC_TRIAGE_IMG"]?.ToString(),
                                EmergencyTriageColor = reader["ER_TRIAGE_COLOR"]?.ToString(),
                                NewVisitImage = reader["NEW_VISIT_IMG"]?.ToString()
                            });
                        }
                        return results;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in EMR_SP_PATI_LIST_DATEWISE: {ex.Message}");
            }
        }




        public async Task<Dictionary<string, List<dynamic>>> GetPatientDetailsAsyncssss(string patiId, string pEdocId = null, string edocId = null)
        {
            try
            {
                var sql = "BEGIN UCHEMR.SP_ONLINE_PATI_DTLS(:PATIID, :P_EDOCID, :EDOCID, :STROUT1, :STROUT2, :STROUT3, :STROUT4, :STROUT5, :STROUT6, :STROUT7, :STROUT8, :STROUT9, :STROUT10, :STROUT11, :STROUT12, :STROUT13, :STROUT14); END;";

                var connection = _DbContext.Database.GetDbConnection();
                await using var command = connection.CreateCommand();

                command.CommandText = sql;
                command.CommandType = CommandType.Text;

                // Input parameters
                command.Parameters.Add(new OracleParameter("PATIID", OracleDbType.Varchar2) { Value = patiId });
                command.Parameters.Add(new OracleParameter("P_EDOCID", OracleDbType.Varchar2) { Value = pEdocId ?? (object)DBNull.Value });
                command.Parameters.Add(new OracleParameter("EDOCID", OracleDbType.Varchar2) { Value = edocId ?? (object)DBNull.Value });

                // Output parameters (REF CURSORS)
                var outputCursors = new List<OracleParameter>();
                for (int i = 1; i <= 14; i++)
                {
                    var param = new OracleParameter($"STROUT{i}", OracleDbType.RefCursor)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(param);
                    outputCursors.Add(param);
                }

                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                // Process each REF CURSOR
                var results = new Dictionary<string, List<dynamic>>();
                foreach (var cursorParam in outputCursors)
                {
                    var cursorResults = new List<dynamic>();

                    await using var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader();
                    while (await reader.ReadAsync())
                    {
                        var record = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            record[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        }
                        cursorResults.Add(record);
                    }

                    results[cursorParam.ParameterName] = cursorResults;
                }

                return results;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in SP_ONLINE_PATI_DTLS: {ex.Message}", ex);
            }
            finally
            {
                if (_DbContext.Database.GetDbConnection().State == ConnectionState.Open)
                    await _DbContext.Database.GetDbConnection().CloseAsync();
            }
        }




        public async Task<dynamic> GetMenuDetails(UserTocken ut)
        {
            try
            {

                var result = (from emp in _DbContext.HRM_EMPLOYEE
                              join em in _DbContext.WEB_MENU_GROUP_DTLS on emp.WEB_MENU_GROUP_ID equals em.WEB_MENU_GROUP_ID
                              join mg in _DbContext.WEB_MENU_GROUP on emp.WEB_MENU_GROUP_ID equals mg.WEB_MENU_GROUP_ID
                              join tvl in _DbContext.EMR_IP_TABS_VIEW_LINK on em.TAB_ID equals tvl.TAB_ID
                              where emp.EMP_ID == ut.AUSR_ID
                                    && mg.ACTIVE_STATUS == 'A'
                                    && tvl.ACTIVE_STATUS == 'A'
                                    && em.MODULE_ID == 48
                                    && tvl.COM_TYPE == 0
                              orderby tvl.TAB_NAME ascending
                              select new
                              {
                                  emp.EMP_ID,
                                  em.MODULE_ID,
                                  tvl.TAB_ID,
                                  tvl.TAB_NAME,
                                  tvl.LINK,
                                  tvl.TAB_ORDER,
                                  tvl.ADMIN_LINK_ID
                              }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                var msg1 = new DefaultMessage.Message1
                {
                    Status = 500,
                    Message = ex.Message
                };
                return msg1;
            }
        }
        public async Task<dynamic> GetInitialEvalDetails(string emrDocId)
        {
            try
            {
                // Corrected SQL query
                var sql = @"
           Select E.PATI_ID,E.EMR_DOC_ID,E.PAIN_SCREENING,E.
			PAIN_ASSESSMENT_NUM,E.PAIN_ASSESSMENT_FACE,E.LOCATION,E.DURATION,E.FREQUENCY,E.FREQUENCY_OTHER,E.
			CHARACTER,E.CHARACTER_OTHER,E.INCREASE_WITH,E.DECREASE_WITH,E.GAIT,E.ABNORMAL_GAIT,E.SPECIAL_TESTS,E.
			RANGE_NECK,E.RANGE_RIGHT_UE,E.RANGE_LEFT_UE,E.RANGE_RIGHT_LE,E.RANGE_LEFT_LE,E.RANGE_LIMITATIONS,E.
			MUSCLE_NECK,E.MUSCLE_RIGHT_UE,E.MUSCLE_LEFT_UE,E.MUSCLE_RIGHT_LE,E.MUSCLE_LEFT_LE,E.MUSCLE_LIMITATIONS,E.
			CREATED_USER,E.MUSCLE_MMT,E.FIM_SCORE,E.BALANCE_SCALE,E.COORDINATION,E.OTHER_MEASURES,E.OTHER_GAIT,E.
			CREATED_DATE,E.PRESENT_HISTORY,E.PAST_HISTORY,E.SOCIAL_HISTORY,E.DIABETES_MELLITUS,E.CHOLESTROL,E.
			HYPERTENSION,E.CARDIC,E.CANCER,E.RESPIRATORY,E.ANY_METAL_IMPLANT,E.OSTEOPOROSIS,E.MEDICAL_HISTORY_OTHERS,E.
			MH_OTHER_REMARKS,E.CURRENT_MEDICATIONS,E.DIAGNOSIS,E.FUNCTIONAL_STATUS,E.AIMS_TERM,E.AIMS_TERM_REMARKS,E.AIMS_HEP_REMARKS,E.AIMS_EROGONOMICS,E.
			AIMS_INSTRUCTIONS,E.AIMS_FOLLOWUP,E.AIMS_EVALUATIN_DATE,E.CURRENT_OCCUPATION,E.CONSULTANT,E.CHIEF_COMPLAINTS,E.PHYSIO,E.PHYSIO_INITIAL,
			A.PARASTHESIA,A.SENSATION,A.PROPRIOCEPTION,A.TONE,A.REFLEXES,A.OTHER,A.OTHER_REMARKS,A.NO_ABNORMALITY,A.LEG_LENGTH_DECREPENCY,A.KYPHOSIS,A.
			 SCOLIOSIS,A.LORDOSIS,A.GENUVARUM,A.PROTRUDED_SHOULDER,A.FORWARD_HEAD,A.OTHER_POSTURE
			from  UCHEMR.EMR_PHISIOTHERAPY_EVALUATION E INNER JOIN UCHEMR.PHISIOTHERAPY_ASSESSMENT A ON E.EMR_DOC_ID = A.EMR_DOC_ID
			 WHERE E.EMR_DOC_ID = :emrDocId";

                // Define the Oracle parameter
                var parameters = new List<OracleParameter>
        {
            new OracleParameter("emrDocId", OracleDbType.Varchar2) { Value = emrDocId }
        };

                // Execute the query
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddRange(parameters.ToArray());

                    // Open the connection if not already open
                    if (cmd.Connection.State != ConnectionState.Open)
                        await cmd.Connection.OpenAsync();

                    // Execute the query and read results
                    using (var reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        var results = new List<dynamic>();

                        while (await reader.ReadAsync())
                        {
                            var row = new ExpandoObject() as IDictionary<string, object>;
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            }
                            results.Add(row);
                        }

                        return results;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle errors
                throw new Exception($"Error executing query: {ex.Message}");
            }
        }
        public async Task<dynamic> SaveInitialEvaluationAsync(InitialEvaluation dentalAssessment, UserTocken ut)
        {
            try
            {
                // Array of SQL statements and corresponding parameters
                var sqlStatements = new List<string>
            {
                @"BEGIN UCHEMR.PHISIOTHERAPY_EVALUATION_SAVE(
					:P_PATI_ID,:P_EMR_DOC_ID,:P_PAIN_SCREENING,:P_PAIN_ASSESSMENT_NUM,:P_PAIN_ASSESSMENT_FACE,
					:P_LOCATION,:P_DURATION,:P_FREQUENCY,:P_FREQUENCY_OTHER,:P_CHARACTER,:P_CHARACTER_OTHER,
					:P_INCREASE_WITH,:P_DECREASE_WITH,:P_GAIT,:P_ABNORMAL_GAIT,:P_SPECIAL_TESTS,:P_RANGE_NECK,
					:P_RANGE_RIGHT_UE,:P_RANGE_LEFT_UE,:P_RANGE_RIGHT_LE,:P_RANGE_LEFT_LE,:P_RANGE_LIMITATIONS,
					:P_MUSCLE_NECK,:P_MUSCLE_RIGHT_UE,:P_MUSCLE_LEFT_UE,:P_MUSCLE_RIGHT_LE,:P_MUSCLE_LEFT_LE,
					:P_MUSCLE_LIMITATIONS,:P_CREATED_USER,:P_PRESENT_HISTORY,:P_PAST_HISTORY,:P_SOCIAL_HISTORY,
					:P_DIABETES_MELLITUS,:P_CHOLESTROL,:P_HYPERTENSION,:P_CARDIC,:P_CANCER,:P_RESPIRATORY,
					:P_ANY_METAL_IMPLANT,:P_OSTEOPOROSIS,:P_MEDICAL_HISTORY_OTHERS,:P_MH_OTHER_REMARKS,
					:P_CURRENT_MEDICATIONS,:P_DIAGNOSIS,:P_FUNCTIONAL_STATUS,:P_AIMS_TERM,:P_AIMS_TERM_REMARKS,
					:P_AIMS_HEP_REMARKS,:P_AIMS_EROGONOMICS,:P_AIMS_INSTRUCTIONS,:P_AIMS_FOLLOWUP,:P_AIMS_EVALUATIN_DATE,
					:P_CURRENT_OCCUPATION,:P_CONSULTANT,:P_CHIEF_COMPLAINTS,:P_MUSCLE_MMT,:P_FIM_SCORE,
					:P_BALANCE_SCALE,:P_COORDINATION,:P_OTHER_MEASURES,:P_OTHER_GAIT,:P_PHYSIO,:P_PHYSIO_INITIAL,:RETVAL	
					); END;",

                @"BEGIN UCHEMR.PHISIOTHERAPY_ASSESSMENT_SAVE(
					:P_PATI_ID,:P_EMR_DOC_ID,:P_PARASTHESIA,:P_SENSATION,:P_PROPRIOCEPTION,
					:P_TONE,:P_REFLEXES,:P_OTHER,:P_OTHER_REMARKS,:P_NO_ABNORMALITY,
					:P_LEG_LENGTH_DECREPENCY,:P_KYPHOSIS,:P_SCOLIOSIS,:P_LORDOSIS,
					:P_GENUVARUM,:P_PROTRUDED_SHOULDER,:P_FORWARD_HEAD,:P_CREATED_USER,
					:P_OTHER_POSTURE,:RETVAL
				); END;",
            
				// Add other SQL statements here
			};

                var parametersList = new List<List<OracleParameter>>
        {

			//new OracleParameter("P_TREATMENT_PLAN", OracleDbType.Varchar2) { Value = ophthalmologyParameters.TreatmentPlan  ?? (object)DBNull.Value},
			new List<OracleParameter>
            {
				//:P_PATI_ID,:P_EMR_DOC_ID,:P_PAIN_SCREENING,:P_PAIN_ASSESSMENT_NUM,:P_PAIN_ASSESSMENT_FACE,
				new OracleParameter("P_PATI_ID", OracleDbType.Varchar2) { Value = dentalAssessment.P_PATI_ID ??(object) DBNull.Value },
                new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = dentalAssessment.P_EMR_DOC_ID ??(object) DBNull.Value },
                new OracleParameter("P_PAIN_SCREENING", OracleDbType.Varchar2) { Value = dentalAssessment.P_PAIN_SCREENING ?? (object) DBNull.Value },
                new OracleParameter("P_PAIN_ASSESSMENT_NUM", OracleDbType.Varchar2) { Value = dentalAssessment.P_PAIN_ASSESSMENT_NUM ?? (object)DBNull.Value },
                new OracleParameter("P_PAIN_ASSESSMENT_FACE", OracleDbType.Varchar2) { Value = dentalAssessment.P_PAIN_ASSESSMENT_FACE ?? (object) DBNull.Value },
				//:P_LOCATION,:P_DURATION,:P_FREQUENCY,:P_FREQUENCY_OTHER,:P_CHARACTER,:P_CHARACTER_OTHER,
				new OracleParameter("P_LOCATION", OracleDbType.Varchar2) { Value = dentalAssessment.P_LOCATION ?? (object) DBNull.Value },
                new OracleParameter("P_DURATION", OracleDbType.Varchar2) { Value = dentalAssessment.P_DURATION ?? (object) DBNull.Value },
                new OracleParameter("P_FREQUENCY", OracleDbType.Varchar2) { Value = dentalAssessment.P_FREQUENCY ?? (object) DBNull.Value },
                new OracleParameter("P_FREQUENCY_OTHER", OracleDbType.Varchar2) { Value = dentalAssessment.P_FREQUENCY_OTHER ?? (object) DBNull.Value },
                new OracleParameter("P_CHARACTER", OracleDbType.Varchar2) { Value = dentalAssessment.P_CHARACTER ?? (object) DBNull.Value },
                new OracleParameter("P_CHARACTER_OTHER", OracleDbType.Varchar2) { Value = dentalAssessment.P_CHARACTER_OTHER ?? (object) DBNull.Value },
				//:P_INCREASE_WITH,:P_DECREASE_WITH,:P_GAIT,:P_ABNORMAL_GAIT,:P_SPECIAL_TESTS,:P_RANGE_NECK,
				new OracleParameter("P_INCREASE_WITH", OracleDbType.Varchar2) { Value = dentalAssessment.P_INCREASE_WITH ?? (object) DBNull.Value },
                new OracleParameter("P_DECREASE_WITH", OracleDbType.Varchar2) { Value = dentalAssessment.P_DECREASE_WITH ?? (object) DBNull.Value },
                new OracleParameter("P_GAIT", OracleDbType.Varchar2) { Value = dentalAssessment.P_GAIT ?? (object) DBNull.Value },
                new OracleParameter("P_ABNORMAL_GAIT", OracleDbType.Varchar2) { Value = dentalAssessment.P_ABNORMAL_GAIT ?? (object) DBNull.Value },
                new OracleParameter("P_SPECIAL_TESTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_SPECIAL_TESTS ?? (object) DBNull.Value },
                new OracleParameter("P_RANGE_NECK", OracleDbType.Varchar2) { Value = dentalAssessment.P_RANGE_NECK ?? (object) DBNull.Value },
				//:P_RANGE_RIGHT_UE,:P_RANGE_LEFT_UE,:P_RANGE_RIGHT_LE,:P_RANGE_LEFT_LE,:P_RANGE_LIMITATIONS,
				new OracleParameter("P_RANGE_RIGHT_UE", OracleDbType.Varchar2) { Value = dentalAssessment.P_RANGE_RIGHT_UE ?? (object)DBNull.Value },
                new OracleParameter("P_RANGE_LEFT_UE", OracleDbType.Varchar2) { Value = dentalAssessment.P_RANGE_LEFT_UE ?? (object)DBNull.Value },
                new OracleParameter("P_RANGE_RIGHT_LE", OracleDbType.Varchar2) { Value = dentalAssessment.P_RANGE_RIGHT_LE ?? (object)DBNull.Value },
                new OracleParameter("P_RANGE_LEFT_LE", OracleDbType.Varchar2) { Value = dentalAssessment.P_RANGE_LEFT_LE ?? (object)DBNull.Value },
                new OracleParameter("P_RANGE_LIMITATIONS", OracleDbType.Varchar2) { Value = dentalAssessment.P_RANGE_LIMITATIONS ?? (object)DBNull.Value },
				//:P_MUSCLE_NECK,:P_MUSCLE_RIGHT_UE,:P_MUSCLE_LEFT_UE,:P_MUSCLE_RIGHT_LE,:P_MUSCLE_LEFT_LE,
				new OracleParameter("P_MUSCLE_NECK", OracleDbType.Varchar2) { Value = dentalAssessment.P_MUSCLE_NECK ?? (object)DBNull.Value },
                new OracleParameter("P_MUSCLE_RIGHT_UE", OracleDbType.Varchar2) { Value = dentalAssessment.P_MUSCLE_RIGHT_UE ?? (object)DBNull.Value },
                new OracleParameter("P_MUSCLE_LEFT_UE", OracleDbType.Varchar2) { Value = dentalAssessment.P_MUSCLE_LEFT_UE ?? (object)DBNull.Value },
                new OracleParameter("P_MUSCLE_RIGHT_LE", OracleDbType.Varchar2) { Value = dentalAssessment.P_MUSCLE_RIGHT_LE ?? (object)DBNull.Value },
                new OracleParameter("P_MUSCLE_LEFT_LE", OracleDbType.Varchar2) { Value = dentalAssessment.P_MUSCLE_LEFT_LE ?? (object)DBNull.Value },
				//:P_MUSCLE_LIMITATIONS,:P_CREATED_USER,:P_PRESENT_HISTORY,:P_PAST_HISTORY,:P_SOCIAL_HISTORY,
				new OracleParameter("P_MUSCLE_LIMITATIONS", OracleDbType.Varchar2) { Value = dentalAssessment.P_MUSCLE_LIMITATIONS ?? (object) DBNull.Value },
                new OracleParameter("P_CREATED_USER", OracleDbType.Varchar2) { Value = ut.AUSR_ID ?? (object) DBNull.Value },
                new OracleParameter("P_PRESENT_HISTORY", OracleDbType.Varchar2) { Value = dentalAssessment.P_PRESENT_HISTORY ?? (object)DBNull.Value },
                new OracleParameter("P_PAST_HISTORY", OracleDbType.Varchar2) { Value = dentalAssessment.P_PAST_HISTORY ?? (object)DBNull.Value },
                new OracleParameter("P_SOCIAL_HISTORY", OracleDbType.Varchar2) { Value = dentalAssessment.P_SOCIAL_HISTORY ?? (object)DBNull.Value },
				//:P_DIABETES_MELLITUS,:P_CHOLESTROL,:P_HYPERTENSION,:P_CARDIC,:P_CANCER,:P_RESPIRATORY,
				new OracleParameter("P_DIABETES_MELLITUS", OracleDbType.Char) { Value = dentalAssessment.P_DIABETES_MELLITUS ?? (object)DBNull.Value },
                new OracleParameter("P_CHOLESTROL", OracleDbType.Char) { Value = dentalAssessment.P_CHOLESTROL ?? (object)DBNull.Value },
                new OracleParameter("P_HYPERTENSION", OracleDbType.Char) { Value = dentalAssessment.P_HYPERTENSION ?? (object)DBNull.Value },
                new OracleParameter("P_CARDIC", OracleDbType.Char) { Value = dentalAssessment.P_CARDIC ?? (object)DBNull.Value },
                new OracleParameter("P_CANCER", OracleDbType.Char) { Value = dentalAssessment.P_CANCER ?? (object)DBNull.Value },
                new OracleParameter("P_RESPIRATORY", OracleDbType.Char) { Value = dentalAssessment.P_RESPIRATORY ?? (object)DBNull.Value },
				//:P_ANY_METAL_IMPLANT,:P_OSTEOPOROSIS,:P_MEDICAL_HISTORY_OTHERS,:P_MH_OTHER_REMARKS,
				new OracleParameter("P_ANY_METAL_IMPLANT", OracleDbType.Char) { Value = dentalAssessment.P_ANY_METAL_IMPLANT ?? (object)DBNull.Value },
                new OracleParameter("P_OSTEOPOROSIS", OracleDbType.Char) { Value = dentalAssessment.P_OSTEOPOROSIS ?? (object)DBNull.Value },
                new OracleParameter("P_MEDICAL_HISTORY_OTHERS", OracleDbType.Char) { Value = dentalAssessment.P_MEDICAL_HISTORY_OTHERS ?? (object)DBNull.Value },
                new OracleParameter("P_MH_OTHER_REMARKS", OracleDbType.Varchar2) { Value = dentalAssessment.P_MH_OTHER_REMARKS ?? (object)DBNull.Value },
				//:P_CURRENT_MEDICATIONS,:P_DIAGNOSIS,:P_FUNCTIONAL_STATUS,:P_AIMS_TERM,:P_AIMS_TERM_REMARKS,
				new OracleParameter("P_CURRENT_MEDICATIONS", OracleDbType.Varchar2) { Value = dentalAssessment.P_CURRENT_MEDICATIONS ?? (object)DBNull.Value },
                new OracleParameter("P_DIAGNOSIS", OracleDbType.Varchar2) { Value = dentalAssessment.P_DIAGNOSIS ?? (object)DBNull.Value },
                new OracleParameter("P_FUNCTIONAL_STATUS", OracleDbType.Varchar2) { Value = dentalAssessment.P_FUNCTIONAL_STATUS ?? (object)DBNull.Value },
                new OracleParameter("P_AIMS_TERM", OracleDbType.Varchar2) { Value = dentalAssessment.P_AIMS_TERM ?? (object)DBNull.Value },
                new OracleParameter("P_AIMS_TERM_REMARKS", OracleDbType.Varchar2) { Value = dentalAssessment.P_AIMS_TERM_REMARKS ?? (object)DBNull.Value },
				//:P_AIMS_HEP_REMARKS,:P_AIMS_EROGONOMICS,:P_AIMS_INSTRUCTIONS,:P_AIMS_FOLLOWUP,:P_AIMS_EVALUATIN_DATE,
				new OracleParameter("P_AIMS_HEP_REMARKS", OracleDbType.Varchar2) { Value = dentalAssessment.P_AIMS_HEP_REMARKS ?? (object)DBNull.Value },
                new OracleParameter("P_AIMS_EROGONOMICS", OracleDbType.Varchar2) { Value = dentalAssessment.P_AIMS_EROGONOMICS ?? (object)DBNull.Value },
                new OracleParameter("P_AIMS_INSTRUCTIONS", OracleDbType.Varchar2) { Value = dentalAssessment.P_AIMS_INSTRUCTIONS ?? (object)DBNull.Value },
                new OracleParameter("P_AIMS_FOLLOWUP", OracleDbType.Varchar2) { Value = dentalAssessment.P_AIMS_FOLLOWUP ?? (object)DBNull.Value },
                new OracleParameter("P_AIMS_EVALUATIN_DATE", OracleDbType.Varchar2) { Value = dentalAssessment.P_AIMS_EVALUATIN_DATE ?? (object)DBNull.Value },
				//:P_CURRENT_OCCUPATION,:P_CONSULTANT,:P_CHIEF_COMPLAINTS,:P_MUSCLE_MMT,:P_FIM_SCORE,
				new OracleParameter("P_CURRENT_OCCUPATION", OracleDbType.Varchar2) { Value = dentalAssessment.P_CURRENT_OCCUPATION ?? (object)DBNull.Value },
                new OracleParameter("P_CONSULTANT", OracleDbType.Varchar2) { Value = dentalAssessment.P_CONSULTANT ?? (object)DBNull.Value },
                new OracleParameter("P_CHIEF_COMPLAINTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_CHIEF_COMPLAINTS ?? (object)DBNull.Value },
                new OracleParameter("P_MUSCLE_MMT", OracleDbType.Varchar2) { Value = dentalAssessment.P_MUSCLE_MMT ?? (object)DBNull.Value },
                new OracleParameter("P_FIM_SCORE", OracleDbType.Varchar2) { Value = dentalAssessment.P_FIM_SCORE ?? (object)DBNull.Value },
				//:P_BALANCE_SCALE,:P_COORDINATION,:P_OTHER_MEASURES,:P_OTHER_GAIT,:P_PHYSIO,:P_PHYSIO_INITIAL,:RETVAL
				new OracleParameter("P_BALANCE_SCALE", OracleDbType.Varchar2) { Value = dentalAssessment.P_BALANCE_SCALE ?? (object)DBNull.Value },
                new OracleParameter("P_COORDINATION", OracleDbType.Varchar2) { Value = dentalAssessment.P_COORDINATION ?? (object)DBNull.Value },
                new OracleParameter("P_OTHER_MEASURES", OracleDbType.Varchar2) { Value = dentalAssessment.P_OTHER_MEASURES ?? (object)DBNull.Value },
                new OracleParameter("P_OTHER_GAIT", OracleDbType.Varchar2) { Value = dentalAssessment.P_OTHER_GAIT ?? (object)DBNull.Value },
                new OracleParameter("P_PHYSIO", OracleDbType.Varchar2) { Value = dentalAssessment.P_PHYSIO ?? (object)DBNull.Value },
                new OracleParameter("P_PHYSIO_INITIAL", OracleDbType.Varchar2) { Value = dentalAssessment.P_PHYSIO_INITIAL ?? (object)DBNull.Value },
				 //new OracleParameter("RETVAL", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }

					  new OracleParameter("RETVAL", OracleDbType.Varchar2) { Direction = ParameterDirection.Output }

            },

            new List<OracleParameter>
            {
                    new OracleParameter("P_PATI_ID", OracleDbType.Varchar2) { Value = dentalAssessment.P_PATI_ID ??(object) DBNull.Value },
                new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = dentalAssessment.P_EMR_DOC_ID ??(object) DBNull.Value },
                new OracleParameter("P_PARASTHESIA", OracleDbType.Char) { Value = dentalAssessment.P_PARASTHESIA ?? (object) DBNull.Value },
                new OracleParameter("P_SENSATION", OracleDbType.Char) { Value = dentalAssessment.P_SENSATION ?? (object) DBNull.Value },
                new OracleParameter("P_PROPRIOCEPTION", OracleDbType.Char) { Value = dentalAssessment.P_PROPRIOCEPTION ?? (object) DBNull.Value },
                new OracleParameter("P_TONE", OracleDbType.Char) { Value = dentalAssessment.P_TONE ?? (object) DBNull.Value },
                new OracleParameter("P_REFLEXES", OracleDbType.Char) { Value = dentalAssessment.P_REFLEXES ?? (object) DBNull.Value },
                new OracleParameter("P_OTHER", OracleDbType.Char) { Value = dentalAssessment.P_OTHER ?? (object) DBNull.Value },
                new OracleParameter("P_OTHER_REMARKS", OracleDbType.Varchar2) { Value = dentalAssessment.P_OTHER_REMARKS ?? (object) DBNull.Value },

                new OracleParameter("P_NO_ABNORMALITY", OracleDbType.Char) { Value = dentalAssessment.P_NO_ABNORMALITY ?? (object)DBNull.Value },
                new OracleParameter("P_LEG_LENGTH_DECREPENCY", OracleDbType.Char) { Value = dentalAssessment.P_LEG_LENGTH_DECREPENCY ?? (object)DBNull.Value },
                new OracleParameter("P_KYPHOSIS", OracleDbType.Char) { Value = dentalAssessment.P_KYPHOSIS ?? (object)DBNull.Value },
                new OracleParameter("P_SCOLIOSIS", OracleDbType.Char) { Value = dentalAssessment.P_SCOLIOSIS ?? (object)DBNull.Value },
                new OracleParameter("P_LORDOSIS", OracleDbType.Char) { Value = dentalAssessment.P_LORDOSIS ?? (object)DBNull.Value },
                new OracleParameter("P_GENUVARUM", OracleDbType.Char) { Value = dentalAssessment.P_GENUVARUM ?? (object)DBNull.Value },
                new OracleParameter("P_PROTRUDED_SHOULDER", OracleDbType.Char) { Value = dentalAssessment.P_PROTRUDED_SHOULDER ?? (object)DBNull.Value },

                new OracleParameter("P_FORWARD_HEAD", OracleDbType.Char) { Value = dentalAssessment.P_FORWARD_HEAD ?? (object)DBNull.Value },
                new OracleParameter("P_CREATED_USER", OracleDbType.Varchar2) { Value = ut.AUSR_ID ?? (object)DBNull.Value },
                new OracleParameter("P_OTHER_POSTURE", OracleDbType.Varchar2) { Value = dentalAssessment.P_OTHER_POSTURE ?? (object)DBNull.Value },
                new OracleParameter("RETVAL", OracleDbType.Varchar2) { Direction = ParameterDirection.Output }


            },
		
            
            // Add other parameter lists here
        };

                using (var connection = _DbContext.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    for (int i = 0; i < sqlStatements.Count; i++)
                    {
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = sqlStatements[i];
                            cmd.CommandType = CommandType.Text;

                            // Add parameters individually
                            foreach (var param in parametersList[i])
                            {
                                cmd.Parameters.Add(param);
                            }

                            try
                            {
                                await cmd.ExecuteNonQueryAsync();
                            }
                            catch (Exception innerEx)
                            {
                                return new { Status = 500, Message = $"Error in statement {i + 1}: {innerEx.Message}" };
                            }
                        }
                    }
                }
                //Message = "ORA-12899: value too large for column \"UCHEMR\".\"EMR_PHISIOTHERAPY_EVALUATION\".\"PHYSIO\" (actual: 3, maximum: 1)\nORA-06512: at \"UCHEMR.PHISIOTHERAPY_EVALUATION_SAVE\", line 75\nORA-06512: at line 1\r\nhttps://docs.oracle.com/error-help/db/ora-12899/"

                return new { Status = 200, Message = "Save successful." };
            }
            catch (ObjectDisposedException ex)
            {
                return new { Status = 500, Message = $"Connection error: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new { Status = 500, Message = $"General error: {ex.Message}" };
            }


        }

        public async Task<dynamic> SaveProgressNoteAsync(Progressnote dentalAssessment, UserTocken ut)
        {

            string stt = "";
            string edt = "";
            string stdate = "";
            string eddate = "";

            try
            {
                // Array of SQL statements and corresponding parameters

                var sqlStatements = new List<string>
    {
        @"BEGIN UCHEMR.EMR_PHISIOTHERAPY_DTLS_SAVE(
			:P_PHI_EVA_DTLS_ID,:P_EMR_DOC_ID,:P_PATI_ID,:P_PHYSIOTHERAPIST,:P_SESSION_NO,:P_START_DATE,
			:P_ADVISED_SESSION,:P_CURRENT_SESSION,:P_VISIT_DATE,:P_START_TIME,:P_END_TIME,:P_TREATMENT_CODE,
			:P_PATIENT_CONDITION,:P_PATIENT_MANAGEMENT,
			:P_ADVERSE_EFFECT,:P_ADVERSE_EFFECT_COMMENTS,:P_RESPONSE_TREATMENT,
			:P_RESPONSE_TREATMENT_OTHER,:P_INCIDENT,:P_INCIDENT_OTHER,:P_NOTES,:P_TYPE,:P_PATI_SAFETY_ENSURE,
			:RETVAL	
			); END;",
							            
		// Add other SQL statements here
	};


                string dateString = dentalAssessment.P_START_DATE; // yyyy/MM/dd format
                DateTime date = DateTime.ParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                // Convert to dd/MM/yyyy format
                string formattedDate = date.ToString("dd/MM/yyyy");
                Console.WriteLine(formattedDate);

                string dateString1 = dentalAssessment.P_VISIT_DATE; // yyyy/MM/dd format
                DateTime date1 = DateTime.ParseExact(dateString1, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                // Convert to dd/MM/yyyy format
                string visitDate = date1.ToString("dd/MM/yyyy");
                Console.WriteLine(visitDate);


                string dateString2 = dentalAssessment.P_START_TIME; // Example: "21/01/2025 18:51:09"

                // Parse the date-time string using the exact format of the input
                //DateTime date2 = DateTime.ParseExact(dateString2, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                //// Convert to the desired 12-hour format with AM/PM
                string starttime = dateString2;
                //sttime = starttime;
                stt = dateString2;

                // Use `starttime` as needed
                Console.WriteLine(starttime);








                //string dateString3 = dentalAssessment.P_END_TIME; // yyyy/MM/dd format
                //            string currentDate1 = DateTime.Now.ToString("yyyy-MM-dd");
                //if (!dateString3.Contains(":"))
                //{
                //	dateString3 += ":00"; // Append ":00" if the time is in "HH:mm" format
                //}
                //string dateTimeString1 = currentDate1 + " " + dateString3;
                //DateTime date3 = DateTime.ParseExact(dateTimeString1, "yyyy-MM-dd HH:mm:ss a", CultureInfo.InvariantCulture);
                //            string endtime = date3.ToString("dd/MM/yyyy hh:mm:ss a", CultureInfo.InvariantCulture);
                //            Console.WriteLine(endtime);



                string dateString3 = dentalAssessment.P_END_TIME; // Input: "13:11:11"
                                                                  //DateTime date22 = DateTime.ParseExact(dateString2, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                // Convert to the desired 12-hour format with AM/PM

                // Convert endtime the desired 12-hour format with AM/PM
                string endtime = dateString3;

                edtime = endtime;

                // Use `starttime` as needed
                Console.WriteLine(endtime);



                var parametersList = new List<List<OracleParameter>>
        {

        new List<OracleParameter>
        {

        new OracleParameter("P_PHI_EVA_DTLS_ID", OracleDbType.Varchar2) { Value = dentalAssessment.P_PHI_EVA_DTLS_ID ?? (object) DBNull.Value },
        new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = dentalAssessment.P_EMR_DOC_ID ??(object) DBNull.Value },
        new OracleParameter("P_PATI_ID", OracleDbType.Varchar2) { Value = dentalAssessment.P_PATI_ID ??(object) DBNull.Value },
        new OracleParameter("P_PHYSIOTHERAPIST", OracleDbType.Varchar2) { Value = ut.AUSR_ID ?? (object)DBNull.Value },
		//new OracleParameter("P_CREATED_USER", OracleDbType.Varchar2) { Value = ut.AUSR_ID ?? (object) DBNull.Value },
		new OracleParameter("P_SESSION_NO", OracleDbType.Int32) { Value = dentalAssessment.P_SESSION_NO ?? (object) DBNull.Value },

		//new OracleParameter("P_START_DATE", OracleDbType.Varchar2) { Value = dentalAssessment.P_START_DATE ?? (object) DBNull.Value },
		//new OracleParameter("P_START_DATE", OracleDbType.Varchar2) { Value = dentalAssessment.P_START_DATE.ToString('dd/MM/yyyy' },
		new OracleParameter("P_START_DATE", OracleDbType.Varchar2) { Value = formattedDate ?? (object)DBNull.Value },
			//new OracleParameter("P_START_DATE", OracleDbType.Varchar2) { Value = "12/10/2024" },
		//:P_ADVISED_SESSION,:P_CURRENT_SESSION,:P_VISIT_DATE,:P_START_TIME,:P_END_TIME,:P_TREATMENT_CODE,
		new OracleParameter("P_ADVISED_SESSION", OracleDbType.Int32) { Value = dentalAssessment.P_ADVISED_SESSION ?? (object) DBNull.Value },
        new OracleParameter("P_CURRENT_SESSION", OracleDbType.Int32) { Value = dentalAssessment.P_CURRENT_SESSION ?? (object) DBNull.Value },

        //new OracleParameter("P_VISIT_DATE", OracleDbType.Date) { Value = dentalAssessment.P_VISIT_DATE ?? (object) DBNull.Value },
        //new OracleParameter("P_START_TIME", OracleDbType.Date) { Value = dentalAssessment.P_START_TIME ?? (object) DBNull.Value },
        //new OracleParameter("P_END_TIME", OracleDbType.Date) { Value = dentalAssessment.P_END_TIME ?? (object) DBNull.Value },
   //     new OracleParameter("P_VISIT_DATE", OracleDbType.Varchar2) { Value = dentalAssessment.P_VISIT_DATE ?? (object) DBNull.Value },
		 //new OracleParameter("P_START_TIME", OracleDbType.Varchar2) { Value = dentalAssessment.P_START_TIME ?? (object) DBNull.Value },
		 // new OracleParameter("P_END_TIME", OracleDbType.Varchar2) { Value = dentalAssessment.P_END_TIME ?? (object) DBNull.Value },
			new OracleParameter("P_VISIT_DATE", OracleDbType.Varchar2) { Value = visitDate ?? (object) DBNull.Value },
        new OracleParameter("P_START_TIME", OracleDbType.Varchar2) { Value = starttime ?? (object) DBNull.Value },
        new OracleParameter("P_END_TIME", OracleDbType.Varchar2) { Value = endtime ?? (object) DBNull.Value },
  //         new OracleParameter("P_START_TIME", OracleDbType.Varchar2) { Value ="21/01/2025 07:27:12 PM"},
		//new OracleParameter("P_END_TIME", OracleDbType.Varchar2) { Value = "21/01/2025 08:27:12 PM" },
        //new OracleParameter("P_VISIT_DATE", OracleDbType.Varchar2) { Value = "12/10/2024" },
		//new OracleParameter("P_START_TIME", OracleDbType.Varchar2) { Value = "12/10/2024 09:04:04 PM" },
		//new OracleParameter("P_END_TIME", OracleDbType.Varchar2) { Value = "12/10/2024 10:45:04 PM" },

		new OracleParameter("P_TREATMENT_CODE", OracleDbType.Varchar2) { Value = dentalAssessment.P_TREATMENT_CODE ?? (object) DBNull.Value },
        new OracleParameter("P_PATIENT_CONDITION", OracleDbType.Varchar2) { Value = dentalAssessment.P_PATIENT_CONDITION ?? (object) DBNull.Value },
        new OracleParameter("P_PATIENT_MANAGEMENT", OracleDbType.Varchar2) { Value = dentalAssessment.P_PATIENT_MANAGEMENT ?? (object) DBNull.Value },
        new OracleParameter("P_ADVERSE_EFFECT", OracleDbType.Varchar2) { Value = dentalAssessment.P_ADVERSE_EFFECT ?? (object) DBNull.Value },
        new OracleParameter("P_ADVERSE_EFFECT_COMMENTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_ADVERSE_EFFECT_COMMENTS ?? (object) DBNull.Value },
        new OracleParameter("P_RESPONSE_TREATMENT", OracleDbType.Varchar2) { Value = dentalAssessment.P_RESPONSE_TREATMENT ?? (object) DBNull.Value },
        new OracleParameter("P_RESPONSE_TREATMENT_OTHER", OracleDbType.Varchar2) { Value = dentalAssessment.P_RESPONSE_TREATMENT_OTHER ?? (object)DBNull.Value },
        new OracleParameter("P_INCIDENT", OracleDbType.Varchar2) { Value = dentalAssessment.P_INCIDENT ?? (object)DBNull.Value },
        new OracleParameter("P_INCIDENT_OTHER", OracleDbType.Varchar2) { Value = dentalAssessment.P_INCIDENT_OTHER ?? (object)DBNull.Value },
        new OracleParameter("P_NOTES", OracleDbType.Varchar2) { Value = dentalAssessment.P_NOTES ?? (object)DBNull.Value },
        new OracleParameter("P_TYPE", OracleDbType.Varchar2) { Value = dentalAssessment.P_TYPE ?? (object)DBNull.Value },
        new OracleParameter("P_PATI_SAFETY_ENSURE", OracleDbType.Varchar2) { Value = dentalAssessment.P_PATI_SAFETY_ENSURE ?? (object) DBNull.Value },

              new OracleParameter("RETVAL", OracleDbType.Varchar2,200) { Direction = ParameterDirection.Output }

    }
    // Add other parameter lists here
};

                using (var connection = _DbContext.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    for (int i = 0; i < sqlStatements.Count; i++)
                    {
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = sqlStatements[i];
                            cmd.CommandType = CommandType.Text;

                            // Add parameters individually
                            foreach (var param in parametersList[i])
                            {
                                cmd.Parameters.Add(param);
                            }

                            try
                            {
                                await cmd.ExecuteNonQueryAsync();

                                var retval = cmd.Parameters["RETVAL"].Value.ToString();
                                var a = new Careplan
                                {
                                    PATI_ID = dentalAssessment.P_PATI_ID,
                                    EMR_DOC_ID = dentalAssessment.P_EMR_DOC_ID,
                                    CREATED_USER = ut.AUSR_ID,
                                    //CREATED_DATE = "",
                                    //EntryDate = "",
                                    Subjective = dentalAssessment.Subjective,
                                    Objective = dentalAssessment.Objective,
                                    Planning = dentalAssessment.Planning,
                                    Assessment = dentalAssessment.Assessment,
                                    PhysioId = Convert.ToInt32(retval)
                                };

                                return await SaveProgressNoteAsync1(a, ut);
                            }
                            catch (OracleException ex)
                            {
                                Console.WriteLine("Oracle error: " + ex.Message);
                            }
                            catch (Exception innerEx)
                            {
                                return new { Status = 500, Message = $"Error in statement {i + 1}: {innerEx.Message}" };
                            }
                        }
                    }
                }
                //Message = "ORA-12899: value too large for column \"UCHEMR\".\"EMR_PHISIOTHERAPY_EVALUATION\".\"PHYSIO\" (actual: 3, maximum: 1)\nORA-06512: at \"UCHEMR.PHISIOTHERAPY_EVALUATION_SAVE\", line 75\nORA-06512: at line 1\r\nhttps://docs.oracle.com/error-help/db/ora-12899/"

                return new { Status = 200, Message = "Save successful." };
            }
            catch (ObjectDisposedException ex)
            {
                return new { Status = 500, Message = $"Connection error: {ex.Message}" };
            }
            catch (Exception ex)
            {
                //string msg = $"General error: {ex.Message}{sttime}{edtime}";
                //return Content($"Status: 500\nMessage: General error: {ex.Message}\nStart Time: {sttime}\nEnd Time: {edtime}");
                return new { Status = 500, Message = $"General error: {ex.Message} st:{stt} ed:{edt} std:{dentalAssessment.P_START_TIME} stdate2:{stdate} eddate22:{eddate}  edd:{dentalAssessment.P_END_TIME}" };
                //return HttpContent($"flag=1\nMessage=General error: {ex.Message}");
                //return Ok(new { Status = true, Counts = Dt1, Data = Dt2, Msg = "PurchaseInward List Selected" });
            }


        }
        public async Task<dynamic> SaveProgressNoteAsync1(Careplan c, UserTocken ut)
        {
            try
            {
                // SQL to call the stored procedure
                var sql = @"
 INSERT INTO UCHTRANS.PHYSIO_NURSING_CARE_PLAN 
 (EMR_DOC_ID, CREATE_USER, CREATE_DATE, ENTRY_DATE, SUBJECTIVE, OBJECTIVE, PLANNING, ASSESSMENT, PHYSIO_ID)
 VALUES 
 (:emrDocId, :CreateUser, TO_DATE(:CreateDate, 'DD/MM/YYYY'), TO_DATE(:EntryDate, 'DD/MM/YYYY'), :Subjective, 
 :Objective, :Planning, :Assessment, :PhysioId)";


                var emrDocId = new OracleParameter("EMR_DOC_ID", OracleDbType.Varchar2) { Value = c.EMR_DOC_ID };
                var CreateUser = new OracleParameter("CREATE_USER", OracleDbType.Varchar2) { Value = ut.AUSR_ID };

                // Use Date type for date parameters
                var CreateDate = new OracleParameter("CREATE_DATE", OracleDbType.Date) { Value = DateTime.UtcNow }; // DateTime
                var EntryDate = new OracleParameter("ENTRY_DATE", OracleDbType.Date) { Value = DateTime.UtcNow };   // DateTime

                var Subjective = new OracleParameter("SUBJECTIVE", OracleDbType.Varchar2) { Value = c.Subjective };
                var Objective = new OracleParameter("OBJECTIVE", OracleDbType.Varchar2) { Value = c.Objective };
                var Planning = new OracleParameter("PLANNING", OracleDbType.Varchar2) { Value = c.Planning };
                var Assessment = new OracleParameter("ASSESSMENT", OracleDbType.Varchar2) { Value = c.Assessment };
                var PhysioId = new OracleParameter("PHYSIO_ID", OracleDbType.Int32) { Value = c.PhysioId };
                //var PhysioId = new OracleParameter("PHYSIO_ID", OracleDbType.Varchar2) { Value = 148 };
                //var UpdateDate = new OracleParameter("UPDATED_DATE", OracleDbType.Date) { Value = DateTime.UtcNow };
                //var Updateby = new OracleParameter("UPDATED_BY", OracleDbType.Varchar2) { Value = ut.AUSR_ID };
                // Execute the procedure
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = System.Data.CommandType.Text;

                    // Add the parameter to the command
                    //cmd.Parameters.Add(EMR_DOC_ID);
                    //cmd.Parameters.Add(CREATE_USER);
                    //cmd.Parameters.Add(CREATED_DATE);
                    cmd.Parameters.Add(emrDocId);
                    cmd.Parameters.Add(CreateUser);
                    cmd.Parameters.Add(CreateDate);
                    cmd.Parameters.Add(EntryDate);
                    cmd.Parameters.Add(Subjective);
                    cmd.Parameters.Add(Objective);
                    cmd.Parameters.Add(Planning);
                    cmd.Parameters.Add(Assessment);
                    //cmd.Parameters.Add(UpdateDate);
                    //cmd.Parameters.Add(Updateby);
                    cmd.Parameters.Add(PhysioId);
                    // Open connection if not already open
                    if (cmd.Connection.State != ConnectionState.Open)
                        await cmd.Connection.OpenAsync();

                    // Execute the procedure
                    await cmd.ExecuteNonQueryAsync();

                    // Return success response
                    return new
                    {
                        Status = 200,
                        Message = " saved successfully."
                    };
                }
            }
            catch (Exception ex)
            {
                return new { Status = 500, Message = $"Error: {ex.Message}" };
            }
        }

        public async Task<dynamic> GetPhysio()
        {
            try
            {
                var result = (from emp in _DbContext.HRM_EMPLOYEE
                              join doc in _DbContext.OPN_DOCTOR on emp.EMP_ID equals doc.EMPLOYEE_ID
                              where doc.OD_CAT_ID == 5
                                    //&& (emp.EMP_ACTIVE_STATUS ?? 'I') == 'A'
                                    && (emp.EMP_ACTIVE_STATUS) == 'A'
                              orderby emp.EMP_OFFL_NAME ascending
                              select new
                              {
                                  emp.EMP_OFFL_NAME,
                                  emp.EMP_ID
                              }).ToList();
                return result;

            }
            catch (Exception ex)
            {
                var msg1 = new DefaultMessage.Message1
                {
                    Status = 500,
                    Message = ex.Message
                };
                return msg1;
            }

        }

        public async Task<int> GetCurrentsession(string patiid, string edocid)
        {
            string connectionString = _DbContext.Database.GetConnectionString();
            string query = @"
    SELECT MAX(CURRENT_SESSION + 1) AS CURRENTSESSION 
    FROM UCHEMR.EMR_PHISIOTHERAPY_EVA_DTLS 
    WHERE EMR_DOC_ID = :emrDocId AND PATI_ID = :patiid";
            //var connectionString = 

            using (var connection = new OracleConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new OracleCommand(query, connection))
                {
                    // Add parameters to avoid SQL injection
                    command.Parameters.Add(new OracleParameter(":edocid", edocid));
                    command.Parameters.Add(new OracleParameter(":patiid", patiid));

                    var result = await command.ExecuteScalarAsync();

                    // Check if the result is null (i.e., no data was found)
                    if (result == DBNull.Value)
                    {
                        return 1; // Return 1 if no result is found
                    }

                    // Return the result as an integer, converting it from object to int
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<dynamic> GetProgressNoteDetails(string emrDocId)
        {
            try
            {
                // Corrected SQL query
                //var sql = @"
                //       SELECT E.PHI_EVA_DTLS_ID,E.EMR_DOC_ID,E.PATI_ID,E.PHYSIOTHERAPIST,E.SESSION_NO,E.START_DATE,E.ADVISED_SESSION,
                // E.CURRENT_SESSION,E.VISIT_DATE,E.START_TIME,E.END_TIME,E.TREATMENT_CODE FROM UCHEMR.EMR_PHISIOTHERAPY_EVA_DTLS E
                // WHERE E.EMR_DOC_ID = :emrDocId";
                var sql = @"
				 SELECT E.PHI_EVA_DTLS_ID,E.EMR_DOC_ID,E.PATI_ID,E.PHYSIOTHERAPIST,E.SESSION_NO,E.START_DATE,E.ADVISED_SESSION,
				 E.CURRENT_SESSION,E.VISIT_DATE,E.START_TIME,E.END_TIME,E.TREATMENT_CODE,P.SUBJECTIVE,P.OBJECTIVE,P.PLANNING,P.ASSESSMENT FROM UCHEMR.EMR_PHISIOTHERAPY_EVA_DTLS E
				 INNER JOIN UCHTRANS.PHYSIO_NURSING_CARE_PLAN P ON P.EMR_DOC_ID=E.EMR_DOC_ID
				 WHERE E.EMR_DOC_ID = :emrDocId";
                // AND  E.PHI_EVA_DTLS_ID=P.PHYSIO_ID
                // Define the Oracle parameter
                var parameters = new List<OracleParameter>
        {
            new OracleParameter("emrDocId", OracleDbType.Varchar2) { Value = emrDocId }
        };

                // Execute the query
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddRange(parameters.ToArray());

                    // Open the connection if not already open
                    if (cmd.Connection.State != ConnectionState.Open)
                        await cmd.Connection.OpenAsync();

                    // Execute the query and read results
                    using (var reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        var results = new List<dynamic>();

                        while (await reader.ReadAsync())
                        {
                            var row = new ExpandoObject() as IDictionary<string, object>;
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            }
                            results.Add(row);
                        }

                        return results;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle errors
                throw new Exception($"Error executing query: {ex.Message}");
            }
        }



    }

}

