using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Ionic.Zip;

public class Utils
{
  public static String mHostUri = "";//set a cgi file path

  public static int GetStringSize(String mStr)
  {
    int mCnt = 0;
    while(mCnt < mStr.Length && mStr[mCnt] != '\0')
    {
      mCnt++;
    }
    return mCnt;
  }

  public static void MyWaitForExit(String mProcName)
  {
    bool mRes = true;
    while(mRes)
    {
      mRes = false;
      Process[] mProcList = Process.GetProcesses();
      foreach(Process mProc in mProcList)
      {
        if(mProc.ProcessName.ToUpper().Equals(mProcName.ToUpper()))
        {
          mRes = true;
        }
      }
      Thread.Sleep(1000);
    }
  }

  public static bool DownloadSettings(String mUserID, String mGameDir)
  {
    try
    {
      String mItem = mGameDir.Replace(":", "").Replace(@"\", "_").Replace(" ", "_").ToLower() + "_211.144.214.68";
      String mZipExtDir = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\..\Local\CCP\EVE\" + mItem + @"\";
      using(WebClient mWebClient = new WebClient())
      {
        mWebClient.DownloadFile(Utils.mHostUri + "?Config=d&UserID=" + mUserID, mZipExtDir + mUserID + ".zip");
      }
      if(File.Exists(mZipExtDir + mUserID + ".zip") != true)
      {
        return false;
      }
      using(ZipFile mZip = new ZipFile(mZipExtDir + mUserID + ".zip", Encoding.UTF8))
      {
        mZip.CompressionMethod = Ionic.Zip.CompressionMethod.BZip2;
        mZip.ExtractAll(mZipExtDir, ExtractExistingFileAction.OverwriteSilently);
      }
      File.Delete(mZipExtDir + mUserID + ".zip");
      return true;
    }
    catch(Exception mException)
    {
      Debug.WriteLine(mException);
      return false;
    }
  }

  public static bool UploadSettings(String mUserID, String mGameDir)
  {
    try
    {
      String mItem = mGameDir.Replace(":", "").Replace(@"\", "_").Replace(" ", "_").ToLower() + "_211.144.214.68";
      String mZipExtDir = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\..\Local\CCP\EVE\" + mItem + @"\";
      using(ZipFile mZip = new ZipFile(Encoding.UTF8))
      {
        mZip.CompressionMethod = Ionic.Zip.CompressionMethod.BZip2;
        mZip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
        mZip.AddDirectory(mZipExtDir + "settings", "settings");
        mZip.Save(mZipExtDir + mUserID + ".zip");
      }
      if(File.Exists(mZipExtDir + mUserID + ".zip") != true)
      {
        return false;
      }
      using(WebClient mWebClient = new WebClient())
      {
        mWebClient.UploadFile(Utils.mHostUri + "?Config=u&UserID=" + mUserID, mZipExtDir + mUserID + ".zip");
      }
      File.Delete(mZipExtDir + mUserID + ".zip");
      return true;
    }
    catch(Exception mException)
    {
      Debug.WriteLine(mException);
      return false;
    }
  }
}
