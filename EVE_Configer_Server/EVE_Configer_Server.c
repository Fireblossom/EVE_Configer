// system
#include <time.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <linux/limits.h>
#include <sys/stat.h>
#include <sys/types.h>
// nonsystem
#include <cgic.h>
#include <openssl/md5.h>

/*
  500 参数错误
  501 用户ID非法
  502 用户不存在
  503 打开文件失败
  504 上传文件失败
  505 用户已存在
*/

int cgiMain()
{
  cgiHeaderContentType("text/plain");
  char mConfig[2];
  cgiFormStringNoNewlines("Config", mConfig, 2);
  if(*mConfig == 0)
  {
    fprintf(cgiOut, "500");
    return -1;
  }
  switch(*mConfig)
  {
    case 'r'://注册
    {
      unsigned char mTmp1[16 + 4], mTmp2[16], mMD5[33];
      time_t mTime = time(0);
      MD5(cgiRemoteAddr, strlen(cgiRemoteAddr), mTmp1);
      mTmp1[16] = (mTime >> 24) & 0xFF;
      mTmp1[17] = (mTime >> 16) & 0xFF;
      mTmp1[18] = (mTime >> 8) & 0xFF;
      mTmp1[19] = mTime & 0xFF;
      MD5(mTmp1, sizeof(mTmp1), mTmp2);
      for(int mCnt = 0; mCnt < sizeof(mTmp2); mCnt++)
      {
        sprintf(&(mMD5[mCnt * 2]), "%02x", mTmp2[mCnt]);
      }
      mMD5[33] = 0;
      char mFilePath[PATH_MAX] = "/var/eve_settings/";
      if(access(mFilePath, F_OK) == -1)
      {
        mkdir(mFilePath, 0755);
      }
      strcat(mFilePath, mMD5);
      if(access(mFilePath, F_OK) != -1)
      {
        fprintf(cgiOut, "505");
        break;
      }
      FILE *mFile = NULL;
      mFile = fopen(mFilePath, "wb");
      fwrite(mMD5, 1, sizeof(mMD5), cgiOut);
      fclose(mFile);
      break;
    }
    case 'u'://上传
    {
      char mUserID[33];
      char mFilePath[PATH_MAX] = "/var/eve_settings/";
      char mFileData[2048];
      cgiFormStringNoNewlines("UserID", mUserID, 33);
      if(*mUserID == 0)
      {
        fprintf(cgiOut, "501");
        break;
      }
      strcat(mFilePath, mUserID);
      if(access(mFilePath, F_OK) == -1)
      {
        fprintf(cgiOut, "502");
        break;
      }
      FILE *mFile = NULL;
      mFile = fopen(mFilePath, "wb");
      if(mFile == NULL)
      {
        fprintf(cgiOut, "503");
        break;
      }
      cgiFilePtr mFormFile;
      int mSize = 0;
      if(cgiFormFileSize("file", &mSize) != cgiFormSuccess)
      {
        fprintf(cgiOut, "504");
        break;
      }
      if(cgiFormFileOpen("file", &mFormFile) != cgiFormSuccess)
      {
        fprintf(cgiOut, "504");
        break;
      }
      while(cgiFormFileRead(mFormFile, mFileData, sizeof(mFileData), &mSize) == cgiFormSuccess)
      {
        fwrite(mFileData, 1, mSize, mFile);
      }
      cgiFormFileClose(mFormFile);
      fclose(mFile);
      break;
    }
    case 'd'://下载
    {
      char mUserID[33];
      char mFilePath[PATH_MAX] = "/var/eve_settings/";
      char mFileData[2048];
      cgiFormStringNoNewlines("UserID", mUserID, 33);
      if(*mUserID == 0)
      {
        fprintf(cgiOut, "501");
        break;
      }
      strcat(mFilePath, mUserID);
      if(access(mFilePath, F_OK) == -1)
      {
        fprintf(cgiOut, "502");
        break;
      }
      FILE *mFile = NULL;
      mFile = fopen(mFilePath, "rb");
      if(mFile == NULL)
      {
        fprintf(cgiOut, "503");
        break;
      }
      do
      {
        int mSize = fread(mFileData, 1, sizeof(mFileData), mFile);
        fwrite(mFileData, 1, mSize, cgiOut);
      }
      while(!feof(mFile));
      fclose(mFile);
      break;
    }
  }
  return 0;
}

