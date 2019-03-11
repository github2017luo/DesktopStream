using DesktopStream.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace DesktopStream.WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string userName = "";//用户名
        private string rtmpUrl = "";//推流RtmpUrl

        private System.Configuration.Configuration configuration = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
        private string desktopResolution = "";//直播桌面时候，推流的分辨率
        private int frameRate = 20;//直播帧率 默认为20
        private static Process p = new Process();//FFmpeg进程
        private readonly string FFmpegPath= System.Windows.Forms.Application.StartupPath + "\\ffmpeg.exe";
        private List<string> audioList = new List<string>();
        private int videoBufferSize = 200;
        private int audioBufferSize = 100;
        private string audioName = "";//音频

        public MainWindow(string username,string url)
        {
            userName = username;
            rtmpUrl = url;
            InitializeComponent();
        }

        public MainWindow()
        {
            InitializeComponent();
            userName = configuration.AppSettings.Settings["UsernameForTest"].Value;
            rtmpUrl = configuration.AppSettings.Settings["StreamurlForTest"].Value;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogHelper.AddEventLog("userName:" + userName+
                "\r\n "+ "rtmpUrl:" + rtmpUrl);
            //直播分辨率
            var itemSource = CommonHelper.GetDeskCapability();
            desktopResolution = itemSource[0].name;
            DesktopResolutionCb.ItemsSource = itemSource;
            DesktopResolutionCb.SelectedIndex = 0;
            //缓存设置
            videoBufferSize = Convert.ToInt32(configuration.AppSettings.Settings["VideoBuffer"].Value);
            audioBufferSize = Convert.ToInt32(configuration.AppSettings.Settings["AudioBuffer"].Value);
            //帧率
            frameRate = Convert.ToInt32(configuration.AppSettings.Settings["FrameRate"].Value);
            //音频
            var audioSource= AudioHelper.GetMicrophoneDevices5();
            if (audioSource.Count != 0)
            {
                AudioCb.ItemsSource = audioSource;
                AudioCb.SelectedIndex = 0;
                audioName = audioSource[0].name;
            }
            //audioList = AudioHelper.GetMicrophoneDevices3();
            //for (int i = 0; i < audioList.Count; i++)
            //{
            //    var microphone = new CheckBox();
            //    microphone.Content = audioList[i];
            //    MicrophonePanel.Children.Add(microphone);
            //}

        }


        private void StartLiveBtn_Click(object sender, RoutedEventArgs e)
        {
            StartLive();
            StartLiveBtn.IsEnabled = false;
            StopLiveBtn.IsEnabled = true;
        }

        private void StopLiveBtn_Click(object sender, RoutedEventArgs e)
        {
            StopLive();
            StartLiveBtn.IsEnabled = true;
            StopLiveBtn.IsEnabled = false;
        }

        private void StartLive()
        {
            if (string.IsNullOrEmpty(rtmpUrl))
            {
                MessageBox.Show("请先设置直播推流地址!", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            //选中的音频
            var checkedAudio = new List<string>();
            foreach (var item in MicrophonePanel.Children)
            {
                if(item is CheckBox)
                {
                    var audio = (item as CheckBox);
                    if (Convert.ToBoolean(audio.IsChecked))
                    {
                        //virtual-audio-capturer 这个不能加引号，否则会报错
                        //其他中文的麦克风，需要添加引号，否则会报错
                        
                    }
                }
            }

            ParameterizedThreadStart threadStart = new ParameterizedThreadStart(LiveDesktop);
            Thread thread = new Thread(threadStart);

            string ffmpegCmd = "";

            ////var strCmd1 = "";
            ////var strCmd2 = "";
            ////var count = 0;
            ////for (int i = 0; i < checkedAudio.Count; i++)
            ////{
            ////    strCmd1 += " -f dshow -rtbufsize "+audioBufferSize+"M -i audio=" + checkedAudio[i] + "";
            ////    strCmd2 += "[" + (count + 1) + ":a]";
            ////    count++;
            ////}
            if (audioName.Contains("(桌面音频)"))
            {
                audioName=(audioName.Replace("(桌面音频)", ""));
            }
            var strCmd1 = " -f dshow -rtbufsize " + audioBufferSize + "M -i audio=" + audioName + "";
            //直播桌面
            var screenWidth = CommonHelper.GetPrimaryScreenWidth();
            var screenHeight = CommonHelper.GetPrimaryScreenHeight();
            if (audioName == "")
            {
                //如果没有选择音频设备，则发送的时候，并不发送音频
                ffmpegCmd = "-f gdigrab -rtbufsize " + audioBufferSize + "M -r " + frameRate + " -video_size " + screenWidth + "x" + screenHeight + " -i desktop -vf scale=" + desktopResolution + " -vcodec h264 -pix_fmt yuv420p -r " + frameRate + " -f flv " + rtmpUrl;
            }
            else
            {
                //单音频
                ffmpegCmd = "-f gdigrab -rtbufsize " + audioBufferSize + "M -r " + frameRate + " -video_size " + screenWidth + "x" + screenHeight + " -i desktop " + strCmd1 + " -vf scale=" + desktopResolution + " -vcodec h264 -r " + frameRate + " -acodec aac -ac 2 -ar 44100 -ab 128k -b:v 1M -pix_fmt yuv420p -f flv " + rtmpUrl;
            }
            //else if (count == 1)
            //{
            //    //单音频
            //    ffmpegCmd = "-f gdigrab -rtbufsize " + audioBufferSize + "M -r " + frameRate + " -video_size " + screenWidth + "x" + screenHeight + " -i desktop " + strCmd1 + " -vf scale=" + desktopResolution + " -vcodec h264 -r " + frameRate + " -acodec aac -ac 2 -ar 44100 -ab 128k -b:v 1M -pix_fmt yuv420p -f flv " + rtmpUrl;
            //}
            //else if (count > 1)
            //{
            //    //多音频
            //    ffmpegCmd = "-f gdigrab -rtbufsize " + audioBufferSize + "M -r " + frameRate + " -video_size " + screenWidth + "x" + screenHeight + " -i desktop " + strCmd1 + " -filter_complex \"" + strCmd2 + "amerge = inputs =" + (count) + "[aout]\"  -map \"[aout]\" -map 0 -vf scale=" + desktopResolution + " -vcodec h264  -r " + frameRate + " -acodec aac -ac 2 -ar 44100 -ab 128k -b:v 1M -pix_fmt yuv420p -f flv " + rtmpUrl;
            //}
            LogHelper.AddFFmpegLog(ffmpegCmd);
            thread.Start(ffmpegCmd);
        }


        private void StopLive()
        {
            p.ErrorDataReceived -= new DataReceivedEventHandler(FFmpegOutput);
            StatusLabel.Content = "已停止";
            SpeedLabel.Content = "---kbps";
            Thread.Sleep(500);

            try
            {
                p.Kill();
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// 开启一个线程进行FFmpeg推流,这个方法是不进行视频录制的时候，推送流 不用在意变量 _createNewFile
        /// </summary>
        /// <param name="parameters"></param>
        private void LiveDesktop(object parameters)
        {
            p = new Process();
            p.StartInfo.FileName = FFmpegPath;
            p.StartInfo.Arguments = (string)parameters;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.ErrorDataReceived += new DataReceivedEventHandler(FFmpegOutput);
            p.Start();
            p.BeginErrorReadLine();
        }

        private void FFmpegOutput(object sendProcess, DataReceivedEventArgs output)
        {
            try
            {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    if (!String.IsNullOrEmpty(output.Data))
                    {
                        LogHelper.AddFFmpegLog(output.Data);
                        SetStatus(output.Data);
                    }
                }));
            }
            catch { }
        }

        

        private void SetStatus(string str)
        {
            if (str.Contains("audio=") && str.Contains("I/O error"))
            {
                StatusLabel.Content="音频设备异常";
                SpeedLabel.Content = "---kbps";
                StartLiveBtn.IsEnabled = true;
                StopLiveBtn.IsEnabled = false;
                return;
            }
            if (str.Contains("video=") && str.Contains("I/O error"))
            {
                StatusLabel.Content = "桌面捕捉异常";
                SpeedLabel.Content = "---kbps";
                StartLiveBtn.IsEnabled = true;
                StopLiveBtn.IsEnabled = false;
                return;
            }
            if (str.Contains("Unknown error occurred") && str.Contains("rtmp:"))
            {
                StatusLabel.Content = "无法连接到RTMP";
                SpeedLabel.Content = "---kbps";
                StartLiveBtn.IsEnabled = true;
                StopLiveBtn.IsEnabled = false;
                return;
            }
            if (str.Contains("Unknown error"))
            {
                StatusLabel.Content = "Unknown error";
                SpeedLabel.Content = "---kbps";
                StartLiveBtn.IsEnabled = true;
                StopLiveBtn.IsEnabled = false;
                return;
            }
            if (str.Contains("bitrate") && str.Contains("speed") && str.Contains("time") && str.Contains("fps") && str.Contains("q="))
            {
                StatusLabel.Content = "正在直播";
                str = str.Replace(" ", "");
                var indexStart = str.IndexOf("bitrate") + 8;
                int offset = 0;
                //速度
                if (str.Contains("dup"))
                {
                    offset = str.IndexOf("dup") - indexStart;
                }
                else
                {
                    offset = str.IndexOf("speed") - indexStart;
                }

                SpeedLabel.Content = str.Substring(indexStart, offset);
                //LabelBitrateColor.Background = _brushLawnGreen;
                //时间
                //indexStart = str.IndexOf("time") + 5;
                //LabelTime.Content = str.Substring(indexStart, 8) + " (Live)";
                //帧率
                //indexStart = str.IndexOf("fps") + 4;
                //offset = str.IndexOf("q=") - indexStart;
                //FpsLabel.Content = str.Substring(indexStart, offset) + "帧/秒";
                return;
            }
            if (str.Contains("bitrate") && str.Contains("speed") && str.Contains("time"))
            {
                StatusLabel.Content = "正在直播";
                str = str.Replace(" ", "");
                var indexStart = str.IndexOf("bitrate") + 8;
                int offset = str.IndexOf("speed") - indexStart;
                //速度
                SpeedLabel.Content = str.Substring(indexStart, offset);
                //LabelBitrateColor.Background = _brushLawnGreen;
                //时间
                //indexStart = str.IndexOf("time") + 5;
                //LabelTime.Content = str.Substring(indexStart, 8) + " (Live)";
                //帧率
                //FpsLabel.Content = "";
                return;
            }
            if (str.Contains("Past duration") && str.Contains("too large"))
            {
                StatusLabel.Content = "Unknown error";
                SpeedLabel.Content = "---kbps";
                StartLiveBtn.IsEnabled = true;
                StopLiveBtn.IsEnabled = false;
                return;
            }
            else
            {
                StatusLabel.Content = "正在连接...";
                SpeedLabel.Content = "---kbps";
                return;
            }
        }

        private void DesktopResolutionCb_DropDownClosed(object sender, EventArgs e)
        {
            desktopResolution = DesktopResolutionCb.Text;
        }

        private void AudioCb_DropDownClosed(object sender, EventArgs e)
        {
            var audioSource = AudioHelper.GetMicrophoneDevices5();
            if (audioSource.Count != 0)
            {
                audioName = AudioCb.SelectedValue.ToString();
            }
            else
            {
                audioName = "";
            }
        }
    }
}
