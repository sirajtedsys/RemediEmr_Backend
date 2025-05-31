using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using RemediEmr.Class;
using RemediEmr.Data.Class;
using RemediEmr.Data.DbModel;
using System.Data;
using System.Dynamic;
using static JwtService;
//using System.Data;

namespace RemediEmr.Repositry
{
    public class DietCommonRepository
    {
        private readonly AppDbContext _DbContext;

        private readonly JwtHandler jwthand;

        public DietCommonRepository(AppDbContext dbContext, JwtHandler _jwthand)
        {
            _DbContext = dbContext;
            jwthand = _jwthand;
        }

        public async Task<dynamic> LoginCheck(string username, string password)
        {
            try
            {
                var data = await (from a in _DbContext.HRM_EMPLOYEE
                                      //join b in _DbContext.OPN_DOCTOR on a.EMP_ID equals b.EMPLOYEE_ID
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
                                  //join doc in _DbContext.OPN_DOCTOR on emp.EMP_ID equals doc.EMPLOYEE_ID into docgroup
                                  //from doc in docgroup.DefaultIfEmpty()
                              where emp.EMP_ID == ut.AUSR_ID
                              select new
                              {
                                  emp.EMP_ID,
                                  emp.EMP_OFFL_NAME,
                                  emp.EMP_CONT_PER_PHONE,
                                  //doc.LEVEL_ID,
                                  //doc.BRANCH_ID,
                                  //doc.DEF_PAGE
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


        //join doc in _DbContext.OPN_DOCTOR on emp.EMP_ID equals doc.EMPLOYEE_ID into docgroup
        //					 from doc in docgroup.DefaultIfEmpty()

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

        public async Task<dynamic> SaveDietChartAsync(string patiId, string edocId, string dietspec, string foodpref, string? totfluid, string? earlymorn, string? breakfast, string? elevenpm, string? lunch, string? fourpm, string? sevenpm, string? bedtime, string? specialinstr, string createduser, string? eight, string docId, string? presillness, string? pheight, string? pweight, UserTocken ut)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(patiId) || string.IsNullOrEmpty(edocId))
                {
                    //|| string.IsNullOrEmpty(specialConsult)
                    return new
                    {
                        Status = 400,
                        Message = "Invalid input. All fields are required."
                    };
                }
                var newDietchart = new UCHEMR_DIET_CHART
                {
                    EMR_DOC_ID = edocId,
                    PATI_ID = patiId,
                    DIET_SPEC = dietspec,
                    FOOD_PREF = foodpref,
                    TOTAL_FLUID = totfluid,
                    EARLY_MORNING = earlymorn,
                    BREAKFAST = breakfast,
                    ELEVENPM = elevenpm,
                    LUNCH = lunch,
                    FOURPM = fourpm,
                    SEVENPM = sevenpm,
                    BEDTIME = bedtime,
                    SPECIAL_INSTRUCTION = specialinstr,
                    CREATED_DATE = DateTime.UtcNow, // Set to the current UTC time
                    CREATED_USER = ut.AUSR_ID
                };

                // Add the new record to the database
                _DbContext.Set<UCHEMR_DIET_CHART>().Add(newDietchart);


                if (presillness != "")
                {
                    // Get current UTC time
                    DateTime currentDateTimeUtc = DateTime.UtcNow;

                    // Create a new instance of EMR_DOCUMENT_DETAILS
                    var newDocDetails = new EMR_DOCUMENT_DETAILS
                    {
                        EMR_DOC_ID = edocId, // Correct mapping to EMR_DOC_ID
                        PATI_ID = patiId,    // Correct mapping to PATI_ID
                        DOCT_ID = docId,     // Correct mapping to DOCT_ID
                        PRSNT_ILLNESS = presillness // Corrected spelling
                    };

                    // Insert the new document details
                    _DbContext.Set<EMR_DOCUMENT_DETAILS>().Add(newDocDetails);

                }

                var newvitalDetails = new UCHEMR_EMR_PATIENT_VITALS_STATUS
                {
                    EMR_DOC_ID = edocId, // Correct mapping to EMR_DOC_ID
                    PATI_ID = patiId,    // Correct mapping to PATI_ID
                    DOCT_ID = docId,     // Correct mapping to DOCT_ID
                    HEIGHT = pheight,
                    WEIGHT = pweight, // Updated to match schema

                };

                // Insert the new document details
                _DbContext.Set<UCHEMR_EMR_PATIENT_VITALS_STATUS>().Add(newvitalDetails);


                await _DbContext.SaveChangesAsync();

                // Return success response
                return new
                {
                    Status = 200,
                    Message = "DietChart saved successfully."
                };
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return new
                {
                    Status = 500,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<dynamic> SaveDocumentDietAsync(string patiId, string edocId, string docId, string presillness)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(patiId) || string.IsNullOrEmpty(edocId) || string.IsNullOrEmpty(docId))
                {
                    return new DefaultMessage.Message1
                    {
                        Status = 400,
                        Message = "Patient ID, Document ID, and Doctor ID are required."
                    };
                }

                // Get current UTC time
                DateTime currentDateTimeUtc = DateTime.UtcNow;

                // Create a new instance of EMR_DOCUMENT_DETAILS
                var newDocDetails = new EMR_DOCUMENT_DETAILS
                {
                    EMR_DOC_ID = edocId, // Correct mapping to EMR_DOC_ID
                    PATI_ID = patiId,    // Correct mapping to PATI_ID
                    DOCT_ID = docId,     // Correct mapping to DOCT_ID
                    PRSNT_ILLNESS = presillness // Corrected spelling
                };

                // Insert the new document details
                _DbContext.Set<EMR_DOCUMENT_DETAILS>().Add(newDocDetails);
                await _DbContext.SaveChangesAsync();

                // Return success response
                return new DefaultMessage.Message1
                {
                    Status = 200,
                    Message = "Details saved successfully."
                };
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return new DefaultMessage.Message1
                {
                    Status = 500,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<dynamic> SavePatientVitalStatusAsync(
            string patiId,
            string edocId,
            string docId,
            string? pheight, string? pweight
            )
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(patiId) || string.IsNullOrEmpty(edocId) || string.IsNullOrEmpty(docId))
                {
                    return new DefaultMessage.Message1
                    {
                        Status = 400,
                        Message = "Patient ID, Document ID, and Doctor ID are required."
                    };
                }

                // Get current UTC time
                DateTime currentDateTimeUtc = DateTime.UtcNow;

                // Create a new instance of EMR_DOCUMENT_DETAILS
                var newvitalDetails = new UCHEMR_EMR_PATIENT_VITALS_STATUS
                {
                    EMR_DOC_ID = edocId, // Correct mapping to EMR_DOC_ID
                    PATI_ID = patiId,    // Correct mapping to PATI_ID
                    DOCT_ID = docId,     // Correct mapping to DOCT_ID
                    HEIGHT = pheight,
                    WEIGHT = pweight, // Updated to match schema

                };

                // Insert the new document details
                _DbContext.Set<UCHEMR_EMR_PATIENT_VITALS_STATUS>().Add(newvitalDetails);
                await _DbContext.SaveChangesAsync();

                // Return success response
                return new DefaultMessage.Message1
                {
                    Status = 200,
                    Message = "Details saved successfully."
                };
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return new DefaultMessage.Message1
                {
                    Status = 500,
                    Message = $"Error: {ex.Message}"
                };
            }
        }


        public async Task<dynamic> SaveBodyCompositionAsync(BODY_COMPO_FEMALE_CL b, UserTocken ut)
        {
            try
            {

                var newDietchart = new BODY_COMPO_FEMALE
                {
                    EMR_DOC_ID = b.P_EMR_DOC_ID,
                    PATI_ID = b.P_PATI_ID,
                    //SAVE_TIME = b.P_SAVE_TIME,
                    SAVE_TIME = DateTime.UtcNow,
                    WEIGHT = b.P_WEIGHT,
                    BODY_FAT = b.P_BODY_FAT,
                    VISCERAL_FAT = b.P_VISCERAL_FAT,
                    RESTING_METABOLISM = b.P_RESTING_METABOLISM,
                    SKELETAL_MUSCLE = b.P_SKELETAL_MUSCLE,
                    SKELETAL_MUSCLE_ARMS = b.P_SKELETAL_MUSCLE_ARMS,
                    SKELETAL_MUSCLE_TRUNK = b.P_SKELETAL_MUSCLE_TRUNK,
                    SKELETAL_MUSCLE_LEGS = b.P_SKELETAL_MUSCLE_LEGS,
                    SUBCUT_FAT = b.P_SUBCUT_FAT,
                    SUBCUT_ARMS = b.P_SUBCUT_ARMS,
                    SUBCUT_TRUNK = b.P_SUBCUT_TRUNK,
                    SUBCUT_LEG = b.P_SUBCUT_LEG,
                    BMI = b.P_BMI,
                    BODY_AGE = b.P_BODY_AGE,
                    HEIGHT = b.P_HEIGHT,
                    WEIGHT1 = b.P_WEIGHT1,
                    CREATED_USER = ut.AUSR_ID,
                    CREATED_DATE = DateTime.UtcNow, // Set to the current UTC time
                };

                // Add the new record to the database
                _DbContext.Set<BODY_COMPO_FEMALE>().Add(newDietchart);
                await _DbContext.SaveChangesAsync();

                // Return success response
                return new
                {
                    Status = 200,
                    Message = "Body Composition Analysis Report saved successfully."
                };
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return new
                {
                    Status = 500,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }



        public async Task<dynamic> SaveSubjGlobalAsync(SubjGlobal dentalAssessment, UserTocken ut)
        {
            try
            {
                // Array of SQL statements and corresponding parameters
                var sqlStatements = new List<string>
            {
                @"BEGIN UCHEMR.DIET_BASIC_ENTRY(
					:P_EMR_DOC_ID, :P_HEIGHT, :P_ASW, :P_IBW, :P_MAC, :P_TSF, :P_BMI, :P_BP, 
					:P_DIAGNOSIS, :P_GOOD, :P_FOOD_50, :P_FOOD_25, :P_ANOREXIA, :P_PALLOR, 
					:P_PALLOR_CMNT, :P_EDEMA, :P_EDEMA_CMNT, :P_DISEASE, :P_CREATED_BY
				); END;",

                @"BEGIN UCHEMR.SP_DIET_FUNCTIONAL_CAPACITY(
					:P_EMR_DOC_ID, :P_DYSFUNCTION_STATUS, :P_DURATION_TYPE, :P_DURATION, 
					:P_DURATION_COMMENTS, :P_DYSFUNCTION_TYPE, :P_DYSFUNCTION_WEEKS, 
					:P_DYSFUNCTION_COMMENTS, :P_CREATED_BY
				); END;",
            @"BEGIN UCHEMR.SP_DIET_DIETARY_INTAKE(
					  :P_EMR_DOC_ID,:P_DIETARY_STATUS,:P_DURATION_TYPE,:P_DURATION,
					  :P_DURATION_COMMENTS,:P_DIETARY_TYPE,:P_DIETARY_COMMENTS,:P_CREATED_BY
				  ); END;",
            @"BEGIN UCHEMR.SP_DIET_GASTRO_SYMPTOMS(
					  :P_EMR_DOC_ID,:P_STATUS,:P_NAVSEA,:P_VOMITTING,:P_DIARRHEA,
					 :P_ANOREXIA,:P_COMMENTS,:P_CREATED_BY
				  ); END;",
            @"BEGIN UCHEMR.SP_DIET_PHYSICAL_SUBJECTIVE(
					  :P_EMR_DOC_ID,:P_FAT_LOSS,:P_FAT_LOSE_COMMENTS,:P_MUSCLE_WAST,:P_COMMENTS,:P_CREATED_BY,
					  :P_ASSESSMENT,:P_ASSESSMENT_RATE,:P_ASSESSMENT_COMMENTS,:P_ANKLE_EDEMA,:P_ANKLE_COMMENTS,
					  :P_SACRAL_EDEMA,:P_SACRAL_COMMENTS,:P_ASCITES,:P_ASCITES_COMMENTS,:P_NUTRITION_DIAGNOSIS,
					  :P_BIOCHEMICAL_VALUES
				  ); END;",
            @"BEGIN UCHEMR.SP_DIET_DISEASE(
					  :P_EMR_DOC_ID,:P_PRIMARY_DIAGNOSIS,:P_COMMENTS,:P_METABOLIC_DEMAND,:P_METABOLIC_COMMENTS,:P_CREATED_BY
				  ); END;",
            @"BEGIN UCHEMR.SP_WEIGHT_CHANGE(
					  :P_EMR_DOC_ID,:P_LOSS_STATUS,:P_LOSS_KG,:P_LOSS_PERCENT,:P_LOSS_COMMENTS,:P_CHANGE_STATUS,:P_CHANGE_TYPE,
					  :P_CHANGE_KG,:P_CHANGE_PERCENT,:P_CHANGE_COMMENTS,:P_CREATED_BY
					); END;"
            
