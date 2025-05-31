using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using RemediEmr.Class;
using RemediEmr.Data.Class;
using RemediEmr.Data.DbModel;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Security.AccessControl;
using static JwtService;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
//using System.Data;
namespace RemediEmr.Repositry
{
	public class ComplaintRepository
	{
		private readonly AppDbContext _DbContext;

		private readonly JwtHandler jwthand;
		private readonly OPNurseRepositry oprep;
		public ComplaintRepository(AppDbContext dbContext, JwtHandler _jwthand, OPNurseRepositry _oprep)
		{
			_DbContext = dbContext;
			jwthand = _jwthand;
			oprep = _oprep;
		}

		public async Task<dynamic> DeleteIcd(string emrDocId)
		{
			try
			{
				var sql = "delete  from  UCHEMR.EMR_VISIT_ICD_DTLS where EMRDOCID=:emrDocId";
				var cmd = _DbContext.Database.GetDbConnection().CreateCommand();
				cmd.CommandText = sql;

				// Add the parameter for the SQL query
				var param = cmd.CreateParameter();
				param.ParameterName = ":emrDocId";
				param.Value = emrDocId;
				cmd.Parameters.Add(param);

				// Open the connection if it is closed
				if (cmd.Connection.State != System.Data.ConnectionState.Open)
				{
					await cmd.Connection.OpenAsync();
				}

				// Execute the query and check how many rows were affected
				var affectedRows = await cmd.ExecuteNonQueryAsync();
				await cmd.Connection.CloseAsync();

				if (affectedRows > 0)
				{
					return new { Status = 1, Message = "Deletion successful" }; // Return success message
				}
				else
				{
					return new { Status = 0, Message = "No records found to delete" }; // Handle no records found case
				}
			}
			catch (Exception ex)
			{
				return new { Status = 500, Message = ex.Message }; // Return error message on exception
			}
		}


