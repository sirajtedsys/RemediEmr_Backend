using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RemediEmr.Class.DbModel;
using RemediEmr.Data.DbModel;
using Oracle.ManagedDataAccess.Client;
using System.Xml;
using RemediEmr.Data.Class;

namespace RemediEmr.Class
{
    public class AppDbContext : DbContext
    {

        protected readonly IConfiguration _configuration;
        protected readonly IServiceProvider _serviceProvider;

        public AppDbContext(IServiceProvider serviceProvider, DbContextOptions<AppDbContext> options, IConfiguration configuration)
             : base(options)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //if (!optionsBuilder.IsConfigured)
            //{
            //    optionsBuilder.UseOracle("User Id=SYSTEM;Password=/:t3d5y5/;Data Source=FORA11G3;"
            //        );
            //}

            //var dbName = httpContextAccessor.HttpContext.Request.Headers["Bookit"].First();

            var httpContextAccessor = _serviceProvider.GetRequiredService<IHttpContextAccessor>();
            string connection = _configuration.GetConnectionString("OracleDbContext");
            optionsBuilder.UseOracle(connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            #region UCHEMR

            modelBuilder.Entity<UCHEMR_Emr_Admin_users>().ToTable("EMR_ADMIN_USERS", schema: "UCHEMR");

            modelBuilder.Entity<UCHEMR_Emr_Admin_Users_Branch_Link>().ToTable("EMR_ADMIN_USERS_BRANCH_LINK", schema: "UCHEMR");

            modelBuilder.Entity<UCHEMR_DIET_CHART>().ToTable("DIET_CHART", schema: "UCHEMR");
            //modelBuilder.Entity<UCHEMR_EMR_DOCUMENT_DETAILS1>().ToTable("EMR_DOCUMENT_DETAILS", schema: "UCHEMR");
            modelBuilder.Entity<UCHEMR_EMR_PATIENT_VITALS_STATUS>().ToTable("EMR_PATIENT_VITALS_STATUS", schema: "UCHEMR");


            modelBuilder.Entity<BODY_COMPO_FEMALE>().ToTable("BODY_COMPO_FEMALE", schema: "UCHEMR");
            #endregion UCHEMR


            #region UCHMASTER

            modelBuilder.Entity<Tbl_Hrm_Department>().ToTable("HRM_DEPARTMENT", schema: "UCHMASTER");
            modelBuilder.Entity<UCHMASTER_Hrm_Branch>().ToTable("HRM_BRANCH", schema: "UCHMASTER");
            modelBuilder.Entity<UCHMASTER_LoginSettings>().ToTable("LOGIN_SETTINGS", schema: "UCHMASTER");
            modelBuilder.Entity<UCHMASTER_Hrm_Employee>().ToTable("HRM_EMPLOYEE", schema: "UCHMASTER");
            modelBuilder.Entity<UCHMASTER_Hrm_employee_branch_Link>().ToTable("HRM_EMPLOYEE_BRANCH_LINK", schema: "UCHMASTER");
            modelBuilder.Entity<UCHMASTER_Hrm_Employee_Hr>().ToTable("HRM_EMPLOYEE_HR", schema: "UCHMASTER");
            ;
            modelBuilder.Entity<Doctor>().ToTable("OPN_DOCTOR", schema: "UCHMASTER");
            modelBuilder.Entity<SPECIAL_CONSULTATION>().ToTable("SPECIAL_CONSULTATION", schema: "UCHEMR");
            modelBuilder.Entity<EMR_DOCUMENT_DETAILS>().ToTable("EMR_DOCUMENT_DETAILS", schema: "UCHEMR");



            modelBuilder.Entity<UCHMASTER_WEB_MENU_GROUP>().ToTable("WEB_MENU_GROUP", schema: "UCHMASTER");
            modelBuilder.Entity<UCHMASTER_WEB_MENU_GROUP_DTLS>().ToTable("WEB_MENU_GROUP_DTLS", schema: "UCHMASTER");
            modelBuilder.Entity<UCHEMR_EMR_IP_TABS_VIEW_LINK>().ToTable("EMR_IP_TABS_VIEW_LINK", schema: "UCHEMR");




            #endregion UCHMASTER



            //modelBuilder.Entity<MyEntity>().ToTable("MyTable", "Schema1");
            //modelBuilder.Entity<AnotherEntity>().ToTable("AnotherTable", "Schema2");

            // Other entity configurations can go here
        }

        //SELECT ausr_pwd, ausr_username, ausr_id, ausr_username, ausr_status FROM UCHEMR.EMR_ADMIN_USERS WHERE ausr_status<>'D' AND ausr_username = 'tedsys' AND ausr_pwd = 'ted@123'

        #region UCHMASTER

        public DbSet<UCHMASTER_Hrm_Branch> HRM_BRANCH { get; set; }
        public DbSet<Tbl_Hrm_Department> HRM_DEPARTMENT { get; set; }
        public DbSet<UCHMASTER_LoginSettings> LOGIN_SETTINGS { get; set; }
        public DbSet<UCHMASTER_Hrm_Employee> HRM_EMPLOYEE { get; set; }
        public DbSet<UCHMASTER_Hrm_employee_branch_Link> HRM_EMPLOYEE_BRANCH_LINK { get; set; }
        public DbSet<UCHMASTER_Hrm_Employee_Hr> HRM_EMPLOYEE_HR { get; set; }
        public DbSet<Doctor> OPN_DOCTOR { get; set; }
        public DbSet<SPECIAL_CONSULTATION> SPECIAL_CONSULTATION { get; set; }
        public DbSet<EMR_DOCUMENT_DETAILS> EMR_DOCUMENT_DETAILS { get; set; }
        public DbSet<UCHMASTER_WEB_MENU_GROUP> WEB_MENU_GROUP { get; set; }
        public DbSet<UCHMASTER_WEB_MENU_GROUP_DTLS> WEB_MENU_GROUP_DTLS { get; set; }


        #endregion UCHMASTER

        #region UCHEMR

        public DbSet<UCHEMR_Emr_Admin_users> EMR_ADMIN_USERS { get; set; }
        public DbSet<UCHEMR_Emr_Admin_Users_Branch_Link> EMR_ADMIN_USERS_BRANCH_LINK { get; set; }
        public DbSet<UCHEMR_EMR_IP_TABS_VIEW_LINK> EMR_IP_TABS_VIEW_LINK { get; set; }



        public DbSet<UCHEMR_DIET_CHART> DIET_CHART { get; set; }
        //public DbSet<UCHEMR_EMR_DOCUMENT_DETAILS1> EMR_DOCUMENT_DETAILS { get; set; }
        public DbSet<UCHEMR_EMR_PATIENT_VITALS_STATUS> EMR_PATIENT_VITALS_STATUS { get; set; }


        public DbSet<BODY_COMPO_FEMALE> BODY_COMPO_FEMALE { get; set; }

        #endregion UCHEMR


        #region UCHTRANS

        #endregion UCHTRANS


    }
}