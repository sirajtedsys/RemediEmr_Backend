using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using RemediEmr.Class;
using RemediEmr.Data.Class;
using System.Data;
using System.Globalization;
using static JwtService;

//using System.Globalization;

namespace RemediEmr.Repositry
{
    public class PatientRepositry
    {

        private readonly AppDbContext _DbContext;

        private readonly JwtHandler jwthand;

        public PatientRepositry(AppDbContext dbContext, JwtHandler _jwthand)
        {
            _DbContext = dbContext;
            jwthand = _jwthand;
        }

        public async Task<dynamic> VerifyMobileNumber(string mob)
        {
            DateTime currentDateTimeUtc = DateTime.UtcNow;
            DateTime currentDateTimeLocal = currentDateTimeUtc.ToLocalTime();

            try
            {
                // Format dates
               

                return new { Status = 200, Message = "Patient inserted successfully." };
            }
            catch (Exception ex)
            {
                return new { Status = 500, Message = ex.Message };
            }
       
        }

        public async Task<dynamic> InsertCampPatientMasterAsync(Patreg pt, UserTocken ut)
        {
            try
            {
                // Format the birth date for Oracle procedure
                string formattedBirthDate = !string.IsNullOrEmpty(pt.PatiBirthDate)
                    ? DateTime.Parse(pt.PatiBirthDate).ToString("dd-MMM-yyyy")
                    : null;

                // SQL to call the stored procedure
                var sql = @"BEGIN UCHMASTER.PKG_CAMP_PATIENT_MASTER.INS_CAMP_PATIENT_MASTER(
            :P_SALT_ID,
            :P_PATI_FIRST_NAME,
            :P_PATI_BIRTH_DATE,
            :P_PATI_GENDER,
            :P_PATI_MOBILE,
            :P_CREATE_USER,
            :P_IDCARD_ID,
            :P_IDCARD_NO,
            :P_PCUST_ID,
            :P_PATI_PRESENT_ADD1,
            :P_PATI_PERMANENT_ADD1,
            :P_BRANCH_ID,
            :P_LOCATION_ID
        ); END;";

                // Oracle parameters
                var parameters = new List<OracleParameter>
        {
            new OracleParameter("P_SALT_ID", OracleDbType.Varchar2) { Value = pt.SaltId ?? (object)DBNull.Value },
            new OracleParameter("P_PATI_FIRST_NAME", OracleDbType.Varchar2) { Value = pt.PatiFirstName ?? (object)DBNull.Value },
            new OracleParameter("P_PATI_BIRTH_DATE", OracleDbType.Varchar2) { Value = formattedBirthDate ?? (object)DBNull.Value },
            new OracleParameter("P_PATI_GENDER", OracleDbType.Varchar2) { Value = pt.PatiGender ?? (object)DBNull.Value },
            new OracleParameter("P_PATI_MOBILE", OracleDbType.Varchar2) { Value = pt.PatiMobile ?? (object)DBNull.Value },
            new OracleParameter("P_CREATE_USER", OracleDbType.Varchar2) { Value = ut.AUSR_ID ?? (object)DBNull.Value },
            new OracleParameter("P_IDCARD_ID", OracleDbType.Varchar2) { Value = pt.IdCardId ?? (object)DBNull.Value },
            new OracleParameter("P_IDCARD_NO", OracleDbType.Varchar2) { Value = pt.IdCardNo ?? (object)DBNull.Value },
            new OracleParameter("P_PCUST_ID", OracleDbType.Varchar2) { Value = pt.PCustId ?? (object)DBNull.Value },
            new OracleParameter("P_PATI_PRESENT_ADD1", OracleDbType.Varchar2) { Value = pt.PatiPresentAdd1 ?? (object)DBNull.Value },
            new OracleParameter("P_PATI_PERMANENT_ADD1", OracleDbType.Varchar2) { Value = pt.PatiPermanentAdd1 ?? (object)DBNull.Value },
            new OracleParameter("P_BRANCH_ID", OracleDbType.Varchar2) { Value = pt.BranchId ?? (object)DBNull.Value },
            new OracleParameter("P_LOCATION_ID", OracleDbType.Varchar2) { Value = pt.P_LOCATION_ID ?? (object)DBNull.Value }
        };

                // Execute the procedure
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddRange(parameters.ToArray());

                    // Open the connection if not already open
                    if (cmd.Connection.State != ConnectionState.Open)
                        await cmd.Connection.OpenAsync();

                    // Execute the stored procedure
                    await cmd.ExecuteNonQueryAsync();
                }

