using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using RemediEmr.Class;
using RemediEmr.Data.Class;
using System.Data;
using System.Reflection.Metadata;
using static JwtService;

namespace RemediEmr.Repositry
{
    public class PrescriptionRepositry
    {


        private readonly AppDbContext _DbContext;

        protected readonly IConfiguration _configuration;

        private readonly JwtHandler jwthand;

        public PrescriptionRepositry(AppDbContext dbContext, JwtHandler _jwthand, IConfiguration configuration)
        {
            _DbContext = dbContext;
            jwthand = _jwthand;
            _configuration = configuration;
        }


        public async Task<DataTable> GetOnlineMedicineAndGeneric(string term, string branchId,UserTocken ut, int? claimId = null)
        {
            DataTable resultTable = new DataTable();

            // Retrieve the connection string from DbContext
            string connectionString = _DbContext.Database.GetConnectionString();

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand command = new OracleCommand("UCHEMR.SP_ONLINE_MEDICINE_AND_GEN_SEL", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Input parameters
                    command.Parameters.Add("TERM", OracleDbType.Varchar2).Value = term;
                    command.Parameters.Add("CLAIMID", OracleDbType.Int32).Value = (object)claimId ?? DBNull.Value;
                    command.Parameters.Add("P_BRANCH_ID", OracleDbType.Varchar2).Value = branchId;
                    command.Parameters.Add("P_EMP_ID", OracleDbType.Varchar2).Value = ut.AUSR_ID;
                    

                    // Output parameter
                    command.Parameters.Add("STROUT", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    try
                    {
                        connection.Open();

                        using (OracleDataAdapter adapter = new OracleDataAdapter(command))
                        {
                            adapter.Fill(resultTable);
                        }
                    }
                    catch (OracleException ex)
                    {
                        // Handle Oracle exceptions
                        Console.WriteLine("Oracle error: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        // Handle other exceptions
                        Console.WriteLine("General error: " + ex.Message);
                    }
                }
            }

            return resultTable;
        }


        public async Task<DataTable> GetFrequencyByEmployeeAsync(string empId)
        {

            DataTable resultTable = new DataTable();

            // Retrieve the connection string from DbContext
            string connectionString = _DbContext.Database.GetConnectionString();

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand command = new OracleCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"
                    SELECT FM.FREQ_ID, FM.FREQ_NAME, FM.FREQ_STATUS, FM.DEFINED_QTY, FM.PRIORITY 
                    FROM UCHMASTER.MED_FREQUENCY_MASTER FM 
                    INNER JOIN uchemr.EMR_USR_FREQUENCY_LINK FL 
                    ON FM.FREQ_ID = FL.FREQ_ID  
                    WHERE FL.EMP_ID = :EMP_ID 
                    ORDER BY FL.PRIORITY";

                    // Add parameter
                    command.Parameters.Add("EMP_ID", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(empId) ? DBNull.Value : empId;

                    try
                    {
                        connection.Open();

                        using (OracleDataAdapter adapter = new OracleDataAdapter(command))
                        {
                            adapter.Fill(resultTable);
                        }
                    }
                    catch (OracleException ex)
                    {
                        // Handle Oracle exceptions
                        Console.WriteLine("Oracle error: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        // Handle other exceptions
                        Console.WriteLine("General error: " + ex.Message);
                    }
                }
            }

            return resultTable;
        }


        public class FrequencyModel
        {
            public int FrequencyId { get; set; }
            public string FrequencyName { get; set; }
            public string FrequencyStatus { get; set; }
            public int? DefinedQuantity { get; set; }
            public int Priority { get; set; }
        }

        public async Task<DataTable> GetRoutesByEmployeeAsync( string empId)
        {
            // Create a DataTable to store the results
            DataTable resultTable = new DataTable();

            // Retrieve the connection string from DbContext
            string connectionString = _DbContext.Database.GetConnectionString();

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand command = new OracleCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"
                SELECT R.MED_ROUTE_ID, R.MED_ROUTE_NAME 
                FROM UCHMASTER.MED_ROUTE R 
                LEFT JOIN uchemr.EMR_USR_ROUTE_LINK RL 
                ON R.MED_ROUTE_ID = RL.MED_ROUTE_ID 
                WHERE R.ACTIVE_STATUS = 'A' AND RL.EMP_ID = :EMP_ID 
                ORDER BY RL.PRIORITY";

                    // Add parameter for EMP_ID
                    command.Parameters.Add("EMP_ID", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(empId) ? DBNull.Value : empId;

                    try
                    {
                        connection.Open();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            resultTable.Load(reader);
                        }
                    }
                    catch (OracleException ex)
                    {
                        Console.WriteLine("Oracle error: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("General error: " + ex.Message);
                    }
                }
            }

            return resultTable;
        }

