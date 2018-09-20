using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
// 添加命令空间
using System.Net;
using System.Text;
using System.Threading;



public class GetSceneFromFTP : MonoBehaviour
{

    public Thread threadLoadImage;
    private const int ftpport = 21;
    private string ftpUristring = null;
    private NetworkCredential networkCredential;
    public string ftppath;
    private string currentDir = "/";
    public string downname;
    public GameObject LoginInformation;//显示是否登录成功
    public GameObject SceneListMenu;
    void Start()
    {

    }
    #region 与服务器的交互
    //创建FTP连接
    public FtpWebRequest CreateFtpWebRequest(string uri, string requestMethod)
    {
        FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(uri);
        request.Credentials = networkCredential;
        request.KeepAlive = true;
        request.UseBinary = true;
        request.Method = requestMethod;
        return request;
    }
    // 获取服务器返回的响应体
    public FtpWebResponse GetFtpResponse(FtpWebRequest request)
    {
        FtpWebResponse response = null;
        try
        {
            response = (FtpWebResponse)request.GetResponse();
            return response;
        }
        catch (WebException ex)
        {
            return null;
        }
    }
    #endregion
    // 登录服务器事件
    public void btnlogin_Click()
    {
        ftpUristring = "ftp://" + ftppath;
        networkCredential = new NetworkCredential("Administrator", "Qwe123456789.");
        if (ShowFtpFileAndDirectory() == true)
        {
            Debug.Log("连接成功");
            SceneListMenu.SetActive(true);
        }
        else
        {
            LoginInformation.gameObject.SetActive(true);
            Text infor = GameObject.Find("Infor").GetComponent<Text>();
            infor.text = "连接失败，请检查网络";
        }
    }


    // 显示资源列表
    public bool ShowFtpFileAndDirectory()
    {
        try
        {
            string uri = string.Empty;
            if (currentDir == "/")
            {
                uri = ftpUristring;
            }
            else
            {
                uri = ftpUristring + currentDir;
            }
            uri = "ftp://" + ftppath;
            FtpWebRequest request = CreateFtpWebRequest(uri, WebRequestMethods.Ftp.ListDirectoryDetails);
            // 获得服务器返回的响应信息
            FtpWebResponse response = GetFtpResponse(request);
            Debug.Log(response);
            if (response == null)
            {
                return false;
            }
            // 读取网络流数据
            Stream stream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream, Encoding.Default);
            string s = streamReader.ReadToEnd();
            streamReader.Close();
            stream.Close();
            response.Close();
            // 处理并显示文件目录列表
            string[] ftpdir = s.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int length = 0;
            for (int i = 0; i < ftpdir.Length; i++)
            {
                if (ftpdir[i].EndsWith("."))
                {
                    length = ftpdir[i].Length - 2;
                    break;
                }
            }


            for (int i = 0; i < ftpdir.Length; i++)
            {
                s = ftpdir[i];
                int index = s.LastIndexOf('\t');
                if (index == -1)
                {
                    if (length < s.Length)
                    {
                        index = length;
                    }
                    else
                    {
                        continue;
                    }
                }


                string name = s.Substring(index + 1);
                if (name == "." || name == "..")
                {
                    continue;
                }
                // 判断是否为目录，在名称前加"目录"来表示
                if (s[0] == 'd' || (s.ToLower()).Contains("<dir>"))
                {
                    string[] namefield = name.Split(' ');
                    int namefieldlength = namefield.Length;
                    string dirname;
                    dirname = namefield[namefieldlength - 1];
                    dirname = dirname.PadRight(34, ' ');
                    name = dirname;


                }
            }
            for (int i = 0; i < ftpdir.Length; i++)
            {
                s = ftpdir[i];
                int index = s.LastIndexOf('\t');
                if (index == -1)
                {
                    if (length < s.Length)
                    {
                        index = length;
                    }
                    else
                    {
                        continue;
                    }
                }
                string name = s.Substring(index + 1);
                if (name == "." || name == "..")
                {
                    continue;
                }
                // 判断是否为文件
                if (!(s[0] == 'd' || (s.ToLower()).Contains("<dir>")))
                {
                    string[] namefield = name.Split(' ');
                    int namefieldlength = namefield.Length;
                    string filename;
                    filename = namefield[namefieldlength - 1];
                    // 对齐
                    filename = filename.PadRight(34, ' ');
                    name = filename;
                    // 显示文件
                }
                Debug.Log(name);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
    #region
    public string GetUriString(string filename)
    {
        string uri = string.Empty;
        if (currentDir.EndsWith("/"))
        {
            uri = ftpUristring + currentDir + filename;
        }
        else
        {
            uri = ftpUristring + currentDir + "/" + filename;
        }
        return uri;
    }
    // 从服务器上下载文件到本地事件
    public void btndownload_Click()
    {
        string s = UnityEngine.Random.Range(1, 99).ToString();
        threadLoadImage = new Thread(LoadImage);
        threadLoadImage.Start(s);
    }
    // 获得选择的文件

    // 如果选择的是目录或者是返回上层目录，则返回null

    //该函数要挂在Button上

    public string GetSelectedFile()
    {
        string filename = downname;
        return filename;
    }
    // 删除服务器文件事件
    #endregion

    void LoadImage(object str)
    {

        string fileName = GetSelectedFile();
        if (fileName.Length == 0)
        {
            threadLoadImage.Abort();
        }
        // 选择保存文件的位置

        string filePath = "D://" + str.ToString() + fileName;//要具体到名字
        try
        {
            string uri = GetUriString(fileName);
            FtpWebRequest request = CreateFtpWebRequest(uri, WebRequestMethods.Ftp.DownloadFile);
            FtpWebResponse response = GetFtpResponse(request);
            if (response == null)
            {
                threadLoadImage.Abort();
            }
            Stream responseStream = response.GetResponseStream();
            FileStream filestream = File.Create(filePath);
            int buflength = 8196;
            byte[] buffer = new byte[buflength];
            int bytesRead = 1;
            while (bytesRead != 0)
            {
                bytesRead = responseStream.Read(buffer, 0, buflength);
                filestream.Write(buffer, 0, bytesRead);
            }
            //LoginInformation.gameObject.SetActive(true);
            //Text infor = GameObject.Find("Infor").GetComponent<Text>();
            //infor.text = "下载成功";
            //SceneListMenu.SetActive(false);
            responseStream.Close();
            filestream.Close();

            threadLoadImage.Abort();
        }
        catch (WebException ex)
        {
            //LoginInformation.gameObject.SetActive(true);
            //Text infor = GameObject.Find("Infor").GetComponent<Text>();
            //infor.text = "下载失败";
            threadLoadImage.Abort();
        }
    }
}



