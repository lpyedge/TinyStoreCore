using System;

namespace TinyStore.Utils
{
    public class IDCard
    {
        public string IDCardNumber { get; set; }
        // public string Province { get; protected set; }
        // public string City { get; protected set; }
        public int Gender { get; protected set; }
        public DateTime BirthDay { get; protected set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="p_IDCardNumber"></param>
        public IDCard(string p_IDCardNumber)
        {
            if (IsIDCard(p_IDCardNumber))
            {
                IDCardNumber = p_IDCardNumber;
                Gender = GetGender(p_IDCardNumber);
                BirthDay = GetBirthDay(p_IDCardNumber);
                // Province = GetProvince(p_IDCardNumber);
                // City = GetCity(p_IDCardNumber);
            }
            else
            {
                throw new Exception("错误的身份证格式！");
            }
        }

        // private string GetCity(string p_IDCardNumber)
        // {
        //     throw new NotImplementedException();
        // }
        //
        // private string GetProvince(string p_IDCardNumber)
        // {
        //     throw new NotImplementedException();
        // }

        public static IDCard Generate(string p_IDCardNumber)
        {
            if (IsIDCard(p_IDCardNumber))
            {
                return new IDCard(p_IDCardNumber);
            }
            return null;
        }


        public static bool IsIDCard(string p_IDCardNumber)
        {
            bool res;
            switch (p_IDCardNumber.Length)
            {
                case 18:
                    res = IsIDCard18(p_IDCardNumber);
                    break;
                case 15:
                    res = IsIDCard15(p_IDCardNumber);
                    break;
                default:
                    res = false;
                    break;
            }
            return res;
        }

        /// <summary>
        /// 身份证判断性别
        /// </summary>
        /// <returns>返回0为女性，返回1为男性</returns>
        public int GetGender(string p_IDCardNumber)
        {
            int res = 0;
            int tempnum;
            switch (p_IDCardNumber.Length)
            {
                case 18:
                    int.TryParse(p_IDCardNumber.Substring(14, 3), out tempnum);
                    Math.DivRem(tempnum, 2, out res);
                    break;
                case 15:
                    int.TryParse(p_IDCardNumber.Substring(12, 3), out tempnum);
                    Math.DivRem(tempnum, 2, out res);
                    break;
            }
            return res;
        }

        /// <summary>
        /// 身份证判断生日
        /// </summary>
        /// <returns>返回生日</returns>
        public DateTime GetBirthDay(string p_IDCardNumber)
        {
            DateTime res = DateTime.MinValue;
            string tempstr = string.Empty;
            switch (p_IDCardNumber.Length)
            {
                case 18:
                    tempstr = p_IDCardNumber.Substring(6, 8);
                    break;
                case 15:
                    tempstr = "19" + p_IDCardNumber.Substring(6, 6);
                    break;
            }
            if (!string.IsNullOrEmpty(tempstr))
            {
                DateTime.TryParse(tempstr, out res);
            }
            return res;
        }

        #region 判断身份证格式

        private static bool IsIDCard18(string p_IDCardNumber)
        {
            long n = 0;
            if (long.TryParse(p_IDCardNumber.Remove(17), out n) == false || n < Math.Pow(10, 16) || long.TryParse(p_IDCardNumber.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证     
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(p_IDCardNumber.Remove(2)) == -1)
            {
                return false;//省份验证    
            }
            string birth = p_IDCardNumber.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证      
            }
            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = p_IDCardNumber.Remove(17).ToCharArray();
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }
            int y = -1;
            Math.DivRem(sum, 11, out y);
            if (arrVarifyCode[y] != p_IDCardNumber.Substring(17, 1).ToLower())
            {
                return false;//校验码验证 
            }
            return true;//符合GB11643-1999标准      
        }

        private static bool IsIDCard15(string p_IDCardNumber)
        {
            long n = 0;
            if (long.TryParse(p_IDCardNumber, out n) == false || n < Math.Pow(10, 14))
            {
                return false;//数字验证     
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(p_IDCardNumber.Remove(2)) == -1)
            {
                return false;//省份验证    
            }
            string birth = p_IDCardNumber.Substring(6, 6).Insert(4, "-").Insert(2, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证    
            }
            return true;//符合15位身份证标准    
        }

        #endregion

        #region 一代身份证格式升级二代格式

        public static string Old2NewIDCard(string p_IDCardNumber)
        {
            string _SfzNumber = p_IDCardNumber;
            if (p_IDCardNumber.Length == 15) _SfzNumber = p_IDCardNumber.Substring(0, 6) + "19" + p_IDCardNumber.Substring(6, 9);
            if (_SfzNumber.Length != 17) return "Error";
            char[] _TempString = _SfzNumber.ToCharArray();
            int[] _Value = new int[] { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
            int _SumCount = 0;

            for (int i = 0; i != _TempString.Length; i++)
            {
                _SumCount += Int32.Parse(_TempString[i].ToString()) * _Value[i];
            }
            switch ((int)_SumCount % 11)
            {
                case 0:
                    return _SfzNumber + "1";
                case 1:
                    return _SfzNumber + "0";
                case 2:
                    return _SfzNumber + "X";
                case 3:
                    return _SfzNumber + "9";
                case 4:
                    return _SfzNumber + "8";
                case 5:
                    return _SfzNumber + "7";
                case 6:
                    return _SfzNumber + "6";
                case 7:
                    return _SfzNumber + "5";
                case 8:
                    return _SfzNumber + "4";
                case 9:
                    return _SfzNumber + "3";
                case 10:
                    return _SfzNumber + "2";
                default:
                    return "Error";
            }

        }

        #endregion
    }
}