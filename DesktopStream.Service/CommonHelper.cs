using System;
using System.Collections.Generic;
using System.Diagnostics;
using DesktopStream.Service.Models;

namespace DesktopStream.Service
{
    public static class CommonHelper
    {
        /// <summary>
        /// 根据视频的宽与高的比例，得到正确的视频显示大小，高度固定为refo
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="refo"></param>
        /// <returns></returns>
        public static int GetWidth(int width,int height,int refo)
        {
            var lwidth = Convert.ToDecimal(width* refo / height);
            return Convert.ToInt32(lwidth);
        }
        /// <summary>
        /// 根据视频的宽与高的比例，得到正确的视频显示大小，宽度固定为refo
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="refo"></param>
        /// <returns></returns>
        public static int GetHeight(int width, int height, int refo)
        {
            var lheight = Convert.ToDecimal(height * refo / width);
            return Convert.ToInt32(lheight);
        }


        /// <summary>
        /// 获得当前主屏幕的桌面分辨率，并向下递减三个等级  注意直播分辨率中不能包含奇数 否则会直播失败
        /// </summary>
        /// <returns></returns>
        public static List<SimpleModel> GetDeskCapability()
        {
            var deskSize = System.Windows.Forms.SystemInformation.PrimaryMonitorSize;
            List<SimpleModel> deskCapabilityList = new List<SimpleModel>();
            for (int i = 2; i <= 6; i++)
            {
                var item = new SimpleModel();
                item.id = 1;
                var width = Math.Floor(deskSize.Width / Math.Round((decimal)i / 2, 1));
                if (width % 2 != 0)
                {
                    width = width - 1;
                }
                var height = Math.Floor(deskSize.Height / (Math.Round((decimal)i / 2, 1)));
                if (height % 2 != 0)
                {
                    height = height - 1;
                }
                item.name = width + "x" + height;

                deskCapabilityList.Add(item);
            }
            return deskCapabilityList;
        }

        /// <summary>
        /// 获得当前主屏幕的桌面分辨率的宽度
        /// </summary>
        /// <returns></returns>
        public static int GetPrimaryScreenWidth()
        {
            var deskSize = System.Windows.Forms.SystemInformation.PrimaryMonitorSize;
            return deskSize.Width;
        }

        /// <summary>
        /// 获得当前主屏幕的桌面分辨率的高度
        /// </summary>
        /// <returns></returns>
        public static int GetPrimaryScreenHeight()
        {
            var deskSize = System.Windows.Forms.SystemInformation.PrimaryMonitorSize;
            return deskSize.Height;

        }

        /// <summary>
        /// 根据字符串大小的宽x高 得到int类型的宽度
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetWidth(string str)
        {
            var width = str.Replace(" ", "");
            try
            {
                width = width.Substring(0, width.IndexOf("x"));
            }
            catch (Exception ex)
            {
                return 0;
            }
            return Convert.ToInt32(width);
        }
        /// <summary>
        /// 根据字符串大小的宽x高 得到int类型的高度
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetHeight(string str)
        {
            var height = str.Replace(" ", "");
            try
            {
                height = height.Substring(height.IndexOf("x")+1);
            }
            catch (Exception ex)
            {
                return 0;
            }
            return Convert.ToInt32(height);
        }

        public static void KillProc(string strProcName)
        {
            try
            {
                //精确进程名  用GetProcessesByName
                foreach (Process p in Process.GetProcessesByName(strProcName))
                {
                    if (!p.CloseMainWindow())
                    {
                        p.Kill();
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