				// Add other SQL statements here
			};

                var parametersList = new List<List<OracleParameter>>
        {

			//new OracleParameter("P_TREATMENT_PLAN", OracleDbType.Varchar2) { Value = ophthalmologyParameters.TreatmentPlan  ?? (object)DBNull.Value},
			new List<OracleParameter>
            {
                new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = dentalAssessment.P_EMR_DOC_ID ??(object) DBNull.Value },
                new OracleParameter("P_HEIGHT", OracleDbType.Varchar2) { Value = dentalAssessment.P_HEIGHT ??(object) DBNull.Value },
                new OracleParameter("P_ASW", OracleDbType.Varchar2) { Value = dentalAssessment.P_ASW ?? (object)DBNull.Value },
                new OracleParameter("P_IBW", OracleDbType.Varchar2) { Value = dentalAssessment.P_IBW ??(object) DBNull.Value },
                new OracleParameter("P_MAC", OracleDbType.Varchar2) { Value = dentalAssessment.P_MAC ??(object) DBNull.Value },
                new OracleParameter("P_TSF", OracleDbType.Varchar2) { Value = dentalAssessment.P_TSF ??(object) DBNull.Value },
                new OracleParameter("P_BMI", OracleDbType.Varchar2) { Value = dentalAssessment.P_BMI ??(object) DBNull.Value },
                new OracleParameter("P_BP", OracleDbType.Varchar2) { Value = dentalAssessment.P_BP ??(object) DBNull.Value },
                new OracleParameter("P_DIAGNOSIS", OracleDbType.Varchar2) { Value = dentalAssessment.P_DIAGNOSIS ??(object) DBNull.Value },
                new OracleParameter("P_GOOD", OracleDbType.Varchar2) { Value = dentalAssessment.P_GOOD ??(object) DBNull.Value },
                new OracleParameter("P_FOOD_50", OracleDbType.Varchar2) { Value = dentalAssessment.P_FOOD_50 ??(object) DBNull.Value },
                new OracleParameter("P_FOOD_25", OracleDbType.Varchar2) { Value = dentalAssessment.P_FOOD_25 ??(object) DBNull.Value },
                new OracleParameter("P_ANOREXIA", OracleDbType.Varchar2) { Value = dentalAssessment.P_ANOREXIA ??(object) DBNull.Value },
                new OracleParameter("P_PALLOR", OracleDbType.Varchar2) { Value = dentalAssessment.P_PALLOR ??(object) DBNull.Value },
                new OracleParameter("P_PALLOR_CMNT", OracleDbType.Varchar2) { Value = dentalAssessment.P_PALLOR_CMNT ??(object) DBNull.Value },
                new OracleParameter("P_EDEMA", OracleDbType.Varchar2) { Value = dentalAssessment.P_EDEMA ??(object) DBNull.Value },
                new OracleParameter("P_EDEMA_CMNT", OracleDbType.Varchar2) { Value = dentalAssessment.P_EDEMA_CMNT ??(object) DBNull.Value },
                new OracleParameter("P_DISEASE", OracleDbType.Varchar2) { Value = dentalAssessment.P_DISEASE ??(object) DBNull.Value },
                new OracleParameter("P_CREATED_BY", OracleDbType.Varchar2) { Value = ut.AUSR_ID ??(object) DBNull.Value }
            },

            new List<OracleParameter>
            {
                new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = dentalAssessment.P_EMR_DOC_ID ??(object) DBNull.Value },
                new OracleParameter("P_DYSFUNCTION_STATUS", OracleDbType.Varchar2) { Value = dentalAssessment.P_DYSFUNCTION_STATUS ??(object) DBNull.Value },
                new OracleParameter("P_DURATION_TYPE", OracleDbType.Varchar2) { Value = dentalAssessment.P_DURATION_TYPE ??(object) DBNull.Value },
                new OracleParameter("P_DURATION", OracleDbType.Varchar2) { Value = dentalAssessment.P_DURATION ??(object) DBNull.Value },
                new OracleParameter("P_DURATION_COMMENTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_DURATION_COMMENTS ??(object) DBNull.Value },
                new OracleParameter("P_DYSFUNCTION_TYPE", OracleDbType.Varchar2) { Value = dentalAssessment.P_DYSFUNCTION_TYPE ??(object) DBNull.Value },
                new OracleParameter("P_DYSFUNCTION_WEEKS", OracleDbType.Varchar2) { Value = dentalAssessment.P_DYSFUNCTION_WEEKS ??(object) DBNull.Value },
                new OracleParameter("P_DYSFUNCTION_COMMENTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_DYSFUNCTION_COMMENTS ??(object) DBNull.Value },
                new OracleParameter("P_CREATED_BY", OracleDbType.Varchar2) { Value = ut.AUSR_ID ??(object) DBNull.Value }
            },
            new List<OracleParameter>
            {
                new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = dentalAssessment.P_EMR_DOC_ID ?? (object)DBNull.Value },
                new OracleParameter("P_DIETARY_STATUS", OracleDbType.Varchar2) { Value = dentalAssessment.P_DIETARY_STATUS  ?? (object)DBNull.Value },
                new OracleParameter("P_DURATION_TYPE", OracleDbType.Varchar2) { Value = dentalAssessment.P_DURATION_TYPE  ?? (object)DBNull.Value },
                new OracleParameter("P_DURATION", OracleDbType.Varchar2) { Value = dentalAssessment.P_DURATION ?? (object)DBNull.Value },
                new OracleParameter("P_DURATION_COMMENTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_DURATION_COMMENTS ?? (object)DBNull.Value },
                new OracleParameter("P_DIETARY_TYPE", OracleDbType.Varchar2) { Value = dentalAssessment.P_DIETARY_TYPE   ?? (object)DBNull.Value },
                new OracleParameter("P_DIETARY_COMMENTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_DIETARY_COMMENTS ?? (object)DBNull.Value },
                new OracleParameter("P_CREATED_BY", OracleDbType.Varchar2) { Value = dentalAssessment.P_CREATED_BY ?? (object)DBNull.Value }
            },
            new List<OracleParameter>
            {
                new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = dentalAssessment.P_EMR_DOC_ID ?? (object)DBNull.Value },
                new OracleParameter("P_STATUS", OracleDbType.Varchar2) { Value = dentalAssessment.P_STATUS  ?? (object)DBNull.Value  },
                new OracleParameter("P_NAVSEA", OracleDbType.Varchar2) { Value = dentalAssessment.P_NAVSEA  ?? (object)DBNull.Value },
                new OracleParameter("P_VOMITTING", OracleDbType.Varchar2) { Value = dentalAssessment.P_VOMITTING ?? (object)DBNull.Value },
                new OracleParameter("P_DIARRHEA", OracleDbType.Varchar2) { Value = dentalAssessment.P_DIARRHEA ?? (object)DBNull.Value },
                new OracleParameter("P_ANOREXIA", OracleDbType.Varchar2) { Value = dentalAssessment.P_ANOREXIA  ?? (object)DBNull.Value  },
                new OracleParameter("P_COMMENTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_COMMENTS  ?? (object)DBNull.Value  },
                new OracleParameter("P_CREATED_BY", OracleDbType.Varchar2) { Value = dentalAssessment.P_CREATED_BY ?? (object)DBNull.Value }

            },
            new List<OracleParameter>
            {
                new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = dentalAssessment.P_EMR_DOC_ID ?? (object)DBNull.Value },
                new OracleParameter("P_FAT_LOSS", OracleDbType.Varchar2) { Value = dentalAssessment.P_FAT_LOSS  ?? (object)DBNull.Value },
                new OracleParameter("P_FAT_LOSE_COMMENTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_FAT_LOSE_COMMENTS ?? (object)DBNull.Value},
                new OracleParameter("P_MUSCLE_WAST", OracleDbType.Varchar2) { Value = dentalAssessment.P_MUSCLE_WAST  ?? (object)DBNull.Value },
                new OracleParameter("P_COMMENTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_COMMENTS ?? (object)DBNull.Value },
                new OracleParameter("P_CREATED_BY", OracleDbType.Varchar2) { Value = dentalAssessment.P_CREATED_BY   ?? (object)DBNull.Value },
                new OracleParameter("P_ASSESSMENT", OracleDbType.Varchar2) { Value = dentalAssessment.P_ASSESSMENT ?? (object)DBNull.Value },
                new OracleParameter("P_ASSESSMENT_RATE", OracleDbType.Varchar2) { Value = dentalAssessment.P_ASSESSMENT_RATE  ?? (object)DBNull.Value  },
                new OracleParameter("P_ASSESSMENT_COMMENTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_ASSESSMENT_COMMENTS ?? (object)DBNull.Value },
                new OracleParameter("P_ANKLE_EDEMA", OracleDbType.Varchar2) { Value = dentalAssessment.P_ANKLE_EDEMA  ?? (object)DBNull.Value  },
                new OracleParameter("P_ANKLE_COMMENTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_ANKLE_COMMENTS ?? (object)DBNull.Value },
                new OracleParameter("P_SACRAL_EDEMA", OracleDbType.Varchar2) { Value = dentalAssessment.P_SACRAL_EDEMA  ?? (object)DBNull.Value },
                new OracleParameter("P_SACRAL_COMMENTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_SACRAL_COMMENTS ?? (object)DBNull.Value },
                new OracleParameter("P_ASCITES", OracleDbType.Varchar2) { Value = dentalAssessment.P_ASCITES  ?? (object)DBNull.Value  },
                new OracleParameter("P_ASCITES_COMMENTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_ASCITES_COMMENTS ?? (object)DBNull.Value },
                new OracleParameter("P_NUTRITION_DIAGNOSIS", OracleDbType.Varchar2) { Value = dentalAssessment.P_NUTRITION_DIAGNOSIS ?? (object)DBNull.Value },
                new OracleParameter("P_BIOCHEMICAL_VALUES", OracleDbType.Varchar2) { Value = dentalAssessment.P_BIOCHEMICAL_VALUES ?? (object)DBNull.Value },
				//new OracleParameter("P_CREATED_BY", OracleDbType.Varchar2) { Value = dentalAssessment.P_CREATED_BY ?? (object)DBNull.Value },
			},
            new List<OracleParameter>
            {
                new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = dentalAssessment.P_EMR_DOC_ID ?? (object)DBNull.Value },
                new OracleParameter("P_PRIMARY_DIAGNOSIS", OracleDbType.Varchar2) { Value = dentalAssessment.P_PRIMARY_DIAGNOSIS  ?? (object)DBNull.Value },
                new OracleParameter("P_COMMENTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_COMMENTS  ?? (object)DBNull.Value },
                new OracleParameter("P_METABOLIC_DEMAND", OracleDbType.Varchar2) { Value = dentalAssessment.P_METABOLIC_DEMAND ?? (object)DBNull.Value },
                new OracleParameter("P_METABOLIC_COMMENTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_METABOLIC_COMMENTS ?? (object)DBNull.Value },
                new OracleParameter("P_CREATED_BY", OracleDbType.Varchar2) { Value = dentalAssessment.P_CREATED_BY ?? (object)DBNull.Value }

            },
            new List<OracleParameter>
            {
                new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = dentalAssessment.P_EMR_DOC_ID ?? (object)DBNull.Value },
                new OracleParameter("P_LOSS_STATUS", OracleDbType.Varchar2) { Value = dentalAssessment.P_LOSS_STATUS ?? (object)DBNull.Value  },
                new OracleParameter("P_LOSS_KG", OracleDbType.Varchar2) { Value = dentalAssessment.P_LOSS_KG ?? (object)DBNull.Value},
                new OracleParameter("P_LOSS_PERCENT", OracleDbType.Varchar2) { Value = dentalAssessment.P_LOSS_PERCENT ?? (object)DBNull.Value },
                new OracleParameter("P_LOSS_COMMENTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_LOSS_COMMENTS ?? (object)DBNull.Value },
                new OracleParameter("P_CHANGE_STATUS", OracleDbType.Varchar2) { Value = dentalAssessment.P_CHANGE_STATUS  ?? (object)DBNull.Value  },
                new OracleParameter("P_CHANGE_TYPE", OracleDbType.Varchar2) { Value = dentalAssessment.P_CHANGE_TYPE  ?? (object)DBNull.Value } ,
                new OracleParameter("P_CHANGE_KG", OracleDbType.Varchar2) { Value = dentalAssessment.P_CHANGE_KG ?? (object)DBNull.Value },
                new OracleParameter("P_CHANGE_PERCENT", OracleDbType.Varchar2) { Value = dentalAssessment.P_CHANGE_PERCENT ?? (object)DBNull.Value },
                new OracleParameter("P_CHANGE_COMMENTS", OracleDbType.Varchar2) { Value = dentalAssessment.P_CHANGE_COMMENTS ?? (object)DBNull.Value },
                new OracleParameter("P_CREATED_BY", OracleDbType.Varchar2) { Value = dentalAssessment.P_CREATED_BY ?? (object)DBNull.Value }

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
                            cmd.Parameters.AddRange(parametersList[i].ToArray());

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

        public async Task<dynamic> GetDietchartDetails(string edocId)
        {
            try
            {

                var result = (from emp in _DbContext.DIET_CHART
                              join em in _DbContext.EMR_DOCUMENT_DETAILS on emp.EMR_DOC_ID equals em.EMR_DOC_ID
                              join mg in _DbContext.EMR_PATIENT_VITALS_STATUS on emp.EMR_DOC_ID equals mg.EMR_DOC_ID
                              where emp.EMR_DOC_ID == edocId
                              orderby emp.CREATED_DATE ascending
                              select new
                              {
                                  emp.EMR_DOC_ID,
                                  emp.PATI_ID,
                                  emp.DIET_SPEC,
                                  emp.FOOD_PREF,
                                  emp.TOTAL_FLUID,
                                  emp.EARLY_MORNING,
                                  emp.BREAKFAST,
                                  emp.ELEVENPM,
                                  emp.LUNCH,
                                  emp.FOURPM,
                                  emp.SEVENPM,
                                  emp.BEDTIME,
                                  emp.SPECIAL_INSTRUCTION,
                                  emp.EIGHT,
                                  em.DOCT_ID,
                                  em.PRSNT_ILLNESS,
                                  mg.HEIGHT,
                                  mg.WEIGHT

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

        public async Task<dynamic> GetBodyCompositionDetails(string edocId)
        {
            try
            {

                var result = (from emp in _DbContext.BODY_COMPO_FEMALE
                              where emp.EMR_DOC_ID == edocId
                              orderby emp.CREATED_DATE ascending
                              select new
                              {
                                  emp.EMR_DOC_ID,
                                  emp.PATI_ID,
                                  emp.SAVE_TIME,
                                  emp.WEIGHT,
                                  emp.BODY_FAT,
                                  emp.VISCERAL_FAT,
                                  emp.RESTING_METABOLISM,
                                  emp.SKELETAL_MUSCLE,
                                  emp.SKELETAL_MUSCLE_ARMS,
                                  emp.SKELETAL_MUSCLE_TRUNK,
                                  emp.SKELETAL_MUSCLE_LEGS,
                                  emp.SUBCUT_FAT,
                                  emp.SUBCUT_ARMS,
                                  emp.SUBCUT_TRUNK,
                                  emp.SUBCUT_LEG,
                                  emp.BMI,
                                  emp.BODY_AGE,
                                  emp.HEIGHT,
                                  emp.WEIGHT1

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

        public async Task<dynamic> GetSubjGlobalDetails(string emrDocId)
        {
            try
            {
                // Corrected SQL query
                var sql = @"
    SELECT 
        b.EMR_DOC_ID, b.HEIGHT, b.ASW, b.IBW, b.MAC, b.TSF, b.BMI, b.BP, b.DIAGNOSIS,
        b.GOOD, b.FOOD_50, b.FOOD_25, b.ANOREXIA, b.PALLOR, b.PALLOR_CMNT, b.EDEMA, b.EDEMA_CMNT,
        b.DISEASE, f.DYSFUNCTION_STATUS, f.DURATION_TYPE, f.DURATION, f.DURATION_COMMENTS, f.DYSFUNCTION_TYPE,
        f.DYSFUNCTION_WEEKS, f.DYSFUNCTION_COMMENTS,
        n.DIETARY_STATUS, n.DURATION_TYPE, n.DURATION, n.DURATION_COMMENTS, n.DIETARY_TYPE,
        n.DIETARY_COMMENTS, g.STATUS, g.NAVSEA, g.VOMITTING, g.DIARRHEA, g.ANOREXIA, g.COMMENTS,
        p.FAT_LOSS, p.FAT_LOSE_COMMENTS, p.MUSCLE_WAST, p.COMMENTS AS PHYSICAL_COMMENTS, 
        p.ASSESSMENT, p.ASSESSMENT_RATE, p.ASSESSMENT_COMMENTS, 
        p.ANKLE_EDEMA, p.ANKLE_COMMENTS, p.SACRAL_EDEMA, p.SACRAL_COMMENTS,
        p.ASCITES, p.ASCITES_COMMENTS, p.NUTRITION_DIAGNOSIS, p.BIOCHEMICAL_VALUES,
        e.PRIMARY_DIAGNOSIS, e.COMMENTS AS DISEASE_COMMENTS, e.METABOLIC_DEMAND, e.METABOLIC_COMMENTS,
        w.LOSS_STATUS, w.LOSS_KG, w.LOSS_PERCENT, w.LOSS_COMMENTS, 
        w.CHANGE_STATUS, w.CHANGE_TYPE, w.CHANGE_KG, w.CHANGE_PERCENT, w.CHANGE_COMMENTS
    FROM UCHEMR.DIET_BASIC_HISTORY b
    INNER JOIN UCHEMR.DIET_FUNCTIONAL_CAPACITY f ON b.EMR_DOC_ID = f.EMR_DOC_ID
    INNER JOIN UCHEMR.DIET_DIETARY_INTAKE n ON b.EMR_DOC_ID = n.EMR_DOC_ID
    INNER JOIN UCHEMR.DIET_GASTROINTESTINAL_SYMPTOMS g ON b.EMR_DOC_ID = g.EMR_DOC_ID
    INNER JOIN UCHEMR.DIET_PHYSICAL_SUBJECTIVE p ON b.EMR_DOC_ID = p.EMR_DOC_ID
    INNER JOIN UCHEMR.DIET_DISEASE e ON b.EMR_DOC_ID = e.EMR_DOC_ID
    INNER JOIN UCHEMR.DIET_WEIGHT_CHANGE w ON b.EMR_DOC_ID = w.EMR_DOC_ID
    WHERE b.EMR_DOC_ID = :emrDocId";

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

