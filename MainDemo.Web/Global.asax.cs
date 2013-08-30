using System;
using System.Configuration;
using System.Globalization;
using System.Web;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.AuditTrail;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.ExpressApp.Web;
using DevExpress.ExpressApp.Web.TestScripts;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.AuditTrail;
using DevExpress.Web.ASPxClasses;
using MainDemo.Module.BusinessObjects;
using StackExchange.Profiling;

namespace MainDemo.Web {
    public class Global : System.Web.HttpApplication {
        public Global() {
#if DEBUG
            DevExpress.EasyTest.Framework.EasyTestTracer.Tracer.SetTraceLevel(System.Diagnostics.TraceLevel.Verbose);
#endif
            InitializeComponent();
        }
        protected void Application_Start(object sender, EventArgs e) {
            RenderHelper.RenderMode = DevExpress.Web.ASPxClasses.ControlRenderMode.Lightweight;
            MiniProfilerHelper.RegisterPathsToIgnore();
            ASPxWebControl.CallbackError += new EventHandler(Application_Error);
#if DEBUG
            TestScriptsManager.EasyTestEnabled = true;
#endif
            WebApplication.SetShared(new MainDemoWebApplication());
        }
        protected void Session_Start(object sender, EventArgs e) {
            WebApplication.SetInstance(Session,
                new MainDemoWebApplication());
            WebApplication.Instance.CreateCustomObjectSpaceProvider += new EventHandler<CreateCustomObjectSpaceProviderEventArgs>(Instance_CreateCustomObjectSpaceProvider);
            AuditTrailService.Instance.CustomizeAuditTrailSettings += new CustomizeAuditSettingsEventHandler(Instance_CustomizeAuditTrailSettings);
            AuditTrailService.Instance.QueryCurrentUserName += new QueryCurrentUserNameEventHandler(Instance_QueryCurrentUserName);
            WebApplication.Instance.LastLogonParametersReading += new EventHandler<LastLogonParametersReadingEventArgs>(Instance_LastLogonParametersReading);
            WebApplication.Instance.CustomizeFormattingCulture += new EventHandler<CustomizeFormattingCultureEventArgs>(Instance_CustomizeFormattingCulture);
            if(ConfigurationManager.AppSettings["SiteMode"] != null && ConfigurationManager.AppSettings["SiteMode"].ToLower() == "true") {
                InMemoryDataStoreProvider.Register();
                WebApplication.Instance.ConnectionString = InMemoryDataStoreProvider.ConnectionString;
            }
            else {
                if(ConfigurationManager.ConnectionStrings["ConnectionString"] != null) {
                    WebApplication.Instance.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                }
            }
            DevExpress.ExpressApp.ScriptRecorder.ScriptRecorderControllerBase.ScriptRecorderEnabled = true;

            WebApplication.Instance.Setup();
            WebApplication.Instance.Start();
        }

        void Instance_CreateCustomObjectSpaceProvider(object sender, CreateCustomObjectSpaceProviderEventArgs e) {
            e.ObjectSpaceProvider = new SecuredObjectSpaceProvider((ISelectDataSecurityProvider)WebApplication.Instance.Security, e.ConnectionString, e.Connection) as IObjectSpaceProvider;
        }
        private void Instance_CustomizeFormattingCulture(object sender, CustomizeFormattingCultureEventArgs e) {
            e.FormattingCulture = CultureInfo.GetCultureInfo("en-US");
        }
        private void Instance_LastLogonParametersReading(object sender, LastLogonParametersReadingEventArgs e) {
            if(string.IsNullOrEmpty(e.SettingsStorage.LoadOption("", "UserName"))) {
                e.SettingsStorage.SaveOption("", "UserName", "Sam");
            }
        }
        private void Instance_QueryCurrentUserName(object sender, QueryCurrentUserNameEventArgs e) {
            e.CurrentUserName = String.Format("Web user ({0})", HttpContext.Current.Request.UserHostAddress);
        }
        private void Instance_CustomizeAuditTrailSettings(object sender, CustomizeAuditTrailSettingsEventArgs e) {
            e.AuditTrailSettings.Clear();
            e.AuditTrailSettings.AddType(typeof(Contact), true);
        }
        protected void Application_BeginRequest(object sender, EventArgs e) {
            if (MiniProfilerHelper.IsEnabled())
            {
                MiniProfiler.Start();
            }
            string filePath = HttpContext.Current.Request.PhysicalPath;
            if(!string.IsNullOrEmpty(filePath)
                && (filePath.IndexOf("Images") >= 0) && !System.IO.File.Exists(filePath)) {
                HttpContext.Current.Response.End();
            }
        }
        protected void Application_EndRequest(object sender, EventArgs e) {
            if (MiniProfilerHelper.IsEnabled())
            {
                MiniProfiler.Stop();
            }
        }
        protected void Application_AuthenticateRequest(object sender, EventArgs e) {
        }
        protected void Application_Error(object sender, EventArgs e) {
            ErrorHandling.Instance.ProcessApplicationError();
        }
        protected void Session_End(object sender, EventArgs e) {
            WebApplication.DisposeInstance(Session);
        }
        protected void Application_End(object sender, EventArgs e) {
        }

        #region Web Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
        }
        #endregion
    }
}
