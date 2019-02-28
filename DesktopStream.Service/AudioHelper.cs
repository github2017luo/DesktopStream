using AForge.Video.DirectShow;
using DesktopStream.Service.Models;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopStream.Service
{
    public static class AudioHelper
    {/// <summary>
     /// 获取所有麦克风设备(音频输入设备)
     /// </summary>
     /// <returns></returns>
        public static List<AudioModel> GetMicrophoneDevices()
        {
            var enumerator = new MMDeviceEnumerator();
            var captureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToArray();

            var microphoneList = new List<AudioModel>();
            if (captureDevices.Length > 0)
            {
                for (int i = 0; i < captureDevices.Length; i++)
                {
                    AudioModel microphone = new AudioModel
                    {
                        id = (i+1).ToString(),
                        name = captureDevices[i].FriendlyName
                    };
                    microphoneList.Add(microphone);
                }
            }
            return microphoneList;
        }

        public static List<AudioModel> GetMicrophoneDevices2()
        {
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.AudioInputDevice);
            var microphoneList = new List<AudioModel>();
            if (videoDevices.Count > 0)
            {
                for (int i = 0; i < videoDevices.Count; i++)
                {

                    if (videoDevices[i].Name != "virtual-audio-capturer")
                    {
                        AudioModel microphone = new AudioModel
                        {
                            id = (i + 1).ToString(),
                            name = videoDevices[i].Name
                        };
                        microphoneList.Add(microphone);
                    }
                }
            }
            return microphoneList;
        }

        /// <summary>
        /// 获取本地音频，包括virtual-audio-capturer
        /// </summary>
        /// <returns></returns>
        public static List<string> GetMicrophoneDevices3()
        {
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.AudioInputDevice);
            var microphoneList = new List<string>();
            if (videoDevices.Count > 0)
            {
                for (int i = 0; i < videoDevices.Count; i++)
                {
                    if (videoDevices[i].Name == "virtual-audio-capturer")
                    {
                        microphoneList.Add("virtual-audio-capturer(桌面音频)");
                    }
                    else
                    {
                        microphoneList.Add(videoDevices[i].Name);
                    }

                }
            }
            return microphoneList;
        }

    }
}
