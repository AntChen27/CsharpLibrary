namespace Adolph.Util.Server
{   
   /// <summary>
    /// ������������
    /// </summary>
    public class ServerTool
    {
        /// <summary>
        /// �ļ����أ�֧�ֶϵ����������߳�����
        /// </summary>
        /// <remarks>��Ҫ���� System.Web.dll</remarks>
        /// <param name="filepath">�ļ�·��</param>
        /// <param name="bufferSize">��������С</param>
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
                //�ļ���С
                totalFileLength = stream.Length;
                //Ĭ�Ͽ�ʼλ��
                startPos = 0;
                //Ĭ�Ͻ���λ��
                endPos = totalFileLength - 1;

                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.ClearHeaders();
                System.Web.HttpContext.Current.Response.ClearContent();


                if (System.Web.HttpContext.Current.Request.Headers["Range"] != null)
                {
                    //����������������ȡ��������ʼλ�ã����Ѿ����ص��ͻ��˵��ֽ���
                    System.Web.HttpContext.Current.Response.StatusCode = 206; //��Ҫ���������룬��ʾ�ֲ���Χ��Ӧ��
                    var pos = System.Web.HttpContext.Current.Request.Headers["Range"].Replace("bytes=", "").Trim().Split('-');
                    startPos = string.IsNullOrEmpty(pos[0]) ? 0 : long.Parse(pos[0]);
                    endPos = string.IsNullOrEmpty(pos[1]) ? totalFileLength - 1 : long.Parse(pos[1]);
                }
                if (startPos != 0)
                {
                    //���Ǵ��ʼ����,
                    //��Ӧ�ĸ�ʽ��:Content-Range: bytes [�ļ���Ŀ�ʼ�ֽ�]-[�ļ����ܴ�С - 1]/[�ļ����ܴ�С]
                    System.Web.HttpContext.Current.Response.AddHeader("Content-Range", "bytes " + startPos.ToString() + "-" + endPos.ToString() + "/" + totalFileLength.ToString());
                }
                System.Web.HttpContext.Current.Response.ContentType = "application/octet-stream";   //MIME���ͣ�ƥ�������ļ����� 
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