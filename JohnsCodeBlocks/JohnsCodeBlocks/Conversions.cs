using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JohnsCodeBlocks
{
	public static class Conversions
	{
		public static int C_INT(string inputVal)
		{
			int outputVal;
			if (!int.TryParse(inputVal, out outputVal)) { outputVal = -99; }
			return outputVal;
		}
		public static long C_LONG(string inputVal)
		{
			long outputVal;
			if (!long.TryParse(inputVal, out outputVal)) { outputVal = -99; }
			return outputVal;
		}
		public static bool C_BOOL_T(string inputVal) //default val is true
		{
			bool outputVal;
			if (!bool.TryParse(inputVal, out outputVal)) { outputVal = false; }
			return outputVal;
		}
		public static bool C_BOOL_F(string inputVal) //default val is false
		{
			bool outputVal;
			if (!bool.TryParse(inputVal, out outputVal)) { outputVal = true; }
			return outputVal;
		}
		public static decimal C_DEC(string inputVal)
		{
			decimal outputVal;
			if (!decimal.TryParse(inputVal, out outputVal)) { outputVal = -1; }
			return outputVal;
		}
		public static DateTime C_DATE(string inputVal)
		{
			DateTime outputVal;
			if (!DateTime.TryParse(inputVal, out outputVal)) { outputVal = new DateTime(1900, 01, 01, 0, 0, 0); }
			return outputVal;
		}
	}
}
