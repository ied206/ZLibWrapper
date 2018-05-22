using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Joveler.ZLibWrapper
{
    #region ZLibException
    [Serializable]
    public class ZLibException : Exception
    {
        public ZLibReturnCode ErrorCode;

        public ZLibException(ZLibReturnCode errorCode)
            : base(ForgeErrorMessage(errorCode))
        {
            ErrorCode = errorCode;
        }

        public ZLibException(ZLibReturnCode errorCode, string msg)
            : base(ForgeErrorMessage(errorCode, msg))
        {
            ErrorCode = errorCode;
        }

        private static string ForgeErrorMessage(ZLibReturnCode errorCode, string msg = null)
        {
            return msg == null ? $"[{errorCode}]" : $"[{errorCode}] {msg}";
        }

        // ReSharper disable once InconsistentNaming
        internal static void CheckZLibOK(ZLibReturnCode ret, ZStream zstream = null)
        {
            if (ret != ZLibReturnCode.OK)
            {
                if (zstream == null)
                    throw new ZLibException(ret);
                else
                    throw new ZLibException(ret, zstream.LastErrorMsg);
            }
        }

        internal static void CheckZLibError(ZLibReturnCode ret, ZStream zstream = null)
        {
            if (ret < 0)
            {
                if (zstream == null)
                    throw new ZLibException(ret);
                else
                    throw new ZLibException(ret, zstream.LastErrorMsg);
            }
        }
    }
    #endregion
}
