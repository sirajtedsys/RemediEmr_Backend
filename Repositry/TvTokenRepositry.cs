using Microsoft.EntityFrameworkCore;
using RemediEmr.Class;
using static JwtService;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.IO;
using RemediEmr.Data.Class;
using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace RemediEmr.Repositry
{
    public class TvTokenRepositry
    {


        private readonly AppDbContext _DbContext;

        protected readonly IConfiguration _configuration;

        private readonly JwtHandler jwthand;
        private readonly string con;

        public TvTokenRepositry(AppDbContext dbContext, JwtHandler _jwthand, IConfiguration configuration)
        {
            _DbContext = dbContext;
            jwthand = _jwthand;
            _configuration = configuration;
            con = _DbContext.Database.GetConnectionString();
        }

        public async Task<dynamic> SaveOrUpdateDevice(string deviceId, string fcmToken, string name, string dispLocationId)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(con))
                {
                    conn.Open();

                    using (OracleCommand cmdCheck = new OracleCommand("SELECT COUNT(*) FROM UCHDISPLAY.DEVICE_EXT WHERE DEVICE_ID = :deviceId", conn))
                    {
                        cmdCheck.Parameters.Add(new OracleParameter("deviceId", deviceId));
                        int count = Convert.ToInt32(cmdCheck.ExecuteScalar());

                        if (count > 0)
                        {
                            // Update existing record
                            using (OracleCommand cmdUpdate = new OracleCommand(
                                "UPDATE UCHDISPLAY.DEVICE_EXT SET FCM_TOKEN = :fcmToken, NAME = :name, DISP_LOCATION_ID = :dispLocationId, ACTIVE_STATUS = 1 WHERE DEVICE_ID = :deviceId", conn))
                            {
                                cmdUpdate.Parameters.Add(new OracleParameter("fcmToken", fcmToken));
                                cmdUpdate.Parameters.Add(new OracleParameter("name", name));
                                cmdUpdate.Parameters.Add(new OracleParameter("dispLocationId", dispLocationId));
                                cmdUpdate.Parameters.Add(new OracleParameter("deviceId", deviceId));

                                cmdUpdate.ExecuteNonQuery();

                                return new { Status = 200, Message = "Device Updated Successfully"};
                            }
                        }
                        else
                        {
                            // Insert new record
                            using (OracleCommand cmdInsert = new OracleCommand(
                                "INSERT INTO UCHDISPLAY.DEVICE_EXT(ID, DEVICE_ID, FCM_TOKEN, NAME, ADDED_DATE, DISP_LOCATION_ID) " +
                                "VALUES (UCHDISPLAY.DEVICE_SEQ.NEXTVAL, :deviceId, :fcmToken, :name, SYSDATE, :dispLocationId)", conn))
                            {
                                cmdInsert.Parameters.Add(new OracleParameter("deviceId", deviceId));
                                cmdInsert.Parameters.Add(new OracleParameter("fcmToken", fcmToken));
                                cmdInsert.Parameters.Add(new OracleParameter("name", name));
                                cmdInsert.Parameters.Add(new OracleParameter("dispLocationId", dispLocationId));

                                cmdInsert.ExecuteNonQuery();
                                return new { Status = 200, Message = "Deveice Added Successfully" };
                            }
                        }
                    }
                }
            }
            catch (OracleException ex)
            {

                return new { Status = 400, Message = ex.Message };
            }
            catch (Exception ex)
            {
                return new { Status = 400, Message = ex.Message };
            }
        }

        public async Task<dynamic> GetTokenDetails(string deviceId)
        {
            var tokens = new List<TokenInfo>();
            try
            {
                using (var connection = new OracleConnection(con))
                {
                    connection.Open();
                    string query = @"
                SELECT DISTINCT TO_DATE(PTC.CREATE_DATE, 'DD/MM/YY'), SPEC_NAME, DOCTOR,
                    UCHTRANS.FN_GET_FORMATTED_OP_NO(P.PATI_OPNO) PATI_OPNO, 
                    BILL_CUSTOMERNAME, BILL_GENDER, PTC.BILL_HDR_ID, PTC.TOKEN_CAT_ID, PTC.TOKEN_READ_VOICE_STS,
                    'ROOM ' || CR.ROOM_NO AS RoomNAME,
                    (PTC.PRCCAT_TOKEN_PREFIX || PTC.PRCCAT_TOKEN_NO) AS TOKENNO, 
                    PTC.TOKEN_CALL_STS, PTC.HOLD_STS, TOKEN_READ_STS, CR.ROOM_NO,
                    NVL(DOC.PROFILE_PIC,'doctor.png') PROFILE_PIC
                FROM UCHTRANS.PRC_TOKEN_CALL PTC 
                INNER JOIN UCHTRANS.LBM_BILLING_HDR H ON H.BILL_HDR_ID = PTC.BILL_HDR_ID
                LEFT JOIN UCHDISPLAY.CONSULT_ROOM CR ON CR.ROOM_ID = NVL(OTH_DOCT_ROOM_ID, PTC.ROOM_ID)
                LEFT JOIN UCHMASTER.OPN_PATIENT P ON P.PATI_ID = H.PATI_ID
                LEFT JOIN UCHMASTER.OPN_DOCTOR DR ON DR.DOCT_ID = NVL(H.OTHER_DOCTOR_ID, H.DOCT_ID)
                LEFT JOIN UCHDISPLAY.DOCTOR_EXT DOC ON DOC.ID = DR.EMPLOYEE_ID
                LEFT JOIN (
                    SELECT EMP_ID,
                           DECODE(NVL(SALT_name, ''), '', '', NVL(SALT_name, '') || ' ') || EMP_OFFL_NAME DOCTOR
                    FROM UCHMASTER.HRM_EMPLOYEE e
                    LEFT JOIN UCHMASTER.HRM_SALUTATION s ON s.SALT_ID = e.SALT_ID
                ) DRE ON DRE.EMP_ID = DR.EMPLOYEE_ID
                LEFT JOIN UCHMASTER.OPN_SPECIALIZATIONS OS ON OS.SPEC_ID = DR.SPEC_ID
                WHERE NVL(OTH_DOCT_ROOM_ID, PTC.ROOM_ID) IN (
                    SELECT DDRM.ROOM_ID
                    FROM UCHDISPLAY.DEVICE_DOCTOR_ROOM_MAP_EXT DDRM 
                    INNER JOIN UCHDISPLAY.DEVICE_EXT D ON DDRM.DEVICE_ID = D.ID
                    INNER JOIN UCHDISPLAY.CONSULT_ROOM CR ON CR.ROOM_ID = DDRM.ROOM_ID 
                    WHERE D.DEVICE_ID = :deviceId
                )
                AND PTC.TOKEN_READ_STS = 'N' 
                AND PTC.TOKEN_CALL_STS = 'Y' 
                AND TO_DATE(PTC.CREATE_DATE, 'DD/MM/YY') = TO_DATE(SYSDATE, 'DD/MM/YY')
            ";

                    using (var command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add(new OracleParameter("deviceId", deviceId));
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tokens.Add(new TokenInfo
                                {
                                    SpecName = reader["SPEC_NAME"].ToString(),
                                    Doctor = reader["DOCTOR"].ToString(),
                                    PatientOpNo = reader["PATI_OPNO"].ToString(),
                                    BillCustomerName = reader["BILL_CUSTOMERNAME"].ToString(),
                                    BillGender = reader["BILL_GENDER"].ToString(),
                                    BillHdrId = reader["BILL_HDR_ID"].ToString(),
                                    TokenCatId = reader["TOKEN_CAT_ID"].ToString(),
                                    TokenReadVoiceStatus = reader["TOKEN_READ_VOICE_STS"].ToString(),
                                    RoomName = reader["RoomNAME"].ToString(),
                                    TokenNo = reader["TOKENNO"].ToString(),
                                    TokenCallStatus = reader["TOKEN_CALL_STS"].ToString(),
                                    HoldStatus = reader["HOLD_STS"].ToString(),
                                    TokenReadStatus = reader["TOKEN_READ_STS"].ToString(),
                                    RoomNo = reader["ROOM_NO"].ToString(),
                                    PROFILE_PIC = reader["PROFILE_PIC"].ToString()
                                });
                            }
                        }
                    }
                }

                await WebSocketHandler.NotifyClients(tokens);
            }
            catch (OracleException ex)
            {
                return new { Status = 400, Message = ex.Message };
            }
            catch (Exception ex)
            {
                return new { Status = 400, Message = ex.Message };
            }
            return tokens;
        }

        public async Task<List<TokenInfo>> GetTokenDetailsForMonitoring(string deviceId)
        {
            var tokens = new List<TokenInfo>();
            try
            {
                using (var connection = new OracleConnection(con))
                {
                    connection.Open();
                    string query = @"
                SELECT DISTINCT TO_DATE(PTC.CREATE_DATE, 'DD/MM/YY'), SPEC_NAME, DOCTOR,
                    UCHTRANS.FN_GET_FORMATTED_OP_NO(P.PATI_OPNO) PATI_OPNO, 
                    BILL_CUSTOMERNAME, BILL_GENDER, PTC.BILL_HDR_ID, PTC.TOKEN_CAT_ID, PTC.TOKEN_READ_VOICE_STS,
                    'ROOM ' || CR.ROOM_NO AS RoomNAME,
                    (PTC.PRCCAT_TOKEN_PREFIX || PTC.PRCCAT_TOKEN_NO) AS TOKENNO, 
                    PTC.TOKEN_CALL_STS, PTC.HOLD_STS, TOKEN_READ_STS, CR.ROOM_NO,
                    NVL(DOC.PROFILE_PIC,'doctor.png') PROFILE_PIC
                FROM UCHTRANS.PRC_TOKEN_CALL PTC 
                INNER JOIN UCHTRANS.LBM_BILLING_HDR H ON H.BILL_HDR_ID = PTC.BILL_HDR_ID
                LEFT JOIN UCHDISPLAY.CONSULT_ROOM CR ON CR.ROOM_ID = NVL(OTH_DOCT_ROOM_ID, PTC.ROOM_ID)
                LEFT JOIN UCHMASTER.OPN_PATIENT P ON P.PATI_ID = H.PATI_ID
                LEFT JOIN UCHMASTER.OPN_DOCTOR DR ON DR.DOCT_ID = NVL(H.OTHER_DOCTOR_ID, H.DOCT_ID)
                LEFT JOIN UCHDISPLAY.DOCTOR_EXT DOC ON DOC.ID = DR.EMPLOYEE_ID
                LEFT JOIN (
                    SELECT EMP_ID,
                           DECODE(NVL(SALT_name, ''), '', '', NVL(SALT_name, '') || ' ') || EMP_OFFL_NAME DOCTOR
                    FROM UCHMASTER.HRM_EMPLOYEE e
                    LEFT JOIN UCHMASTER.HRM_SALUTATION s ON s.SALT_ID = e.SALT_ID
                ) DRE ON DRE.EMP_ID = DR.EMPLOYEE_ID
                LEFT JOIN UCHMASTER.OPN_SPECIALIZATIONS OS ON OS.SPEC_ID = DR.SPEC_ID
                WHERE NVL(OTH_DOCT_ROOM_ID, PTC.ROOM_ID) IN (
                    SELECT DDRM.ROOM_ID
                    FROM UCHDISPLAY.DEVICE_DOCTOR_ROOM_MAP_EXT DDRM 
                    INNER JOIN UCHDISPLAY.DEVICE_EXT D ON DDRM.DEVICE_ID = D.ID
                    INNER JOIN UCHDISPLAY.CONSULT_ROOM CR ON CR.ROOM_ID = DDRM.ROOM_ID 
                    WHERE D.DEVICE_ID = :deviceId
                )
                AND PTC.TOKEN_READ_STS = 'N' 
                AND PTC.TOKEN_CALL_STS = 'Y' 
                AND TO_DATE(PTC.CREATE_DATE, 'DD/MM/YY') = TO_DATE(SYSDATE, 'DD/MM/YY')
            ";

                    using (var command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add(new OracleParameter("deviceId", deviceId));
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tokens.Add(new TokenInfo
                                {
                                    SpecName = reader["SPEC_NAME"].ToString(),
                                    Doctor = reader["DOCTOR"].ToString(),
                                    PatientOpNo = reader["PATI_OPNO"].ToString(),
                                    BillCustomerName = reader["BILL_CUSTOMERNAME"].ToString(),
                                    BillGender = reader["BILL_GENDER"].ToString(),
                                    BillHdrId = reader["BILL_HDR_ID"].ToString(),
                                    TokenCatId = reader["TOKEN_CAT_ID"].ToString(),
                                    TokenReadVoiceStatus = reader["TOKEN_READ_VOICE_STS"].ToString(),
                                    RoomName = reader["RoomNAME"].ToString(),
                                    TokenNo = reader["TOKENNO"].ToString(),
                                    TokenCallStatus = reader["TOKEN_CALL_STS"].ToString(),
                                    HoldStatus = reader["HOLD_STS"].ToString(),
                                    TokenReadStatus = reader["TOKEN_READ_STS"].ToString(),
                                    RoomNo = reader["ROOM_NO"].ToString(),
                                    PROFILE_PIC = reader["PROFILE_PIC"].ToString()
                                });
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving token details: {ex.Message}");
            }
            return tokens;
        }

        public async Task<DataTable> GetSettings()
        {
            DataTable dataTable = new DataTable();

            using (OracleConnection conn = new OracleConnection(con))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("SELECT * FROM UCHDISPLAY.SETTINGS_EXT", conn))
                    {
                        using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }

            return dataTable;
        }

        public async Task<bool> UpdateTokenReadVoiceStatusAsync(string billHdrId, string tokenCatId)
        {
            //string connectionString = "Your_Oracle_Connection_String_Here";

            using (OracleConnection conn = new OracleConnection(con))
            {
                try
                {
                    await conn.OpenAsync();
                    string sql = "UPDATE UCHTRANS.PRC_TOKEN_CALL SET TOKEN_READ_VOICE_STS='N' WHERE BILL_HDR_ID=:billHdrId AND TOKEN_CAT_ID=:tokenCatId";

                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("billHdrId", billHdrId));
                        cmd.Parameters.Add(new OracleParameter("tokenCatId", tokenCatId));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0; // True if update successful
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }


        public async Task<dynamic> GetAdsAsync(string deviceId)
        {
            var adsList = new List<dynamic>();

            using (OracleConnection conn = new OracleConnection(con))
            {
                try
                {
                    await conn.OpenAsync();

                    string sql = @"
                SELECT 
                    AD.ID,
                    AD.TITLE,
                    NVL(TO_CHAR(GTT.TRANSLATION), AD.DESCRIPTION) AS DESCRIPTION,
                    AD.URL,
                    AD.DISPLAY_LIMIT,
                    AD.IS_FULLSCREEN,
                    ADT.TITLE AS AD_TYPE
                FROM UCHDISPLAY.ADS_EXT AD
                INNER JOIN UCHMASTER.DISPLAY_DEVICE_ADS_MAPPING M ON M.ADS_ID = AD.ID
                INNER JOIN UCHDISPLAY.DEVICE_EXT D ON D.ID = M.DEVICE_ID
                INNER JOIN UCHDISPLAY.AD_STATISTICS_EXT ADS ON AD.ID = ADS.AD_ID
                INNER JOIN UCHDISPLAY.AD_TYPE_EXT ADT ON ADT.ID = AD.AD_TYPE_ID
                LEFT JOIN UCHMASTER.GEN_TRANSLATION_TEXT GTT ON GTT.GEN_TEXT = AD.DESCRIPTION
                WHERE 
                    ADS.DISPLAYED <= NVL(AD.DISPLAY_LIMIT, 999999)
                    AND D.DEVICE_ID = :deviceId
                    AND TO_DATE(ADS.DISPLAY_DATE, 'DD/MM/YY') = TO_DATE(SYSDATE, 'DD/MM/YY')
                    AND ADS.ACTIVE_STATUS = 1
                    AND AD.ACTIVE_STATUS = 1
                    AND AD.DELETE_STATUS = 0";

                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("deviceId", deviceId));

                        using (OracleDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                dynamic ad = new System.Dynamic.ExpandoObject();
                                ad.Id = reader["ID"];
                                ad.Title = reader["TITLE"];
                                ad.Description = reader["DESCRIPTION"];
                                ad.Url = reader["URL"];
                                ad.DisplayLimit = reader["DISPLAY_LIMIT"];
                                ad.IsFullscreen = reader["IS_FULLSCREEN"];
                                ad.AdType = reader["AD_TYPE"];

                                adsList.Add(ad);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new { Status = 500, Message=ex.Message };
                }
            }


            return new { Status = 200, Data =adsList };
        }



    }
}
