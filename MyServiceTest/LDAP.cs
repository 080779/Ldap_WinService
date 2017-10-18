using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Diagnostics;



namespace MyServiceTest
{
    class LDAP
    {

        const uint LOGON32_LOGON_INTERACTIVE = 2; //通过网络验证账户合法性
        const uint LOGON32_PROVIDER_DEFAULT = 0; //使用默认的Windows 2000/NT NTLM验证方
        public string errorinfo
        {
            get;
            set;
        }

        [DllImport("advapi32.dll")]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, uint dwLogonType, uint dwLogonProvider, out IntPtr phToken);

        public bool IsAuthenticated(string userName, string password, string dc, string domain)
        {
            string srvr = dc;
            string sAMAccountName = userName;
            IntPtr tokenHandle;
            bool checkok = LogonUser(sAMAccountName, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out tokenHandle);
            //EventLog.WriteEntry("RTX", "登录状态：" + checkok, EventLogEntryType.Information,1234);//系统日志
            if (!checkok)
                this.errorinfo = sAMAccountName;
            return checkok;
        }
        public bool CheckADUser(string domainPath, string userName, string password)
        {
            try
            {
                DirectoryEntry domain = new DirectoryEntry(domainPath, userName, password);
                domain.AuthenticationType = AuthenticationTypes.Secure;
                domain.RefreshCache();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public string GetLoginName(string server, string userName)
        {
            string returnStr = string.Empty;
            SearchResultCollection results = null;
            string filter = "(&(objectCategory=user)(objectClass=person)(cn=" + userName + "))";
            string connectionPrefix = string.Format("LDAP://{0}", server);
            try
            {
                using (DirectoryEntry root = new DirectoryEntry(connectionPrefix))
                {
                    using (DirectorySearcher searcher = new DirectorySearcher(root))
                    {
                        searcher.ReferralChasing = ReferralChasingOption.All;
                        searcher.SearchScope = SearchScope.Subtree;
                        searcher.Filter = filter;
                        results = searcher.FindAll();
                    }
                }
                foreach (SearchResult sr in results)
                {
                    DirectoryEntry entry = sr.GetDirectoryEntry();
                    PropertyValueCollection pg = entry.Properties["sAMAccountName"];
                    returnStr = (string)pg.Value;
                }
            }
            catch (System.Exception ex)
            {
                EventLog.WriteEntry("RTX", "验证错误：" + ex.Message, EventLogEntryType.Error, 8830);//系统日志
            }

            return returnStr;
        }

    }
}
