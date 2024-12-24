using System.Security.Cryptography;

namespace RemediEmr.Data.Class
{
	public class SubjGlobal
	{
		//UCHEMR.DIET_BASIC_ENTRY-Parameters
		public string?  P_EMR_DOC_ID { get; set; }
		public string? P_HEIGHT { get; set; }
		public string? P_ASW { get; set; }
		public string? P_IBW { get; set; }
		public string? P_MAC { get; set; }
		public string? P_TSF { get; set; }
		public string? P_BMI { get; set; }
		public string? P_BP { get; set; }
		public string? P_DIAGNOSIS { get; set; }
		public string? P_GOOD { get; set; }
		public string? P_FOOD_50 { get; set; }
		public string? P_FOOD_25 { get; set; }
		public string? P_ANOREXIA { get; set; }
		public string? P_PALLOR { get; set; }
		public string? P_PALLOR_CMNT { get; set; }
		public string? P_EDEMA { get; set; }
		public string? P_EDEMA_CMNT { get; set; }
		public string? P_DISEASE { get; set; }
		public string? P_CREATED_BY { get; set; }

		//UCHEMR.SP_DIET_FUNCTIONAL_CAPACITY-parameters

		public char? P_DYSFUNCTION_STATUS { get; set; }
		public char? P_DURATION_TYPE { get; set; }
		public string? P_DURATION { get; set; }
		public string? P_DURATION_COMMENTS { get; set; }
		public char? P_DYSFUNCTION_TYPE { get; set; }
		public string? P_DYSFUNCTION_WEEKS { get; set; }
		public string? P_DYSFUNCTION_COMMENTS { get; set; }

		//UCHEMR.SP_DIET_DIETARY_INTAKE-parameters

		public char? P_DIETARY_STATUS { get; set; }
		//public char? P_DURATION_TYPE { get; set; }
		//public string P_DURATION { get; set; }
		//public string P_DURATION_COMMENTS { get; set; }
		public char? P_DIETARY_TYPE { get; set; }
		public string? P_DIETARY_COMMENTS { get; set; }

		//UCHEMR.SP_DIET_GASTRO_SYMPTOMS-parameters

		public char? P_STATUS { get; set; }
		public char? P_NAVSEA { get; set; }
		public string? P_VOMITTING { get; set; }
		public string? P_DIARRHEA { get; set; }
		//public char? P_ANOREXIA { get; set; }
		public string? P_COMMENTS { get; set; }

		//UCHEMR.SP_DIET_PHYSICAL_SUBJECTIVE-parameters

		public char? P_FAT_LOSS { get; set; }
		public string? P_FAT_LOSE_COMMENTS { get; set; }
		public char? P_MUSCLE_WAST { get; set; }
		//public string P_COMMENTS { get; set; }
		//public char? P_CREATED_BY { get; set; }
		public string? P_ASSESSMENT { get; set; }
		public char? P_ASSESSMENT_RATE { get; set; }
		public string? P_ASSESSMENT_COMMENTS { get; set; }
		public char? P_ANKLE_EDEMA { get; set; }
		public string? P_ANKLE_COMMENTS { get; set; }
		public char? P_SACRAL_EDEMA { get; set; }
		public string? P_SACRAL_COMMENTS { get; set; }
		public char? P_ASCITES { get; set; }
		public string? P_ASCITES_COMMENTS { get; set; }
		public string? P_NUTRITION_DIAGNOSIS { get; set; }
		public string? P_BIOCHEMICAL_VALUES { get; set; }

		//UCHEMR.SP_DIET_DISEASE-parameters

		public string? P_PRIMARY_DIAGNOSIS { get; set; }
		//public string P_COMMENTS { get; set; }
		public string? P_METABOLIC_DEMAND { get; set; }
		public string? P_METABOLIC_COMMENTS { get; set; }

		//UCHEMR.SP_WEIGHT_CHANGE-parameters

		public char? P_LOSS_STATUS { get; set; }
		public string? P_LOSS_KG { get; set; }
		public string? P_LOSS_PERCENT { get; set; }
		public string? P_LOSS_COMMENTS { get; set; }
		public char? P_CHANGE_STATUS { get; set; }
		public char? P_CHANGE_TYPE { get; set; }
		public string? P_CHANGE_KG { get; set; }
		public string? P_CHANGE_PERCENT { get; set; }
		public string? P_CHANGE_COMMENTS { get; set; }


	}
}
