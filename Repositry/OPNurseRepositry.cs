using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using RemediEmr.Class;
using RemediEmr.Data.Class;
using System.Data;
using System.Dynamic;
using static JwtService;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RemediEmr.Repositry
{
    public class OPNurseRepositry
    {


        private readonly AppDbContext _DbContext;

        protected readonly IConfiguration _configuration;

        private readonly JwtHandler jwthand;
        private readonly string con;

        public OPNurseRepositry(AppDbContext dbContext, JwtHandler _jwthand, IConfiguration configuration)
        {
            _DbContext = dbContext;
            jwthand = _jwthand;
            _configuration = configuration;
            con = _DbContext.Database.GetConnectionString();
        }

        public async Task<dynamic> GetVitalsComplaint()
        {
            try
            {
                //var data = await _DbContext.EMR_ADMIN_USERS
                //    .Where(x => x.AUSR_USERNAME == username
                //    && x.AUSR_PWD == password
                //    && x.AUSR_STATUS != "D")
                //    .ToListAsync();


                var VitalsComplaint = _configuration["VitalsComplaint"];



                return new { VitalsComplaint = VitalsComplaint };




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



        public async Task<List<LoginLevel>> GetActiveLoginLevelsAsync()
        {
            try
            {
                // SQL query to execute
                var sql = @"
            SELECT DISTINCT 
                OPNURSE_LOGIN_LEVEL.LEVEL_ID, 
                LEVEL_NAME 
            FROM UCHEMR.OPNURSE_LOGIN_LEVEL_MAPPING MAP
            INNER JOIN UCHMASTER.OPNURSE_LOGIN_LEVEL  
                ON MAP.LEVEL_ID = OPNURSE_LOGIN_LEVEL.LEVEL_ID
            WHERE LEVEL_ACTIVE_STATUS = 'A' 
            ORDER BY LEVEL_ID ASC";

                // List to store results
                var loginLevels = new List<LoginLevel>();

                // Create database command
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;

                    // Open connection if not already open
                    if (cmd.Connection.State != ConnectionState.Open)
                        await cmd.Connection.OpenAsync();

                    // Execute the query and process the result set
                    using (var reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            loginLevels.Add(new LoginLevel
                            {
                                LevelId = reader["LEVEL_ID"]?.ToString(),
                                LevelName = reader["LEVEL_NAME"]?.ToString()
                            });
                        }
                    }
                }

                return loginLevels;
            }
            catch (Exception ex)
            {
                // Log or handle the error as needed
                throw new Exception($"Error retrieving active login levels: {ex.Message}");
            }
        }

        // Model class to map the results
        public class LoginLevel
        {
            public string LevelId { get; set; }
            public string LevelName { get; set; }
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
                                  doc.BRANCH_ID
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

        public async Task<List<dynamic>> GetPAtientVitals(string emrDocId)
        {
            try
            {
                string query = @"
                SELECT * 
                FROM (
                    SELECT 
                        EMP_OFFL_NAME, VISIT_ID, DOCT_ID, HEIGHT, WEIGHT, TEMP, PULSE, UNIT, EVENT_DT, STATUS, MAX_BP, MIN_BP, RR, EMR_DOC_ID, V.EMP_ID, 
                        HCIRCUMFERENCE, BP_LEFTMAX, BP_LEFTMIN, TEMP_UNIT, VITAL_EMP_ID, SPO2, NURSE_NOTE, PAINASSESSMENT, NURSE_REMARKS, GRBS, NURSE,
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/HEIGHT.PNG' HEIGHT_IMG, 
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/WEIGHT.PNG' WEIGHT_IMG,
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/TEMP.PNG' TEMP_IMG, 
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/PULSE.PNG' PULSE_IMG,
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/MAX_BP.PNG' MAX_BP_IMG, 
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/MIN_BP.PNG' MIN_BP_IMG,
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/RR.PNG' RR_IMG, 
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/HCIRCUMFERENCE.PNG' HCIRCUMFERENCE_IMG,
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/BP_LEFTMAX.PNG' BP_LEFTMAX_IMG, 
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/BP_LEFTMIN.PNG' BP_LEFTMIN_IMG,
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/SPO2.PNG' SPO2_IMG, 
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/GRBS.PNG' GRBS_IMG
                    FROM UCHEMR.EMR_PATIENT_VITALS_STATUS V
                    INNER JOIN UCHMASTER.HRM_EMPLOYEE E ON E.EMP_ID = V.EMP_ID
                    WHERE EMR_DOC_ID = :EmrDocId
                    
                    UNION ALL
                    
                    SELECT 
                        EMP_OFFL_NAME, '' VISIT_ID, '' DOCT_ID, HEIGHT, WEIGHT, TEMP, PULSE, UNIT, EVENT_DT, STATUS, MAX_BP, MIN_BP, RR, EMR_DOC_ID, V.EMP_ID, 
                        HCIRCUMFERENCE, BP_LEFTMAX, BP_LEFTMIN, TEMP_UNIT, VITAL_EMP_ID, SPO2, NURSE_NOTE, PAINASSESSMENT, '' NURSE_REMARKS, GRBS, '' NURSE,
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/HEIGHT.PNG' HEIGHT_IMG, 
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/WEIGHT.PNG' WEIGHT_IMG,
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/TEMP.PNG' TEMP_IMG, 
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/PULSE.PNG' PULSE_IMG,
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/MAX_BP.PNG' MAX_BP_IMG, 
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/MIN_BP.PNG' MIN_BP_IMG,
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/RR.PNG' RR_IMG, 
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/HCIRCUMFERENCE.PNG' HCIRCUMFERENCE_IMG,
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/BP_LEFTMAX.PNG' BP_LEFTMAX_IMG, 
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/BP_LEFTMIN.PNG' BP_LEFTMIN_IMG,
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/SPO2.PNG' SPO2_IMG, 
                        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/GRBS.PNG' GRBS_IMG
                    FROM UCHEMR.EMR_PATIENT_VITALS_STATUS_EDIt V
                    INNER JOIN UCHMASTER.HRM_EMPLOYEE E ON E.EMP_ID = V.EMP_ID
                    WHERE EMR_DOC_ID = :EmrDocId
                ) 
                ORDER BY EVENT_DT DESC";

                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;

                    // Add parameters
                    var parameter = new OracleParameter("EmrDocId", OracleDbType.Varchar2)
                    {
                        Value = emrDocId
                    };
                    cmd.Parameters.Add(parameter);

                    if (cmd.Connection.State != ConnectionState.Open)
                        await cmd.Connection.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
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
                throw new Exception($"Error in GetOpNursePatientListAsync: {ex.Message}", ex);
            }
        }

        public async Task<dynamic> ExecuteSpOnlineVitalStatusUpd(Vitals parameters, UserTocken ut)
        {
            // Cast the DbConnection to OracleConnection
            var connection = (OracleConnection)_DbContext.Database.GetDbConnection();

            try
            {
                // Open the connection asynchronously
                await connection.OpenAsync();

                // Create and configure the Oracle command
                using (var command = connection.CreateCommand() as OracleCommand)
                {
                    // Check if command is not null
                    if (command == null)
                    {
                        throw new InvalidOperationException("Failed to create OracleCommand.");
                    }

                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "UCHEMR.SP_ONLINE_VITALSTATUS_UPD";

                    // Adding parameters (ensure types match the procedure)
                    command.Parameters.Add("PATIID", OracleDbType.Varchar2).Value = parameters.PatiId;
                    command.Parameters.Add("VISITID", OracleDbType.Varchar2).Value = parameters.VisitId;
                    command.Parameters.Add("DOCTID", OracleDbType.Varchar2).Value = parameters.DoctId;
                    command.Parameters.Add("EDOCID", OracleDbType.Varchar2).Value = parameters.EdocId;
                    command.Parameters.Add("EMPID", OracleDbType.Varchar2).Value = ut.AUSR_ID;

                    command.Parameters.Add("HEIGHT1", OracleDbType.Decimal).Value = parameters.Height1;
                    command.Parameters.Add("WEIGHT1", OracleDbType.Decimal).Value = parameters.Weight1;
                    command.Parameters.Add("TEMP1", OracleDbType.Decimal).Value = parameters.Temp1;
                    command.Parameters.Add("PULSE1", OracleDbType.Decimal).Value = parameters.Pulse1;
                    command.Parameters.Add("MAXBP", OracleDbType.Decimal).Value = parameters.MaxBp;
                    command.Parameters.Add("MINBP", OracleDbType.Decimal).Value = parameters.MinBp;
                    command.Parameters.Add("RR1", OracleDbType.Decimal).Value = parameters.Rr1;
                    command.Parameters.Add("UNIT1", OracleDbType.Char).Value = parameters.Unit1;
                    command.Parameters.Add("HCIRCUM", OracleDbType.Decimal).Value = parameters.HCircum;
                    command.Parameters.Add("WCIRCUM", OracleDbType.Decimal).Value = parameters.WCircum;
                    command.Parameters.Add("BWEIGHT", OracleDbType.Decimal).Value = parameters.BWeight;
                    command.Parameters.Add("P_GRBS", OracleDbType.Varchar2).Value = parameters.PGrbs;
                    command.Parameters.Add("MAXBPLEFT", OracleDbType.Decimal).Value = parameters.MaxBpLeft;
                    command.Parameters.Add("MINBPLEFT", OracleDbType.Decimal).Value = parameters.MinBpLeft;
                    command.Parameters.Add("P_TEMPUNIT", OracleDbType.Char).Value = parameters.PTempUnit;
                    command.Parameters.Add("P_SPO2", OracleDbType.Decimal).Value = parameters.PSpo2;
                    command.Parameters.Add("P_NURSENOTE", OracleDbType.Varchar2).Value = parameters.PNurseNote;
                    command.Parameters.Add("P_NURSE_REMARKS", OracleDbType.Varchar2).Value = parameters.PNurseRemarks;
                    command.Parameters.Add("P_PAINASSESSMENT", OracleDbType.Char).Value = parameters.PPainAssessment;
                    command.Parameters.Add("P_LOCATION", OracleDbType.Varchar2).Value = parameters.PLocation;
                    command.Parameters.Add("P_REMARKS", OracleDbType.Varchar2).Value = parameters.PRemarks;
                    command.Parameters.Add("P_ASSESSMENTDISTRUS", OracleDbType.Varchar2).Value = parameters.PAssessmentDistrus;
                    command.Parameters.Add("P_BG", OracleDbType.Varchar2).Value = parameters.PBg;
                    command.Parameters.Add("P_FSH", OracleDbType.Varchar2).Value = parameters.PFsh;
                    command.Parameters.Add("P_SUPPLEMENTAL", OracleDbType.Varchar2).Value = parameters.PSupplemental;
                    command.Parameters.Add("P_LEVEL_OF_CONSCIOUSNESS", OracleDbType.Varchar2).Value = parameters.PLevelOfConsciousness;
                    command.Parameters.Add("P_DURATION", OracleDbType.Varchar2).Value = parameters.PDuration;
                    command.Parameters.Add("P_PAIN_RADIATING", OracleDbType.Varchar2).Value = parameters.PPainRadiating;
                    command.Parameters.Add("P_ONSET_PAIN", OracleDbType.Varchar2).Value = parameters.POnsetPain;
                    command.Parameters.Add("P_CLINIC_TRIAGE", OracleDbType.Varchar2).Value = parameters.PClinicTriage;
                    command.Parameters.Add("P_PRIORITY", OracleDbType.Varchar2).Value = parameters.PPriority;
                    command.Parameters.Add("P_BP_DOWN", OracleDbType.Varchar2).Value = parameters.PBpDown;
                    command.Parameters.Add("P_BP_UP", OracleDbType.Varchar2).Value = parameters.PBpUp;
                    command.Parameters.Add("P_HEALTH_EDUCATION", OracleDbType.Char).Value = parameters.PHealthEducation;
                    command.Parameters.Add("P_URINE_ACR", OracleDbType.Varchar2).Value = parameters.PUrineAcr;
                    command.Parameters.Add("P_HBA1C", OracleDbType.Varchar2).Value = parameters.PHba1c;
                    command.Parameters.Add("P_VITAMIN_D", OracleDbType.Varchar2).Value = parameters.PVitaminD;
                    command.Parameters.Add("P_NOTES", OracleDbType.Varchar2).Value = parameters.PNotes;
                    command.Parameters.Add("P_BLD_GRP_ID", OracleDbType.Varchar2).Value = parameters.PBldGrpId;

                    // Execute the command asynchronously
                    await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();

                    var vasl =  UpdateCurrentMedication(parameters.EdocId, parameters.currentmedication, parameters.PatiId);
                    var vasl2 = await SubmitPatientDetailsAsync(parameters.Ve);

                    if (vasl == 1 && vasl2 == true)
                    {
                        return new { Status = 200, Message = "inserted successfully." };

                    }
                    else
                    {

                        return new { Status = 505, Message = "Error while insertion." };
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception($"Error in SP_OPNURSE_PATI_LR_LIST: {ex.Message}", ex);
            }
            finally
            {
                // Ensure connection is closed if open
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }

        public int UpdateCurrentMedication(string emrDocId, string currentMed, string patientId)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(con))
                {
                    using (OracleCommand cmd = new OracleCommand("UCHTRANS.SP_CURRENT_MED_INS", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("P_EMR_DOC_ID", OracleDbType.Varchar2).Value = emrDocId;
                        cmd.Parameters.Add("P_CURR_MED", OracleDbType.Varchar2).Value = currentMed;
                        cmd.Parameters.Add("P_PATI_ID", OracleDbType.Varchar2).Value = patientId;
                        cmd.Parameters.Add("RETVAL", OracleDbType.Int32).Direction = ParameterDirection.Output;

                        conn.Open();
                        cmd.ExecuteNonQuery();
                        OracleDecimal retval = (OracleDecimal)cmd.Parameters["RETVAL"].Value;
                        return retval.ToInt32();
                    }
                }
            }
            catch (OracleException ex)
            {
                // Log the Oracle-specific error
                Console.WriteLine($"Oracle Error: {ex.Message}");
                return -1; // Indicate failure with a specific code
            }
            catch (Exception ex)
            {
                // Log generic errors
                Console.WriteLine($"Error: {ex.Message}");
                return -1;
            }
        }

        //added by siraj //14-feb-2024 new fields added in vitals
        public async Task<string> GetPreviousMedicationAsync(string emrDocId)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(con))
                {
                    string query = "SELECT PREV_MEDI FROM UCHEMR.EMR_OPTHOLOGY_DETAILS WHERE EMR_DOC_ID = :EMR_DOC_ID";
                    await conn.OpenAsync();

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("EMR_DOC_ID", OracleDbType.Varchar2).Value = emrDocId;

                        using (OracleDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return reader["PREV_MEDI"] != DBNull.Value ? reader["PREV_MEDI"].ToString() : null;
                            }
                        }
                    }
                }
                return null; // No record found
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"Oracle Error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
        // Helper method to add parameters to the command
        private void AddParameter(OracleCommand command, string name, OracleDbType dbType, object value)
        {
            var param = command.Parameters.Add(name, dbType);
            param.Value = value ?? DBNull.Value; // Ensure null values are handled correctly
        }


        //using for lab taken list
        public async Task<List<dynamic>> GetOpNursePatiLRListAsync(
        string empId, string sctId, string name = "", string mobile = "", string cprNo = "", string visitType = "", int? groupId = 0, DateTime? date = null, string listType = "", string levelId = "", string branchId = "")
        {
            try
            {
                var sql = @"
            BEGIN 
                UCHEMR.SP_OPNURSE_PATI_LR_LIST(
                    :EMPID, 
                    :SCTID, 
                    :NAME, 
                    :MOBILE, 
                    :CPRNO, 
                    :VISITTYPE, 
                    :P_GRPID, 
                    :P_DATE, 
                    :P_LIST_TYPE, 
                    :P_LEVEL_ID, 
                    :P_BRANCH_ID, 
                    :STROUT
                ); 
            END;";

                // Define Oracle parameters
                var parameters = new List<OracleParameter>
        {
            new OracleParameter("EMPID", OracleDbType.Varchar2) { Value = empId ?? (object)DBNull.Value },
            new OracleParameter("SCTID", OracleDbType.Varchar2) { Value = sctId ?? (object)DBNull.Value },
            new OracleParameter("NAME", OracleDbType.Varchar2) { Value = name ?? (object)DBNull.Value },
            new OracleParameter("MOBILE", OracleDbType.Varchar2) { Value = mobile ?? (object)DBNull.Value },
            new OracleParameter("CPRNO", OracleDbType.Varchar2) { Value = cprNo ?? (object)DBNull.Value },
            new OracleParameter("VISITTYPE", OracleDbType.Varchar2) { Value = visitType ?? (object)DBNull.Value },
            new OracleParameter("P_GRPID", OracleDbType.Int32) { Value = groupId ?? (object)DBNull.Value },
            new OracleParameter("P_DATE", OracleDbType.Date) { Value = date ?? (object)DBNull.Value },
            new OracleParameter("P_LIST_TYPE", OracleDbType.Char) { Value = listType ?? (object)DBNull.Value },
            new OracleParameter("P_LEVEL_ID", OracleDbType.Varchar2) { Value = levelId ?? (object)DBNull.Value },
            new OracleParameter("P_BRANCH_ID", OracleDbType.Varchar2) { Value = branchId ?? (object)DBNull.Value },
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
                throw new Exception($"Error in SP_OPNURSE_PATI_LR_LIST: {ex.Message}", ex);
            }
        }



        //patient list in other cases
        public async Task<List<dynamic>> GetOpNursePatientListAsync(string empId, string sctId, string name = "", string mobile = "", string cprNo = "", string visitType = "", int? groupId = 0, DateTime? date = null, string listType = "", string levelId = "",
       string branchId = "")
        {
            try
            {
                var sql = @"BEGIN 
                        UCHEMR.SP_OPNURSE_PATIENT_LIST(
                            :EMPID, 
                            :SCTID, 
                            :NAME, 
                            :MOBILE, 
                            :CPRNO, 
                            :VISITTYPE, 
                            :P_GRPID, 
                            :P_DATE, 
                            :P_LIST_TYPE, 
                            :P_LEVEL_ID, 
                            :P_BRANCH_ID, 
                            :STROUT
                        ); 
                    END;";

                var parameters = new List<OracleParameter>
        {
            new OracleParameter("EMPID", OracleDbType.Varchar2) { Value = empId },
            new OracleParameter("SCTID", OracleDbType.Varchar2) { Value = sctId },
            new OracleParameter("NAME", OracleDbType.Varchar2) { Value = name ?? (object)DBNull.Value },
            new OracleParameter("MOBILE", OracleDbType.Varchar2) { Value = mobile ?? (object)DBNull.Value },
            new OracleParameter("CPRNO", OracleDbType.Varchar2) { Value = cprNo ?? (object)DBNull.Value },
            new OracleParameter("VISITTYPE", OracleDbType.Varchar2) { Value = visitType ?? (object)DBNull.Value },
            new OracleParameter("P_GRPID", OracleDbType.Int32) { Value = groupId ?? (object)DBNull.Value },
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

                    if (cmd.Connection.State != ConnectionState.Open)
                        await cmd.Connection.OpenAsync();

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
                throw new Exception($"Error in SP_OPNURSE_PATIENT_LIST: {ex.Message}", ex);
            }
        }

        public async Task<List<dynamic>> ExecuteOpNursePatiLrsListAsync(
     string empId, string sctId, string name = "", string mobile = "", string cprNo = "", string visitType = "", int? groupId = 0, DateTime? date = null, string listType = "", string levelId = "", string branchId = "")
        {
            try
            {
                var sql = @"
            BEGIN 
                UCHEMR.SP_OPNURSE_PATI_LRS_LIST(
                    :EMPID, 
                    :SCTID, 
                    :NAME, 
                    :MOBILE, 
                    :CPRNO, 
                    :VISITTYPE, 
                    :P_GRPID, 
                    :P_DATE, 
                    :P_LIST_TYPE, 
                    :P_LEVEL_ID, 
                    :P_BRANCH_ID, 
                    :STROUT
                ); 
            END;";

                // Define Oracle parameters
                var parameters = new List<OracleParameter>
        {
            new OracleParameter("EMPID", OracleDbType.Varchar2) { Value = empId ?? (object)DBNull.Value },
            new OracleParameter("SCTID", OracleDbType.Varchar2) { Value = sctId ?? (object)DBNull.Value },
            new OracleParameter("NAME", OracleDbType.Varchar2) { Value = name ?? (object)DBNull.Value },
            new OracleParameter("MOBILE", OracleDbType.Varchar2) { Value = mobile ?? (object)DBNull.Value },
            new OracleParameter("CPRNO", OracleDbType.Varchar2) { Value = cprNo ?? (object)DBNull.Value },
            new OracleParameter("VISITTYPE", OracleDbType.Varchar2) { Value = visitType ?? (object)DBNull.Value },
            new OracleParameter("P_GRPID", OracleDbType.Int32) { Value = groupId ?? (object)DBNull.Value },
            new OracleParameter("P_DATE", OracleDbType.Date) { Value = date ?? (object)DBNull.Value },
            new OracleParameter("P_LIST_TYPE", OracleDbType.Char) { Value = listType ?? (object)DBNull.Value },
            new OracleParameter("P_LEVEL_ID", OracleDbType.Varchar2) { Value = levelId ?? (object)DBNull.Value },
            new OracleParameter("P_BRANCH_ID", OracleDbType.Varchar2) { Value = branchId ?? (object)DBNull.Value },
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
                throw new Exception($"Error in SP_OPNURSE_PATI_LRS_LIST: {ex.Message}", ex);
            }
        }




        public async Task<int> GetActiveLabTakenStatusCountAsyncother()
        {
            try
            {
                // Define the SQL query
                var sql = "SELECT COUNT(*) FROM UCHMASTER.EMR_LAB_TAKEN_STATUS WHERE NVL(ACTIVE_STATUS, 'A') = 'A'";

                // Initialize the result variable
                int count = 0;

                // Create the database command
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;

                    // Open the connection if not already open
                    if (cmd.Connection.State != ConnectionState.Open)
                        await cmd.Connection.OpenAsync();

                    // Execute the query and get the result
                    count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                return count;
            }
            catch (Exception ex)
            {
                // Handle and log the error as needed
                throw new Exception($"Error executing query: {ex.Message}");
            }
        }

        public async Task<int> InsertOrUpdateEmrLabTakenAsync(string emrDocId, int eltsId, string notes, char type, UserTocken ut)
        {
            try
            {
                var sql = @"BEGIN UCHTRANS.SP_INS_EMR_LAB_TAKEN(
                  :P_EMR_DOC_ID,
                  :P_ELTS_ID,
                  :P_NOTES,
                  :P_CREATE_USER,
                  :P_TYPE,
                  :RETVAL
              ); END;";

                // Define the Oracle parameters
                var parameters = new List<OracleParameter>
        {
            new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = emrDocId },
            new OracleParameter("P_ELTS_ID", OracleDbType.Int32) { Value = eltsId },
            new OracleParameter("P_NOTES", OracleDbType.Varchar2) { Value = notes ?? (object)DBNull.Value },
            new OracleParameter("P_CREATE_USER", OracleDbType.Varchar2) { Value = ut.AUSR_ID },
            new OracleParameter("P_TYPE", OracleDbType.Char) { Value = type },
            new OracleParameter("RETVAL", OracleDbType.Int32) { Direction = ParameterDirection.Output }
        };

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

                    // Handle RETVAL as OracleDecimal and convert to int
                    var retval = cmd.Parameters["RETVAL"].Value;

                    // If the value is OracleDecimal, convert it to int
                    if (retval is OracleDecimal oracleDecimal)
                    {
                        return oracleDecimal.ToInt32();
                    }
                    else
                    {
                        return Convert.ToInt32(retval);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle errors
                throw new Exception($"Error in SP_INS_EMR_LAB_TAKEN: {ex.Message}", ex);
            }
        }


        public async Task<List<string>> GetEmrDocIdsAsync(string emrDocId)
        {
            try
            {
                // SQL query with a placeholder for the EMR_DOC_ID parameter
                var sql = @"
            SELECT E.EMR_DOC_ID 
            FROM UCHTRANS.OPN_VISIT_MASTER V
            INNER JOIN UCHEMR.EMR_DOCUMENT E ON E.OPVISIT_ID = V.OPVISIT_ID
            INNER JOIN (
                SELECT PATI_ID, TO_DATE(EMR_DOC_DATE) EMR_DOC_DATE 
                FROM UCHEMR.EMR_DOCUMENT 
                WHERE EMR_DOC_ID = :EMR_DOC_ID
            ) CMP ON CMP.PATI_ID = V.PATI_ID 
            AND TO_DATE(CMP.EMR_DOC_DATE) = TO_DATE(E.EMR_DOC_DATE)
            WHERE NVL(CAMP_VISIT, 'N') = 'Y'";

                // Define the Oracle parameter
                var parameters = new List<OracleParameter>
        {
            new OracleParameter("EMR_DOC_ID", OracleDbType.Varchar2) { Value = emrDocId }
        };

                // List to store the results
                var result = new List<string>();

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
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(reader["EMR_DOC_ID"].ToString());
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                // Handle errors
                throw new Exception($"Error executing query: {ex.Message}");
            }
        }

        public async Task<List<dynamic>> GetEmrDocIdBeforesubmitsAsync(string emrDocId)
        {
            try
            {
                // SQL query with a placeholder for the EMR_DOC_ID parameter
                string query = @"SELECT A.EMR_DOC_ID, A.HEIGHT, A.WEIGHT, A.TEMP, A.PULSE, A.MAX_BP, A.MIN_BP, A.EVENT_DT, A.RR, A.HCIRCUMFERENCE, A.WCIRCUMFERENCE, 
                                A.BIRTH_WEIGHT, DSG_NAME, BG, DECODE(DESGP_ID, 'UCHM000003', EMP.EMP_OFFL_NAME, NULL) EMP_OFFL_NAME, 
                                EMPV.EMP_OFFL_NAME VIT_EMP_NAME, BP_LEFTMAX, BP_LEFTMIN, TEMP_UNIT, EDD_BEGIN_DT, SCANNED_EDD, ISPRGNT, A.SPO2, 
                                NURSE_NOTE, PAINASSESSMENT, LOCATION, PAINREMARKS, PATIENTASSESSMENTDISTRESSLEVEL, NURSE_REMARKS, GRBS, 
                                LEVEL_OF_CONSCIOUSNESS, SUPPLEMENTAL, DURATION, PAIN_RADIATING, ONSET_PAIN, CLINIC_TRIAGE, PRIORITY, HEALTH_EDUCATION, 
                                A.BP_UP, A.BP_DOWN, A.URINE_ACR, A.HBA1C, A.VITAMIN_D, A.NOTES, A.FSH
                                FROM UCHEMR.EMR_PATIENT_VITALS_STATUS A
                                LEFT JOIN UCHMASTER.HRM_EMPLOYEE EMP ON EMP.EMP_ID = A.EMP_ID
                                LEFT JOIN UCHMASTER.HRM_EMPLOYEE EMPV ON EMPV.EMP_ID = A.VITAL_EMP_ID
                                LEFT JOIN UCHMASTER.HRM_DESIGNATION CC ON CC.DSG_ID = EMP.EMP_CUR_DSG_ID
                                LEFT JOIN UCHEMR.EMR_PATI_EDD EDD ON EDD.PATI_ID = A.PATI_ID
                                WHERE A.EMR_DOC_ID = :emrDocId
                                ORDER BY A.EVENT_DT DESC";


                // Define the Oracle parameter
                var parameters = new List<OracleParameter>
        {
            new OracleParameter("EMR_DOC_ID", OracleDbType.Varchar2) { Value = emrDocId }
        };

                // List to store the results
                var result = new List<string>();

                // Execute the query
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = query;
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

                //return result;
            }
            catch (Exception ex)
            {
                // Handle errors
                throw new Exception($"Error executing query: {ex.Message}");
            }
        }

        //insert audiology
        public async Task<int> SaveAudiologyAsync(string emrDocId, string history, string notes, string createdBy)
        {
            // Define the output parameter
            var retvalParameter = new OracleParameter
            {
                ParameterName = "RETVAL",
                DbType = System.Data.DbType.Int32,
                Direction = System.Data.ParameterDirection.Output
            };

            // Prepare the SQL command for calling the procedure
            var sql = @"
        BEGIN
            UCHEMR.SP_AUDIOLOGY_SAVE(
                :P_EMR_DOC_ID,
                :P_HISTORY,
                :P_NOTES,
                :P_CREATED_BY,
                :RETVAL
            );
        END;";

            // Add the input parameters
            var parameters = new[]
            {
        new OracleParameter("P_EMR_DOC_ID", emrDocId),
        new OracleParameter("P_HISTORY", history),
        new OracleParameter("P_NOTES", notes),
        new OracleParameter("P_CREATED_BY", createdBy),
        retvalParameter
    };

            // Execute the command
            using var transaction = await _DbContext.Database.BeginTransactionAsync();
            try
            {
                await _DbContext.Database.ExecuteSqlRawAsync(sql, parameters);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            // Return the output parameter value
            return Convert.ToInt32(retvalParameter.Value);
        }


        public async Task<dynamic?> GetAudiologyByEmrDocIdAndCurrentDateAsync(string emrDocId)
        {
            if (string.IsNullOrWhiteSpace(emrDocId))
                throw new ArgumentException("EMR_DOC_ID cannot be null or empty.", nameof(emrDocId));

            try
            {
                DateTime today = DateTime.Today; // Current date without time

                // SQL query to fetch data
                string sqlQuery = @"
            SELECT EMR_DOC_ID, HISTORY, NOTES, CREATED_BY, CREATED_ON
            FROM UCHEMR.AUDIOLOGY
            WHERE EMR_DOC_ID = :EmrDocId
              AND TRUNC(CREATED_ON) = TRUNC(SYSDATE)";

                // Parameters for the SQL query
                var parameters = new[]
                {
            new OracleParameter("EmrDocId", emrDocId)
        };

                var result = new List<string>();

                // Execute the query
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sqlQuery;
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
                // Log the exception
                throw new Exception("An error occurred while fetching audiology data.", ex);
            }
        }


        //SELECT ROOM_NO, ROOM_ID from UCHDISPLAY.CONSULT_ROOM where STATUS = 'A'
        public async Task<List<dynamic>> GetAllRoom()
        {
            try
            {
                // SQL query to retrieve active lab taken status
                var sql = "SELECT ROOM_NO, ROOM_ID from UCHDISPLAY.CONSULT_ROOM where STATUS = 'A'";

                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;

                    await _DbContext.Database.GetDbConnection().OpenAsync();

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
                throw new Exception($"Error fetching GetAllRoom: {ex.Message}", ex);
            }
        }


        public async Task<List<dynamic>> GetEmrLabTakenStatusAsync()
        {
            try
            {
                // SQL query to retrieve active lab taken status
                var sql = "SELECT * FROM UCHMASTER.EMR_LAB_TAKEN_STATUS WHERE NVL(ACTIVE_STATUS, 'A') = 'A'";

                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;

                    await _DbContext.Database.GetDbConnection().OpenAsync();

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
                throw new Exception($"Error fetching EMR_LAB_TAKEN_STATUS: {ex.Message}", ex);
            }
        }


        public async Task<List<dynamic>> patilrlist()
        {
            try
            {
                // SQL query to retrieve active lab taken status
                var sql = "SELECT * FROM UCHMASTER.EMR_LAB_TAKEN_STATUS WHERE NVL(ACTIVE_STATUS, 'A') = 'A'";

                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;

                    await _DbContext.Database.GetDbConnection().OpenAsync();

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
                throw new Exception($"Error fetching EMR_LAB_TAKEN_STATUS: {ex.Message}", ex);
            }
        }


        public async Task<int> UpdateVitalsStatusAsync(string emrDocId)
        {
            try
            {
                // SQL query to update the EMR_DOCUMENT table
                var sql = @"
            UPDATE UCHEMR.EMR_DOCUMENT 
            SET NO_VITALS_USER = '', 
                NO_VITALS_DATE = SYSDATE, 
                NO_VITALS_STATUS = 'Y' 
            WHERE EMR_DOC_ID = :emrDocId";

                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;

                    // Add parameter to prevent SQL injection
                    var parameter = cmd.CreateParameter();
                    parameter.ParameterName = "emrDocId";
                    parameter.Value = emrDocId;
                    cmd.Parameters.Add(parameter);

                    // Open the database connection
                    await _DbContext.Database.GetDbConnection().OpenAsync();

                    // Execute the update query and return the number of affected rows
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating vitals status: {ex.Message}", ex);
            }
        }




        public async Task<dynamic> SaveOphthalmologyDetailss(OpthalmologyDetails ophthalmologyParameters, UserTocken ut)
        {
            try
            {
                // Define the output parameter
                var retvalParameter = new OracleParameter
                {
                    ParameterName = "RETVAL",
                    DbType = System.Data.DbType.Int32,
                    Direction = System.Data.ParameterDirection.Output
                };

                // Define the SQL procedure call
                var sql = @"BEGIN UCHEMR.SP_OPHTHALMOLOGY_DETAIL_SAVE(
            :P_EMR_DOC_ID, :P_PATIID,  :P_CREATE_USER, :P_RETINO_OD_SPH, :P_RETINI_OD_DYCL, 
            :P_RETINO_OD_AXIS, :P_RETINO_OD_METHOD, :P_RETINO_OS_SPH, :P_RETINI_OS_DYCL, 
            :P_RETINO_OS_AXIS, :P_RETINO_OS_METHOD, :P_REFRA_OD_SPH, :P_REFRA_OD_DYCL, 
            :P_REFRA_OD_AXIS, :P_REFRA_OD_ADD, :P_REFRA_OD_METHOD, :P_REFRA_OS_SPH, 
            :P_REFRA_OS_DYCL, :P_REFRA_OS_AXIS, :P_REFRA_OS_ADD, :P_REFRA_OS_OPTOM, :P_OD_UCDVA, 
            :P_OD_BCDVA, :P_OD_UCNVA, :P_OD_BCNVA, :P_OS_UCDVA, :P_OS_BCDVA, :P_OS_UCNVA, 
            :P_OS_BCNVA, :P_PREV_OD_SPH, :P_PREV_OD_DCYL, :P_PREV_OD_AXIS, :P_PREV_OD_DV, 
            :P_PREV_OD_NV, :P_PREV_OD_TYPE, :P_PREV_OD_DURATION, :P_PREV_OS_SPH, 
            :P_PREV_OS_DCYL, :P_PREV_OS_AXIS, :P_PREV_OS_DV, :P_PREV_OS_NV, :P_IOP_METHOD, 
            :P_IPO_OD, :P_IOP_OS, :P_PACHY_METHOD, :P_PACHY_OD, :P_PACHY_OS, :P_COLOR_OD_TEST, 
            :P_COCLOR_OD_RESULT, :P_COLOR_OS_TEST, :P_COCLOR_OS_RESULT, :P_SENSORY_TEST, 
            :P_SENSORY_CORRECTION, :P_SENSORY_DIST, :P_SENSORY_NEAR, :P_EXOP_BASE, :P_EXOP_OD, 
            :P_EXOP_OS, :P_OD_EOMWORK1, :P_OD_EOMWORK2, :P_OD_EOMWORK3, :P_OD_EOMWORK4, 
            :P_OD_EOMWORK5, :P_OD_EOMWORK6, :P_OD_EOMWORK7, :P_OD_EOMWORK8, :P_OS_EOMWORK1, 
            :P_OS_EOMWORK2, :P_OS_EOMWORK3, :P_OS_EOMWORK4, :P_OS_EOMWORK5, :P_OS_EOMWORK6, 
            :P_OS_EOMWORK7, :P_OS_EOMWORK8, :P_ORTHO_SC, :P_ORTHO_CC, :P_ALIGN_WITHOUT, 
            :P_ALIGN_WITH, :P_OD_EYELID, :P_OD_CONJU, :P_OD_SCLERA, :P_OD_CORNEA, :P_OD_CHAMBER, 
            :P_OS_EYELID, :P_OS_CONJU, :P_OS_SCLERA, :P_OS_CORNEA, :P_OS_CHAMBER, :P_OD_GONIO1, 
            :P_OD_GONIO2, :P_OD_GONIO3, :P_OD_GONIO4, :P_OD_GONIO5, :P_OS_GONIO1, :P_OS_GONIO2, 
            :P_OS_GONIO3, :P_OS_GONIO4, :P_OS_GONIO5, :P_OD_IRIS, :P_OD_PUPIL, :P_OD_LENS, 
            :P_OD_PUPIL_DIAL, :P_OS_IRIS, :P_OS_PUPIL, :P_OS_LENS, :P_OS_PUPIL_DIAL, :P_OD_NERVE, 
            :P_OD_VITREOUS, :P_OD_RETINA, :P_OD_MACULA, :P_OD_PRIPHERY, :P_OS_NERVE, 
            :P_OS_VITREOUS, :P_OS_RETINA, :P_OS_MACULA, :P_OS_PRIPHERY, :P_DIAGNOSIS, 
            :P_TREATMENT_PLAN, :P_ORTHO_CCOD, :P_ORTHO_SCOS, :P_PREV_OD_ADD , :P_PREV_OS_ADD, :P_REFRA_OD_NOTES, :RETVAL); END;";

                // Add the input parameters
                var parameters = new List<OracleParameter>
        {
            new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = ophthalmologyParameters.EmrDocId ?? (object)DBNull.Value},
            new OracleParameter("P_PATIID", OracleDbType.Varchar2) { Value = ophthalmologyParameters.PatiId ?? (object)DBNull.Value},
            new OracleParameter("P_CREATE_USER", OracleDbType.Varchar2) { Value = ut.AUSR_ID },
            new OracleParameter("P_RETINO_OD_SPH", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RetinoOdSph ?? (object)DBNull.Value},
            new OracleParameter("P_RETINI_OD_DYCL", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RetiniOdDycl ?? (object)DBNull.Value},
            new OracleParameter("P_RETINO_OD_AXIS", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RetinoOdAxis ?? (object)DBNull.Value},
            new OracleParameter("P_RETINO_OD_METHOD", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RetinoOdMethod ?? (object)DBNull.Value},
            new OracleParameter("P_RETINO_OS_SPH", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RetinoOsSph ?? (object)DBNull.Value },
            new OracleParameter("P_RETINI_OS_DYCL", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RetiniOsDycl ?? (object)DBNull.Value},
            new OracleParameter("P_RETINO_OS_AXIS", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RetinoOsAxis ?? (object)DBNull.Value},
            new OracleParameter("P_RETINO_OS_METHOD", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RetinoOsMethod ?? (object)DBNull.Value},
            new OracleParameter("P_REFRA_OD_SPH", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RefraOdSph ?? (object)DBNull.Value},
            new OracleParameter("P_REFRA_OD_DYCL", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RefraOdDycl ?? (object)DBNull.Value},
            new OracleParameter("P_REFRA_OD_AXIS", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RefraOdAxis ?? (object)DBNull.Value},
            new OracleParameter("P_REFRA_OD_ADD", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RefraOdAdd ?? (object)DBNull.Value},
            new OracleParameter("P_REFRA_OD_METHOD", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RefraOdMethod ?? (object)DBNull.Value},
            new OracleParameter("P_REFRA_OS_SPH", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RefraOsSph ?? (object)DBNull.Value},
            new OracleParameter("P_REFRA_OS_DYCL", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RefraOsDycl ?? (object)DBNull.Value},
            new OracleParameter("P_REFRA_OS_AXIS", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RefraOsAxis ?? (object)DBNull.Value},
            new OracleParameter("P_REFRA_OS_ADD", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RefraOsAdd ?? (object)DBNull.Value},   //20
            new OracleParameter("P_REFRA_OS_OPTOM", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RefraOsOptom ?? (object)DBNull.Value},
            new OracleParameter("P_OD_UCDVA", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdUcdva ?? (object)DBNull.Value},
            new OracleParameter("P_OD_BCDVA", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdBcdva ?? (object)DBNull.Value},
            new OracleParameter("P_OD_UCNVA", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdUcnva ?? (object)DBNull.Value},
            new OracleParameter("P_OD_BCNVA", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdBcnva ?? (object)DBNull.Value},
            new OracleParameter("P_OS_UCDVA", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsUcdva ?? (object)DBNull.Value},
            new OracleParameter("P_OS_BCDVA", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsBcdva ?? (object)DBNull.Value},
            new OracleParameter("P_OS_UCNVA", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsUcnva ?? (object)DBNull.Value},
            new OracleParameter("P_OS_BCNVA", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsBcnva ?? (object)DBNull.Value},
            new OracleParameter("P_PREV_OD_SPH", OracleDbType.Varchar2) { Value = ophthalmologyParameters.PrevOdSph ?? (object)DBNull.Value},  //30
            new OracleParameter("P_PREV_OD_DCYL", OracleDbType.Varchar2) { Value = ophthalmologyParameters.PrevOdDcyl ?? (object)DBNull.Value},
            new OracleParameter("P_PREV_OD_AXIS", OracleDbType.Varchar2) { Value = ophthalmologyParameters.PrevOdAxis ?? (object)DBNull.Value},
            new OracleParameter("P_PREV_OD_DV", OracleDbType.Varchar2) { Value = ophthalmologyParameters.PrevOdDv ?? (object)DBNull.Value},
            new OracleParameter("P_PREV_OD_NV", OracleDbType.Varchar2) { Value = ophthalmologyParameters.PrevOdNv ?? (object)DBNull.Value},
            new OracleParameter("P_PREV_OD_TYPE", OracleDbType.Varchar2) { Value = ophthalmologyParameters.PrevOdType ?? (object)DBNull.Value},
            new OracleParameter("P_PREV_OD_DURATION", OracleDbType.Varchar2) { Value = ophthalmologyParameters.PrevOdDuration ?? (object)DBNull.Value},
            new OracleParameter("P_PREV_OS_SPH", OracleDbType.Varchar2) { Value = ophthalmologyParameters.PrevOsSph ?? (object)DBNull.Value},
            new OracleParameter("P_PREV_OS_DCYL", OracleDbType.Varchar2) { Value = ophthalmologyParameters.PrevOsDcyl ?? (object)DBNull.Value},
            new OracleParameter("P_PREV_OS_AXIS", OracleDbType.Varchar2) { Value = ophthalmologyParameters.PrevOsAxis ?? (object)DBNull.Value},
            new OracleParameter("P_PREV_OS_DV", OracleDbType.Varchar2) { Value = ophthalmologyParameters.PrevOsDv ?? (object)DBNull.Value},  //40
            new OracleParameter("P_PREV_OS_NV", OracleDbType.Varchar2) { Value = ophthalmologyParameters.PrevOsNv ?? (object)DBNull.Value},
            new OracleParameter("P_IOP_METHOD", OracleDbType.Varchar2) { Value = ophthalmologyParameters.IopMethod ?? (object)DBNull.Value},
            new OracleParameter("P_IPO_OD", OracleDbType.Varchar2) { Value = ophthalmologyParameters.IpoOd ?? (object)DBNull.Value},
            new OracleParameter("P_IOP_OS", OracleDbType.Varchar2) { Value = ophthalmologyParameters.IopOs ?? (object)DBNull.Value},
            new OracleParameter("P_PACHY_METHOD", OracleDbType.Varchar2) { Value = ophthalmologyParameters.PachyMethod ?? (object)DBNull.Value},
            new OracleParameter("P_PACHY_OD", OracleDbType.Varchar2) { Value = ophthalmologyParameters.PachyOd ?? (object)DBNull.Value},
            new OracleParameter("P_PACHY_OS", OracleDbType.Varchar2) { Value = ophthalmologyParameters.PachyOs ?? (object)DBNull.Value},
            new OracleParameter("P_COLOR_OD_TEST", OracleDbType.Varchar2) { Value = ophthalmologyParameters.ColorOdTest ?? (object)DBNull.Value},
            new OracleParameter("P_COCLOR_OD_RESULT", OracleDbType.Varchar2) { Value = ophthalmologyParameters.CocolorOdResult ?? (object)DBNull.Value},
            new OracleParameter("P_COLOR_OS_TEST", OracleDbType.Varchar2) { Value = ophthalmologyParameters.ColorOsTest ?? (object)DBNull.Value},  //50
            new OracleParameter("P_COCLOR_OS_RESULT", OracleDbType.Varchar2) { Value = ophthalmologyParameters.CocolorOsResult ?? (object)DBNull.Value},
            new OracleParameter("P_SENSORY_TEST", OracleDbType.Varchar2) { Value = ophthalmologyParameters.SensoryTest ?? (object)DBNull.Value},   //52


             new OracleParameter("P_SENSORY_CORRECTION", OracleDbType.Varchar2) { Value = ophthalmologyParameters.SensoryCorrection ?? (object)DBNull.Value},
            new OracleParameter("P_SENSORY_DIST", OracleDbType.Varchar2) { Value = ophthalmologyParameters.SensoryDist ?? (object)DBNull.Value},
            new OracleParameter("P_SENSORY_NEAR", OracleDbType.Varchar2) { Value = ophthalmologyParameters.SensoryNear ?? (object)DBNull.Value},
            new OracleParameter("P_EXOP_BASE", OracleDbType.Varchar2) { Value = ophthalmologyParameters.ExopBase ?? (object)DBNull.Value},
            new OracleParameter("P_EXOP_OD", OracleDbType.Varchar2) { Value = ophthalmologyParameters.ExopOd ?? (object)DBNull.Value},
            new OracleParameter("P_EXOP_OS", OracleDbType.Varchar2) { Value = ophthalmologyParameters.ExopOs ?? (object)DBNull.Value},
            new OracleParameter("P_OD_EOMWORK1", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdEomwork1 ?? (object)DBNull.Value},
            new OracleParameter("P_OD_EOMWORK2", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdEomwork2 ?? (object)DBNull.Value},
            new OracleParameter("P_OD_EOMWORK3", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdEomwork3 ?? (object)DBNull.Value},
            new OracleParameter("P_OD_EOMWORK4", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdEomwork4 ?? (object)DBNull.Value},
            new OracleParameter("P_OD_EOMWORK5", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdEomwork5 ?? (object)DBNull.Value},
            new OracleParameter("P_OD_EOMWORK6", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdEomwork6 ?? (object)DBNull.Value},
            new OracleParameter("P_OD_EOMWORK7", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdEomwork7 ?? (object)DBNull.Value},
            new OracleParameter("P_OD_EOMWORK8", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdEomwork8 ?? (object)DBNull.Value},
            new OracleParameter("P_OS_EOMWORK1", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsEomwork1 ?? (object)DBNull.Value},
            new OracleParameter("P_OS_EOMWORK2", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsEomwork2 ?? (object)DBNull.Value},   //16


             new OracleParameter("P_OS_EOMWORK3", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsEomwork3 ?? (object)DBNull.Value},
            new OracleParameter("P_OS_EOMWORK4", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsEomwork4 ?? (object)DBNull.Value},
            new OracleParameter("P_OS_EOMWORK5", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsEomwork5 ?? (object)DBNull.Value},
            new OracleParameter("P_OS_EOMWORK6", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsEomwork6  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_EOMWORK7", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsEomwork7  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_EOMWORK8", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsEomwork8  ?? (object)DBNull.Value},
            new OracleParameter("P_ORTHO_SC", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OrthoSc  ?? (object)DBNull.Value},
            new OracleParameter("P_ORTHO_CC", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OrthoCc  ?? (object)DBNull.Value},
            new OracleParameter("P_ALIGN_WITHOUT", OracleDbType.Varchar2) { Value = ophthalmologyParameters.AlignWithout  ?? (object)DBNull.Value},
            new OracleParameter("P_ALIGN_WITH", OracleDbType.Varchar2) { Value = ophthalmologyParameters.AlignWith  ?? (object)DBNull.Value},
            new OracleParameter("P_OD_EYELID", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdEyelid  ?? (object)DBNull.Value},
            new OracleParameter("P_OD_CONJU", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdConju  ?? (object)DBNull.Value},
            new OracleParameter("P_OD_SCLERA", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdSclera  ?? (object)DBNull.Value},
            new OracleParameter("P_OD_CORNEA", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdCornea  ?? (object)DBNull.Value},
            new OracleParameter("P_OD_CHAMBER", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdChamber  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_EYELID", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsEyelid  ?? (object)DBNull.Value},   //16


              new OracleParameter("P_OS_CONJU", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsConju  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_SCLERA", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsSclera  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_CORNEA", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsCornea  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_CHAMBER", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsChamber  ?? (object)DBNull.Value},
            new OracleParameter("P_OD_GONIO1", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdGonio1  ?? (object)DBNull.Value},
            new OracleParameter("P_OD_GONIO2", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdGonio2  ?? (object)DBNull.Value},
            new OracleParameter("P_OD_GONIO3", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdGonio3  ?? (object)DBNull.Value},
            new OracleParameter("P_OD_GONIO4", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdGonio4  ?? (object)DBNull.Value},
            new OracleParameter("P_OD_GONIO5", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdGonio5  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_GONIO1", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsGonio1  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_GONIO2", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsGonio2  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_GONIO3", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsGonio3  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_GONIO4", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsGonio4  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_GONIO5", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsGonio5  ?? (object)DBNull.Value},
            new OracleParameter("P_OD_IRIS", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdIris  ?? (object)DBNull.Value},
            new OracleParameter("P_OD_PUPIL", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdPupil  ?? (object)DBNull.Value},   //16



              new OracleParameter("P_OD_LENS", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdLens  ?? (object)DBNull.Value},
            //new OracleParameter("P_OS_IRIS", OracleDbType.Decimal) { Value = ophthalmologyParameters.OsIris },
              new OracleParameter("P_OD_PUPIL_DIAL", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdPupilDial  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_IRIS", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsIris  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_PUPIL", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsPupil  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_LENS", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsLens  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_PUPIL_DIAL", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsPupilDial  ?? (object)DBNull.Value},
            new OracleParameter("P_OD_NERVE", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdNerve  ?? (object)DBNull.Value},
            new OracleParameter("P_OD_VITREOUS", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdVitreous  ?? (object)DBNull.Value},
            new OracleParameter("P_OD_RETINA", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdRetina  ?? (object)DBNull.Value},
            new OracleParameter("P_OD_MACULA", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdMacula  ?? (object)DBNull.Value},
            new OracleParameter("P_OD_PRIPHERY", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OdPriphery  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_NERVE", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsNerve  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_VITREOUS", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsVitreous  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_RETINA", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsRetina  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_MACULA", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsMacula  ?? (object)DBNull.Value},
            new OracleParameter("P_OS_PRIPHERY", OracleDbType.Varchar2) { Value = ophthalmologyParameters.OsPriphery  ?? (object)DBNull.Value},
            new OracleParameter("P_DIAGNOSIS", OracleDbType.Varchar2) { Value = ophthalmologyParameters.Diagnosis  ?? (object)DBNull.Value},
            new OracleParameter("P_TREATMENT_PLAN", OracleDbType.Varchar2) { Value = ophthalmologyParameters.TreatmentPlan  ?? (object)DBNull.Value},   //19

            
            new OracleParameter("P_ORTHO_CCOD", OracleDbType.Varchar2) { Value = ophthalmologyParameters.P_ORTHO_CCOD  ?? (object)DBNull.Value},
            new OracleParameter("P_ORTHO_SCOS", OracleDbType.Varchar2) { Value = ophthalmologyParameters.P_ORTHO_SCOS  ?? (object)DBNull.Value},
            new OracleParameter("P_PREV_OD_ADD", OracleDbType.Varchar2) { Value = ophthalmologyParameters.P_PREV_OD_ADD  ?? (object)DBNull.Value},
            new OracleParameter("P_PREV_OS_ADD", OracleDbType.Varchar2) { Value = ophthalmologyParameters.P_REFRA_OD_NOTES  ?? (object)DBNull.Value},
            new OracleParameter("P_REFRA_OD_NOTES", OracleDbType.Varchar2) { Value = ophthalmologyParameters.P_ORTHO_CCOD  ?? (object)DBNull.Value},
                   //new OracleParameter("RETVAL", OracleDbType.Decimal) { Direction = ParameterDirection.Output }
                   retvalParameter
      
            //new OracleParameter("RETVAL", OracleDbType.Varchar2) { Value = ophthalmologyParameters.RetVal },
            
            };

                // Log SQL query and parameters for debugging


                // Execute the procedure
                await _DbContext.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
                if (ophthalmologyParameters.IcdList.Count > 0)
                {
                    if (ophthalmologyParameters.IcdList[0].icdCodeId != 0)
                    {
                        for (int i = 0; i < ophthalmologyParameters.IcdList.Count; i++)
                        {
                            await InsertOnlineDocICDDetailsAsync(ophthalmologyParameters.IcdList[i], ut, i);
                        }
                    }
                }



                return Convert.ToInt32(retvalParameter.Value);

            }
            catch (Exception ex)
            {
                return new Exception("An error occurred while saving ophthalmology details.", ex);
            }
        }

        public async Task<dynamic?> SelectExistingPatientOptholomology(string emrDocId)
        {
            if (string.IsNullOrWhiteSpace(emrDocId))
                throw new ArgumentException("EMR_DOC_ID cannot be null or empty.", nameof(emrDocId));

            try
            {
                DateTime today = DateTime.Today; // Current date without time

                // SQL query to fetch data
                string sqlQuery = @"select * from UCHEMR.OPHTHALMOLOGY_DETAIL where EMR_DOC_ID = :EmrDocId";

                // Parameters for the SQL query
                var parameters = new[]
                {
            new OracleParameter("EmrDocId", emrDocId)
        };

                var result = new List<string>();

                // Execute the query
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sqlQuery;
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
                // Log the exception
                throw new Exception("An error occurred while fetching ophthalmology data.", ex);
            }
        }


        public async Task<dynamic?> GetDDevices(string RoomId)
        {
            if (string.IsNullOrWhiteSpace(RoomId))
                throw new ArgumentException("RoomId cannot be null or empty.", nameof(RoomId));

            try
            {
                DateTime today = DateTime.Today; // Current date without time

                // SQL query to fetch data
                string sqlQuery = @"  SELECT D.ID, D.DEVICE_ID FROM UCHDISPLAY.DEVICE_DOCTOR_ROOM_MAP_EXT DDRM 
                        INNER JOIN UCHDISPLAY.DEVICE_EXT D ON D.ID= DDRM.DEVICE_ID WHERE ROOM_ID =:RoomId";

                // Parameters for the SQL query
                var parameters = new[]
                {
            new OracleParameter("ROOM_ID", RoomId)
        };

                var result = new List<string>();

                // Execute the query
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sqlQuery;
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
                // Log the exception
                throw new Exception("An error occurred while fetching ophthalmology data.", ex);
            }
        }


        public async Task<List<Dictionary<string, object>>> GetOnlineICDSelectionAsync(string typ, string code)
        {
            try
            {
                // Define the output parameter for the REF CURSOR
                var refCursorParameter = new OracleParameter
                {
                    ParameterName = "STROUT",
                    OracleDbType = OracleDbType.RefCursor,
                    Direction = ParameterDirection.Output
                };

                // Define the SQL procedure call
                var sql = "BEGIN UCHMASTER.SP_ONLINE_ICDSEL(:TYP, :CODE, :STROUT); END;";

                // Create the input parameters
                var parameters = new List<OracleParameter>
        {
            new OracleParameter("TYP", OracleDbType.Varchar2) { Value = typ },
            new OracleParameter("CODE", OracleDbType.Varchar2) { Value = code },
            refCursorParameter
        };

                // Create a list to store the results
                var result = new List<Dictionary<string, object>>();

                // Execute the procedure
                using (var connection = _DbContext.Database.GetDbConnection() as OracleConnection)
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddRange(parameters.ToArray());

                        await command.ExecuteNonQueryAsync();

                        // Retrieve the REF CURSOR results
                        using (var reader = ((OracleRefCursor)refCursorParameter.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new Dictionary<string, object>();
                                for (var i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                }
                                result.Add(row);
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                // Handle the exception (log or rethrow as needed)
                throw new Exception("Error executing SP_ONLINE_ICDSEL", ex);
            }





            //select* from UCHEMR.OPHTHALMOLOGY_DETAIL where EMR_DOC_ID = @emrdocid


        }

        public async Task InsertOnlineDocICDDetailsAsync(ICDselection a, UserTocken ut, int i)
        {
            try
            {
                var retvalParameter = new OracleParameter
                {
                    ParameterName = "RETVAL",
                    OracleDbType = OracleDbType.Int32,
                    Direction = ParameterDirection.Output
                };

                var sql = @"BEGIN UCHEMR.SP_ONLINE_DOC_ICD_DTLS_INS(
            :ICDID, :ICDSL_NO, :ICDCODE_ID, :ICDCODE_DTLS_ID, 
            :RMRKS, :EMRDOCID, :PATIID, 
            :P_MRD_UPDATE_STS, :P_MRD_UPDATE_USR, :P_MRD_UPDATE_DATE, 
            :RETVAL); 
        END;";

                var parameters = new List<OracleParameter>
        {
            new OracleParameter("ICDID", OracleDbType.Int32) { Value = a.icdId },
            new OracleParameter("ICDSL_NO", OracleDbType.Int32) { Value = i + 1 },
            new OracleParameter("ICDCODE_ID", OracleDbType.Int32) { Value = a.icdCodeId },
            new OracleParameter("ICDCODE_DTLS_ID", OracleDbType.Int32) { Value = a.icdCodeDtlsId },
            new OracleParameter("RMRKS", OracleDbType.Varchar2) { Value = a.remarks },
            new OracleParameter("EMRDOCID", OracleDbType.Varchar2) { Value = a.emrDocId },
            new OracleParameter("PATIID", OracleDbType.Varchar2) { Value = a.patiId },
            new OracleParameter("P_MRD_UPDATE_STS", OracleDbType.Varchar2) { Value = a.mrdUpdateSts },
            new OracleParameter("P_MRD_UPDATE_USR", OracleDbType.Varchar2) { Value = (object)ut.AUSR_ID ?? DBNull.Value },
            new OracleParameter("P_MRD_UPDATE_DATE", OracleDbType.Date) { Value = DateTime.UtcNow },
            retvalParameter
        };

                using (var connection = new OracleConnection(_DbContext.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddRange(parameters.ToArray());

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing SP_ONLINE_DOC_ICD_DTLS_INS for item {i}", ex);
            }
        }


        public async Task<dynamic?> GetAllIcds(string emrDocId)
        {
            if (string.IsNullOrWhiteSpace(emrDocId))
                throw new ArgumentException("EMR_DOC_ID cannot be null or empty.", nameof(emrDocId));

            try
            {
                DateTime today = DateTime.Today; // Current date without time

                // SQL query to fetch data
                string sqlQuery = @"SELECT
              ICD.ICD_ID,
              ICD.ICD_CODE_ID AS ICODE_ID,
              ICD.ICD_CODE_DTLS_ID AS ICODE_DTLS_ID,
              D.ICODE_DTLS_NAME,
              D.ICODE_DTLS_CODE,
              ICD.ICD_RMRKS,
              ICD.MRD_UPDATE_STS,
              ICD.MRD_UPDATE_USR,
              ICD.MRD_UPDATE_DATE
                FROM

              UCHEMR.EMR_VISIT_ICD_DTLS ICD

                LEFT JOIN

              UCHMASTER.ICD_CODE_DTLS D

                 ON
              ICD.ICD_CODE_ID = D.ICODE_ID

              AND ICD.ICD_CODE_DTLS_ID = D.ICODE_DTLS_ID

          WHERE
              EMRDOCID = :EMRDOCID";

                // Parameters for the SQL query
                var parameters = new[]
                {
            new OracleParameter("EmrDocId", emrDocId)
        };

                var result = new List<string>();

                // Execute the query
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sqlQuery;
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
                // Log the exception
                throw new Exception("An error occurred while fetching ophthalmology data.", ex);
            }
        }


        public async Task<dynamic?> GetLensPowerDetails(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                throw new ArgumentException("Search term cannot be null or empty.", nameof(term));

            try
            {
                // SQL query to fetch data
                string sqlQuery = @"
            SELECT 
                LP_DTLS_ID, 
                LENS_POWER_VALUE 
            FROM 
                UCHMASTER.INV_LENS_POWER_DTLS D
            WHERE 
                D.LP_TYPE_ID IN (1, 2)
                AND UPPER(LENS_POWER_VALUE) LIKE UPPER(:SearchTerm)
            ORDER BY 
                PRIORITY ASC";

                // Parameters for the SQL query
                var parameters = new[]
                {
            new OracleParameter("SearchTerm", $"%{term}%")
        };

                // Execute the query
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddRange(parameters);

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
                // Log the exception
                throw new Exception("An error occurred while fetching lens power details.", ex);
            }
        }

        public async Task<dynamic?> GetAccessDetails(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                throw new ArgumentException("Search term cannot be null or empty.", nameof(term));

            try
            {
                // SQL query to fetch data
                string sqlQuery = @"
            SELECT 
                LP_DTLS_ID, 
                LENS_POWER_VALUE 
            FROM 
                UCHMASTER.INV_LENS_POWER_DTLS D
            WHERE 
                D.LP_TYPE_ID IN (3)
                AND UPPER(LENS_POWER_VALUE) LIKE UPPER(:SearchTerm)
            ORDER BY 
                PRIORITY ASC";

                // Parameters for the SQL query
                var parameters = new[]
                {
            new OracleParameter("SearchTerm", $"%{term}%")
        };

                // Execute the query
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddRange(parameters);

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
                // Log the exception
                throw new Exception("An error occurred while fetching lens power details.", ex);
            }
        }


        public async Task<dynamic> GetActiveDoctorsAsync()
        {

            var results = new List<dynamic>();

            try
            {
                using (var connection = new OracleConnection(con))
                {
                    await connection.OpenAsync();

                    string query = @"
                    SELECT EMP_ID, DOCT_ID, EMP_OFFL_NAME 
                    FROM UCHMASTER.OPN_DOCTOR D 
                    INNER JOIN UCHMASTER.HRM_EMPLOYEE E ON E.EMP_ID = D.EMPLOYEE_ID
                    WHERE NVL(DOCT_ACTIVE_STATUS, 'A') = 'A' 
                    AND NVL(EMP_ACTIVE_STATUS, 'A') = 'A'
                    ORDER BY EMP_OFFL_NAME ASC";

                    using (var command = new OracleCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new ExpandoObject() as IDictionary<string, object>;
                                for (var i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                }
                                results.Add(row);
                                //return results;
                            }
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                return new { Status = 500, Message = ex.Message };
            }
            catch (Exception ex)
            {
                return new { Status = 500, Message = ex.Message };
            }

            return results;
        }


        public async Task<bool> SubmitPatientDetailsAsync(VitalsExtra request)
        {
            using var connection = new OracleConnection(con);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try
            {
                // 1️⃣ Execute SP_ONLINE_TAB_PMH_OTHER_SAVE
                using (var cmd = new OracleCommand("BEGIN UCHEMR.SP_ONLINE_TAB_PMH_OTHER_SAVE(:PATIID, :p_mh_Other, :P_FAMILY_MED_HISTORY, :P_DOCID, :P_EMR_DOC_ID); END;", connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Transaction = transaction;

                    cmd.Parameters.Add(":PATIID", OracleDbType.Varchar2).Value = request.PatientId ?? (object)DBNull.Value;
                    cmd.Parameters.Add(":p_mh_Other", OracleDbType.Varchar2).Value = request.MhOther ?? (object)DBNull.Value;
                    cmd.Parameters.Add(":P_FAMILY_MED_HISTORY", OracleDbType.Varchar2).Value = request.FamilyMedHistory ?? (object)DBNull.Value;
                    cmd.Parameters.Add(":P_DOCID", OracleDbType.Varchar2).Value = request.DoctorId ?? (object)DBNull.Value;
                    cmd.Parameters.Add(":P_EMR_DOC_ID", OracleDbType.Varchar2).Value = request.EmrDocId ?? (object)DBNull.Value;

                    await cmd.ExecuteNonQueryAsync();
                }

                // 2️⃣ Execute SP_ONLINE_COMP_IMMU_SAVE
                using (var cmd = new OracleCommand("BEGIN UCHEMR.SP_ONLINE_COMP_IMMU_SAVE(:PATIID, :EDOCID, :DOCID, :COMPLNT, :HSTRY, :IMMUN, :GENRMRKS, :I_TREATMENT_REMARKS, :P_TREATMENT_REMARKS_NEW, :P_NOTES, :P_DOCT_NOTES, :RETVAL); END;", connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Transaction = transaction;

                    // Parameters
                    cmd.Parameters.Add(":PATIID", OracleDbType.Varchar2).Value = request.PatientId?.ToString() ?? (object)DBNull.Value;
                    cmd.Parameters.Add(":EDOCID", OracleDbType.Varchar2).Value = request.EmrDocId?.ToString() ?? (object)DBNull.Value;
                    cmd.Parameters.Add(":DOCID", OracleDbType.Varchar2).Value = request.DoctorId?.ToString() ?? (object)DBNull.Value;
                    cmd.Parameters.Add(":COMPLNT", OracleDbType.Varchar2).Value = request.Complaint ?? (object)DBNull.Value;
                    cmd.Parameters.Add(":HSTRY", OracleDbType.Varchar2).Value = request.History ?? (object)DBNull.Value;
                    cmd.Parameters.Add(":IMMUN", OracleDbType.Varchar2).Value = request.Immunization ?? (object)DBNull.Value;
                    cmd.Parameters.Add(":GENRMRKS", OracleDbType.Varchar2).Value = request.GeneralRemarks ?? (object)DBNull.Value;
                    cmd.Parameters.Add(":I_TREATMENT_REMARKS", OracleDbType.Varchar2).Value = request.TreatmentRemarksNew ?? (object)DBNull.Value;
                    cmd.Parameters.Add(":P_TREATMENT_REMARKS_NEW", OracleDbType.Varchar2).Value = request.TreatmentRemarksNew ?? (object)DBNull.Value;
                    cmd.Parameters.Add(":P_NOTES", OracleDbType.Varchar2).Value = request.Notes ?? (object)DBNull.Value;
                    cmd.Parameters.Add(":P_DOCT_NOTES", OracleDbType.Varchar2).Value = request.DoctorNotes ?? (object)DBNull.Value;

                    // Output parameter
                    var retvalParam = cmd.Parameters.Add(":RETVAL", OracleDbType.Int32);
                    retvalParam.Direction = ParameterDirection.Output;

                    // Execute the command
                    await cmd.ExecuteNonQueryAsync();

                    // Handle the output parameter correctly
                    var resultValue = retvalParam.Value != DBNull.Value
                        ? Convert.ToInt32(((OracleDecimal)retvalParam.Value).Value)
                        : 0;

                    // Handle result
                    if (resultValue != 1) // 1 means success
                    {
                        throw new Exception("Immunization save failed with return value: " + resultValue);
                    }
                }

                // 3️⃣ Execute SP_ONLINE_ALLERGY_UPD
                using (var cmd = new OracleCommand("BEGIN UCHEMR.SP_ONLINE_ALLERGY_UPD(:PATIID, :ALLERGYDTLS, :P_ALLERGY_STATUS, :P_DOCT_ID); END;", connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Transaction = transaction;

                    cmd.Parameters.Add(":PATIID", OracleDbType.Varchar2).Value = request.PatientId ?? (object)DBNull.Value;
                    cmd.Parameters.Add(":ALLERGYDTLS", OracleDbType.Varchar2).Value = request.AllergyDetails ?? (object)DBNull.Value;
                    cmd.Parameters.Add(":P_ALLERGY_STATUS", OracleDbType.Varchar2).Value = request.AllergyStatus ?? (object)DBNull.Value;
                    cmd.Parameters.Add(":P_DOCT_ID", OracleDbType.Varchar2).Value = request.DoctorId ?? (object)DBNull.Value;

                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
                //throw;
            }
        }


        public async Task<dynamic> GetComplaintDetails(string edocId)
        {
            try
            {
                // Corrected SQL query
                //	var sql = @"
                //         SELECT NUTRITIONAL_ASSE_ID,PATI_ID,EMR_DOC_ID,CREATED_USER,CREATED_DATE,UPDATED_USER,UPDATED_DATE,
                //DOA,NEED_REASSESSMENT,PATI_ADMITTED_DUE_TO,MEDICAL_HISTORY,ANY_THERAPY,FAMILY_HISTORY,HIGHT,
                //WEIGHT,PRESENT_WEGHT,BMI,HIGHT_APPROXIMATION,WEIGHT_APPROXIMATION,ANY_WEIGHT_LOSS,ACTIVITY,
                //APPETITE,BOWEL_MOVEMENT,REGULAR,CONSTIPATED_DAYS,DIARRHEA,DIARRHEA_DAYS,FOOD_ALLERGY,FOOD_ALLERGY_REMARKS,
                //EATING_HABITS,MODE_OF_INTAKE,NUTRITIONAL_SUPPLEMENT,FOOD_DRUG_INTERACTION,FOOD_DRUG_INTERACTION_REMARKS,
                //NUTRITIONAL_RISK_FACTORS,NUTRITIONAL_STATUS,BODY_WEIGHT_STATUS,NUTRITIONAL_GOALS,DIET,ESTIMATED_CALORIC_REQ,
                //ESTIMATES_PROTEIN_REQ,REASSESSMENT_ON,DIETICIAN_NAME,NUTRITIONAL_DATE,NUTRITIONAL_TIME,BIOCHEMICAL_HB,
                //BIOCHEMICAL_ALB,BIOCHEMICAL_GRBS,BIOCHEMICAL_NQ,BIOCHEMICAL_K,BIOCHEMICAL_OTHERS
                //FROM
                //	UCHEMR.NUTRITIONAL_ASSESSMENT_DIET
                //WHERE
                //	EMR_DOC_ID =:edocId";
                //	var sql = @"	SELECT B.MH_OTHER COMPLNT, B.FAMILY_MED_HISTORY, B.PATI_ID, B.EMR_DOC_ID, C.DOCT_REMARKS, C.PRSNT_ILLNESS, C.TREATMENT_REMARKS_NEW,
                // D.OPERATION_NOTE, E.ICD_DIGNOSIS FROM UCHMASTER.OPN_PATIENT_MEDICAL_HISTORY B INNER JOIN   UCHEMR.EMR_DOCUMENT_DETAILS C ON
                // B.EMR_DOC_ID = C.EMR_DOC_ID and B.PATI_ID = C.PATI_ID  INNER JOIN  UCHEMR.Operation_note D ON B.EMR_DOC_ID = D.EMR_DOC_ID  
                //and B.PATI_ID = D.PATI_ID  INNER JOIN  UCHEMR.EMR_VISIT_ICD E ON B.EMR_DOC_ID = E.EMR_DOC_ID  and B.PATI_ID = E.PATI_ID
                //     where  B.EMR_DOC_ID =:edocId";

                var sql = @"  select EMR_DOC_ID,PATI_ID, DOCT_ID, PRSNT_ILLNESS from   UCHEMR.EMR_DOCUMENT_DETAILS where EMR_DOC_ID =:edocId";

                // Define the Oracle parameter
                var parameters = new List<OracleParameter>
                {
                    new OracleParameter("EMR_DOC_ID", OracleDbType.Varchar2) { Value = edocId }
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
