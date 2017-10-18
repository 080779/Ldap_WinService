using System;
using RTXSAPILib;
using System.Diagnostics;
using System.Threading;

namespace MyServiceTest
{

    class RTX
    {
        RTXSAPILib.RTXSAPIRootObj RootObj; //声明一个根对象
        RTXSAPILib.RTXSAPIUserAuthObj UserAuthObj; //声明一个用户认证对象
        string server = "127.0.0.1";
        string port = "8006";
        string domain = "LDAP://127.0.0.1/DC=test,DC=com";

        public void CreateRoot()
        {
            //RTXSAPIRootObjClass RootObj = new RTXSAPIRootObjClass();  //创建根对象
            RootObj = new RTXSAPILib.RTXSAPIRootObj(); //创建根对象
            UserAuthObj = RootObj.UserAuthObj;//通过根对象创建用户认证对象
            UserAuthObj.OnRecvUserAuthRequest += new _IRTXSAPIUserAuthObjEvents_OnRecvUserAuthRequestEventHandler(UserAuthObj_OnRecvUserAuthRequest); //订阅用户认证响应事件
            RootObj.ServerIP = server;
            RootObj.ServerPort = Convert.ToInt16(port);
            UserAuthObj.AppGUID = "{8E85315D-342B-417d-9093-57F824638040}"; //设置应用GUID
            UserAuthObj.AppName = "RTX_LDAP_SERVICE"; //设置应用名
        }

        public bool RegApp()
        {
            CreateRoot();
            try
            {
                UserAuthObj.RegisterApp();  //注册应用
                EventLog.WriteEntry("RTX", "注册应用成功", EventLogEntryType.Information, 8811);//系统日志
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool UnregApp()
        {
            CreateRoot();
            try
            {
                UserAuthObj.UnRegisterApp();  //注销应用
                EventLog.WriteEntry("RTX", "注消应用成功", EventLogEntryType.Information, 8812);//系统日志
                return true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("RTX", "注消应用失败" + ex.Message, EventLogEntryType.Information, 8812);//系统日志
                return false;
            }
        }

        public bool StartApp()
        {
            CreateRoot();
            try
            {
                UserAuthObj.StartApp("", 8);  //启动应用
                EventLog.WriteEntry("RTX", "应用启动成功", EventLogEntryType.Information, 8813);//系统日志
                return true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("RTX", "应用启动失败：" + ex.Message, EventLogEntryType.Error, 8803);//系统日志
                return false;

            }
        }

        public bool StopApp()
        {
            CreateRoot();
            try
            {
                UserAuthObj.StopApp();  //停止应用
                EventLog.WriteEntry("RTX", "应用停止成功", EventLogEntryType.Information, 8814);//系统日志
                return true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("RTX", "应用停止失败：" + ex.Message, EventLogEntryType.Error, 8804);//系统日志
                return false;
            }
        }

        public void UserAuthObj_OnRecvUserAuthRequest(string bstrUserName, string bstrPwd, out RTXSAPI_USERAUTH_RESULT pResult)
        {
            pResult = RTXSAPI_USERAUTH_RESULT.RTXSAPI_USERAUTH_RESULT_ERRNOUSER;
            LDAP ldap = new LDAP();
            bool login = false;
            login = ldap.CheckADUser(domain,bstrUserName, bstrPwd);

            if (login)
            {
                pResult = RTXSAPI_USERAUTH_RESULT.RTXSAPI_USERAUTH_RESULT_OK;//设置认证成功，客户端将正常登录
                //RTX_LDAP.WriteLog.LogManager.WriteLog(RTX_LDAP.WriteLog.LogFile.Trace, "用户登录成功：" + bstrUserName);//写入日志到文件
                EventLog.WriteEntry("RTX", "用户登录成功：" + bstrUserName, EventLogEntryType.Information, 8815);//系统日志
            }

            else
            {
                LDAP ldaperror = new LDAP();
                pResult = RTXSAPI_USERAUTH_RESULT.RTXSAPI_USERAUTH_RESULT_ERRNOUSER;//设置认证失败，客户端弹出相应提示
                //RTX_LDAP.WriteLog.LogManager.WriteLog(RTX_LDAP.WriteLog.LogFile.Error, "用户登录失败：" + bstrUserName);//写入日志到文件
                EventLog.WriteEntry("RTX", "用户登录失败：" + bstrUserName + "\nERROR:" + ldaperror.errorinfo, EventLogEntryType.Error, 8805);//系统日志
            }
        }

        public void startService()
        {
            bool regok = false;
            bool startok = false;
            regok = RegApp();
            int regcount = 0;
            while ((!regok) && (regcount < 10))
            {
                regcount++;
                EventLog.WriteEntry("RTX", "注册应用失败，20秒后重试,重试次数：" + regcount, EventLogEntryType.Error, 8802);//系统日志
                Thread.Sleep(30000);
                regok = RegApp();
            }
            int startcount = 0;
            startok = StartApp();
            while ((!startok) && (startcount < 5))
            {
                startcount++;
                EventLog.WriteEntry("RTX", "启动应用失败，10秒后重试,重试次数：" + startcount, EventLogEntryType.Error, 8802);//系统日志
                Thread.Sleep(10000);
                startok = StartApp();
            }
        }

        public void stopService()
        {
            //StopApp();
            bool unregok = false;
            unregok = UnregApp();
            int unregcount = 0;
            while ((!unregok) && (unregcount < 5))
            {
                unregcount++;
                EventLog.WriteEntry("RTX", "停止应用失败，10秒后重试,重试次数：" + unregcount, EventLogEntryType.Error, 8802);//系统日志
                Thread.Sleep(10000);
                unregok = UnregApp();
            }
        }
    }
}
