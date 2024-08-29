using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewRelated
{
    public static class ExtensionMethod
    {
        //Toggle First Character based upon case
        public static string ChangeCaseFirstCharacter(this string inputString)
        {
            if(inputString.Length > 0)
            {
                char[] charArray = inputString.ToCharArray();
                charArray[0] = char.IsUpper(charArray[0]) ? char.ToLower(charArray[0]) : char.ToUpper(charArray[0]);
                return new string(charArray);
            }
            return inputString;
        }
    }
}