                return new { Status = 200, Message = "Patient inserted successfully." };
            }
            catch (Exception ex)
            {
                return new { Status = 500, Message = $"Error inserting patient: {ex.Message}" };
            }
        }



        public async Task<string> GetLocationMandatorySettingAsync()
        {
            try
            {
                // SQL to execute the procedure
                var sql = "BEGIN UCHMASTER.PKG_CAMP_PATIENT_MASTER.LOCATION_MANDATORY_SETTINGS(:STROUT); END;";

                // Define the output parameter for the REF CURSOR
                var strOutParam = new OracleParameter("STROUT", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };

                // Create the database command
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(strOutParam);

                    // Open the connection if not already open
                    if (cmd.Connection.State != ConnectionState.Open)
                        await cmd.Connection.OpenAsync();

                    // Execute the command and process the REF CURSOR
                    using (var reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        // Read the result
                        if (await reader.ReadAsync())
                        {
                            return reader["SETTINGS_VALUE"].ToString();
                        }
                        else
                        {
                            throw new Exception("No data returned from LOCATION_MANDATORY_SETTINGS.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle errors and throw a meaningful message
                throw new Exception($"Error in LOCATION_MANDATORY_SETTINGS: {ex.Message}");
            }
        }

        public async Task<List<dynamic>> LookupNationalityAsync()
        {
            try
            {
                var sql = "BEGIN UCHMASTER.PKG_CAMP_PATIENT_MASTER.LOOKUP_NATIONALITY(:STROUT); END;";
                var strOutParam = new OracleParameter("STROUT", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };

                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(strOutParam);

                    await _DbContext.Database.GetDbConnection().OpenAsync();

                    using (var reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        var results = new List<dynamic>();
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                Nationality = reader["NATIONALITY"].ToString(),
                                Id = reader["TRD_ID"].ToString()
                            });
                        }
                        return results;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in LOOKUP_NATIONALITY: {ex.Message}");
            }
        }

        public async Task<List<dynamic>> LookupLocationAsync()
        {
            try
            {
                // Define the procedure call
                var sql = "BEGIN UCHMASTER.PKG_CAMP_PATIENT_MASTER.LOOKUP_LOCATION(:STROUT); END;";

                // Define the output parameter
                var strOutParam = new OracleParameter("STROUT", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };

                // Create a command using the EF DbContext
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(strOutParam);

                    // Open the database connection if not already open
                    if (cmd.Connection.State != ConnectionState.Open)
                        await cmd.Connection.OpenAsync();

                    using (var reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        var results = new List<dynamic>();

                        // Read the data from the SYS_REFCURSOR
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                Place = reader["Place"].ToString(),
                                TrdId = reader["TRD_ID"].ToString()
                            });
                        }

                        return results;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in LOOKUP_LOCATION: {ex.Message}");
            }
        }

        public async Task<List<dynamic>> GetPatientTypesAsync()
        {
            try
            {
                // SQL to execute the procedure
                var sql = "BEGIN UCHMASTER.PKG_CAMP_PATIENT_MASTER.LOOKUP_PATIENT_TYPE(:STROUT); END;";

                // Define the output parameter for the REF CURSOR
                var strOutParam = new OracleParameter("STROUT", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };

                // Create the database command
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(strOutParam);

                    // Open the connection if not already open
                    if (cmd.Connection.State != ConnectionState.Open)
                        await cmd.Connection.OpenAsync();

                    // Execute the command and process the REF CURSOR
                    using (var reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        // Create a list to store the results
                        var results = new List<dynamic>();

                        // Iterate through the data returned by the REF CURSOR
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                ShortName = reader["SHORT NAME"].ToString(),
                                PatientType = reader["PATIENT TYPE"].ToString(),
                                PtypId = reader["PTYP_ID"].ToString(),
                                CatgType = reader["CATG_TYPE"]?.ToString()
                            });
                        }

                        return results;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle errors and throw a meaningful message
                throw new Exception($"Error in LOOKUP_PATIENT_TYPE: {ex.Message}");
            }
        }

        public async Task<dynamic> GetDefaultPatientTypeAsync()
        {
            try
            {
                var sql = "BEGIN UCHMASTER.PKG_CAMP_PATIENT_MASTER.GET_DEFAULT_PATIENT_TYPE(:STROUT); END;";
                var strOutParam = new OracleParameter("STROUT", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };

                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(strOutParam);

                    await _DbContext.Database.GetDbConnection().OpenAsync();

                    using (var reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new
                            {
                                ShortName = reader["Short Name"].ToString(),
                                PatientType = reader["Patient Type"].ToString(),
                                TypeId = reader["PTYP_ID"].ToString(),
                                CategoryType = reader["CATG_TYPE"].ToString()
                            };
                        }
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GET_DEFAULT_PATIENT_TYPE: {ex.Message}");
            }
        }

        public async Task<dynamic> GetDefaultNationalityAsync()
        {
            try
            {
                var sql = "BEGIN UCHMASTER.PKG_CAMP_PATIENT_MASTER.GET_DEFAULT_NATIONALITY(:STROUT); END;";
                var strOutParam = new OracleParameter("STROUT", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };

                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(strOutParam);

                    await _DbContext.Database.GetDbConnection().OpenAsync();

                    using (var reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new
                            {
                                TerritoryId = reader["TRD_ID"].ToString(),
                                Territory = reader["TRD_TERRITORY"].ToString()
                            };
                        }
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GET_DEFAULT_NATIONALITY: {ex.Message}");
            }
        }

        public async Task<List<dynamic>> LookupSalutationAsync()
        {
            try
            {
                var sql = "BEGIN UCHMASTER.PKG_CAMP_PATIENT_MASTER.LOOKUP_SALUTATION(:STROUT); END;";
                var strOutParam = new OracleParameter("STROUT", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };

                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(strOutParam);

                    await _DbContext.Database.GetDbConnection().OpenAsync();

                    using (var reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        var results = new List<dynamic>();
                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                SalutationId = reader["SALT_ID"].ToString(),  // Match with SALT_ID from the procedure
                                Salutation = reader["Salutation"].ToString(), // Match with "Salutation" from the procedure
                                Gender = reader["SALT_GENDER"].ToString()
                            });
                        }
                        return results;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in LOOKUP_SALUTATION: {ex.Message}");
            }
        }

        public async Task<List<Customer>> GetDefaultCampCustAsync()
        {
            try
            {
                // SQL to execute the stored procedure
                var sql = "BEGIN UCHMASTER.PKG_CAMP_PATIENT_MASTER.GET_DEFAULT_CAMP_CUST(:STROUT); END;";

                // Define the output parameter for the REF CURSOR
                var strOutParam = new OracleParameter("STROUT", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };

                // List to store the results
                var customers = new List<Customer>();

                // Execute the procedure
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(strOutParam);

                    // Open the connection if not already open
                    if (cmd.Connection.State != ConnectionState.Open)
                        await cmd.Connection.OpenAsync();

                    // Execute the command and process the REF CURSOR
                    using (var reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            customers.Add(new Customer
                            {
                                CustId = reader["CUST_ID"]?.ToString(),
                                CustName = reader["CUST_NAME"]?.ToString()
                            });
                        }
                    }
                }

                return customers;
            }
            catch (Exception ex)
            {
                // Handle errors and throw a meaningful message
                throw new Exception($"Error in GetDefaultCampCustAsync: {ex.Message}");
            }
        }

        // Define the Customer model class
        public class Customer
        {
            public string CustId { get; set; }
            public string CustName { get; set; }
        }


        public async Task<List<dynamic>> GetCampPatientAsync(string branchCode, string patientTypeId, string year)
        {
            try
            {
                var sql = "BEGIN UCHMASTER.PKG_CAMP_PATIENT_MASTER.GET_CAMP_PATIENT(" +
                          ":P_BRANCH_CODE, :P_PATI_TYPE_ID, :P_YEAR, :STROUT); END;";
                var parameters = new List<OracleParameter>
        {
            new OracleParameter("P_BRANCH_CODE", branchCode),
            new OracleParameter("P_PATI_TYPE_ID", patientTypeId),
            new OracleParameter("P_YEAR", year),
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
                                PatientId = reader["PATIENT_ID"].ToString(),
                                Name = reader["NAME"].ToString(),
                                Gender = reader["GENDER"].ToString(),
                                Mobile = reader["MOBILE"].ToString(),
                                Address = reader["ADDRESS"].ToString(),
                                RegistrationDate = reader["REG_DATE"].ToString()
                            });
                        }
                        return results;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GET_CAMP_PATIENT: {ex.Message}");
            }
        }

        public async Task<dynamic> LookupCustomerAsync(string patiId, string branchId)
        {
            try
            {
                var sql = "BEGIN UCHMASTER.PKG_CAMP_PATIENT_MASTER.LOOKUP_CUSTOMER(:P_PATI_ID, :P_BRANCH_ID, :STROUT); END;";
                var patiIdParam = new OracleParameter("P_PATI_ID", OracleDbType.Varchar2) { Value = patiId };
                var branchIdParam = new OracleParameter("P_BRANCH_ID", OracleDbType.Varchar2) { Value = branchId };
                var strOutParam = new OracleParameter("STROUT", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };

                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(patiIdParam);
                    cmd.Parameters.Add(branchIdParam);
                    cmd.Parameters.Add(strOutParam);

                    await _DbContext.Database.GetDbConnection().OpenAsync();

                    using (var reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        var results = new List<dynamic>();

                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                CustomerName = reader["CUSTOMER"]?.ToString(),
                                Address = reader["ADDRESS"]?.ToString(),
                                CustomerId = reader["CUST_ID"]?.ToString(),
                                ClassId = reader["PRC_CLASS_ID"]?.ToString(),
                                StaffDependant = reader["STAFF_DEPENDANT"]?.ToString(),
                                TdsPercentage = reader["TDS_PERC"] != DBNull.Value ? Convert.ToDecimal(reader["TDS_PERC"]) : (decimal?)null,
                                OpCreditConsult = reader["CUST_OP_CREDIT_CONSULT"]?.ToString(),
                                OpCreditLab = reader["CUST_OP_CREDIT_LAB"]?.ToString(),
                                OpCreditPharmacy = reader["CUST_OP_CREDIT_PHARMACY"]?.ToString(),
                                ExpiryDateNeeded = reader["PATI_EXPIRY_DATE_NEEDED"]?.ToString(),
                                ApprovalNeeded = reader["NEED_APPROVAL"]?.ToString(),
                                EnableScheme = reader["ENABLE_SCHEME"]?.ToString()
                            });
                        }

                        return results;
                    }
                }
            }
            catch (Exception ex)
            {
                return new { Status = 500, Message = ex.Message };
            }
        }

        public async Task<dynamic> GetPatientDetailsByMobileAsync(string patientMobile)
        {
            try
            {
                var sql = "BEGIN UCHMASTER.PKG_CAMP_PATIENT_MASTER.GET_PATI_DTLS_WITH_MOBILE_NO(:P_PATI_MOBILE, :STROUT); END;";
                var mobileParam = new OracleParameter("P_PATI_MOBILE", OracleDbType.Varchar2) { Value = patientMobile };
                var strOutParam = new OracleParameter("STROUT", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };

                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(mobileParam);
                    cmd.Parameters.Add(strOutParam);

                    await _DbContext.Database.GetDbConnection().OpenAsync();

                    using (var reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        var results = new List<dynamic>();

                        while (await reader.ReadAsync())
                        {
                            results.Add(new
                            {
                                PatientId = reader["PATI_ID"]?.ToString(),
                                PatientOpNo = reader["PATI_OPNO"]?.ToString(),
                                SalutationId = reader["SALT_ID"]?.ToString(),
                                FirstName = reader["PATI_FIRST_NAME"]?.ToString(),
                                PatientTypeId = reader["PATI_TYPE_ID"]?.ToString(),
                                BirthDate = reader["PATI_BIRTH_DATE"] != DBNull.Value ? Convert.ToDateTime(reader["PATI_BIRTH_DATE"]) : (DateTime?)null,
                                Gender = reader["PATI_GENDER"]?.ToString(),
                                Mobile = reader["PATI_MOBILE"]?.ToString(),
                                PlaceId = reader["PATI_PLACE_ID"]?.ToString(),
                                CreatedBy = reader["CREATE_USER"]?.ToString(),
                                CreatedDate = reader["CREATE_DATE"] != DBNull.Value ? Convert.ToDateTime(reader["CREATE_DATE"]) : (DateTime?)null,
                                ActiveYear = reader["ACTIVE_YEAR"]?.ToString(),
                                BranchCode = reader["BRANCH_CODE"]?.ToString(),
                                PaymentType = reader["PATI_PAY_TYPE"]?.ToString(),
                                CPRId = reader["CPR_ID"]?.ToString(),
                                IdCardId = reader["IDCARD_ID"]?.ToString(),
                                IdCardNo = reader["IDCARD_NO"]?.ToString(),
                                FinancialYearIdM = reader["FIN_YEAR_ID_M"]?.ToString(),
                                PCustomerId = reader["PCUST_ID"]?.ToString(),
                                PresentAddress = reader["PATI_PRESENT_ADD1"]?.ToString(),
                                PermanentAddress = reader["PATI_PERMANENT_ADD1"]?.ToString(),
                                CampPatient = reader["CAMP_PATIENT"]?.ToString(),
                                BranchId = reader["BRANCH_ID"]?.ToString(),
                                CampVisitDone = reader["CAMP_PATIENT_VISIT_DONE"]?.ToString()
                            });
                        }

                        return results;
                    }
                }
            }
            catch (Exception ex)
            {
                return new { Status = 500, Message = ex.Message };
            }
        }


    }
}