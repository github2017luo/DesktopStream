using log4net;
using System;
using System.IO;
using System.Windows.Forms;

namespace DesktopStream.WPF
{
    public static class LogHelper
    {    

        private static ILog logWriter = LogManager.GetLogger("DeskstopStreamLog");
        public static void AddErrorLog(string str)
        {
            logWriter.Error(str);
        }

        public static void AddEventLog(string str)
        {
            logWriter.Info(str);
        }


        public static void AddFFmpegLog(string str)
        {
            try
            {
                FileStream fs = new FileStream(Application.StartupPath + "\\Log\\ffmpeg.log", FileMode.OpenOrCreate);
                byte[] data = System.Text.Encoding.Default.GetBytes("==============="+DateTime.Now + "===============\r\n" + str + "\r\n");
                fs.Position = fs.Length;
                fs.Write(data, 0, data.Length);
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("非常抱歉，日志写入失败，请联系开发人员。" + ex.ToString(), "系统提示");
            }
            finally
            {
            }
        }
    }
}