        public class Dosage
        {
            public int MedDosId { get; set; }
            public string MedDosName { get; set; }
        }
        public async Task<DataTable> GetDosagesByEmployeeAsync(string empId)
        {
            //List<Dosage> dosages = new List<Dosage>();

            DataTable resultTable = new DataTable();
            string connectionString = _DbContext.Database.GetConnectionString();

            string query = @"
            SELECT NVL(D.MED_DOS_ID, 0) AS MED_DOS_ID, MED_DOS_NAME
            FROM UCHMASTER.MED_DOSAGE D
            LEFT JOIN uchemr.EMR_USR_DOSAGE_LINK DL
            ON D.MED_DOS_ID = DL.MED_DOS_ID
            WHERE D.ACTIVE_STATUS = 'A' AND DL.EMP_ID = :EMP_ID
            ORDER BY DL.PRIORITY";

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add("EMP_ID", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(empId) ? DBNull.Value : empId;

                    try
                    {
                        await connection.OpenAsync();

                        using (OracleDataAdapter adapter = new OracleDataAdapter(command))
                        {
                            adapter.Fill(resultTable);
                        }
                    }
                    catch (OracleException ex)
                    {
                        Console.WriteLine("Oracle error: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("General error: " + ex.Message);
                    }
                }
            }

            return resultTable;
        }


        public async Task<DataTable> GetOnlineGenericNamesAsync(string term, string branchId, int? claimId=0)
        {
            string connectionString = _DbContext.Database.GetConnectionString();
            DataTable resultTable = new DataTable();

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand command = new OracleCommand("UCHEMR.SP_ONLINE_GENERICNAME_SEL2_NEW", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Input Parameters
                    command.Parameters.Add("TERM", OracleDbType.Varchar2).Value = term;
                    command.Parameters.Add("CLAIMID", OracleDbType.Int32).Value = claimId.HasValue ? claimId.Value : DBNull.Value;
                    command.Parameters.Add("P_BRANCH_ID", OracleDbType.Varchar2).Value = branchId;

                    // Output Parameters
                    OracleParameter refCursorParam = new OracleParameter("STROUT", OracleDbType.RefCursor)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(refCursorParam);

                    try
                    {
                        await connection.OpenAsync();

                        using (OracleDataReader reader = await command.ExecuteReaderAsync())
                        {
                            resultTable.Load(reader);
                        }
                    }
                    catch (OracleException ex)
                    {
                        Console.WriteLine("Oracle error: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("General error: " + ex.Message);
                    }
                }
            }

            return resultTable;
        }