		public async Task<dynamic> SaveComplaintAsync(Complaint dentalAssessment, UserTocken ut)
		{
			try
			{
				await DeleteIcd(dentalAssessment.P_EMR_DOC_ID);
				for (var j = 0; j < dentalAssessment.IcdList.Count; j++)
				{

					//await oprep.InsertOnlineDocICDDetailsAsync(dentalAssessment.IcdList[j], ut, j);
					await InsertOnlineDocICDDetailsAsync(dentalAssessment.IcdList[j], ut, j);
				}

				// Array of SQL statements and corresponding parameters
				var sqlStatements = new List<string>
			{
				@"BEGIN UCHEMR.EMR_SP_PRESC_ADVICE(
					:P_EMR_DOC_ID,:P_PRESC_ADV,:P_INVEST_ADV,:P_RADIO_ADV, :P_CREATED_USER,:P_TYPE 
				); END;",
				@"BEGIN UCHEMR.EMR_SP_PRESC_ADVICE(
					:P_EMR_DOC_ID,:P_PRESC_ADV,:P_INVEST_ADV,:P_RADIO_ADV, :P_CREATED_USER,:P_TYPE 
				); END;",


				@"BEGIN UCHEMR.SP_ONLINE_TAB_PMH_OTHER_SAVE(
					:PATIID,:p_mh_Other,:P_FAMILY_MED_HISTORY,:P_DOCID,:P_EMR_DOC_ID 
				); END;",
			@"BEGIN UCHEMR.SP_ONLINE_COMP_IMMU_SAVE(
					:PATIID,:EDOCID,:DOCID,:COMPLNT,:HSTRY,:IMMUN,:GENRMRKS,:I_TREATMENT_REMARKS,
					:P_TREATMENT_REMARKS_NEW,:P_NOTES,:P_DOCT_NOTES,:RETVAL
				  ); END;",
			@"BEGIN UCHEMR.SP_ONLINE_PATI_OPTN_NOTE_SAVE(
					   :PATIID,:EDOC_ID,:P_OPERTN_NOTE,:USER_ID
				  ); END;",

			@"BEGIN UCHEMR.SP_ONLINE_ALLERGY_UPD(
					 :PATIID,:ALLERGYDTLS,:P_ALLERGY_STATUS,:P_DOCT_ID
				  ); END;",

			@"BEGIN UCHEMR.SP_CPAST_MEDISUR_SAVE(
					 :PATIID,:EDOC_ID,:P_MEDI_SUR_HISTORY,:P_PATI_ADVICE,:P_PATI_EDU,:P_PATI_COMORBIDITY,
					:P_PSYCHO_SOCIAL,:USER_ID
				  ); END;",

			@"BEGIN UCHEMR.SP_ONLINE_DOC_ICDVISIT_INS(
					 :CREATEUSR,:ICDSL_NO,:EDOCID,:DOCID,:ICDDIGNOSIS,:PATI,:BRANCHCODE,:RETVAL
				  ); END;"

			
            
				// Add other SQL statements here
			};

				var parametersList = new List<List<OracleParameter>>
			{
					new List<OracleParameter>
				{
	
					new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = dentalAssessment.P_EMR_DOC_ID ?? (object)DBNull.Value },
					new OracleParameter("P_PRESC_ADV", OracleDbType.Varchar2) { Value = dentalAssessment.P_PRESC_ADV ?? (object)DBNull.Value },
					new OracleParameter("P_INVEST_ADV", OracleDbType.Varchar2) { Value = dentalAssessment.SYSTEMIC ?? (object)DBNull.Value },
					new OracleParameter("P_RADIO_ADV", OracleDbType.Varchar2) { Value = dentalAssessment.P_RADIO_ADV ?? (object)DBNull.Value },
					  new OracleParameter("P_CREATED_USER", OracleDbType.Varchar2) { Value = ut.AUSR_ID ??(object) DBNull.Value },
					//new OracleParameter("P_CREATED_USER", OracleDbType.Varchar2) { Value = dentalAssessment.P_CREATED_USER ?? (object)DBNull.Value },
					new OracleParameter("P_TYPE", OracleDbType.Int64) { Value = 4 }
				},
				new List<OracleParameter>
				{
					new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = dentalAssessment.P_EMR_DOC_ID ?? (object)DBNull.Value },
					new OracleParameter("P_PRESC_ADV", OracleDbType.Varchar2) { Value = dentalAssessment.P_PRESC_ADV ?? (object)DBNull.Value },
					new OracleParameter("P_INVEST_ADV", OracleDbType.Varchar2) { Value = dentalAssessment.LOCALEX ?? (object)DBNull.Value },
					new OracleParameter("P_RADIO_ADV", OracleDbType.Varchar2) { Value = dentalAssessment.P_RADIO_ADV ?? (object)DBNull.Value },
					  new OracleParameter("P_CREATED_USER", OracleDbType.Varchar2) { Value = ut.AUSR_ID ??(object) DBNull.Value },
					//new OracleParameter("P_CREATED_USER", OracleDbType.Varchar2) { Value = dentalAssessment.P_CREATED_USER ?? (object)DBNull.Value },
					new OracleParameter("P_TYPE", OracleDbType.Int64) { Value = 5}
				},

				new List<OracleParameter>
				{
					new OracleParameter("PATIID", OracleDbType.Varchar2) { Value = dentalAssessment.PATIID ?? (object)DBNull.Value },
					new OracleParameter("p_mh_Other", OracleDbType.Varchar2) { Value = dentalAssessment.p_mh_Other ?? (object)DBNull.Value },
					new OracleParameter("P_FAMILY_MED_HISTORY", OracleDbType.Varchar2) { Value = dentalAssessment.P_FAMILY_MED_HISTORY ?? (object)DBNull.Value },
					new OracleParameter("P_DOCID", OracleDbType.Varchar2) { Value = dentalAssessment.P_DOCID ?? (object)DBNull.Value },
					new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = dentalAssessment.P_EMR_DOC_ID ?? (object)DBNull.Value }
				},

				new List<OracleParameter>
				{
					new OracleParameter("PATIID", OracleDbType.Varchar2) { Value = dentalAssessment.PATIID ?? (object)DBNull.Value },
					new OracleParameter("EDOCID", OracleDbType.Varchar2) { Value = dentalAssessment.EDOCID ?? (object)DBNull.Value },
					new OracleParameter("DOCID", OracleDbType.Varchar2) { Value = dentalAssessment.DOCID ?? (object)DBNull.Value },
					new OracleParameter("COMPLNT", OracleDbType.Varchar2) { Value = dentalAssessment.COMPLNT ?? (object)DBNull.Value },
					new OracleParameter("HSTRY", OracleDbType.Varchar2) { Value = dentalAssessment.HSTRY ?? (object)DBNull.Value },
					new OracleParameter("IMMUN", OracleDbType.Varchar2) { Value = dentalAssessment.IMMUN ?? (object)DBNull.Value },
					new OracleParameter("GENRMRKS", OracleDbType.Varchar2) { Value = dentalAssessment.GENRMRKS ?? (object)DBNull.Value },
					new OracleParameter("I_TREATMENT_REMARKS", OracleDbType.Varchar2) { Value = dentalAssessment.I_TREATMENT_REMARKS ?? (object)DBNull.Value },
					new OracleParameter("P_TREATMENT_REMARKS_NEW", OracleDbType.Varchar2) { Value = dentalAssessment.P_TREATMENT_REMARKS_NEW ?? (object)DBNull.Value },
					new OracleParameter("P_NOTES", OracleDbType.Varchar2) { Value = dentalAssessment.P_NOTES ?? (object)DBNull.Value },
					new OracleParameter("P_DOCT_NOTES", OracleDbType.Varchar2) { Value = dentalAssessment.P_DOCT_NOTES ?? (object)DBNull.Value },
					new OracleParameter("RETVAL", OracleDbType.Int32) { Direction = ParameterDirection.Output }             },

				new List<OracleParameter>
				{
					new OracleParameter("PATIID", OracleDbType.Varchar2) { Value = dentalAssessment.PATIID ?? (object)DBNull.Value },
					new OracleParameter("EDOC_ID", OracleDbType.Varchar2) { Value = dentalAssessment.EDOC_ID ?? (object)DBNull.Value },
					new OracleParameter("P_OPERTN_NOTE", OracleDbType.Varchar2) { Value = dentalAssessment.P_OPERTN_NOTE ?? (object)DBNull.Value },
					//new OracleParameter("USER_ID", OracleDbType.Varchar2) { Value = dentalAssessment.USER_ID ?? (object)DBNull.Value }
					  new OracleParameter("USER_ID", OracleDbType.Varchar2) { Value = ut.AUSR_ID ??(object) DBNull.Value },
				},
					new List<OracleParameter>
				{
					new OracleParameter("PATIID", OracleDbType.Varchar2) { Value = dentalAssessment.PATIID ?? (object)DBNull.Value },
					new OracleParameter("ALLERGYDTLS", OracleDbType.Varchar2) { Value = dentalAssessment.ALLERGYDTLS ?? (object)DBNull.Value },
					new OracleParameter("P_ALLERGY_STATUS", OracleDbType.Varchar2) { Value = dentalAssessment.P_ALLERGY_STATUS ?? (object)DBNull.Value },
					new OracleParameter("P_DOCT_ID", OracleDbType.Varchar2) { Value = dentalAssessment.DOCID ?? (object)DBNull.Value }

				},
					new List<OracleParameter>
				{
					new OracleParameter("PATIID", OracleDbType.Varchar2) { Value = dentalAssessment.PATIID ?? (object)DBNull.Value },
					new OracleParameter("EDOC_ID", OracleDbType.Varchar2) { Value = dentalAssessment.EDOC_ID ?? (object)DBNull.Value },
					new OracleParameter("P_MEDI_SUR_HISTORY", OracleDbType.Varchar2) { Value = dentalAssessment.P_MEDI_SUR_HISTORY ?? (object)DBNull.Value },
					new OracleParameter("P_PATI_ADVICE", OracleDbType.Varchar2) { Value = dentalAssessment.P_PATI_ADVICE ?? (object)DBNull.Value },
					new OracleParameter("P_PATI_EDU", OracleDbType.Varchar2) { Value = dentalAssessment.P_PATI_EDU ?? (object)DBNull.Value },
					new OracleParameter("P_PATI_COMORBIDITY", OracleDbType.Varchar2) { Value = dentalAssessment.P_PATI_COMORBIDITY ?? (object)DBNull.Value },
					new OracleParameter("P_PSYCHO_SOCIAL", OracleDbType.Varchar2) { Value = dentalAssessment.P_PSYCHO_SOCIAL ?? (object)DBNull.Value },
					new OracleParameter("USER_ID", OracleDbType.Varchar2) { Value = ut.AUSR_ID ?? (object)DBNull.Value }


				},

				new List<OracleParameter>
				{
					//new OracleParameter("CREATEUSR", OracleDbType.Varchar2) { Value = dentalAssessment.CREATEUSR ?? (object)DBNull.Value },
					  new OracleParameter("CREATEUSR", OracleDbType.Varchar2) { Value = ut.AUSR_ID ??(object) DBNull.Value },
					new OracleParameter("ICDSL_NO", OracleDbType.Int32) { Value = dentalAssessment.ICDSL_NO ?? (object)DBNull.Value },
					new OracleParameter("EDOCID", OracleDbType.Varchar2) { Value = dentalAssessment.EDOCID ?? (object)DBNull.Value },
					new OracleParameter("DOCID", OracleDbType.Varchar2) { Value = dentalAssessment.DOCID ?? (object)DBNull.Value },
					new OracleParameter("ICDDIGNOSIS", OracleDbType.Varchar2) { Value = dentalAssessment.ICDDIGNOSIS ?? (object)DBNull.Value },
					new OracleParameter("PATI", OracleDbType.Varchar2) { Value = dentalAssessment.PATI ?? (object)DBNull.Value },
					new OracleParameter("BRANCHCODE", OracleDbType.Varchar2) { Value = dentalAssessment.BRANCHCODE ?? (object)DBNull.Value },
					new OracleParameter("RETVAL", OracleDbType.Int32) { Direction = ParameterDirection.Output }
				}
			
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
								//await connection.CloseAsync();
								//if(i==sqlStatements.Count-1)
								//{
								//	//var val = oprep.ExecuteSpOnlineVitalStatusUpd(dentalAssessment.vital, ut);

								//	//if(val.Result.Status==200)
								//	//{
								//	//	await connection.CloseAsync();
								//	//	for (var j = 0; j < dentalAssessment.IcdList.Count; j++)
								//	//	{

								//	//		await oprep.InsertOnlineDocICDDetailsAsync(dentalAssessment.IcdList[j], ut, j);
								//	//	}

								//	//	return new { Status = 200, Message = "Submission Successfull" };
								//	//}
								//	//else
								//	//{

								//	//	return new { Status = 500, Message = "Error while inserting vitals" };
								//	//}


								//}


							}
							catch (Exception innerEx)
							{
								return new { Status = 500, Message = $"Error in statement {i + 1}: {innerEx.Message}" };
							}
						}
					}
					await connection.CloseAsync();



					//await connection.CloseAsync();


				

						return new { Status = 200, Message = "Submission Successfull" };
				


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


		
		public async Task<int> GetCurrentMedi(string edocid)
		{
			string connectionString = _DbContext.Database.GetConnectionString();
			string query = @"
				SELECT Count(*) Count FROM  UCHEMR.EMR_BASIC_ASSESSMENT WHERE EMR_DOC_ID= :edocid";
			//var connectionString = 
			int strVal = 0;
			using (var connection = new OracleConnection(connectionString))
			{
				await connection.OpenAsync();

				using (var command = new OracleCommand(query, connection))
				{
					// Add parameters to avoid SQL injection
					command.Parameters.Add(new OracleParameter(":edocid", edocid));

					var result = await command.ExecuteScalarAsync();

					// Check if the result is null (i.e., no data was found)
					if (result == DBNull.Value)
					{
						return 1; // Return 1 if no result is found
					}
					strVal=Convert.ToInt32(result);
					// Return the result as an integer, converting it from object to int
					//return Convert.ToInt32(result);
					return strVal;
				}
			}
		}

		public async Task<dynamic> SaveComplaintAsync1(Complaint c, UserTocken ut)
		{
			try
			{
				int stcount;
				var result = await GetCurrentMedi(c.EDOCID); // Call the method
				stcount = result;
				if (stcount == 0)
				{
					var sql = @"INSERT INTO UCHEMR.EMR_BASIC_ASSESSMENT(PATI_ID, EMR_DOC_ID, CURRENT_MEDICATION, CREATED_BY)
                    VALUES(:PATI_ID, :P_EMR_DOC_ID, :CURRENT_MEDICATION, :CREATED_BY)";

					var patid = new OracleParameter("PATI_ID", OracleDbType.Varchar2) { Value = c.PATI_ID };
					var emrDocId = new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = c.P_EMR_DOC_ID };
					var curmedication = new OracleParameter("CURRENT_MEDICATION", OracleDbType.Varchar2) { Value = c.CURRENT_MEDICATION };
					var createdby = new OracleParameter("CREATED_BY", OracleDbType.Varchar2) { Value = ut.AUSR_ID };

					using (var cmd1 = _DbContext.Database.GetDbConnection().CreateCommand())
					{
						cmd1.CommandText = sql;
						cmd1.CommandType = CommandType.Text;

						// Add parameters
						cmd1.Parameters.Add(patid);
						cmd1.Parameters.Add(emrDocId);
						cmd1.Parameters.Add(curmedication);
						cmd1.Parameters.Add(createdby);

						// Add timeout to avoid hanging
						cmd1.CommandTimeout = 30; // Timeout in seconds

						// Open connection if necessary
						if (cmd1.Connection.State != ConnectionState.Open)
							await cmd1.Connection.OpenAsync();

						// Execute the procedure
						await cmd1.ExecuteNonQueryAsync();

						cmd1.Connection.CloseAsync();

						// Handle output or result here if needed
						return new { Status = 200, Message = "Saved successfully." };
					}
				}
				else if (stcount >= 1)
				{
					var sql = @"UPDATE UCHEMR.EMR_BASIC_ASSESSMENT 
            SET CURRENT_MEDICATION = :CURRENT_MEDICATION
            WHERE EMR_DOC_ID = :P_EMR_DOC_ID";

					var emrDocId = new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2)
					{
						Value = c.P_EMR_DOC_ID
					};

					var curMedication = new OracleParameter("CURRENT_MEDICATION", OracleDbType.Varchar2)
					{
						Value = c.CURRENT_MEDICATION
					};

					using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
					{
						cmd.CommandText = sql;
						cmd.CommandType = CommandType.Text;

						// Add parameters
						cmd.Parameters.Add(curMedication);
						cmd.Parameters.Add(emrDocId);

						// Add timeout to avoid hanging
						cmd.CommandTimeout = 30; // Timeout in seconds

						// Open connection if necessary
						if (cmd.Connection.State != ConnectionState.Open)
							await cmd.Connection.OpenAsync();

						// Execute the query
						var result1 = await cmd.ExecuteNonQueryAsync();

						if (result1 > 0)
						{
							Console.WriteLine("Update successful.");
						}
						else
						{
							Console.WriteLine("No rows were updated. Check if EMR_DOC_ID exists.");
						}
						cmd.Connection.CloseAsync();

						// Handle output or result here if needed
						//	//return new { Status = 200, Message = "Saved successfully." };
						//}
						//using (var cmd2 = _DbContext.Database.GetDbConnection().CreateCommand())
						//{
						//cmd1.CommandText = sql1;
						//cmd1.CommandType = CommandType.Text;
						//// Add parameters
						////cmd1.Parameters.Add(emrDocId);
						////cmd1.Parameters.Add(prevm);
						//// Add timeout to avoid hanging
						//cmd1.CommandTimeout = 30; // Timeout in seconds

						//// Open connection if necessary
						//if (cmd1.Connection.State != ConnectionState.Open)
						//	await cmd1.Connection.OpenAsync();

						//// Execute the procedure
						//await cmd1.ExecuteNonQueryAsync();

						// Handle output or result here if needed
						return new { Status = 200, Message = "Saved successfully." };
					}

				}
				else
				{
					return new { Status = 500, Message = $"Error: Invalid" };
				}
			}
			catch (Exception ex)
			{
				// Handle errors
				return new { Status = 500, Message = $"Error: {ex.Message}" };
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

				var sql = @" SELECT A.SYSTEMIC,A.LOCALEX , 
               B.MH_OTHER p_mh_Other,B.FAMILY_MED_HISTORY,B.PATI_ID,B.EMR_DOC_ID,C.DOCT_REMARKS COMPLNT ,C.PRSNT_ILLNESS HSTRY,C.TREATMENT_REMARKS_NEW,
               D.OPERATION_NOTE,E.ICD_DIGNOSIS ,F.CURRENT_MEDICATION
               FROM UCHEMR.EMR_PRESCRIPTION_ADVICE A 
			   INNER JOIN   UCHMASTER.OPN_PATIENT_MEDICAL_HISTORY B ON A.EMR_DOC_ID= B.EMR_DOC_ID
               INNER JOIN   UCHEMR.EMR_DOCUMENT_DETAILS C ON B.EMR_DOC_ID=C.EMR_DOC_ID and B.PATI_ID=C.PATI_ID  
			   INNER JOIN  UCHEMR.Operation_note  D ON  B.EMR_DOC_ID=D.EMR_DOC_ID  and B.PATI_ID=D.PATI_ID  
			   INNER JOIN  UCHEMR.EMR_VISIT_ICD  E ON  B.EMR_DOC_ID=E.EMR_DOC_ID  and B.PATI_ID=E.PATI_ID
				INNER JOIN UCHEMR.EMR_BASIC_ASSESSMENT F ON F.EMR_DOC_ID=A.EMR_DOC_ID
               where A.EMR_DOC_ID=:edocId";

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

                    //var vasl = UpdateCurrentMedication(parameters.EdocId, parameters.currentmedication, parameters.PatiId);

                    //if (vasl == 1)
                    //{
                    return new { Status = 200, Message = "inserted successfully." };

                    //}
                    //else
                    //{

                    //return new { Status = 505, Message = "Error while insertion." };
                    //}
                }
            }
            catch (Exception ex)
            {
                return new { Status = 500, Message = ex.Message };
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
				//:P_ICD_NO,
				var sql = @"BEGIN UCHEMR.SP_ONLINE_DOC_ICD_DTLS_INS(
                :ICDID, :ICDSL_NO, :ICDCODE_ID, :ICDCODE_DTLS_ID, 
                :RMRKS, :EMRDOCID, :PATIID, 
                :P_MRD_UPDATE_STS, :P_MRD_UPDATE_USR, :P_MRD_UPDATE_DATE,:P_ICD_NO,
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
					new OracleParameter("P_ICD_NO", OracleDbType.Int32) { Value = i + 1 },
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
					await connection.CloseAsync();
				}
			}
				catch (Exception ex)
			{
				throw new Exception($"Error executing SP_ONLINE_DOC_ICD_DTLS_INS for item {i}", ex);
			}
		}

		public async Task InsertCaseAsync(CaseDetails a, UserTocken ut, int i)
		{
			try
			{
				var retvalParameter = new OracleParameter
				{
					ParameterName = "RETVAL",
					OracleDbType = OracleDbType.Int32,
					Direction = ParameterDirection.Output
				};
				//:P_ICD_NO,
				//CASE_ID,EMR_DOC_ID, TREATMENT_STS,MEDICATIONS
				var sql = @"BEGIN  UCHEMR.SP_KNOW_CASE_OF_INS_UPD(
                :CASE_ID, :EMR_DOC_ID, :TREATMENT_STS, :MEDICATIONS, 
                :RETVAL); 
                END;";

				var parameters = new List<OracleParameter>
				{
					new OracleParameter("CASE_ID", OracleDbType.Varchar2) { Value = a.CaseId },
					new OracleParameter("EMR_DOC_ID", OracleDbType.Varchar2) { Value = a.emrDocId},
					new OracleParameter("TREATMENT_STS", OracleDbType.Varchar2) { Value = a.TreatmentSts },
					new OracleParameter("MEDICATIONS", OracleDbType.Varchar2) { Value = a.Medication },
					//new OracleParameter("P_ICD_NO", OracleDbType.Int32) { Value = i + 1 },
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
					await connection.CloseAsync();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error executing SP_ONLINE_DOC_ICD_DTLS_INS for item {i}", ex);
			}
		}

		public async Task InsertSymptomsAsync(Symptoms a,Symptomsdet sym, UserTocken ut, int i)
		{
			try
			{
				var retvalParameter = new OracleParameter
				{
					ParameterName = "RETVAL",
					OracleDbType = OracleDbType.Int32,
					Direction = ParameterDirection.Output
				};
				//:P_ICD_NO,
				//CASE_ID,EMR_DOC_ID, TREATMENT_STS,MEDICATIONS
				var sql = @"BEGIN   UCHEMR.SP_SYMPTOMS_EMRDOCID_INS_UPD(
                :SE_ID,:SYMP_ID, :EMR_DOC_ID, :REMARKS,  
                :RETVAL); 
                END;";

				var parameters = new List<OracleParameter>
				{
					new OracleParameter("SE_ID", OracleDbType.Varchar2) { Value = sym.SeId},
					new OracleParameter("SYMP_ID", OracleDbType.Varchar2) { Value = a.SYMP_ID },
					new OracleParameter("EMR_DOC_ID", OracleDbType.Varchar2) { Value = sym.EmrdocId},
					new OracleParameter("REMARKS", OracleDbType.Varchar2) { Value = sym.Remarks },
					//new OracleParameter("RETVAL", OracleDbType.Int32) { Direction = ParameterDirection.Output },
					//new OracleParameter("P_ICD_NO", OracleDbType.Int32) { Value = i + 1 },
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
					await connection.CloseAsync();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error executing SP_SYMPTOMS_EMRDOCID_INS_UPD for item {i}", ex);
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
                    HCIRCUMFERENCE, BP_LEFTMAX, BP_LEFTMIN, TEMP_UNIT, VITAL_EMP_ID, SPO2, NURSE_NOTE, PAINASSESSMENT, NURSE_REMARKS, GRBS, NURSE,LOCATION,PAINREMARKS P_REMARKS,BG,BLD_GRP_ID,
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
                ) 
            ORDER BY EVENT_DT DESC";
                //    UNION ALL

                //    SELECT 
                //        EMP_OFFL_NAME, '' VISIT_ID, '' DOCT_ID, HEIGHT, WEIGHT, TEMP, PULSE, UNIT, EVENT_DT, STATUS, MAX_BP, MIN_BP, RR, EMR_DOC_ID, V.EMP_ID, 
                //        HCIRCUMFERENCE, BP_LEFTMAX, BP_LEFTMIN, TEMP_UNIT, VITAL_EMP_ID, SPO2, NURSE_NOTE, PAINASSESSMENT, '' NURSE_REMARKS, GRBS, '' NURSE,LOCATION,REMARKS,BG,BLD_GRP_ID,
                //        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/HEIGHT.PNG' HEIGHT_IMG, 
                //        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/WEIGHT.PNG' WEIGHT_IMG,
                //        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/TEMP.PNG' TEMP_IMG, 
                //        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/PULSE.PNG' PULSE_IMG,
                //        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/MAX_BP.PNG' MAX_BP_IMG, 
                //        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/MIN_BP.PNG' MIN_BP_IMG,
                //        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/RR.PNG' RR_IMG, 
                //        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/HCIRCUMFERENCE.PNG' HCIRCUMFERENCE_IMG,
                //        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/BP_LEFTMAX.PNG' BP_LEFTMAX_IMG, 
                //        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/BP_LEFTMIN.PNG' BP_LEFTMIN_IMG,
                //        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/SPO2.PNG' SPO2_IMG, 
                //        'REMEDISERVICE/IMAGES/VITALS/OP/ICONS/GRBS.PNG' GRBS_IMG
                //    FROM UCHEMR.EMR_PATIENT_VITALS_STATUS_EDIt V
                //    INNER JOIN UCHMASTER.HRM_EMPLOYEE E ON E.EMP_ID = V.EMP_ID
                //    WHERE EMR_DOC_ID = :EmrDocId
                //) 
                //ORDER BY EVENT_DT DESC";

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

		public async Task<List<CaseDetails>> GetActiveCaseDetails()
		{
			try
			{
				string connectionString = _DbContext.Database.GetConnectionString();
				string query = @"
             SELECT * FROM UCHEMR.KNOW_CASE_OF_MASTER where NVL(ACTIVE_STATUS,'A')='A'";

				List<CaseDetails> caseList = new List<CaseDetails>();

				using (var connection = new OracleConnection(connectionString))
				{
					await connection.OpenAsync();

					using (var command = new OracleCommand(query, connection))
					{
						using (var reader = await command.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								var caseDetails = new CaseDetails
								{
									CaseId = reader["CASE_ID"] != DBNull.Value ? Convert.ToInt32(reader["CASE_ID"]) : 0,
									CaseName = reader["CASE_NAME"] != DBNull.Value ? reader["CASE_NAME"].ToString() : string.Empty,
									ActiveStatus = reader["ACTIVE_STATUS"] != DBNull.Value ? reader["ACTIVE_STATUS"].ToString() : string.Empty,
									CaseCode = reader["CASE_CODE"] != DBNull.Value ? reader["CASE_CODE"].ToString() : string.Empty
								};

								caseList.Add(caseDetails);
							}
						}
					}
				}

				return caseList;
			}
			catch (OracleException oracleEx)
			{
				// Log or handle Oracle-specific exceptions
				throw new Exception($"Oracle error executing query: {oracleEx.Message}");
			}
			catch (Exception ex)
			{
				// General exception catch
				throw new Exception($"Error executing query: {ex.Message}");
			}
		}

		public async Task<dynamic> DeleteKnowcase(string emrDocId)
		{
			try
			{
				var sql = "delete from UCHEMR.KNOW_CASE_OF_PATIENT where EMR_DOC_ID=:emrDocId";
				var cmd = _DbContext.Database.GetDbConnection().CreateCommand();
				cmd.CommandText = sql;

				// Add the parameter for the SQL query
				var param = cmd.CreateParameter();
				param.ParameterName = ":emrDocId";
				param.Value = emrDocId;
				cmd.Parameters.Add(param);

				// Open the connection if it is closed
				if (cmd.Connection.State != System.Data.ConnectionState.Open)
				{
					await cmd.Connection.OpenAsync();
				}

				// Execute the query and check how many rows were affected
				var affectedRows = await cmd.ExecuteNonQueryAsync();
				await cmd.Connection.CloseAsync();

				if (affectedRows > 0)
				{
					return new { Status = 1, Message = "Deletion successful" }; // Return success message
				}
				else
				{
					return new { Status = 0, Message = "No records found to delete" }; // Handle no records found case
				}
			}
			catch (Exception ex)
			{
				return new { Status = 500, Message = ex.Message }; // Return error message on exception
			}
		}

		public async Task<dynamic> SaveKnowcaseAsync(KnowCase dentalAssessment, UserTocken ut)
		{
			try
			{
				await DeleteKnowcase(dentalAssessment.EmrdocId);

				for (var j = 0; j < dentalAssessment.CaseList.Count; j++)
				{

					//await oprep.InsertOnlineDocICDDetailsAsync(dentalAssessment.IcdList[j], ut, j);
					await InsertCaseAsync(dentalAssessment.CaseList[j], ut, j);
				}

				// Array of SQL statements and corresponding parameters
				var sqlStatements = new List<string>
			{
				@"BEGIN  UCHEMR.SP_KNOW_CASE_OF_RMRK_INS_UPD(
					:P_EMR_DOC_ID,:P_REMARKS,:RETVAL
				); END;",
						
            
				// Add other SQL statements here
			};

				var parametersList = new List<List<OracleParameter>>
			{
					new List<OracleParameter>
				{

					new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = dentalAssessment.EmrdocId?? (object)DBNull.Value },
					new OracleParameter("P_REMARKS", OracleDbType.Varchar2) { Value = dentalAssessment.Remarks ?? (object)DBNull.Value },
						 new OracleParameter("RETVAL", OracleDbType.Int32) { Direction = ParameterDirection.Output }
				},
				
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
					await connection.CloseAsync();



					//await connection.CloseAsync();




					return new { Status = 200, Message = "Submission Successfull" };



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


		public async Task<dynamic> DeleteSymptoms(string emrDocId)
		{
			try
			{
				var sql = "delete from UCHEMR.SYMPTOMS_EMRDOCID  where EMR_DOC_ID=:emrDocId";
				var cmd = _DbContext.Database.GetDbConnection().CreateCommand();
				cmd.CommandText = sql;

				// Add the parameter for the SQL query
				var param = cmd.CreateParameter();
				param.ParameterName = ":emrDocId";
				param.Value = emrDocId;
				cmd.Parameters.Add(param);

				// Open the connection if it is closed
				if (cmd.Connection.State != System.Data.ConnectionState.Open)
				{
					await cmd.Connection.OpenAsync();
				}

				// Execute the query and check how many rows were affected
				var affectedRows = await cmd.ExecuteNonQueryAsync();
				await cmd.Connection.CloseAsync();

				if (affectedRows > 0)
				{
					return new { Status = 1, Message = "Deletion successful" }; // Return success message
				}
				else
				{
					return new { Status = 0, Message = "No records found to delete" }; // Handle no records found case
				}
			}
			catch (Exception ex)
			{
				return new { Status = 500, Message = ex.Message }; // Return error message on exception
			}
		}
		public async Task<dynamic> SaveSymptomsAsync(Symptomsdet dentalAssessment, UserTocken ut)
		{
			try
			{
				await DeleteSymptoms(dentalAssessment.EmrdocId);

				for (var j = 0; j < dentalAssessment.SymptList.Count; j++)
				{

					//await oprep.InsertOnlineDocICDDetailsAsync(dentalAssessment.IcdList[j], ut, j);
					await InsertSymptomsAsync(dentalAssessment.SymptList[j], dentalAssessment, ut, j);
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




		public async Task<List<Symptoms>> GetSymptoms(string DoctId)
		{
			try
			{
				string connectionString = _DbContext.Database.GetConnectionString();
				string query = @"
           SELECT * FROM UCHMASTER.MED_SYMPTOMS_MASTER MSM  
           INNER JOIN UCHEMR.DOCT_SYMPTOM_SETTINGS DS ON MSM.SYMP_ID=DS.SYMP_ID AND DOCTID=:DoctId
           WHERE DS.ACTIVE_STATUS='A' ORDER BY DS.PRIORITY";

				List<Symptoms> SymptomsList = new List<Symptoms>();

				using (var connection = new OracleConnection(connectionString))
				{
					await connection.OpenAsync();

					using (var command = new OracleCommand(query, connection))
					{
						// Bind the parameter DoctId
						command.Parameters.Add(new OracleParameter("DoctId", OracleDbType.Varchar2) { Value = DoctId });

						using (var reader = await command.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								var Symptomsdet = new Symptoms
								{
									SYMP_ID = reader["SYMP_ID"] != DBNull.Value ? reader["SYMP_ID"].ToString() : string.Empty,
									SYMP_NAME = reader["SYMP_NAME"] != DBNull.Value ? reader["SYMP_NAME"].ToString() : string.Empty,
								};

								SymptomsList.Add(Symptomsdet);
							}
						}
					}
				}

				return SymptomsList;
			}
			catch (OracleException oracleEx)
			{
				// Log or handle Oracle-specific exceptions
				throw new Exception($"Oracle error executing query: {oracleEx.Message}");
			}
			catch (Exception ex)
			{
				// General exception catch
				throw new Exception($"Error executing query: {ex.Message}");
			}
		}


		public async Task<dynamic> GetAllergy(string patId,string DoctId)
		{
			try
			{

				var sql = @" select * from UCHMASTER.OPN_PATIENT_MEDICAL_HISTORY where  PATI_ID=:patId and DOCT_ID=:DoctId";

				// Define the Oracle parameter
				var parameters = new List<OracleParameter>
				{
					new OracleParameter("PATI_ID", OracleDbType.Varchar2) { Value = patId },
					new OracleParameter("DOCT_ID", OracleDbType.Varchar2) { Value = DoctId }
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


		public async Task<dynamic> GetEduAdv(string emrDocId)
		{
			try
			{

				var sql = @"select * from  UCHEMR.PAST_MEDI_SUR_HISTORY  where EMR_DOC_ID=:emrDocId";

				// Define the Oracle parameter
				var parameters = new List<OracleParameter>
				{
					new OracleParameter("EMR_DOC_ID", OracleDbType.Varchar2) { Value = emrDocId }
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
		
			public async Task<dynamic> GetKnowcase(string emrDocId)
		{
			try
			{

				var sql = @"Select K.CASE_ID, C.CASE_NAME, C.CASE_CODE, K.EMR_DOC_ID, K.TREATMENT_STS, 
				K.MEDICATIONS , R.REMARKS FROM UCHEMR.KNOW_CASE_OF_PATIENT K
				INNER JOIN UCHEMR.KNOW_CASE_OF_REMARKS R ON R.EMR_DOC_ID= K.EMR_DOC_ID
				INNER JOIN UCHEMR.KNOW_CASE_OF_MASTER C ON C.CASE_ID= K.CASE_ID
				WHERE K.EMR_DOC_ID=:emrDocId";

				// Define the Oracle parameter
				var parameters = new List<OracleParameter>
				{
					new OracleParameter("EMR_DOC_ID", OracleDbType.Varchar2) { Value = emrDocId }
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


		public async Task<dynamic> GetSymptomsDet(string emrDocId)
		{
			try
			{

				var sql = @"SELECT S.SE_ID, S.SYMP_ID, S.EMR_DOC_ID,S.REMARKS,M.SYMP_NAME FROM UCHEMR.SYMPTOMS_EMRDOCID S
			INNER JOIN UCHMASTER.MED_SYMPTOMS_MASTER  M ON M.SYMP_ID=S.SYMP_ID WHERE  S.EMR_DOC_ID=:emrDocId";

				// Define the Oracle parameter
				var parameters = new List<OracleParameter>
				{
					new OracleParameter("EMR_DOC_ID", OracleDbType.Varchar2) { Value = emrDocId }
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

        public async Task<dynamic> GetBloodGroup()
        {
            try
            {

                var sql = @"select BLD_GRP_ID,UPPER(BLD_GRP_NAME) ||' ' || BLD_RH_FACTOR BLD_GRP_NAME  FROM UCHMASTER.GEN_BLOOD_GROUP";

                // Define the Oracle parameter
                //var parameters = new List<OracleParameter>
                //{
                //	new OracleParameter("PATI_ID", OracleDbType.Varchar2) { Value = patId },
                //	new OracleParameter("DOCT_ID", OracleDbType.Varchar2) { Value = DoctId }
                //};

                // Execute the query
                using (var cmd = _DbContext.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    //cmd.Parameters.AddRange(parameters.ToArray());

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

        //public async Task<dynamic> SaveComplaintAsync1(Complaint c, UserTocken ut)
        //{
        //	try
        //	{
        //		// SQL to call the stored procedure

        //		var sql = @"INSERT INTO UCHEMR.EMR_BASIC_ASSESSMENT(PATI_ID,EMR_DOC_ID,CURRENT_MEDICATION,CREATED_BY)
        //		VALUES(:PATI_ID,:P_EMR_DOC_ID,:CURRENT_MEDICATION,:CREATED_BY)";


        //		var patid = new OracleParameter("PATI_ID", OracleDbType.Varchar2) { Value = c.PATI_ID };
        //		var emrDocId = new OracleParameter("P_EMR_DOC_ID", OracleDbType.Varchar2) { Value = c.P_EMR_DOC_ID };

        //		var curmedication = new OracleParameter("CURRENT_MEDICATION", OracleDbType.Varchar2) { Value = c.CURRENT_MEDICATION };
        //		var createdby = new OracleParameter("CREATED_BY", OracleDbType.Varchar2) { Value =ut.AUSR_ID };
        //		using (var cmd1 = _DbContext.Database.GetDbConnection().CreateCommand())
        //		{
        //			cmd1.CommandText = sql;
        //			cmd1.CommandType = System.Data.CommandType.Text;

        //			// Add the parameter to the command
        //			//cmd.Parameters.Add(EMR_DOC_ID);
        //			//cmd.Parameters.Add(CREATE_USER);
        //			//cmd.Parameters.Add(CREATED_DATE);

        //			cmd1.Parameters.Add(patid);
        //			cmd1.Parameters.Add(emrDocId);
        //			cmd1.Parameters.Add(curmedication);
        //			cmd1.Parameters.Add(createdby);

        //			// Open connection if not already open
        //			if (cmd1.Connection.State != ConnectionState.Open)
        //				await cmd1.Connection.OpenAsync();

        //			// Execute the procedure
        //			await cmd1.ExecuteNonQueryAsync();

        //			// Return success response
        //			return new
        //			{
        //				Status = 200,
        //				Message = " saved successfully."
        //			};
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		return new { Status = 500, Message = $"Error: {ex.Message}" };
        //	}
        //}



    }
}
