using Microsoft.EntityFrameworkCore;
using RemediEmr.Class;
using RemediEmr.Data.Class;
using RemediEmr.Data.DbModel;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Dynamic;
using static JwtService;
using Oracle.ManagedDataAccess.Types;
using static System.Collections.Specialized.BitVector32;

namespace RemediEmr.Repositry
{
    public class CommonRepositry
    {
        private readonly AppDbContext _DbContext;

        private readonly JwtHandler jwthand;

        private readonly IConfiguration _configuration;
        private readonly string _con;

        public CommonRepositry(AppDbContext dbContext, JwtHandler _jwthand, IConfiguration configuration)
        {
            _DbContext = dbContext;
            jwthand = _jwthand;
            _configuration = configuration;
            _con = _DbContext.Database.GetConnectionString();
        }


        public async Task<dynamic> LoginCheck1(string username, string password)
        {
            try
            {
                //var data = await _DbContext.EMR_ADMIN_USERS
                //    .Where(x => x.AUSR_USERNAME == username
                //    && x.AUSR_PWD == password
                //    && x.AUSR_STATUS != "D")
                //    .ToListAsync();
                var data = await (from a in _DbContext.HRM_EMPLOYEE
                                  join b in _DbContext.OPN_DOCTOR on a.EMP_ID equals b.EMPLOYEE_ID
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

                //return data/*;*/

                //return userdata;

                //SELECT B.BRANCH_ID, B.BRCH_NAME FROM UCHMASTER.HRM_BRANCH B
                //                INNER JOIN UCHEMR.EMR_ADMIN_USERS_BRANCH_LINK BL ON BL.BRANCH_ID = B.BRANCH_ID
                //                INNER JOIN UCHEMR.EMR_ADMIN_USERS USR ON USR.AUSR_ID = BL.AUSR_ID
                //                WHERE NVL(B.ACTIVE_STATUS, 'A')= 'A' AND ausr_username = 'tedsys' AND ausr_pwd = 'ted@123' ORDER BY B.BRANCH_ID



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

        public async Task<dynamic> GetUserDet(UserTocken ut)
        {
            try
            {

                //var result =
                //    from b in _DbContext.HRM_BRANCH
                //    join bl in _DbContext.EMR_ADMIN_USERS_BRANCH_LINK
                //        on b.BRANCH_ID equals bl.BRANCH_ID
                //    join usr in _DbContext.EMR_ADMIN_USERS
                //        on bl.AUSR_ID equals usr.AUSR_ID
                //    where b.ACTIVE_STATUS == "A"
                //          && usr.AUSR_USERNAME == ut.USERNAME
                //          && usr.AUSR_PWD == ut.PASSWORD
                //    orderby b.BRANCH_ID
                //    select new
                //    {
                //        b.BRANCH_ID,
                //        b.BRCH_NAME
                //    };
                var result = (from emp in _DbContext.HRM_EMPLOYEE
                              join doc in _DbContext.OPN_DOCTOR on emp.EMP_ID equals doc.EMPLOYEE_ID into docgroup
                              from doc in docgroup.DefaultIfEmpty()
                              where emp.EMP_ID == ut.AUSR_ID
                              select new
                              {
                                  emp.EMP_ID,
                                  emp.EMP_OFFL_NAME,
                                  emp.EMP_CONT_PER_PHONE,
                                  doc.LEVEL_ID,
                                  doc.BRANCH_ID,
                                  doc.DOCT_ID
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



        public async Task<dynamic> GetUserDetails(UserTocken ut)
        {
            try
            {

                //var result =
                //    from b in _DbContext.HRM_BRANCH
                //    join bl in _DbContext.EMR_ADMIN_USERS_BRANCH_LINK
                //        on b.BRANCH_ID equals bl.BRANCH_ID
                //    join usr in _DbContext.EMR_ADMIN_USERS
                //        on bl.AUSR_ID equals usr.AUSR_ID
                //    where b.ACTIVE_STATUS == "A"
                //          && usr.AUSR_USERNAME == ut.USERNAME
                //          && usr.AUSR_PWD == ut.PASSWORD
                //    orderby b.BRANCH_ID
                //    select new
                //    {
                //        b.BRANCH_ID,
                //        b.BRCH_NAME
                //    };
                var result = (from emp in _DbContext.HRM_EMPLOYEE
                              join doc in _DbContext.OPN_DOCTOR on emp.EMP_ID equals doc.EMPLOYEE_ID into docgroup
                              from doc in docgroup.DefaultIfEmpty()
                              where emp.EMP_ID == ut.AUSR_ID
                              select new
                              {
                                  emp.EMP_ID,
                                  emp.EMP_OFFL_NAME,
                                  emp.EMP_CONT_PER_PHONE,
                                  doc.LEVEL_ID,
                                  doc.BRANCH_ID,
                                  doc.DEF_PAGE
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


        public async Task<dynamic> LoginCheck(string username, string password)
        {
            try
            {
                //var data = await _DbContext.EMR_ADMIN_USERS
                //    .Where(x => x.AUSR_USERNAME == username
                //    && x.AUSR_PWD == password
                //    && x.AUSR_STATUS != "D")
                //    .ToListAsync();
                var data = await _DbContext.HRM_EMPLOYEE
                   .Where(e => e.EMP_ACTIVE_STATUS == 'A' &&
                               e.EMP_LOGIN_NAME == username &&
                               e.EMP_PASSWORD == password)
                   .Select(e => new
                   {
                       e.EMP_ID,
                       e.EMP_OFFL_NAME,
                       e.EMP_LOGIN_NAME,
                       e.EMP_PASSWORD
                   })
                   .ToListAsync();

                //return data/*;*/

                //return userdata;

                //SELECT B.BRANCH_ID, B.BRCH_NAME FROM UCHMASTER.HRM_BRANCH B
                //                INNER JOIN UCHEMR.EMR_ADMIN_USERS_BRANCH_LINK BL ON BL.BRANCH_ID = B.BRANCH_ID
                //                INNER JOIN UCHEMR.EMR_ADMIN_USERS USR ON USR.AUSR_ID = BL.AUSR_ID
                //                WHERE NVL(B.ACTIVE_STATUS, 'A')= 'A' AND ausr_username = 'tedsys' AND ausr_pwd = 'ted@123' ORDER BY B.BRANCH_ID



                if (data != null && data.Count>0)
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
        public async Task<dynamic> GetAllUserBranches(UserTocken ut)
        {
            try
            {

                //var result =
                //    from b in _DbContext.HRM_BRANCH
                //    join bl in _DbContext.EMR_ADMIN_USERS_BRANCH_LINK
                //        on b.BRANCH_ID equals bl.BRANCH_ID
                //    join usr in _DbContext.EMR_ADMIN_USERS
                //        on bl.AUSR_ID equals usr.AUSR_ID
                //    where b.ACTIVE_STATUS == "A"
                //          && usr.AUSR_USERNAME == ut.USERNAME
                //          && usr.AUSR_PWD == ut.PASSWORD
                //    orderby b.BRANCH_ID
                //    select new
                //    {
                //        b.BRANCH_ID,
                //        b.BRCH_NAME
                //    };
                var result = (from b in _DbContext.HRM_BRANCH
                              join bl in _DbContext.HRM_EMPLOYEE_BRANCH_LINK on b.BRANCH_ID equals bl.BRANCH_ID
                              join hr in _DbContext.HRM_EMPLOYEE_HR on bl.EMP_ID equals hr.EMP_ID
                              join emp in _DbContext.HRM_EMPLOYEE on hr.EMP_ID equals emp.EMP_ID_HR
                              where b.ACTIVE_STATUS  == "A"
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


        public async  Task<dynamic> GetActiveDisplayLocations()
        {
            var locations = new List<dynamic>();
            try
            {
                using (var connection = new OracleConnection(_con))
                {
                    connection.Open();
                    using (var command = new OracleCommand("SELECT ID, TITLE FROM UCHDISPLAY.DISPLAY_LOCATION_EXT WHERE ACTIVE_STATUS = 1", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                locations.Add(new
                                {
                                    Id = reader.GetInt32(0),
                                    Title = reader.GetString(1)
                                });
                            }
                        }
                    }
                }

                return locations;
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



        public async Task<dynamic> GetDentalAssessmentAsync(string emrDocId)
        {
            try
            {
                // Define the SQL statement to query the EMR_DENTAL_ASSESSMENT table
                var sql = "SELECT DENTAL_ID, EMR_DOC_ID, COMPLAINTS, INTRA_ORAL_EXAMINATION " +
                          "FROM UCHEMR.EMR_DENTAL_ASSESSMENT WHERE EMR_DOC_ID = :EMR_DOC_ID";

                // Define the parameter
                var emrDocIdParam = new OracleParameter("EMR_DOC_ID", OracleDbType.Varchar2) { Value = emrDocId };

                // Execute the SQL query
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = System.Data.CommandType.Text;

                    // Add the parameter to the command
                    cmd.Parameters.Add(emrDocIdParam);

                    await _DbContext.Database.GetDbConnection().OpenAsync();

                    using (var reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        var results = new List<dynamic>();

                        while (await reader.ReadAsync())
                        {
                            var result = new
                            {
                                DentalId = reader["DENTAL_ID"] != DBNull.Value ? reader["DENTAL_ID"].ToString() : null,
                                EmrDocId = reader["EMR_DOC_ID"] != DBNull.Value ? reader["EMR_DOC_ID"].ToString() : null,
                                Complaints = reader["COMPLAINTS"] != DBNull.Value ? reader["COMPLAINTS"].ToString() : null,
                                IntraOralExamination = reader["INTRA_ORAL_EXAMINATION"] != DBNull.Value ? reader["INTRA_ORAL_EXAMINATION"].ToString() : null
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


        public async Task<dynamic> SaveDentalAssessmentAsync(DentalAssessmentModel dentalAssessment, UserTocken ut)
        {
            try
            {
                // SQL to call the stored procedure
                var sql = @"BEGIN UCHTRANS.SP_INS_UPD_EMR_DENTAL_ASSE(
            :P_EMR_DOC_ID,
            :P_PAST_HISTORY,
            :P_MEDICAL_HISTORY,
            :P_INTRA_ORAL_EXAMINATION,
            :P_EXTRA_ORAL_EXAMINATION,
            :P_INTERPRETATIONS,
            :P_BLOOD_INVESTIGATIONS,
            :P_DIAGNOSIS,
            :P_PROCEDURE_PLANNED,
            :P_PROCEDURE_GIVEN,
            :P_SEL_SURFACE,
            :P_SEL_LOCATION,
            :P_SEL_AMOUNT,
            :P_DOSE_TYPE,
            :P_LOCATION,
            :P_AMOUNT,
            :P_REMARKS,
            :P_CONSENT,
            :P_PATIENT_EDUCATION,
            :P_FOLLOW_UP_DATE,
            :P_FOLLOW_UP,
            :P_CREATE_USER,
            :P_TYPE,
            :P_MATERIAL_USED_STATUS,
            :P_MATERIAL_USED,
            :P_TOOTH_TREATED_STATUS,
            :P_TOOTH_TREATED,
            :P_COMPLAINTS,
            :P_LA_STATUS,
            :P_CLINICAL_FINIDINGS,
            :RETVAL
        ); END;";

                // Oracle parameters
                var parameters = new List<OracleParameter>
        {
            new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = dentalAssessment.EmrDocId ?? (object)DBNull.Value },
            new OracleParameter("P_PAST_HISTORY", OracleDbType.Varchar2) { Value = dentalAssessment.P_PAST_HISTORY ?? (object)DBNull.Value },
            new OracleParameter("P_MEDICAL_HISTORY", OracleDbType.Varchar2) { Value = dentalAssessment.P_MEDICAL_HISTORY ?? (object)DBNull.Value },
            new OracleParameter("P_INTRA_ORAL_EXAMINATION", OracleDbType.Varchar2) { Value = dentalAssessment.IntraOralExamination ?? (object)DBNull.Value },
            new OracleParameter("P_EXTRA_ORAL_EXAMINATION", OracleDbType.Varchar2) { Value = dentalAssessment.P_EXTRA_ORAL_EXAMINATION ?? (object)DBNull.Value },
            new OracleParameter("P_INTERPRETATIONS", OracleDbType.Varchar2) { Value = dentalAssessment.P_INTERPRETATIONS ?? (object)DBNull.Value },
            new OracleParameter("P_BLOOD_INVESTIGATIONS", OracleDbType.Varchar2) { Value = dentalAssessment.P_INTERPRETATIONS ?? (object)DBNull.Value },
            new OracleParameter("P_DIAGNOSIS", OracleDbType.Varchar2) { Value = dentalAssessment.P_DIAGNOSIS ?? (object)DBNull.Value },
            new OracleParameter("P_PROCEDURE_PLANNED", OracleDbType.Varchar2) { Value = dentalAssessment.P_DIAGNOSIS ?? (object)DBNull.Value },
            new OracleParameter("P_PROCEDURE_GIVEN", OracleDbType.Varchar2) { Value = dentalAssessment.P_PROCEDURE_GIVEN ?? (object)DBNull.Value },
            new OracleParameter("P_SEL_SURFACE", OracleDbType.Varchar2) { Value = dentalAssessment.P_SEL_SURFACE ?? "0" },
            new OracleParameter("P_SEL_LOCATION", OracleDbType.Varchar2) { Value = dentalAssessment.P_SEL_LOCATION ?? "0" },
            new OracleParameter("P_SEL_AMOUNT", OracleDbType.Varchar2) { Value = dentalAssessment.P_SEL_AMOUNT ?? "0" },
            new OracleParameter("P_DOSE_TYPE", OracleDbType.Varchar2) { Value = dentalAssessment.P_DOSE_TYPE ?? (object)DBNull.Value },
            new OracleParameter("P_LOCATION", OracleDbType.Varchar2) { Value = dentalAssessment.P_LOCATION ?? (object)DBNull.Value },
            new OracleParameter("P_AMOUNT", OracleDbType.Varchar2) { Value = dentalAssessment.P_AMOUNT ?? (object)DBNull.Value },
            new OracleParameter("P_REMARKS", OracleDbType.Varchar2) { Value = dentalAssessment.P_REMARKS ?? (object)DBNull.Value },
            new OracleParameter("P_CONSENT", OracleDbType.Varchar2) { Value = dentalAssessment.P_CONSENT ?? (object)DBNull.Value },
            new OracleParameter("P_PATIENT_EDUCATION", OracleDbType.Varchar2) { Value = dentalAssessment.P_PATIENT_EDUCATION ?? (object)DBNull.Value },
            new OracleParameter("P_FOLLOW_UP_DATE", OracleDbType.Varchar2) { Value = dentalAssessment.P_FOLLOW_UP_DATE ?? (object)DBNull.Value },
            new OracleParameter("P_FOLLOW_UP", OracleDbType.Varchar2) { Value = dentalAssessment.P_FOLLOW_UP ?? (object)DBNull.Value },
            new OracleParameter("P_CREATE_USER", OracleDbType.Varchar2) { Value = ut.AUSR_ID ?? (object)DBNull.Value },
            new OracleParameter("P_TYPE", OracleDbType.Varchar2) { Value = dentalAssessment.Type ?? (object)DBNull.Value },
            new OracleParameter("P_MATERIAL_USED_STATUS", OracleDbType.Varchar2) { Value = dentalAssessment.P_MATERIAL_USED_STATUS ?? (object)DBNull.Value },
            new OracleParameter("P_MATERIAL_USED", OracleDbType.Varchar2) { Value = dentalAssessment.P_MATERIAL_USED ?? (object)DBNull.Value },
            new OracleParameter("P_TOOTH_TREATED_STATUS", OracleDbType.Varchar2) { Value = dentalAssessment.P_TOOTH_TREATED_STATUS ?? (object)DBNull.Value },
            new OracleParameter("P_TOOTH_TREATED", OracleDbType.Varchar2) { Value = dentalAssessment.P_TOOTH_TREATED ?? (object)DBNull.Value },
            new OracleParameter("P_COMPLAINTS", OracleDbType.Varchar2) { Value = dentalAssessment.Complaints ?? (object)DBNull.Value },
            new OracleParameter("P_LA_STATUS", OracleDbType.Varchar2) { Value = dentalAssessment.P_LA_STATUS ?? (object)DBNull.Value },
            new OracleParameter("P_CLINICAL_FINIDINGS", OracleDbType.Varchar2) { Value = dentalAssessment.P_CLINICAL_FINIDINGS ?? (object)DBNull.Value },
            new OracleParameter("RETVAL", OracleDbType.Int32) { Direction = ParameterDirection.Output }
        };

                // Execute the procedure
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddRange(parameters.ToArray());

                    // Open connection if not already open
                    if (cmd.Connection.State != ConnectionState.Open)
                        await cmd.Connection.OpenAsync();

                    // Execute the procedure
                    await cmd.ExecuteNonQueryAsync();

                    // Retrieve the return value
                    int retval = int.Parse(parameters.First(p => p.ParameterName == "RETVAL").Value.ToString());
                    return new { Status = retval == 1 ? 200 : 400, Message = retval == 1 ? "Operation successful." : "Operation failed." };
                }
            }
            catch (Exception ex)
            {
                return new { Status = 500, Message = $"Error: {ex.Message}" };
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



        public async Task<dynamic> SaveSpecialConsultationAsync(string patiId, string edocId, string specialConsult, UserTocken ut)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(patiId) || string.IsNullOrEmpty(edocId) || string.IsNullOrEmpty(specialConsult))
                {
                    return new
                    {
                        Status = 400,
                        Message = "Invalid input. All fields are required."
                    };
                }

                // Create a new consultation object
                var newConsultation = new SPECIAL_CONSULTATION
                {
                    EMR_DOC_ID = edocId,
                    PATI_ID = patiId,
                    SPECIAL_CONSULT = specialConsult,
                    CREATE_DATE = DateTime.UtcNow, // Set to the current UTC time
                    CREATE_USER = ut.AUSR_ID
                };

                // Add the new record to the database
                _DbContext.Set<SPECIAL_CONSULTATION>().Add(newConsultation);
                await _DbContext.SaveChangesAsync();

                // Return success response
                return new
                {
                    Status = 200,
                    Message = "Special consultation saved successfully."
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

        public async Task<dynamic> SaveCompImmuDetailsAsync(
        string patiId,
        string edocId,
        string docId,
        string complnt,
        string pTreatmentRemarksNew = null,
        string hstry = null,
        string immun = null,
        string genRmrks = null,
        string iTreatmentRemarks = null,
        string pNotes = null,
        string pDoctNotes = null)
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
                    CREATE_DATE = currentDateTimeUtc,
                    COMMENTS = complnt, // Updated to match schema
                    TREATMENT_REMARKS_NEW = pTreatmentRemarksNew,
                    DOCT_IMMUN = immun,
                    PRSNT_ILLNESS = hstry, // Corrected spelling
                    GEN_REMARKS = genRmrks,
                    TREATMENT_REMARKS = iTreatmentRemarks,
                    NOTES = pNotes,
                    DOCT_NOTES = pDoctNotes
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

    }


    ////op discount approval
    //public async Task<dynamic> UpdateOpDiscountAppAsync(int requestId, decimal discountAmount, string remarks, decimal discountPercentage, UserTocken ut)
    //{
    //    try
    //    {
    //        // SQL query with placeholders for parameters
    //        var query = @"
    //UPDATE UCHTRANS.OPN_RQST_BILL_DISCOUNT
    //SET DISC_AMT = :DiscountAmount, 
    //    APRVL_REMARKS = :ApprovalRemarks, 
    //    REQUEST_STATUS = 'A', 
    //    APPRVD_ON = SYSDATE, 
    //    DISC_PER = :DiscountPercentage, 
    //    APPRVD_BY = :ApprovedBy
    //WHERE RQST_ID = :RequestId";

    //        // Define the parameters
    //        var parameters = new List<OracleParameter>
    //{
    //    new OracleParameter("DiscountAmount", OracleDbType.Decimal) { Value = discountAmount },
    //    new OracleParameter("ApprovalRemarks", OracleDbType.Varchar2) { Value = remarks },
    //    new OracleParameter("DiscountPercentage", OracleDbType.Decimal) { Value = discountPercentage },
    //    new OracleParameter("ApprovedBy", OracleDbType.Varchar2) { Value = ut.AUSR_ID },
    //    new OracleParameter("RequestId", OracleDbType.Int32) { Value = requestId }
    //};

    //        // Execute the query
    //        using (var connection = _DbContext.Database.GetDbConnection())
    //        {
    //            await connection.OpenAsync();

    //            using (var command = connection.CreateCommand())
    //            {
    //                command.CommandText = query;
    //                command.CommandType = CommandType.Text;
    //                command.Parameters.AddRange(parameters.ToArray());

    //                var rowsAffected = await command.ExecuteNonQueryAsync();
    //                if (rowsAffected > 0)
    //                {
    //                    return new
    //                    {
    //                        Status = 200,
    //                        Message = "Successfully approved"
    //                    };
    //                }
    //            }
    //        }

    //        // If no rows were updated, return an error
    //        return new
    //        {
    //            Status = 500,
    //            Message = "Failed to approve discount"
    //        };
    //    }
    //    catch (Exception ex)
    //    {
    //        return new
    //        {
    //            Status = 500,
    //            Message = ex.Message
    //        };
    //    }
    //}


    ////op discoutn rejection
    //public async Task<dynamic> RejectOPDiscountAsync(int requestId, string approvalRemarks, UserTocken ut)
    //{
    //    try
    //    {
    //        // SQL query with placeholders for parameters
    //        var query = @"
    //UPDATE UCHTRANS.OPN_RQST_BILL_DISCOUNT
    //SET APRVL_REMARKS = :ApprovalRemarks,
    //    REQUEST_STATUS = 'R',
    //    APPRVD_ON = SYSDATE,
    //    APPRVD_BY = :ApprovedBy
    //WHERE RQST_ID = :RequestId";

    //        // Define the parameters
    //        var parameters = new List<OracleParameter>
    //{
    //    new OracleParameter("ApprovalRemarks", OracleDbType.Varchar2) { Value = approvalRemarks },
    //    new OracleParameter("ApprovedBy", OracleDbType.Varchar2) { Value = ut.AUSR_ID },
    //    new OracleParameter("RequestId", OracleDbType.Int32) { Value = requestId }
    //};

    //        // Execute the query
    //        using (var connection = _DbContext.Database.GetDbConnection())
    //        {
    //            await connection.OpenAsync();

    //            using (var command = connection.CreateCommand())
    //            {
    //                command.CommandText = query;
    //                command.CommandType = CommandType.Text;
    //                command.Parameters.AddRange(parameters.ToArray());

    //                var rowsAffected = await command.ExecuteNonQueryAsync();
    //                if (rowsAffected > 0)
    //                {
    //                    return new
    //                    {
    //                        Status = 200,
    //                        Message = "Successfully rejected"
    //                    };
    //                }
    //            }
    //        }

    //        // If no rows were updated, return an error
    //        return new
    //        {
    //            Status = 500,
    //            Message = "Failed to reject discount"
    //        };
    //    }
    //    catch (Exception ex)
    //    {
    //        return new
    //        {
    //            Status = 500,
    //            Message = ex.Message
    //        };
    //    }
    //}

    ////procedure bill update

    //public async Task<dynamic> UpdateLbmRequestAsync(int requestId, string status, decimal discountPercentage, string remarks, decimal discountAmount, UserTocken ut)
    //{
    //    try
    //    {
    //        // SQL query with placeholders for parameters
    //        var query = @"
    //    UPDATE UCHTRANS.LBM_RQST_DISC_OR_CANCEL 
    //    SET APPRVD_BY = :ApprovedBy,
    //        APPRVD_ON = SYSDATE,
    //        RQST_STATUS = :RequestStatus,
    //        DISC_PER = :DiscountPercentage,
    //        REMARKS = :Remarks,
    //        DISC_AMT = :DiscountAmount
    //    WHERE RQST_ID = :RequestId";

    //        // Define the parameters
    //        var parameters = new List<OracleParameter>
    //{
    //    new OracleParameter("ApprovedBy", OracleDbType.Varchar2) { Value = ut.AUSR_ID },
    //    new OracleParameter("RequestStatus", OracleDbType.Varchar2) { Value = status },
    //    new OracleParameter("DiscountPercentage", OracleDbType.Decimal) { Value = discountPercentage },
    //    new OracleParameter("Remarks", OracleDbType.Varchar2) { Value = remarks },
    //    new OracleParameter("DiscountAmount", OracleDbType.Decimal) { Value = discountAmount },
    //    new OracleParameter("RequestId", OracleDbType.Int32) { Value = requestId }
    //};

    //        // Execute the query
    //        using (var connection = _DbContext.Database.GetDbConnection())
    //        {
    //            await connection.OpenAsync();

    //            using (var command = connection.CreateCommand())
    //            {
    //                command.CommandText = query;
    //                command.CommandType = CommandType.Text;
    //                command.Parameters.AddRange(parameters.ToArray());

    //                var rowsAffected = await command.ExecuteNonQueryAsync();

    //                // Return success response if rows were updated
    //                if (rowsAffected > 0)
    //                {
    //                    if(status=="A")
    //                    {
    //                        return new
    //                        {
    //                            Status = 200,
    //                            Message = "successfully Approved"
    //                        };
    //                    }
    //                    else
    //                    {
    //                        return new
    //                        {
    //                            Status = 200,
    //                            Message = "Rejected successfully"
    //                        };
    //                    }

    //                }
    //            }
    //        }

    //        // Return failure response if no rows were updated
    //        return new
    //        {
    //            Status = 500,
    //            Message = "No rows updated"
    //        };
    //    }
    //    catch (Exception ex)
    //    {
    //        // Return error response
    //        return new
    //        {
    //            Status = 500,
    //            Message = $"An error occurred: {ex.Message}"
    //        };
    //    }
    //}





}
