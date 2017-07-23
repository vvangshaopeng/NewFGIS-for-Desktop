
using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DotSpatialGISManager
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Window win = (Window)((FrameworkElement)sender).TemplatedParent;
                win.DragMove();
            }
        }

        //拖动改变窗体大小
        bool isWiden = false;

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isWiden = true;
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isWiden = false;
            Border b = (Border)sender;
            b.ReleaseMouseCapture();
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            Border b = (Border)sender;
            if (isWiden)
            {
                Window win = (Window)((FrameworkElement)sender).TemplatedParent;
                b.CaptureMouse();
                double newWidth = e.GetPosition(win).X + 5;
                double newheight = e.GetPosition(win).Y + 5;
                if (newWidth > 0)
                {
                    win.Width = newWidth;

                }
                if (newheight > 0)
                {
                    win.Height = newheight;
                }
            }
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            Window win = (Window)((FrameworkElement)sender).TemplatedParent;
            win.WindowState = WindowState.Minimized;
        }

        private void btnMaxOrMin_Click(object sender, RoutedEventArgs e)
        {
            Window win = (Window)((FrameworkElement)sender).TemplatedParent;
            if (win.WindowState == System.Windows.WindowState.Maximized)
            {
                win.WindowState = System.Windows.WindowState.Normal;
                return;
            }
            win.WindowState = System.Windows.WindowState.Maximized;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window win = (Window)((FrameworkElement)sender).TemplatedParent;
            win.Close();
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Window win = (Window)((FrameworkElement)sender).TemplatedParent;
            if (win.WindowState == System.Windows.WindowState.Maximized)
            {
                win.WindowState = System.Windows.WindowState.Normal;
                return;
            }
            win.WindowState = System.Windows.WindowState.Maximized;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            //验证License
            if (!StartWin.CanStart())
            {
                this.Shutdown();
            }
            //检查上传地图服务器信息是否配置
            HostConfig.InitHostConfig();
            //启动界面
            CircleProgressBox f = new CircleProgressBox();
            f.ShowPregress();
            f.SetDefaultDescription();
            //Thread.Sleep(3000);//3秒后进入系统
            f.CloseProgress();
            base.OnStartup(e);
        }
    }
}
