using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using RemediEmr.Class;
using RemediEmr.Data.Class;
using System.Data;
using static JwtService;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace RemediEmr.Repositry
{
    public class DoctorTvTokenRepositry
    {


        private readonly AppDbContext _DbContext;

        protected readonly IConfiguration _configuration;

        private readonly JwtHandler jwthand;
        private readonly string con;

        public DoctorTvTokenRepositry(AppDbContext dbContext, JwtHandler _jwthand, IConfiguration configuration)
        {
            _DbContext = dbContext;
            jwthand = _jwthand;
            _configuration = configuration;
            con = _DbContext.Database.GetConnectionString();
        }


        //        using Oracle.ManagedDataAccess.Client;
        //using System.Data;

        public async Task<dynamic> GetDoctorRoomInfoDataTableAsync(string deviceId)
        {

            var tokens = new List<DoctorTokenInFo>();

            try
            {
                using (var conn = new OracleConnection(con))
                {
                    await conn.OpenAsync();

                    string query = @"SELECT DISTINCT OVD.OPVDTLS_ID,DE.ID,TRS.READ_STATUS TOKEN_READ_STS,OVD.TOKEN_CALL_DATE,'N' LAST_CALLED,UCHTRANS.FN_GET_FORMATTED_OP_NO(OP.PATI_OPNO) OPNO,OP.PATI_FIRST_NAME||' '||OP.PATI_LAST_NAME PATIENT_NAME ,NVL(D.NAME, 
EMP.EMP_OFFL_NAME) DOCTOR_NAME,NVL(D.PROFILE_PIC,'doctor.png') PROFILE_PIC,NVL(D.BACKGROUND_COLOR,'#FFFFFF') BG_COLOR,NVL(DD.TITLE, OS.SPEC_NAME) DEPARTMENT,
DECODE(NVL(INS_TOKEN_SETTINGS,'N'),'N', OVD.TOKEN_WAR||OD.DOCT_TOKEN_PREFIX||'-'||NVL(OVM.CONTINUOUS_TOKEN,OVD.TOCKEN_REGULAR) ,pbt.TOKEN_NO) TOKEN,DCM.ROOM_NO,EMP.EMP_ID
FROM UCHTRANS.OPN_VISIT_DTLS OVD
INNER JOIN UCHTRANS.OPN_VISIT_MASTER OVM ON OVM.OPVISIT_ID=OVD.OPVISIT_ID
INNER JOIN UCHMASTER.OPN_PATIENT OP ON OP.PATI_ID=OVM.PATI_ID
INNER JOIN UCHMASTER.OPN_DOCTOR OD ON OD.DOCT_ID=OVD.OPVDTLS_DOCTOR_ID
LEFT JOIN UCHDISPLAY.DOC_DEPARTMENT_EXT DD ON DD.ID=OD.SPEC_ID 
INNER JOIN UCHMASTER.OPN_SPECIALIZATIONS OS ON OS.SPEC_ID=OD.SPEC_ID
INNER JOIN UCHMASTER.HRM_EMPLOYEE EMP ON EMP.EMP_ID=OD.EMPLOYEE_ID
LEFT JOIN UCHDISPLAY.DOCTOR_EXT D ON D.ID=EMP.EMP_ID
INNER JOIN UCHDISPLAY.TOKEN_READ_STATUS TRS ON TRS.OPVDTLS_ID=OVD.OPVDTLS_ID
INNER JOIN UCHDISPLAY.DOCTOR_CONSROOM_MAP DCM ON DCM.DOCT_ID=TRS.DOCT_ID
INNER JOIN UCHDISPLAY.DEVICE_EXT DE ON DE.DEVICE_ID= :deviceId
left join ( SELECT SETTINGS_VALUE As INS_TOKEN_SETTINGS FROM UCHADMIN.OP_SETTINGS WHERE SETTINGS_NAME = 'ENABLE_PATIENT_BILLING_TOKEN_NO') TK_SET on 1=1
 Left join UCHTRANS.GEN_PATIENT_BILLING_TOKEN Pbt on pbt.TOKEN_ID=OVD.TOKEN_ID
WHERE TO_DATE(OVM.CREATE_DATE,'DD/MM/YY')=TO_DATE(SYSDATE,'DD/MM/YY') 
AND TRS.DEVICE_ID=DE.ID
AND OPVDTLS_DOCTOR_ID IN 
(SELECT DISTINCT DCM.MAIN_DOCT_ID FROM UCHDISPLAY.DOCTOR_CONSROOM_MAP DCM
INNER JOIN UCHDISPLAY.CONSULT_ROOM CR ON CR.ROOM_NO=DCM.ROOM_NO 
INNER JOIN UCHDISPLAY.DEVICE_DOCTOR_ROOM_MAP_EXT DDM ON DDM.ROOM_ID=CR.ROOM_ID
INNER JOIN UCHDISPLAY.DEVICE_EXT D ON DDM.DEVICE_ID=D.ID WHERE D.DEVICE_ID= :deviceId1)";

                    using (var cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("deviceId", deviceId ?? string.Empty));
                        cmd.Parameters.Add(new OracleParameter("deviceId1", deviceId ?? string.Empty));

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tokens.Add(new DoctorTokenInFo
                                {
                                    DEPARTMENT = reader["DEPARTMENT"].ToString(),
                                    DOCTOR_NAME = reader["DOCTOR_NAME"].ToString(),
                                    OPVDTLS_ID = reader["OPVDTLS_ID"].ToString(),
                                    ID = reader["ID"].ToString(),
                                    TOKEN_READ_STS = reader["TOKEN_READ_STS"].ToString(),
                                    TOKEN_CALL_DATE = reader["TOKEN_CALL_DATE"].ToString(),
                                    LAST_CALLED = reader["LAST_CALLED"].ToString(),
                                    OPNO = reader["OPNO"].ToString(),
                                    PATIENT_NAME = reader["PATIENT_NAME"].ToString(),
                                    BG_COLOR = reader["BG_COLOR"].ToString(),
                                    TOKEN = reader["TOKEN"].ToString(),
                                    ROOM_NO = reader["ROOM_NO"].ToString(),
                                    EMP_ID = reader["EMP_ID"].ToString(),
                                    PROFILE_PIC = reader["PROFILE_PIC"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (OracleException ex)
            {
                return new { Status = 500, Messsage = ex.Message };
            }
            catch (Exception ex)
            {

                return new { Status = 500, Messsage = ex.Message };
            }


            return tokens;
        }


        public async Task<dynamic> MarkTokenAsReadAsync(string opvdtlsId, string deviceId)
        {
            try
            {
                using (var conn = (OracleConnection)_DbContext.Database.GetDbConnection()) // Cast to OracleConnection
                {
                    await conn.OpenAsync();

                    // Update TOKEN_READ_STATUS
                    var tokenReadResult = await UpdateTOKEN_READ_STATUS(conn, opvdtlsId, deviceId);
                    if (tokenReadResult.Status == 200)
                    {
                        // Update OPN_VISIT_DTLS
                        var visitDtsResult = await UpdateOPN_VISIT_DTLS(conn, opvdtlsId);
                        if (visitDtsResult.Status == 200)
                        {
                            return new { Status = 200, Message = "Updated Successfully" };
                        }
                        else
                        {
                            return visitDtsResult; // Return the visit update failure result
                        }
                    }
                    else
                    {
                        return tokenReadResult; // Return the token read update failure result
                    }
                }
            }
            catch (Exception ex)
            {
                return new { Status = 500, Message = ex.Message };
            }
        }

        public async Task<dynamic> UpdateTOKEN_READ_STATUS(OracleConnection conn, string opvdtlsId, string deviceId)
        {
            try
            {
                var sql = @"
            UPDATE UCHDISPLAY.TOKEN_READ_STATUS 
            SET READ_STATUS = 'Y' 
            WHERE DEVICE_ID = :deviceId AND OPVDTLS_ID = :opvdtlsId";

                var parameters = new List<OracleParameter>
        {

            new OracleParameter("deviceId", OracleDbType.Varchar2) { Value = deviceId },
            new OracleParameter("opvdtlsId", OracleDbType.Varchar2) { Value = opvdtlsId }
        };

                using (var cmd = conn.CreateCommand()) // Use the existing open connection
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddRange(parameters.ToArray());

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    return new { Status = 200, Message = "Update successful", Rows = rowsAffected };
                }
            }
            catch (Exception ex)
            {
                return new { Status = 500, Message = $"Error in UpdateTOKEN_READ_STATUS: {ex.Message}" };
            }
        }

        public async Task<dynamic> UpdateOPN_VISIT_DTLS(OracleConnection conn, string opvdtlsId)
        {
            try
            {
                var sql = @"
            UPDATE UCHTRANS.OPN_VISIT_DTLS 
            SET TOKEN_READ_STS = 'Y' 
            WHERE OPVDTLS_ID = :opvdtlsId";

                var parameters = new List<OracleParameter>
        {
            new OracleParameter("opvdtlsId", OracleDbType.Varchar2) { Value = opvdtlsId }
        };

                using (var cmd = conn.CreateCommand()) // Use the existing open connection
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddRange(parameters.ToArray());

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    return new { Status = 200, Message = "Update successful", Rows = rowsAffected };
                }
            }
            catch (Exception ex)
            {
                return new { Status = 500, Message = $"Error in UpdateOPN_VISIT_DTLS: {ex.Message}" };
            }
        }


        //        public async Task<dynamic> GetTokenPatientInfoDataTableAsync(string deviceId)
        //        {
        //            var dataTable = new DataTable();

        //            try
        //            {
        //                using (var conn = new OracleConnection(con))
        //                {
        //                    await conn.OpenAsync();

        //                    string query = @"
        //SELECT DISTINCT 
        //    OVD.OPVDTLS_ID,
        //    TRS.READ_STATUS AS TOKEN_READ_STS,
        //    OVD.TOKEN_CALL_DATE,
        //    'N' AS LAST_CALLED,
        //    UCHTRANS.FN_GET_FORMATTED_OP_NO(OP.PATI_OPNO) AS OPNO,
        //    OP.PATI_FIRST_NAME || ' ' || OP.PATI_LAST_NAME AS PATIENT_NAME,
        //    NVL(D.NAME, EMP.EMP_OFFL_NAME) AS DOCTOR_NAME,
        //    NVL(D.PROFILE_PIC, 'doctor.png') AS PROFILE_PIC,
        //    NVL(D.BACKGROUND_COLOR, '#FFFFFF') AS BG_COLOR,
        //    NVL(DD.TITLE, OS.SPEC_NAME) AS DEPARTMENT,
        //    DECODE(
        //        NVL(INS_TOKEN_SETTINGS,'N'),
        //        'N',
        //        OVD.TOKEN_WAR || OD.DOCT_TOKEN_PREFIX || '-' || NVL(OVM.CONTINUOUS_TOKEN, OVD.TOCKEN_REGULAR),
        //        pbt.TOKEN_NO
        //    ) AS TOKEN,
        //    DCM.ROOM_NO,
        //    EMP.EMP_ID
        //FROM UCHTRANS.OPN_VISIT_DTLS OVD
        //INNER JOIN UCHTRANS.OPN_VISIT_MASTER OVM ON OVM.OPVISIT_ID = OVD.OPVISIT_ID
        //INNER JOIN UCHMASTER.OPN_PATIENT OP ON OP.PATI_ID = OVM.PATI_ID
        //INNER JOIN UCHMASTER.OPN_DOCTOR OD ON OD.DOCT_ID = OVD.OPVDTLS_DOCTOR_ID
        //LEFT JOIN UCHDISPLAY.DOC_DEPARTMENT_EXT DD ON DD.ID = OD.SPEC_ID
        //INNER JOIN UCHMASTER.OPN_SPECIALIZATIONS OS ON OS.SPEC_ID = OD.SPEC_ID
        //INNER JOIN UCHMASTER.HRM_EMPLOYEE EMP ON EMP.EMP_ID = OD.EMPLOYEE_ID
        //LEFT JOIN UCHDISPLAY.DOCTOR_EXT D ON D.ID = EMP.EMP_ID
        //INNER JOIN UCHDISPLAY.TOKEN_READ_STATUS TRS ON TRS.OPVDTLS_ID = OVD.OPVDTLS_ID
        //INNER JOIN UCHDISPLAY.DOCTOR_CONSROOM_MAP DCM ON DCM.DOCT_ID = TRS.DOCT_ID
        //INNER JOIN UCHDISPLAY.DEVICE_EXT DE ON DE.DEVICE_ID = :deviceId
        //LEFT JOIN (
        //    SELECT SETTINGS_VALUE AS INS_TOKEN_SETTINGS 
        //    FROM UCHADMIN.OP_SETTINGS 
        //    WHERE SETTINGS_NAME = 'ENABLE_PATIENT_BILLING_TOKEN_NO'
        //) TK_SET ON 1 = 1
        //LEFT JOIN UCHTRANS.GEN_PATIENT_BILLING_TOKEN PBT ON PBT.TOKEN_ID = OVD.TOKEN_ID
        //WHERE TO_DATE(OVM.CREATE_DATE, 'DD/MM/YY') = TO_DATE(SYSDATE, 'DD/MM/YY')
        //  AND TRS.DEVICE_ID = DE.ID
        //  AND OPVDTLS_DOCTOR_ID IN (
        //    SELECT DISTINCT DCM.MAIN_DOCT_ID
        //    FROM UCHDISPLAY.DOCTOR_CONSROOM_MAP DCM
        //    INNER JOIN UCHDISPLAY.CONSULT_ROOM CR ON CR.ROOM_NO = DCM.ROOM_NO
        //    INNER JOIN UCHDISPLAY.DEVICE_DOCTOR_ROOM_MAP_EXT DDM ON DDM.ROOM_ID = CR.ROOM_ID
        //    INNER JOIN UCHDISPLAY.DEVICE_EXT D ON DDM.DEVICE_ID = D.ID
        //    WHERE D.DEVICE_ID = :deviceId
        //)";

        //                    using (var cmd = new OracleCommand(query, conn))
        //                    {
        //                        cmd.Parameters.Add(new OracleParameter("deviceId", deviceId ?? string.Empty));
        //                        cmd.Parameters.Add(new OracleParameter("deviceId", deviceId ?? string.Empty)); // for second usage

        //                        using (var adapter = new OracleDataAdapter(cmd))
        //                        {
        //                            adapter.Fill(dataTable);
        //                        }
        //                    }
        //                }
        //            }
        //            catch (OracleException ex)
        //            {


        //                return new { Status = 200, Messsage = ex.Message };
        //            }
        //            catch (Exception ex)
        //            {


        //                return new { Status = 200, Messsage = ex.Message };
        //            }


        //            return new { Status = 500, Data = dataTable };
        //        }




    }




}


