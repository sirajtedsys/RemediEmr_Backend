using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using RemediEmr.Class;
using RemediEmr.Data.Opthalmology2Class;
using System.Data;
using static JwtService;

namespace RemediEmr.Repositry
{
    public class OPthalmology2OpnurseRepositry
    {

        private readonly AppDbContext _DbContext;

        protected readonly IConfiguration _configuration;

        private readonly JwtHandler jwthand;
        private readonly string con;

        public OPthalmology2OpnurseRepositry(AppDbContext dbContext, JwtHandler _jwthand, IConfiguration configuration)
        {
            _DbContext = dbContext;
            jwthand = _jwthand;
            _configuration = configuration;
            con = _DbContext.Database.GetConnectionString();
        }


        #region OPthalmic history



        public async Task<dynamic> GetOpthalmicHistoryByEmrDocId(string emrDocId)
        {
            var query = "SELECT * FROM UCHEMR.OPTHALMIC_HISTORY WHERE EMR_DOC_ID = :emrDocId";
            var dataTable = new DataTable();

            using (var connection = new OracleConnection(con))
            using (var command = new OracleCommand(query, connection))
            {
                command.Parameters.Add(":emrDocId", OracleDbType.Varchar2).Value = emrDocId;

                try
                {
                    connection.Open();
                    using (var adapter = new OracleDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as needed
                    //Console.WriteLine("Oracle query failed: " + ex.Message);
                    return new {Status=500,Message=ex.Message};
                }
            }

            return new { Status = 200, Data = dataTable };
        }


        public async Task<dynamic> ExecuteOphthalmicHistoryProcedure(OpthalmicHistory parameters,string User)
        {
            using (var connection = new OracleConnection(con))
            using (var command = new OracleCommand("UCHEMR.SP_OPTHALMIC_HISTORY_INS", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("P_EMR_DOC_ID", OracleDbType.Varchar2).Value = parameters.EmrDocId;
                command.Parameters.Add("P_ERROR_REF", OracleDbType.Varchar2).Value = parameters.ErrorRef;
                command.Parameters.Add("P_CATRACT", OracleDbType.Varchar2).Value = parameters.Catract;
                command.Parameters.Add("P_GLAUCOMA", OracleDbType.Varchar2).Value = parameters.Glaucoma;
                command.Parameters.Add("P_DR", OracleDbType.Varchar2).Value = parameters.Dr;
                command.Parameters.Add("P_DRY_EYES", OracleDbType.Varchar2).Value = parameters.DryEyes;
                command.Parameters.Add("P_OCULAR_SURGERY", OracleDbType.Varchar2).Value = parameters.OcularSurgery;
                command.Parameters.Add("P_OTHER", OracleDbType.Varchar2).Value = parameters.Other;
                command.Parameters.Add("P_CREATED_BY", OracleDbType.Varchar2).Value = User;

                var retvalParam = new OracleParameter("RETVAL", OracleDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(retvalParam);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    return new { Status = 200, Message = "Opthalmic History Saved", Data = Convert.ToInt32(retvalParam.Value) };
                }
                catch (Exception ex)
                {
                    // Log error as needed

                    return new { Status = 500, Message = ex.Message };
                }
            }
        }


        #endregion OPthalmic history




        #region OPthalmic Family history

        public async Task<dynamic> GetOpthalmicFamilyHistoryByEmrDocId(string emrDocId)
        {
            var query = "SELECT * FROM UCHEMR.OPTHALMIC_FAMILY_HISTORY WHERE EMR_DOC_ID = :emrDocId";
            var dataTable = new DataTable();

            using (var connection = new OracleConnection(con))
            using (var command = new OracleCommand(query, connection))
            {
                command.Parameters.Add(":emrDocId", OracleDbType.Varchar2).Value = emrDocId;

                try
                {
                    connection.Open();
                    using (var adapter = new OracleDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as needed
                    //Console.WriteLine("Oracle query failed: " + ex.Message);
                    return new { Status = 500, Message = ex.Message };
                }
            }

            return new { Status = 200, Data = dataTable };
        }


        public async Task<dynamic> ExecuteOphthalmicFamilyHistoryProcedure(OpthalmicFamilyHistory parameters, string User)
        {
            using (var connection = new OracleConnection(con))
            using (var command = new OracleCommand("UCHEMR.SP_OPTHALMIC_FAMILY_HIST_INS", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("P_EMR_DOC_ID", OracleDbType.Varchar2).Value = parameters.EmrDocId;
                command.Parameters.Add("P_ERROR_REF", OracleDbType.Varchar2).Value = parameters.ErrorRef;
                command.Parameters.Add("P_CATRACT", OracleDbType.Varchar2).Value = parameters.Catract;
                command.Parameters.Add("P_GLAUCOMA", OracleDbType.Varchar2).Value = parameters.Glaucoma;
                command.Parameters.Add("P_DR", OracleDbType.Varchar2).Value = parameters.Dr;
                command.Parameters.Add("P_DRY_EYES", OracleDbType.Varchar2).Value = parameters.DryEyes;
                command.Parameters.Add("P_OTHER", OracleDbType.Varchar2).Value = parameters.Other;
                command.Parameters.Add("P_CREATED_BY", OracleDbType.Varchar2).Value = User;

                var retvalParam = new OracleParameter("RETVAL", OracleDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(retvalParam);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    return new { Status = 200, Message = "Opthalmic Family History Saved", Data = Convert.ToInt32(retvalParam.Value) };
                }
                catch (Exception ex)
                {
                    // Log error as needed

                    return new { Status = 500, Message = ex.Message };
                }
            }
        }




        #endregion OPthalmic Family history



        #region Diabetes

        public async Task<dynamic> GetOPTHALMIC_DIABETESByEmrDocId(string emrDocId)
        {
            var query = "SELECT * FROM UCHEMR.OPTHALMIC_DIABETES  WHERE EMR_DOC_ID = :emrDocId";
            var dataTable = new DataTable();

            using (var connection = new OracleConnection(con))
            using (var command = new OracleCommand(query, connection))
            {
                command.Parameters.Add(":emrDocId", OracleDbType.Varchar2).Value = emrDocId;

                try
                {
                    connection.Open();
                    using (var adapter = new OracleDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as needed
                    //Console.WriteLine("Oracle query failed: " + ex.Message);
                    return new { Status = 500, Message = ex.Message };
                }
            }

            return new { Status = 200, Data = dataTable };
        }


        //public async Task<dynamic> ExecuteOphthalmicFamilyHistoryProcedure(OpthalmicFamilyHistory parameters, string User)
        //{
        //    using (var connection = new OracleConnection(con))
        //    using (var command = new OracleCommand("UCHEMR.SP_OPTHALMIC_FAMILY_HIST_INS", connection))
        //    {
        //        command.CommandType = CommandType.StoredProcedure;

        //        command.Parameters.Add("P_EMR_DOC_ID", OracleDbType.Varchar2).Value = parameters.EmrDocId;
        //        command.Parameters.Add("P_ERROR_REF", OracleDbType.Varchar2).Value = parameters.ErrorRef;
        //        command.Parameters.Add("P_CATRACT", OracleDbType.Varchar2).Value = parameters.Catract;
        //        command.Parameters.Add("P_GLAUCOMA", OracleDbType.Varchar2).Value = parameters.Glaucoma;
        //        command.Parameters.Add("P_DR", OracleDbType.Varchar2).Value = parameters.Dr;
        //        command.Parameters.Add("P_DRY_EYES", OracleDbType.Varchar2).Value = parameters.DryEyes;
        //        command.Parameters.Add("P_OTHER", OracleDbType.Varchar2).Value = parameters.Other;
        //        command.Parameters.Add("P_CREATED_BY", OracleDbType.Varchar2).Value = User;

        //        var retvalParam = new OracleParameter("RETVAL", OracleDbType.Int32)
        //        {
        //            Direction = ParameterDirection.Output
        //        };
        //        command.Parameters.Add(retvalParam);

        //        try
        //        {
        //            connection.Open();
        //            command.ExecuteNonQuery();
        //            return new { Status = 200, Message = "Opthalmic Family History Saved", Data = Convert.ToInt32(retvalParam.Value) };
        //        }
        //        catch (Exception ex)
        //        {
        //            // Log error as needed

        //            return new { Status = 500, Message = ex.Message };
        //        }
        //    }
        //}




        #endregion OPthalmic Family history





    }
}
