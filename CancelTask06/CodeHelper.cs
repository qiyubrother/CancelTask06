using System;
using System.Security.Cryptography;
using System.Text;

namespace CancelTask06
{
    public class CodeHelper
    {
        #region [服务器|机器人]报文内容校验
        public static bool Sha1VerifyValidity(string signature, string username, string curtime, string robotsn, string robotmac, string nonce)
        {
            bool result = false;
            string[] ArrTmp = { username, curtime, robotsn, robotmac, nonce };
            Array.Sort(ArrTmp);
            string tmpStr = string.Join(string.Empty, ArrTmp).ToUpper();
            using (var md5 = MD5.Create())
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(tmpStr);
                byte[] md5Array = md5.ComputeHash(byteArray);
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < md5Array.Length; i++)
                {
                    sBuilder.Append(md5Array[i].ToString("x2"));
                }
                string temp = sBuilder.ToString().ToUpper();
                if (temp == signature)
                {
                    result = true;
                }
            }
            return result;
        }
        #endregion

        public static string GetSignature(string username, string curtime, string robotsn, string robotmac, string nonce)
        {
            string[] ArrTmp = { username.ToUpper(), curtime.ToUpper(), robotsn.ToUpper(), robotmac.ToUpper(), nonce.ToUpper() };
            Array.Sort(ArrTmp);
            string tmpStr = string.Join(string.Empty, ArrTmp);
            using (var md5 = MD5.Create())
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(tmpStr);
                byte[] md5Array = md5.ComputeHash(byteArray);
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < md5Array.Length; i++)
                {
                    sBuilder.Append(md5Array[i].ToString("x2"));
                }
                return sBuilder.ToString().ToUpper();
            }
        }

        public static byte TransFloorCode(string floorName)
        {
            switch (floorName)
            {
                case "B1": return 0x3e;
                case "B2": return 0x3f;
                case "B3": return 0x40;
                case "B4": return 0x41;
                default: return (byte)Convert.ToInt16(floorName);
            }
        }

    }
}
