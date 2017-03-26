using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace EVE_Configer_Client
{
  public partial class MainWindow : Window
  {
    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
    [DllImport("kernel32")]
    private static extern int GetPrivateProfileString(string section, string key, string default_var, StringBuilder retVal, int buff_size, string filePath);

    public MainWindow()
    {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      String mINI_Path = Directory.GetCurrentDirectory() + @"\" + System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".ini";
      StringBuilder mIsAutoUpload = new StringBuilder();
      StringBuilder mIsAutoDownload = new StringBuilder();
      StringBuilder mUesrID = new StringBuilder();
      StringBuilder mGameDir = new StringBuilder();
      GetPrivateProfileString("EVE", "AutoUpload", "False", mIsAutoUpload, 6, mINI_Path);
      GetPrivateProfileString("EVE", "AutoDownload", "False", mIsAutoDownload, 6, mINI_Path);
      GetPrivateProfileString("EVE", "UesrID", "", mUesrID, 33, mINI_Path);
      GetPrivateProfileString("EVE", "GameDir", "", mGameDir, 249, mINI_Path);
      CheckBox1.IsChecked = bool.Parse(mIsAutoUpload.ToString());
      CheckBox2.IsChecked = bool.Parse(mIsAutoDownload.ToString());
      if("".Equals(mUesrID) == false && Utils.GetStringSize(mUesrID.ToString()) == 32)
      {
        TextBox1.Text = mUesrID.ToString();
      }
      if("".Equals(mGameDir) == false)
      {
        TextBox2.Text = mGameDir.ToString();
      }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if(MessageBox.Show("是否保存设置?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
      {
        String mINI_Path = Directory.GetCurrentDirectory() + @"\" + System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".ini";
        WritePrivateProfileString("EVE", "AutoUpload", CheckBox1.IsChecked.ToString(), mINI_Path);
        WritePrivateProfileString("EVE", "AutoDownload", CheckBox2.IsChecked.ToString(), mINI_Path);
        if("".Equals(TextBox1.Text) == false && Utils.GetStringSize(TextBox1.Text) == 32)
        {
          WritePrivateProfileString("EVE", "UesrID", TextBox1.Text, mINI_Path);
        }
        if("".Equals(TextBox2.Text) == false)
        {
          WritePrivateProfileString("EVE", "GameDir", TextBox2.Text, mINI_Path);
        }
      }
    }

    private void Button1_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Button1.IsEnabled = false;
        WebClient mWebClient = new WebClient();
        String mData = System.Text.Encoding.Default.GetString(mWebClient.DownloadData(Utils.mHostUri + "?Config=r"));
        mWebClient.Dispose();
        if("".Equals(mData) == false && Utils.GetStringSize(mData) == 32)
        {
          TextBox1.Text = mData.ToString();
          MessageBox.Show("注册成功,请妥善保管用户ID!!", "", MessageBoxButton.OK);
        }
        else
        {
          MessageBox.Show("服务器错误!!", "", MessageBoxButton.OK);
        }
        Button1.IsEnabled = true;
      }
      catch(Exception mException)
      {
        Debug.WriteLine(mException);
        this.Close();
      }
    }

    private void Button2_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        this.Hide();
        if(CheckBox2.IsChecked == true)
        {
          if(!Utils.DownloadSettings(TextBox1.Text, TextBox2.Text))
          {
            MessageBox.Show("下载设置失败!!", "", MessageBoxButton.OK);
          }
        }
        Process mProc = Process.Start(TextBox2.Text + @"\launcher\launcher.exe");
        mProc.WaitForExit();
        Utils.MyWaitForExit("exefile");
        if(CheckBox1.IsChecked == true)
        {
          if(!Utils.UploadSettings(TextBox1.Text, TextBox2.Text))
          {
            MessageBox.Show("上传设置失败!!", "", MessageBoxButton.OK);
          }
        }
        if(File.Exists(Directory.GetCurrentDirectory() + @"\" + "debug.log") == true)
        {
          File.Delete(Directory.GetCurrentDirectory() + @"\" + "debug.log");
        }
        this.Show();
      }
      catch(Exception mException)
      {
        Debug.WriteLine(mException);
        this.Close();
      }
    }

    private void Button3_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        System.Windows.Forms.FolderBrowserDialog mFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
        System.Windows.Forms.DialogResult mRes = mFolderBrowserDialog.ShowDialog();
        if(mRes == System.Windows.Forms.DialogResult.OK)
        {
          TextBox2.Text = mFolderBrowserDialog.SelectedPath;
        }
      }
      catch(Exception mException)
      {
        Debug.WriteLine(mException);
        this.Close();
      }
    }
  }
}
