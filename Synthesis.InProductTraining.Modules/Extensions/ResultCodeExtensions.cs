using System.Net;
using Synthesis.InProductTrainingService.Enums;

namespace Synthesis.InProductTrainingService.Extensions
{
    /// <summary>
    /// Extensions for the ResultCode enumeration.
    /// </summary>
    public static class ResultCodeExtensions
    {
        /// <summary>
        /// Converts the ResultCode to an HttpStatusCode.
        /// </summary>
        /// <param name="resultCode">The ResultCode enumeration value</param>
        /// <returns>A corresponding HttpStatusCode, or HttpStatusCode.InternalServerError by default.</returns>
        public static HttpStatusCode ToHttpStatusCode(this ResultCode resultCode)
        {
            HttpStatusCode statusCode;
            switch (resultCode)
            {
                case ResultCode.Success:
                    statusCode = HttpStatusCode.OK;
                    break;
                case ResultCode.RecordNotFound:
                    statusCode = HttpStatusCode.NotFound;
                    break;
                case ResultCode.Unauthorized:
                    statusCode = HttpStatusCode.Unauthorized;
                    break;
                case ResultCode.AccessDeniedToSystemSetting:
                case ResultCode.InsufficientPermissions:
                    statusCode = HttpStatusCode.Forbidden;
                    break;
                case ResultCode.InvalidClientCertificate:
                case ResultCode.ValidClientCertificate:
                    statusCode = HttpStatusCode.NotImplemented;
                    break;
                case ResultCode.ArgumentNull:
                    statusCode = HttpStatusCode.BadRequest;
                    break;
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    break;
            }

            return statusCode;
        }
    }
}