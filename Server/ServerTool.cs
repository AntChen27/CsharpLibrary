namespace Adolph.Util.Server
{   
   /// <summary>
    /// 服务器工具类
    /// </summary>
    public class ServerTool
    {
        /// <summary>
        /// 文件下载，支持断点续传、多线程下载
        /// </summary>
        /// <remarks>需要引用 System.Web.dll</remarks>
        /// <param name="filepath">文件路径</param>
        /// <param name="bufferSize">缓冲区大小</param>
        public static void DownloadFile(string filepath, int bufferSize = 1024*1024)
        {
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize", "bufferSize must be greater than 0");
            if (!System.IO.File.Exists(filepath))
                throw new FileNotFoundException("file not found", filepath);
            System.IO.Stream stream = null;
            byte[] buffer = new byte[bufferSize];
            int readCount = bufferSize;
            long totalFileLength, startPos, endPos;
            try
            {
                stream = new System.IO.FileStream(filepath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                //文件大小
                totalFileLength = stream.Length;
                //默认开始位置
                startPos = 0;
                //默认结束位置
                endPos = totalFileLength - 1;

                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.ClearHeaders();
                System.Web.HttpContext.Current.Response.ClearContent();


                if (System.Web.HttpContext.Current.Request.Headers["Range"] != null)
                {
                    //如果是续传请求，则获取续传的起始位置，即已经下载到客户端的字节数
                    System.Web.HttpContext.Current.Response.StatusCode = 206; //重要：续传必须，表示局部范围响应。
                    var pos = System.Web.HttpContext.Current.Request.Headers["Range"].Replace("bytes=", "").Trim().Split('-');
                    startPos = string.IsNullOrEmpty(pos[0]) ? 0 : long.Parse(pos[0]);
                    endPos = string.IsNullOrEmpty(pos[1]) ? totalFileLength - 1 : long.Parse(pos[1]);
                }
                if (startPos != 0)
                {
                    //不是从最开始下载,
                    //响应的格式是:Content-Range: bytes [文件块的开始字节]-[文件的总大小 - 1]/[文件的总大小]
                    System.Web.HttpContext.Current.Response.AddHeader("Content-Range", "bytes " + startPos.ToString() + "-" + endPos.ToString() + "/" + totalFileLength.ToString());
                }
                System.Web.HttpContext.Current.Response.ContentType = "application/octet-stream";   //MIME类型：匹配任意文件类型 
                System.Web.HttpContext.Current.Response.AddHeader("Connection", "Keep-Alive");
                System.Web.HttpContext.Current.Response.AddHeader("Content-Length", (endPos + 1 - startPos).ToString());
                System.Web.HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + System.Web.HttpUtility.UrlEncode(System.Web.HttpContext.Current.Request.ContentEncoding.GetBytes(System.IO.Path.GetFileName(filepath))));

                stream.Position = startPos;
                while (System.Web.HttpContext.Current.Response.IsClientConnected && readCount == buffer.Length)
                {
                    if (endPos + 1 - stream.Position > buffer.Length)
                    {
                        readCount = stream.Read(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        readCount = stream.Read(buffer, 0, (int)(endPos + 1 - stream.Position));
                    }
                    System.Web.HttpContext.Current.Response.OutputStream.Write(buffer, 0, readCount);
                    System.Web.HttpContext.Current.Response.Flush();
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (stream != null)
                    stream.Close();
                System.Web.HttpContext.Current.Response.Flush();
                System.Web.HttpContext.Current.Response.End();
            }
        }
    }
}