        public async Task<int> ExecuteSPOnlineDocMediIns(List<Prescription> parameters, UserTocken ut)
        {
            try
            {
                string connectionString = _DbContext.Database.GetConnectionString();
                int lastResult = 0; // To track the last result, if needed

                foreach (var parameter in parameters)
                {
                    try
                    {
                        using (var connection = new OracleConnection(connectionString))
                        {
                            // Open connection before executing the command
                            await connection.OpenAsync();

                            using (var command = new OracleCommand("UCHEMR.SP_ONLINE_DOC_MEDI_INS", connection))
                            {
                                command.CommandType = CommandType.StoredProcedure;

                                // Input Parameters
                                command.Parameters.Add("MEDPSLNO", OracleDbType.Int32).Value = parameter.MedPslno ?? (object)DBNull.Value;
                                command.Parameters.Add("EDOCID", OracleDbType.Varchar2).Value = parameter.EdocId ?? (object)DBNull.Value;
                                command.Parameters.Add("DOCID", OracleDbType.Varchar2).Value = parameter.DocId ?? (object)DBNull.Value;
                                command.Parameters.Add("MEDID", OracleDbType.Varchar2).Value = parameter.MedId ?? (object)DBNull.Value;
                                command.Parameters.Add("FREQID", OracleDbType.Varchar2).Value = parameter.FreqId ?? (object)DBNull.Value;
                                command.Parameters.Add("MEDROUTEID", OracleDbType.Varchar2).Value = parameter.MedRouteId ?? (object)DBNull.Value;
                                command.Parameters.Add("DUR", OracleDbType.Int32).Value = parameter.Dur ?? (object)DBNull.Value;
                                command.Parameters.Add("RMRKS", OracleDbType.Varchar2).Value = parameter.Rmrks ?? (object)DBNull.Value;
                                command.Parameters.Add("BF", OracleDbType.Char).Value = parameter.Bf ?? (object)DBNull.Value;
                                command.Parameters.Add("AF", OracleDbType.Char).Value = parameter.Af ?? (object)DBNull.Value;
                                command.Parameters.Add("PATI", OracleDbType.Varchar2).Value = parameter.PatI ?? (object)DBNull.Value;
                                command.Parameters.Add("CREATEUSR", OracleDbType.Varchar2).Value = ut.AUSR_ID ?? (object)DBNull.Value;
                                command.Parameters.Add("DOSID", OracleDbType.Varchar2).Value = parameter.DosId ?? (object)DBNull.Value;
                                command.Parameters.Add("CLAIMID", OracleDbType.Int32).Value = parameter.ClaimId ?? (object)DBNull.Value;
                                command.Parameters.Add("CLAIMPERCENT", OracleDbType.Int32).Value = parameter.ClaimPercent ?? (object)DBNull.Value;
                                command.Parameters.Add("DISCPERCENT", OracleDbType.Int32).Value = parameter.DiscPercent ?? (object)DBNull.Value;
                                command.Parameters.Add("APRVLTYPE_ID", OracleDbType.Int32).Value = parameter.AprvlTypeId ?? (object)DBNull.Value;
                                command.Parameters.Add("READYTOBILL", OracleDbType.Char).Value = parameter.ReadyToBill ?? (object)DBNull.Value;
                                command.Parameters.Add("QTYTOTAL", OracleDbType.Decimal).Value = parameter.QtyTotal ?? (object)DBNull.Value;
                                command.Parameters.Add("P_SALE_BRK_QTY", OracleDbType.Decimal).Value = parameter.PSaleBrkQty ?? (object)DBNull.Value;
                                command.Parameters.Add("P_SALE_BRK_UNIT", OracleDbType.Decimal).Value = parameter.PSaleBrkUnit ?? (object)DBNull.Value;
                                command.Parameters.Add("DONOTBILL", OracleDbType.Char).Value = parameter.DoNotBill ?? (object)DBNull.Value;
                                command.Parameters.Add("P_TEST_DOSE", OracleDbType.Char).Value = parameter.PTestDose ?? (object)DBNull.Value;
                                command.Parameters.Add("OBS", OracleDbType.Char).Value = parameter.Obs ?? (object)DBNull.Value;
                                command.Parameters.Add("TOTUNIT", OracleDbType.Varchar2).Value = parameter.TotUnit ?? (object)DBNull.Value;
                                command.Parameters.Add("P_DUNIT_ID", OracleDbType.Varchar2).Value = parameter.PDUnitId ?? (object)DBNull.Value;
                                command.Parameters.Add("P_DOSAGE_VAL", OracleDbType.Varchar2).Value = parameter.PDosageVal ?? (object)DBNull.Value;

                                // Output Parameter
                                var retVal = new OracleParameter("RETVAL", OracleDbType.Decimal)
                                {
                                    Direction = ParameterDirection.Output
                                };
                                command.Parameters.Add(retVal);

                                // Execute the command asynchronously
                                await command.ExecuteNonQueryAsync();

                                // Retrieve the output value
                                int result = retVal.Value != DBNull.Value
                                    ? ((OracleDecimal)retVal.Value).ToInt32()
                                    : 0; // Handle DBNull gracefully

                                // Update last result
                                lastResult = result;

                                // Log error but continue processing
                                if (result != 0)
                                {
                                    Console.WriteLine($"Stored procedure returned error for parameter {parameter}: {result}");
                                }
                            }
                        }
                    }
                    catch (OracleException oex)
                    {
                        // Log Oracle-specific error but continue processing
                        Console.WriteLine($"OracleException for parameter {parameter}: {oex.Message}");
                    }
                    catch (Exception ex)
                    {
                        // Log generic error but continue processing
                        Console.WriteLine($"Exception for parameter {parameter}: {ex.Message}");
                    }
                }

                // Return the last result, if needed
                return lastResult;
            }
            catch (Exception ex)
            {
                // Log the exception (generic error)
                Console.WriteLine($"Critical Exception: {ex.Message}");
                return 3;
            }
        }



        public async Task<DataTable> GetMedicinePlan(string patId, string emrDocId)
        {
            DataTable resultTable = new DataTable();
            var _connectionString = _DbContext.Database.GetConnectionString();

            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                using (OracleCommand command = new OracleCommand("UCHEMR.SP_ONLINE_DOC_MEDI_GRIDSEL_NEW", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Input parameters
                    command.Parameters.Add("PATIID", OracleDbType.Varchar2).Value = patId;
                    command.Parameters.Add("EMRDOCID", OracleDbType.Varchar2).Value = emrDocId;

                    // Output parameter
                    command.Parameters.Add("strOut", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    try
                    {
                        connection.Open();

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            resultTable.Load(reader);
                        }
                    }
                    catch (OracleException oex)
                    {
                        // Log Oracle-specific error but continue processing
                        Console.WriteLine($"OracleException for parameter {command.Parameters}: {oex.Message}");
                    }
                    catch (Exception ex)
                    {
                        // Log or handle the exception as needed
                        throw new Exception("An error occurred while fetching medicine plan data.", ex);
                    }
                }
            }

            return resultTable;
        }

    }


}
