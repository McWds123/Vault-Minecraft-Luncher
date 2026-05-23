using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Demo
{
    /// <summary>
    /// DownloadProgressWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadProgressWindow : Window
    {
        private readonly string _versionId;
        private long _totalBytes = 0;
        private long _downloadedBytes = 0;
        private DateTime _startTime = DateTime.Now;
        private DispatcherTimer _speedTimer = new DispatcherTimer();
        private long _lastDownloadedBytes = 0;

        public DownloadProgressWindow(string versionId)
        {
            _versionId = versionId;
            
            InitializeComponent();
            
            VersionText.Text = $"正在下载版本: {_versionId}";
            StatusText.Text = "初始化中...";
            
            // 设置定时器用于更新下载速度
            _speedTimer.Interval = TimeSpan.FromSeconds(1);
            _speedTimer.Tick += SpeedTimer_Tick;
            _speedTimer.Start();
            
            // 设置窗口关闭事件
            Closing += OnWindowClosing;
        }

        private void SpeedTimer_Tick(object? sender, EventArgs e)
        {
            UpdateDownloadSpeed();
        }

        private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _speedTimer.Stop();
        }

        public void UpdateStatus(string message, bool isSuccess = false, bool isError = false)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = message;
                
                if (isSuccess)
                {
                    StatusText.Foreground = System.Windows.Media.Brushes.Green;
                }
                else if (isError)
                {
                    StatusText.Foreground = System.Windows.Media.Brushes.Red;
                }
                else
                {
                    StatusText.Foreground = System.Windows.Media.Brushes.Black;
                }
            });
        }

        public void SetTotalSize(long totalBytes)
        {
            _totalBytes = totalBytes;
        }

        public void UpdateProgress(int percent, long downloadedBytes, long totalBytes)
        {
            Dispatcher.Invoke(() =>
            {
                _downloadedBytes = downloadedBytes;
                
                ProgressBar.Value = percent;
                
                double downloadedMB = downloadedBytes / 1024.0 / 1024.0;
                double totalMB = totalBytes / 1024.0 / 1024.0;
                ProgressText.Text = $"{percent}% ({downloadedMB:F2} MB / {totalMB:F2} MB)";
            });
        }

        public void UpdateDownloadSpeed(long downloadedBytes = 0)
        {
            if (downloadedBytes > 0)
            {
                _downloadedBytes = downloadedBytes;
            }
            
            Dispatcher.Invoke(() =>
            {
                var elapsed = DateTime.Now - _startTime;
                if (elapsed.TotalSeconds > 0 && _downloadedBytes > 0)
                {
                    double speed = _downloadedBytes / elapsed.TotalSeconds;
                    double speedMB = speed / 1024.0 / 1024.0;
                    
                    SpeedText.Text = $"下载速度: {speedMB:F2} MB/s";
                }
                
                _lastDownloadedBytes = _downloadedBytes;
            });
        }

        public void SetErrorState()
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Foreground = System.Windows.Media.Brushes.Red;
                SpeedText.Foreground = System.Windows.Media.Brushes.Red;
            });
        }
    }
}
